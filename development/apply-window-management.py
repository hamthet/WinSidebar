#!/usr/bin/env python3
"""One-shot, anchored development patch. Fail closed if the baseline source differs."""
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
source = ROOT / 'src' / 'WinSidebar.cs'
project = ROOT / 'WinSidebar.csproj'
text = source.read_text(encoding='utf-8')
csproj = project.read_text(encoding='utf-8')

def replace_once(old, new):
    global text
    n = text.count(old)
    if n != 1:
        raise SystemExit(f'Expected exactly one source anchor, found {n}: {old[:90]!r}')
    text = text.replace(old, new, 1)

replace_once('    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback, IntPtr state);',
'''    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback, IntPtr state);
    [DllImport("user32.dll")] internal static extern bool EnumChildWindows(IntPtr hwnd, EnumProc callback, IntPtr state);''')
replace_once('    private readonly ShortcutEntry[] entries = new ShortcutEntry[4];',
'''    private readonly ShortcutEntry[] entries = new ShortcutEntry[4];
    private readonly WindowManagement windows = new WindowManagement();
    private readonly Button savePreferencesButton = new Button();
    private readonly Button restoreDefaultsButton = new Button();''')
replace_once('        Text = "WinSidebar";\n        FormBorderStyle = FormBorderStyle.None;',
'''        try { windows.Load(); }
        catch (Exception ex) { configurationError += "\\n\\nA lista de aplicativos ignorados não pôde ser carregada. O arquivo foi preservado.\\n" + ex.Message; }
        Text = "WinSidebar";
        FormBorderStyle = FormBorderStyle.None;''')
replace_once('        tree.NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs e)',
'''        tree.NodeMouseClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Button == MouseButtons.Right && e.Node != null && e.Node.Tag is IntPtr) {
                tree.SelectedNode = e.Node;
                selectedHandle = (IntPtr)e.Node.Tag;
                windows.ShowWindowMenu(this, tree, selectedHandle, e.Location, delegate { RefreshWindows(true); });
            }
        };
        tree.NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs e)''')
replace_once('            if (e.KeyCode == Keys.Enter) { ActivateSelected(); e.Handled = true; e.SuppressKeyPress = true; }',
'''            if ((e.KeyCode == Keys.Apps || (e.Shift && e.KeyCode == Keys.F10)) && tree.SelectedNode != null && tree.SelectedNode.Tag is IntPtr) {
                TreeNode selected = tree.SelectedNode;
                windows.ShowWindowMenu(this, tree, (IntPtr)selected.Tag,
                    new Point(selected.Bounds.Left + 12, selected.Bounds.Bottom), delegate { RefreshWindows(true); });
                e.Handled = true; e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter) { ActivateSelected(); e.Handled = true; e.SuppressKeyPress = true; }''')
replace_once('        folderTitle.Text = " PASTAS / WEB";', '        folderTitle.Text = " ATALHOS";')
replace_once('        configureButton.BringToFront();\n        tips.SetToolTip(configureButton, "Entrar ou sair do modo Configurar");',
'''        configureButton.BringToFront();
        tips.SetToolTip(configureButton, "Entrar ou sair do modo Configurar");
        ConfigureShortcutHeaderButton(savePreferencesButton, "S", "Salvar preferências", delegate { SavePreferences(); });
        ConfigureShortcutHeaderButton(restoreDefaultsButton, "↺", "Restaurar configurações padrão", delegate { RestoreDefaults(); });''')
replace_once('        menu.Items.Add(new ToolStripSeparator());\n        menu.Items.Add(exit);',
'''        ToolStripMenuItem manageIgnored = new ToolStripMenuItem("Gerenciar aplicativos ignorados...");
        manageIgnored.Click += delegate { windows.ManageIgnored(this, delegate { RefreshWindows(true); }); };
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(manageIgnored);
        menu.Items.Add(exit);''')
replace_once('    private void LoadSettings()\n    {',
'''    private void ConfigureShortcutHeaderButton(Button button, string caption, string tooltip, Action action)
    {
        button.Text = caption;
        button.Width = 26; button.Height = 20; button.Dock = DockStyle.Right;
        button.FlatStyle = FlatStyle.Standard; button.BackColor = Face;
        button.TabStop = true; button.AccessibleName = tooltip;
        tips.SetToolTip(button, tooltip);
        button.Click += delegate { action(); };
        folderHeader.Controls.Add(button);
        button.BringToFront();
    }

    private void LoadSettings()
    {''')
replace_once('    private Screen TargetScreen()\n    {',
'''    // Unlike legacy auto-save, explicit Save reports errors and replaces the file atomically.
    private void SaveSettingsStrict()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
        string temporary = settingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temporary, new string[] {
                "width=" + widthIndex, "left=" + (leftSide ? "1" : "0"),
                "secondary=" + (secondary ? "1" : "0"),
                "browser=" + browserExecutable,
                "browserSystem=" + (browserUseSystem ? "1" : "0")
            });
            if (File.Exists(settingsPath)) File.Replace(temporary, settingsPath, settingsPath + ".bak", true);
            else File.Move(temporary, settingsPath);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private static byte[] SnapshotFile(string path) { return File.Exists(path) ? File.ReadAllBytes(path) : null; }
    private static void RestoreFile(string path, byte[] bytes)
    {
        if (bytes == null) { if (File.Exists(path)) File.Delete(path); }
        else { Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, bytes); }
    }

    private void SavePreferences()
    {
        try
        {
            SaveSettingsStrict();
            ShortcutStore.Write(entries);
            windows.Save();
            MessageBox.Show(this, "Preferências salvas.", "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Não foi possível salvar todas as preferências. Algumas alterações podem já estar gravadas.\\n\\n" + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RestoreDefaults()
    {
        if (MessageBox.Show(this, "Restaurar atalhos, posição, navegador e aplicativos ignorados aos padrões?\\nO idioma atual será mantido.",
            "WinSidebar", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        byte[] oldSettings, oldShortcuts, oldIgnored;
        try {
            oldSettings = SnapshotFile(settingsPath);
            oldShortcuts = SnapshotFile(ShortcutStore.FilePath);
            oldIgnored = SnapshotFile(windows.FilePath);
        }
        catch (Exception ex) { MessageBox.Show(this, "Não foi possível preparar a restauração: " + ex.Message, "WinSidebar"); return; }
        ShortcutEntry[] previous = new ShortcutEntry[4];
        for (int i = 0; i < 4; i++) previous[i] = entries[i].Copy();
        int oldWidth = widthIndex; bool oldLeft = leftSide, oldSecondary = secondary;
        string oldBrowser = browserExecutable; bool oldUseSystem = browserUseSystem;
        try
        {
            ShortcutEntry[] defaults = ShortcutStore.Defaults();
            ShortcutStore.Write(defaults);
            widthIndex = 0; leftSide = false; secondary = false;
            browserExecutable = ""; browserUseSystem = true;
            SaveSettingsStrict();
            windows.ResetRules();
            Array.Copy(defaults, entries, 4);
            configureMode = false; folderTitle.Text = " ATALHOS"; configureButton.Text = "⚙";
            RefreshShortcutVisuals(); Reposition(); RefreshWindows(true);
            MessageBox.Show(this, "Configurações padrão restauradas.", "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            widthIndex = oldWidth; leftSide = oldLeft; secondary = oldSecondary;
            browserExecutable = oldBrowser; browserUseSystem = oldUseSystem;
            Array.Copy(previous, entries, 4);
            string recoveryError = "";
            try { RestoreFile(settingsPath, oldSettings); RestoreFile(ShortcutStore.FilePath, oldShortcuts); RestoreFile(windows.FilePath, oldIgnored); windows.Load(); }
            catch (Exception recovery) { recoveryError = "\\nFalha adicional ao recuperar arquivos: " + recovery.Message; }
            RefreshShortcutVisuals(); Reposition(); RefreshWindows(true);
            MessageBox.Show(this, "Não foi possível concluir a restauração; foi tentada a recuperação das preferências anteriores.\\n" + ex.Message + recoveryError,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private Screen TargetScreen()
    {''')
replace_once('        folderTitle.Text = configureMode ? " CONFIGURANDO" : " PASTAS / WEB";',
             '        folderTitle.Text = configureMode ? " CONFIGURANDO" : " ATALHOS";')
replace_once('        Dictionary<uint, string> names = new Dictionary<uint, string>();\n', '')
replace_once('''            if (name.Equals("Calculadora", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Calculator", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Configurações", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Settings", StringComparison.OrdinalIgnoreCase)) return true;
''', '')
replace_once('''            if (pid != 0)
            {
                string processName;
                if (!names.TryGetValue(pid, out processName))
                {
                    processName = "";
                    try { using (Process p = Process.GetProcessById((int)pid)) processName = p.ProcessName; }
                    catch (ArgumentException) { return true; }
                    catch (InvalidOperationException) { return true; }
                    catch (System.ComponentModel.Win32Exception) { }
                    names[pid] = processName;
                }
                if (processName.Equals("CalculatorApp", StringComparison.OrdinalIgnoreCase) ||
                    processName.Equals("WindowsCalculator", StringComparison.OrdinalIgnoreCase) ||
                    processName.Equals("Win32Calc", StringComparison.OrdinalIgnoreCase) ||
                    processName.Equals("SystemSettings", StringComparison.OrdinalIgnoreCase)) return true;
            }
''', '''            if (windows.IsIgnored(hwnd)) return true;
''')
replace_once('''            List<WindowItem> items = Snapshot();
            StringBuilder sb = new StringBuilder();''',
'''            List<WindowItem> items = Snapshot();
            List<IntPtr> live = new List<IntPtr>();
            foreach (WindowItem w in items) live.Add(w.Handle);
            windows.RetainAliases(live);
            StringBuilder sb = new StringBuilder();''')
replace_once('''                sb.Append(w.Handle.ToInt64()).Append('|').Append(w.Title).Append('|')''',
             '''                sb.Append(w.Handle.ToInt64()).Append('|').Append(windows.DisplayTitle(w.Handle, w.Title)).Append('|')''')
replace_once('''                        TreeNode node = new TreeNode((w.Minimized ? "_ " : "") + w.Title);
                        node.Tag = w.Handle;
                        node.ToolTipText = w.Title;''',
'''                        string displayTitle = windows.DisplayTitle(w.Handle, w.Title);
                        TreeNode node = new TreeNode((w.Minimized ? "_ " : "") + displayTitle);
                        node.Tag = w.Handle;
                        node.ToolTipText = displayTitle == w.Title ? w.Title : displayTitle + " — " + w.Title;''')
if '<Compile Include="src/WindowManagement.cs" />' in csproj:
    raise SystemExit('Project already includes WindowManagement.cs; no double application')
anchor = '    <Compile Include="src/ShortcutConfig.cs" />'
if csproj.count(anchor) != 1: raise SystemExit('Unexpected csproj baseline')
csproj = csproj.replace(anchor, anchor + '\n    <Compile Include="src/WindowManagement.cs" />', 1)
source.write_text(text, encoding='utf-8')
project.write_text(csproj, encoding='utf-8')
print('Applied anchored feature patch to src/WinSidebar.cs and WinSidebar.csproj')
