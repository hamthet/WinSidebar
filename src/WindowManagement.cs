using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

// Aliases belong to live windows; ignore rules belong to applications, not window titles.
internal sealed class WindowManagement
{
    private sealed class Alias
    {
        internal uint Pid;
        internal long Started;
        internal string Label;
    }

    private readonly Dictionary<IntPtr, Alias> aliases = new Dictionary<IntPtr, Alias>();
    private Dictionary<string, string> ignored = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private readonly string file = Path.Combine(ShortcutStore.Root, "ignored-apps.json");
    private bool corruptStore;

    internal string FilePath { get { return file; } }
    internal string[] IgnoredKeys { get { return ignored.Keys.ToArray(); } }

    internal void Load()
    {
        if (!File.Exists(file)) return;
        if (new FileInfo(file).Length > 65536) { corruptStore = true; throw new InvalidDataException(Localization.Text("windows.ignored_file_too_large")); }
        try
        {
            Dictionary<string, string> parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(file));
            if (parsed == null || parsed.Count > 128) throw new InvalidDataException(Localization.Text("windows.invalid_list"));
            foreach (KeyValuePair<string, string> item in parsed)
                if (string.IsNullOrWhiteSpace(item.Key) || item.Key.Length > 2048 ||
                    !(item.Key.StartsWith("path:", StringComparison.OrdinalIgnoreCase) || item.Key.StartsWith("name:", StringComparison.OrdinalIgnoreCase)) ||
                    string.IsNullOrWhiteSpace(item.Value) || item.Value.Length > 256)
                    throw new InvalidDataException(Localization.Text("windows.invalid_rule"));
            ignored = new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is JsonException || ex is InvalidDataException)
        {
            corruptStore = true;
            throw new InvalidDataException(Localization.Text("windows.load_failed"), ex);
        }
    }

    internal void Save()
    {
        if (corruptStore) throw new InvalidDataException(Localization.Text("windows.corrupt_store"));
        Directory.CreateDirectory(ShortcutStore.Root);
        string temp = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(temp, JsonSerializer.Serialize(ignored), new System.Text.UTF8Encoding(false));
            if (File.Exists(file)) File.Replace(temp, file, file + ".bak", true);
            else File.Move(temp, file);
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    internal void ResetRules()
    {
        ignored.Clear();
        corruptStore = false;
        aliases.Clear();
        Save();
    }

    internal void ForgetAliases() { aliases.Clear(); }

    internal void RetainAliases(ICollection<IntPtr> liveHandles)
    {
        HashSet<IntPtr> live = new HashSet<IntPtr>(liveHandles);
        foreach (IntPtr handle in aliases.Keys.ToArray())
            if (!live.Contains(handle) || !MatchesLifetime(handle, aliases[handle])) aliases.Remove(handle);
    }

    private static long StartTicks(uint pid)
    {
        try { using (Process p = Process.GetProcessById((int)pid)) return p.StartTime.ToUniversalTime().Ticks; }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception) { return 0; }
    }

    private static bool MatchesLifetime(IntPtr hwnd, Alias alias)
    {
        if (!Native.IsWindow(hwnd)) return false;
        uint pid;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        return pid == alias.Pid && (alias.Started == 0 || StartTicks(pid) == alias.Started);
    }

    internal string DisplayTitle(IntPtr hwnd, string original)
    {
        Alias alias;
        if (!aliases.TryGetValue(hwnd, out alias)) return original;
        if (!MatchesLifetime(hwnd, alias)) { aliases.Remove(hwnd); return original; }
        return alias.Label;
    }

    private static string ApplicationIdentity(IntPtr hwnd, out string display)
    {
        display = "";
        uint pid;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        if (pid == 0) return null;
        try
        {
            using (Process outer = Process.GetProcessById((int)pid))
            {
                // UWP frames can host unrelated apps; never ignore ApplicationFrameHost globally.
                if (outer.ProcessName.Equals("ApplicationFrameHost", StringComparison.OrdinalIgnoreCase))
                {
                    uint childPid = 0;
                    Native.EnumChildWindows(hwnd, delegate(IntPtr child, IntPtr unused) {
                        uint candidate;
                        Native.GetWindowThreadProcessId(child, out candidate);
                        if (candidate != 0 && candidate != pid) { childPid = candidate; return false; }
                        return true;
                    }, IntPtr.Zero);
                    if (childPid == 0) return null;
                    return ProcessIdentity(childPid, out display);
                }
            }
            return ProcessIdentity(pid, out display);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception)
        { return null; }
    }

    private static string ProcessIdentity(uint pid, out string display)
    {
        display = "";
        using (Process process = Process.GetProcessById((int)pid))
        {
            string name = process.ProcessName;
            if (string.IsNullOrWhiteSpace(name) || name.Equals("ApplicationFrameHost", StringComparison.OrdinalIgnoreCase)) return null;
            string path = null;
            try { if (process.MainModule != null) path = process.MainModule.FileName; }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception) { }
            if (!string.IsNullOrWhiteSpace(path))
            {
                display = name + " (" + path + ")";
                return "path:" + Path.GetFullPath(path);
            }
            display = name + Localization.Text("windows.identity_by_name");
            return "name:" + name;
        }
    }

    internal bool IsIgnored(IntPtr hwnd)
    {
        string display;
        string identity = ApplicationIdentity(hwnd, out display);
        return identity != null && ignored.ContainsKey(identity);
    }

    internal void ShowWindowMenu(Form owner, TreeView tree, IntPtr hwnd, Point where, Action changed)
    {
        if (!Native.IsWindow(hwnd)) return;
        string display;
        string identity = ApplicationIdentity(hwnd, out display);
        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripMenuItem rename = new ToolStripMenuItem(Localization.Text("windows.rename"));
        rename.Click += delegate { Rename(owner, hwnd, changed); };
        menu.Items.Add(rename);
        ToolStripMenuItem reset = new ToolStripMenuItem(Localization.Text("windows.reset_name"));
        reset.Enabled = aliases.ContainsKey(hwnd);
        reset.Click += delegate { aliases.Remove(hwnd); changed(); };
        menu.Items.Add(reset);
        menu.Items.Add(new ToolStripSeparator());
        ToolStripMenuItem ignore = new ToolStripMenuItem(Localization.Text("windows.ignore") + (identity == null ? Localization.Text("windows.unavailable") : ": " + display));
        ignore.Enabled = identity != null;
        ignore.Click += delegate {
            if (identity == null) return;
            if (MessageBox.Show(owner, Localization.Text("windows.ask_ignore") + display +
                Localization.Text("windows.undo_hint"), "WinSidebar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try { ignored.Add(identity, display); Save(); changed(); }
            catch (Exception ex) { ignored.Remove(identity); MessageBox.Show(owner, Localization.Text("windows.rule_save_failed") + ex.Message, "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        menu.Items.Add(ignore);
        menu.Closed += delegate { menu.Dispose(); };
        menu.Show(tree, where);
    }

    private void Rename(Form owner, IntPtr hwnd, Action changed)
    {
        if (!Native.IsWindow(hwnd)) return;
        uint pid;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        using (Form dialog = new Form())
        using (TextBox input = new TextBox())
        using (Button ok = new Button())
        using (Button cancel = new Button())
        {
            dialog.Text = Localization.Text("windows.rename_title");
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.MaximizeBox = false; dialog.MinimizeBox = false;
            dialog.ClientSize = new Size(400, 110);
            input.SetBounds(12, 15, 376, 23); input.MaxLength = 80;
            Alias previous;
            input.Text = aliases.TryGetValue(hwnd, out previous) ? previous.Label : "";
            ok.Text = Localization.Text("editor.save"); ok.SetBounds(200, 65, 90, 28); ok.DialogResult = DialogResult.OK;
            cancel.Text = Localization.Text("editor.cancel"); cancel.SetBounds(298, 65, 90, 28); cancel.DialogResult = DialogResult.Cancel;
            dialog.Controls.Add(input); dialog.Controls.Add(ok); dialog.Controls.Add(cancel);
            dialog.AcceptButton = ok; dialog.CancelButton = cancel;
            if (dialog.ShowDialog(owner) != DialogResult.OK) return;
            string label = input.Text.Trim();
            if (label.Length == 0) { aliases.Remove(hwnd); changed(); return; }
            if (Native.IsWindow(hwnd)) { aliases[hwnd] = new Alias { Pid = pid, Started = StartTicks(pid), Label = label }; changed(); }
        }
    }

    internal void ManageIgnored(Form owner, Action changed)
    {
        using (Form dialog = new Form())
        using (ListBox list = new ListBox())
        using (Button remove = new Button())
        using (Button clear = new Button())
        using (Button close = new Button())
        {
            dialog.Text = Localization.Text("windows.ignored_title");
            dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.ClientSize = new Size(550, 310);
            list.SetBounds(12, 12, 526, 230);
            list.DisplayMember = "Value";
            foreach (KeyValuePair<string, string> item in ignored.OrderBy(x => x.Value)) list.Items.Add(item);
            remove.Text = Localization.Text("windows.show_again"); remove.SetBounds(12, 258, 160, 30);
            clear.Text = Localization.Text("windows.show_all"); clear.SetBounds(180, 258, 160, 30);
            close.Text = Localization.Text("windows.close"); close.SetBounds(448, 258, 90, 30);
            close.DialogResult = DialogResult.Cancel;
            remove.Click += delegate {
                if (list.SelectedItem == null) return;
                KeyValuePair<string, string> item = (KeyValuePair<string, string>)list.SelectedItem;
                ignored.Remove(item.Key);
                try { Save(); list.Items.Remove(item); changed(); }
                catch (Exception ex) { ignored[item.Key] = item.Value; MessageBox.Show(dialog, ex.Message, Localization.Text("windows.could_not_save"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            clear.Click += delegate {
                if (ignored.Count == 0 || MessageBox.Show(dialog, Localization.Text("windows.show_all_question"), "WinSidebar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                Dictionary<string, string> old = new Dictionary<string, string>(ignored, StringComparer.OrdinalIgnoreCase);
                ignored.Clear();
                try { Save(); list.Items.Clear(); changed(); }
                catch (Exception ex) { ignored = old; MessageBox.Show(dialog, ex.Message, Localization.Text("windows.could_not_save"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            };
            dialog.Controls.Add(list); dialog.Controls.Add(remove); dialog.Controls.Add(clear); dialog.Controls.Add(close);
            dialog.CancelButton = close;
            dialog.ShowDialog(owner);
        }
    }
}
