using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Development spike for the text-snippet feature.
// It exercises the same clipboard + SendInput path intended for production,
// but is currently wired only to Ctrl+Shift+F1 with diagnostic literal text.
internal sealed class TextInjectionProbe : IDisposable
{
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;
    private const string MarkerFormat = "WinSidebar.TextInjectionProbe.Marker";

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        internal uint type;
        internal InputUnion data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        internal KEYBDINPUT keyboard;
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

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, INPUT[] inputs, int inputSize);

    [DllImport("user32.dll")]
    private static extern uint GetClipboardSequenceNumber();

    private enum ProbeState
    {
        Idle,
        WaitingForModifierRelease,
        WaitingToRestoreClipboard
    }

    private readonly Timer timer = new Timer();
    private ProbeState state;
    private IntPtr target;
    private Keys triggerKey;
    private string text;
    private string markerToken;
    private DataObject previousClipboard;
    private bool previousClipboardWasEmpty;
    private bool ownsClipboard;
    private uint ownedClipboardSequence;
    private DateTime releaseDeadlineUtc;
    private DateTime restoreAtUtc;

    internal event Action<string> Failed;
    internal event Action Completed;

    internal TextInjectionProbe()
    {
        timer.Interval = 20;
        timer.Tick += delegate { Advance(); };
    }

    internal bool Begin(IntPtr targetWindow, Keys trigger, string literalText, out string error)
    {
        error = "";
        if (state != ProbeState.Idle)
        {
            error = "Another paste probe is still active.";
            return false;
        }
        if (targetWindow == IntPtr.Zero || !Native.IsWindow(targetWindow))
        {
            error = "The foreground target is no longer available.";
            return false;
        }
        uint pid;
        Native.GetWindowThreadProcessId(targetWindow, out pid);
        if (pid == (uint)Process.GetCurrentProcess().Id)
        {
            error = "WinSidebar itself cannot be the paste target.";
            return false;
        }
        if (string.IsNullOrEmpty(literalText))
        {
            error = "The diagnostic text is empty.";
            return false;
        }

        target = targetWindow;
        triggerKey = trigger;
        text = literalText;
        releaseDeadlineUtc = DateTime.UtcNow.AddMilliseconds(1500);
        state = ProbeState.WaitingForModifierRelease;
        timer.Start();
        return true;
    }

    private static bool IsDown(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }

    private void Advance()
    {
        if (state == ProbeState.WaitingForModifierRelease)
        {
            if (DateTime.UtcNow > releaseDeadlineUtc)
            {
                Fail("Timed out while waiting for Ctrl, Shift and the function key to be released.");
                return;
            }
            if (IsDown(Keys.ControlKey) || IsDown(Keys.ShiftKey) || IsDown(triggerKey)) return;
            if (!Native.IsWindow(target) || Native.GetForegroundWindow() != target)
            {
                Fail("The foreground window changed before the paste could start.");
                return;
            }

            try
            {
                CaptureClipboard();
                PutDiagnosticTextOnClipboard();
            }
            catch (Exception ex)
            {
                Fail("The clipboard could not be prepared: " + ex.Message);
                return;
            }

            if (!SendPasteKeystroke())
            {
                TryRestoreAfterFailure();
                Fail("Windows did not accept the simulated Ctrl+V. The target may be elevated or may block simulated input.");
                return;
            }

            restoreAtUtc = DateTime.UtcNow.AddMilliseconds(350);
            state = ProbeState.WaitingToRestoreClipboard;
            return;
        }

        if (state == ProbeState.WaitingToRestoreClipboard && DateTime.UtcNow >= restoreAtUtc)
        {
            try
            {
                RestoreClipboardIfStillOwned();
                Complete();
            }
            catch (Exception ex)
            {
                ownsClipboard = false;
                Fail("The text was sent, but the previous clipboard could not be restored: " + ex.Message);
            }
        }
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

    private void PutDiagnosticTextOnClipboard()
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
        return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) == inputs.Length;
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
            return; // Something newer owns the clipboard; never overwrite it.
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
        state = ProbeState.Idle;
        ClearRequest();
        Action handler = Completed;
        if (handler != null) handler();
    }

    private void Fail(string message)
    {
        timer.Stop();
        state = ProbeState.Idle;
        ClearRequest();
        Action<string> handler = Failed;
        if (handler != null) handler(message);
    }

    private void ClearRequest()
    {
        target = IntPtr.Zero;
        triggerKey = Keys.None;
        text = null;
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
        state = ProbeState.Idle;
        ClearRequest();
    }
}
