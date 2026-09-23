using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

internal enum TextInjectionFailure
{
    None,
    Busy,
    TargetUnavailable,
    EmptyText,
    ModifierTimeout,
    TargetChanged,
    FocusFailed,
    ClipboardPrepareFailed,
    SendInputFailed,
    ClipboardRestoreFailed
}

// Cross-application literal text insertion for snippet slots.
// The content is never executed. It is staged on the clipboard, pasted with
// SendInput Ctrl+V, and the previous clipboard is restored only if WinSidebar
// still owns the clipboard generation it created.
internal sealed class TextInjector : IDisposable
{
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;
    private const string MarkerFormat = "WinSidebar.TextInjector.Marker";

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        internal uint type;
        internal InputUnion data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        // MOUSEINPUT is intentionally present even though snippets only send
        // keyboard input. The native INPUT union must retain its full x64 size.
        [FieldOffset(0)]
        internal MOUSEINPUT mouse;
        [FieldOffset(0)]
        internal KEYBDINPUT keyboard;
        [FieldOffset(0)]
        internal HARDWAREINPUT hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        internal int dx;
        internal int dy;
        internal uint mouseData;
        internal uint flags;
        internal uint time;
        internal IntPtr extraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        internal ushort virtualKey;
        internal ushort scanCode;
        internal uint flags;
        internal uint time;
        internal IntPtr extraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        internal uint message;
        internal ushort parameterLow;
        internal ushort parameterHigh;
    }

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, INPUT[] inputs, int inputSize);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    private enum PasteState
    {
        Idle,
        WaitingForModifierRelease,
        WaitingForTargetFocus,
        WaitingToRestoreClipboard
    }

    private readonly Timer timer = new Timer();
    private PasteState state;
    private IntPtr target;
    private Keys triggerKey;
    private string text;
    private bool restoreTargetFocus;
    private string markerToken;
    private DataObject previousClipboard;
    private bool previousClipboardWasEmpty;
    private bool ownsClipboard;
    private uint ownedClipboardSequence;
    private DateTime stateDeadlineUtc;
    private DateTime restoreAtUtc;

    internal event Action<TextInjectionFailure> Failed;
    internal event Action Completed;
    internal bool IsBusy { get { return state != PasteState.Idle; } }

    internal TextInjector()
    {
        timer.Interval = 20;
        timer.Tick += delegate { Advance(); };
    }

    internal bool Begin(IntPtr targetWindow, Keys trigger, string literalText,
        bool refocusTarget, out TextInjectionFailure failure)
    {
        failure = TextInjectionFailure.None;
        if (state != PasteState.Idle)
        {
            failure = TextInjectionFailure.Busy;
            return false;
        }
        if (!IsExternalTarget(targetWindow))
        {
            failure = TextInjectionFailure.TargetUnavailable;
            return false;
        }
        if (string.IsNullOrEmpty(literalText))
        {
            failure = TextInjectionFailure.EmptyText;
            return false;
        }

        target = targetWindow;
        triggerKey = trigger;
        text = literalText;
        restoreTargetFocus = refocusTarget;
        stateDeadlineUtc = DateTime.UtcNow.AddMilliseconds(1500);
        state = PasteState.WaitingForModifierRelease;
        timer.Start();
        return true;
    }

    private static bool IsDown(Keys key)
    {
        return key != Keys.None && (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private static bool IsExternalTarget(IntPtr hwnd)
    {
        if (hwnd == IntPtr.Zero || !Native.IsWindow(hwnd)) return false;
        uint pid;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        return pid != (uint)Process.GetCurrentProcess().Id;
    }

    private void Advance()
    {
        if (state == PasteState.WaitingForModifierRelease)
        {
            if (DateTime.UtcNow > stateDeadlineUtc)
            {
                Fail(TextInjectionFailure.ModifierTimeout);
                return;
            }
            if (IsDown(Keys.ControlKey) || IsDown(Keys.ShiftKey) || IsDown(triggerKey)) return;
            if (!IsExternalTarget(target))
            {
                Fail(TextInjectionFailure.TargetUnavailable);
                return;
            }

            if (restoreTargetFocus)
            {
                if (Native.GetForegroundWindow() != target)
                {
                    Native.SetForegroundWindow(target);
                    stateDeadlineUtc = DateTime.UtcNow.AddMilliseconds(800);
                    state = PasteState.WaitingForTargetFocus;
                    return;
                }
            }
            else if (Native.GetForegroundWindow() != target)
            {
                Fail(TextInjectionFailure.TargetChanged);
                return;
            }

            StartPaste();
            return;
        }

        if (state == PasteState.WaitingForTargetFocus)
        {
            if (!IsExternalTarget(target))
            {
                Fail(TextInjectionFailure.TargetUnavailable);
                return;
            }
            if (Native.GetForegroundWindow() == target)
            {
                StartPaste();
                return;
            }
            if (DateTime.UtcNow > stateDeadlineUtc)
            {
                Fail(TextInjectionFailure.FocusFailed);
                return;
            }
            return;
        }

        if (state == PasteState.WaitingToRestoreClipboard && DateTime.UtcNow >= restoreAtUtc)
        {
            try
            {
                RestoreClipboardIfStillOwned();
                Complete();
            }
            catch (Exception)
            {
                ownsClipboard = false;
                Fail(TextInjectionFailure.ClipboardRestoreFailed);
            }
        }
    }

    private void StartPaste()
    {
        try
        {
            CaptureClipboard();
            PutTextOnClipboard();
        }
        catch (Exception)
        {
            Fail(TextInjectionFailure.ClipboardPrepareFailed);
            return;
        }

        if (!SendPasteKeystroke())
        {
            TryRestoreAfterFailure();
            Fail(TextInjectionFailure.SendInputFailed);
            return;
        }

        restoreAtUtc = DateTime.UtcNow.AddMilliseconds(350);
        state = PasteState.WaitingToRestoreClipboard;
    }

    private void CaptureClipboard()
    {
        IDataObject current = Clipboard.GetDataObject();
        DataObject snapshot = new DataObject();
        bool any = false;
        if (current != null)
        {
            foreach (string format in current.GetFormats(false))
            {
                object value = current.GetData(format, false);
                if (value == null) continue;
                snapshot.SetData(format, false, value);
                any = true;
            }
        }
        previousClipboard = snapshot;
        previousClipboardWasEmpty = !any;
    }

    private void PutTextOnClipboard()
    {
        markerToken = Guid.NewGuid().ToString("N");
        DataObject payload = new DataObject();
        payload.SetText(text, TextDataFormat.UnicodeText);
        payload.SetData(MarkerFormat, false, markerToken);
        Clipboard.SetDataObject(payload, true, 10, 40);
        ownedClipboardSequence = GetClipboardSequenceNumber();
        ownsClipboard = true;
    }

    private static INPUT KeyboardInput(Keys key, bool keyUp)
    {
        INPUT input = new INPUT();
        input.type = InputKeyboard;
        input.data.keyboard.virtualKey = (ushort)key;
        input.data.keyboard.flags = keyUp ? KeyEventKeyUp : 0;
        return input;
    }

    private static bool SendPasteKeystroke()
    {
        INPUT[] inputs = new INPUT[] {
            KeyboardInput(Keys.ControlKey, false),
            KeyboardInput(Keys.V, false),
            KeyboardInput(Keys.V, true),
            KeyboardInput(Keys.ControlKey, true)
        };
        return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) == (uint)inputs.Length;
    }

    private bool ClipboardStillOwned()
    {
        if (!ownsClipboard || GetClipboardSequenceNumber() != ownedClipboardSequence) return false;
        IDataObject current = Clipboard.GetDataObject();
        if (current == null || !current.GetDataPresent(MarkerFormat, false)) return false;
        return string.Equals(current.GetData(MarkerFormat, false) as string, markerToken,
            StringComparison.Ordinal);
    }

    private void RestoreClipboardIfStillOwned()
    {
        if (!ClipboardStillOwned())
        {
            ownsClipboard = false;
            return; // Never overwrite a clipboard generation created after ours.
        }

        if (previousClipboardWasEmpty) Clipboard.Clear();
        else Clipboard.SetDataObject(previousClipboard, true, 10, 40);
        ownsClipboard = false;
    }

    private void TryRestoreAfterFailure()
    {
        try { RestoreClipboardIfStillOwned(); }
        catch (Exception) { ownsClipboard = false; }
    }

    private void Complete()
    {
        timer.Stop();
        state = PasteState.Idle;
        ClearRequest();
        Action handler = Completed;
        if (handler != null) handler();
    }

    private void Fail(TextInjectionFailure failure)
    {
        timer.Stop();
        state = PasteState.Idle;
        ClearRequest();
        Action<TextInjectionFailure> handler = Failed;
        if (handler != null) handler(failure);
    }

    private void ClearRequest()
    {
        target = IntPtr.Zero;
        triggerKey = Keys.None;
        text = null;
        restoreTargetFocus = false;
        markerToken = null;
        previousClipboard = null;
        previousClipboardWasEmpty = false;
        ownsClipboard = false;
        ownedClipboardSequence = 0;
    }

    public void Dispose()
    {
        if (ownsClipboard) TryRestoreAfterFailure();
        timer.Stop();
        timer.Dispose();
        state = PasteState.Idle;
        ClearRequest();
    }
}
