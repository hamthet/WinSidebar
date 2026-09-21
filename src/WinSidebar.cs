using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

internal static class Native
{
    internal delegate bool EnumProc(IntPtr hwnd, IntPtr state);
    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback, IntPtr state);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool IsWindow(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern bool IsIconic(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern IntPtr GetShellWindow();
    [DllImport("user32.dll")] internal static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern IntPtr GetWindow(IntPtr hwnd, uint command);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern int GetWindowTextLength(IntPtr hwnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int capacity);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")] private static extern IntPtr GetLong64(IntPtr hwnd, int index);
    [DllImport("user32.dll", EntryPoint = "GetWindowLong")] private static extern IntPtr GetLong32(IntPtr hwnd, int index);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("dwmapi.dll")] private static extern int DwmGetWindowAttribute(IntPtr hwnd, int attribute, out int value, int size);
    [DllImport("user32.dll")] internal static extern bool ShowWindowAsync(IntPtr hwnd, int command);
    [DllImport("user32.dll")] internal static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    [DllImport("shell32.dll")] private static extern int SHGetKnownFolderPath(ref Guid id, uint flags, IntPtr token, out IntPtr path);

    internal static long ExStyle(IntPtr hwnd)
    {
        return (IntPtr.Size == 8 ? GetLong64(hwnd, -20) : GetLong32(hwnd, -20)).ToInt64();
    }
    internal static bool IsCloaked(IntPtr hwnd)
    {
        int value;
        try { return DwmGetWindowAttribute(hwnd, 14, out value, 4) == 0 && value != 0; }
        catch (DllNotFoundException) { return false; }
        catch (EntryPointNotFoundException) { return false; }
    }
    internal static string DownloadsPath()
    {
        Guid id = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        IntPtr path = IntPtr.Zero;
        try
        {
            if (SHGetKnownFolderPath(ref id, 0, IntPtr.Zero, out path) == 0 && path != IntPtr.Zero)
                return Marshal.PtrToStringUni(path);
        }
        catch (DllNotFoundException) { }
        catch (EntryPointNotFoundException) { }
        finally { if (path != IntPtr.Zero) Marshal.FreeCoTaskMem(path); }
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
    }
}

internal sealed class WindowItem
{
    internal IntPtr Handle;
    internal string Title;
    internal string Monitor;
    internal bool Minimized;
}

// Style nativo: impede somente a rolagem horizontal, preservando a vertical.
internal sealed class VerticalTree : TreeView
{
    protected override CreateParams CreateParams
    {
        get { CreateParams p = base.CreateParams; p.Style |= 0x8000; return p; } // TVS_NOHSCROLL
    }
}

internal static class IconArt
{
    internal static Image Draw(int kind)
    {
        Bitmap bitmap = new Bitmap(20, 20);
        using (Graphics g = Graphics.FromImage(bitmap))
        using (Pen dark = new Pen(Color.FromArgb(64, 64, 64), 1.5f))
        using (Pen blue = new Pen(Color.Navy, 2f))
        using (Brush gold = new SolidBrush(Color.FromArgb(255, 214, 70)))
        {
            g.Clear(Color.Transparent);
            if (kind == 0 || kind == 1)
            {
                g.FillRectangle(gold, 2, 6, 16, 11);
                g.FillRectangle(gold, 3, 3, 7, 5);
                g.DrawRectangle(dark, 2, 6, 16, 11);
                if (kind == 1)
                {
                    g.DrawLine(blue, 10, 6, 10, 13);
                    g.DrawLine(blue, 7, 10, 10, 13);
                    g.DrawLine(blue, 13, 10, 10, 13);
                }
            }
            else if (kind == 2)
            {
                g.DrawLine(blue, 5, 4, 5, 15);
                g.DrawLine(blue, 5, 5, 14, 10);
                g.DrawLine(blue, 5, 15, 14, 10);
                g.FillEllipse(Brushes.White, 2, 1, 6, 6);
                g.FillEllipse(Brushes.White, 2, 12, 6, 6);
                g.FillEllipse(Brushes.White, 11, 7, 6, 6);
                g.DrawEllipse(blue, 2, 1, 6, 6);
                g.DrawEllipse(blue, 2, 12, 6, 6);
                g.DrawEllipse(blue, 11, 7, 6, 6);
            }
            else
            {
                g.FillEllipse(Brushes.LightSkyBlue, 2, 2, 15, 15);
                g.DrawEllipse(blue, 2, 2, 15, 15);
                g.DrawEllipse(blue, 7, 2, 5, 15);
                g.DrawLine(blue, 2, 9, 17, 9);
            }
        }
        return bitmap;
    }
}

internal sealed class SidebarWindow : Form
{
    private const int TabWidth = 21;
    private const int HeightDefault = 504;
    private const int WmHotkey = 0x0312;
    private const uint ShiftNoRepeat = 0x4004;
    private static readonly int[] Widths = { 211, 260, 324 }; // 211 = -35% de 324
    private static readonly Color Face = Color.FromArgb(212, 208, 200);
    private static readonly Color Navy = Color.FromArgb(0, 0, 128);

    private readonly Font regular = new Font("Microsoft Sans Serif", 8.25f);
    private readonly Font bold = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
    private readonly Panel content = new Panel();
    private readonly Panel header = new Panel();
    private readonly Panel folders = new Panel();
    private readonly Label title = new Label();
    private readonly Label status = new Label();
    private readonly Button tab = new Button();

    private readonly Button sideButton = new Button();
    private readonly Button smallerButton = new Button();
    private readonly Button largerButton = new Button();
    private readonly Button configureButton = new Button();
    private readonly Label folderTitle = new Label();
    private readonly Panel folderHeader = new Panel();
    private readonly Button closeButton = new Button();
    private readonly VerticalTree tree = new VerticalTree();
    private readonly NotifyIcon tray = new NotifyIcon();
    private readonly Timer poll = new Timer();
    private readonly ToolTip tips = new ToolTip();
    private readonly Button[] shortcuts = new Button[4];
    private readonly Image[] icons = new Image[4];
    private readonly ShortcutEntry[] entries = new ShortcutEntry[4];
    private readonly List<int> registered = new List<int>();
    private readonly string settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinSidebar", "settings.ini");

    private int widthIndex;
    private bool leftSide;
    private bool secondary;
    private bool expanded;
    private bool refreshing;
    private bool internalSelection;
    private bool firstRefresh = true;
    private IntPtr selectedHandle = IntPtr.Zero;
    private IntPtr hotkeyHandle = IntPtr.Zero;
    private string signature = "";

    private string hotkeyErrors = "";
    private string browserExecutable = "";
    private bool browserUseSystem;
    private bool configureMode;
    private string configurationError = "";

    internal SidebarWindow()
    {
        browserUseSystem = !File.Exists(settingsPath); // primeira instalação: navegador padrão
        LoadSettings();
        try
        {
            ShortcutEntry[] loaded = ShortcutStore.Read();
            Array.Copy(loaded, entries, 4);
        }
        catch (Exception ex)
        {
            Array.Copy(ShortcutStore.Defaults(), entries, 4);
            configurationError = "A configuração de atalhos não pôde ser carregada. " +
                "O arquivo original foi preservado.\n\n" + ex.Message;
        }
        Text = "WinSidebar";
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        AutoScaleMode = AutoScaleMode.None;
        BackColor = Face;
        Font = regular;
        ClientSize = new Size(TabWidth, HeightDefault);
        content.BackColor = Face;
        content.BorderStyle = BorderStyle.Fixed3D;
        content.Visible = false;
        Controls.Add(content);
        tab.FlatStyle = FlatStyle.Standard;
        tab.BackColor = Face;
        tab.TabStop = false;
        tab.Click += delegate { Expand(!expanded); };
        Controls.Add(tab);
        tips.SetToolTip(tab, "Shift+F1: abrir/recolher");

        header.BackColor = Navy;
        content.Controls.Add(header);
        title.Text = " WinSidebar";
        title.ForeColor = Color.White;
        title.Font = bold;
        title.TextAlign = ContentAlignment.MiddleLeft;
        title.AutoEllipsis = true;
        title.Dock = DockStyle.Fill;
        header.Controls.Add(title);

        ConfigureHeaderButton(closeButton, "X", "Encerrar (com confirmação)");
        closeButton.FlatStyle = FlatStyle.Flat;
        closeButton.FlatAppearance.BorderColor = Color.White;
        closeButton.BackColor = Color.FromArgb(185, 38, 38);
        closeButton.ForeColor = Color.White;
        closeButton.AccessibleName = "Encerrar WinSidebar";
        ConfigureHeaderButton(smallerButton, "◀", "Diminuir largura");
        smallerButton.AccessibleName = "Diminuir largura";
        ConfigureHeaderButton(largerButton, "▶", "Aumentar largura");
        largerButton.AccessibleName = "Aumentar largura";
        ConfigureHeaderButton(sideButton, "↔", "Mover para a outra borda");
        sideButton.AccessibleName = "Mover a barra para a outra borda";
        closeButton.Click += delegate { RequestExit(); };
        smallerButton.Click += delegate { ChangeWidth(-1); };
        largerButton.Click += delegate { ChangeWidth(1); };
        sideButton.Click += delegate { leftSide = !leftSide; Reposition(); SaveSettings(); };
        // Controles na ordem visual: mover, diminuir, aumentar, encerrar.
        header.Controls.Add(closeButton);
        header.Controls.Add(largerButton);
        header.Controls.Add(smallerButton);
        header.Controls.Add(sideButton);
        closeButton.BringToFront();
        largerButton.BringToFront();
        smallerButton.BringToFront();
        sideButton.BringToFront();

        tree.BackColor = Color.White;
        tree.BorderStyle = BorderStyle.Fixed3D;
        tree.Font = regular;
        tree.HideSelection = false;
        tree.FullRowSelect = true;
        tree.ShowPlusMinus = false;
        tree.ShowLines = true;
        tree.ShowRootLines = true;
        tree.ShowNodeToolTips = true;
        tree.AfterSelect += delegate(object sender, TreeViewEventArgs e)
        {
            if (!internalSelection && e.Node != null && e.Node.Tag is IntPtr)
                selectedHandle = (IntPtr)e.Node.Tag;
        };
        tree.NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is IntPtr) Activate((IntPtr)e.Node.Tag);
        };
        tree.KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { ActivateSelected(); e.Handled = true; e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { Expand(false); e.Handled = true; e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.F5) { RefreshWindows(true); e.Handled = true; e.SuppressKeyPress = true; }
        };
        content.Controls.Add(tree);
        status.TextAlign = ContentAlignment.MiddleLeft;
        status.AutoEllipsis = true;
        content.Controls.Add(status);

        folders.BackColor = Face;
        folders.BorderStyle = BorderStyle.Fixed3D;
        content.Controls.Add(folders);
        folderHeader.BackColor = Navy;
        folderHeader.Dock = DockStyle.Top;
        folderHeader.Height = 20;
        folders.Controls.Add(folderHeader);
        folderTitle.Text = " PASTAS / WEB";
        folderTitle.BackColor = Navy;
        folderTitle.ForeColor = Color.White;
        folderTitle.Font = bold;
        folderTitle.TextAlign = ContentAlignment.MiddleLeft;
        folderTitle.Dock = DockStyle.Fill;
        folderHeader.Controls.Add(folderTitle);
        configureButton.Text = "⚙";
        configureButton.Dock = DockStyle.Right;
        configureButton.Width = 27;
        configureButton.Height = 20;
        configureButton.BackColor = Face;
        configureButton.FlatStyle = FlatStyle.Standard;
        configureButton.AccessibleName = "Configurar atalhos";
        configureButton.Click += delegate { ToggleConfigure(); };
        folderHeader.Controls.Add(configureButton);
        configureButton.BringToFront();
        tips.SetToolTip(configureButton, "Entrar ou sair do modo Configurar");
        for (int i = 0; i < 4; i++)
        {
            int slot = i;
            shortcuts[i] = new Button();
            shortcuts[i].FlatStyle = FlatStyle.Standard;
            shortcuts[i].BackColor = Face;
            shortcuts[i].UseVisualStyleBackColor = false;
            shortcuts[i].Text = "";
            shortcuts[i].ImageAlign = ContentAlignment.MiddleCenter;
            shortcuts[i].Click += delegate {
                if (configureMode) EditShortcut(slot);
                else OpenShortcut(slot);
            };
            folders.Controls.Add(shortcuts[i]);
        }
        RefreshShortcutVisuals();

        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripMenuItem main = new ToolStripMenuItem("Usar monitor principal");
        ToolStripMenuItem other = new ToolStripMenuItem("Usar monitor secundário");
        ToolStripMenuItem exit = new ToolStripMenuItem("Encerrar WinSidebar...");
        main.Click += delegate { secondary = false; Reposition(); SaveSettings(); RefreshWindows(true); };
        other.Click += delegate { secondary = true; Reposition(); SaveSettings(); RefreshWindows(true); };
        exit.Click += delegate { RequestExit(); };
        menu.Items.Add(main);
        menu.Items.Add(other);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exit);
        tab.ContextMenuStrip = menu;
        content.ContextMenuStrip = menu;
        tray.Icon = SystemIcons.Application;
        tray.Text = "WinSidebar";
        tray.ContextMenuStrip = menu;
        tray.Visible = true;
        tray.DoubleClick += delegate { Expand(!expanded); };
        poll.Interval = 1800;
        poll.Tick += delegate { if (expanded) RefreshWindows(false); };
        poll.Start();
        Shown += delegate
        {
            Reposition();
            RefreshWindows(true);
            if (configurationError.Length > 0)
                MessageBox.Show(this, configurationError, "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (hotkeyErrors.Length > 0)
            {
                status.Text = "Atalhos indisponíveis: " + hotkeyErrors;
                tray.BalloonTipTitle = "WinSidebar";
                tray.BalloonTipText = "Atalhos em uso: " + hotkeyErrors;
                tray.ShowBalloonTip(6000);
            }
        };
        FormClosed += delegate
        {
            poll.Stop(); poll.Dispose();
            tray.Visible = false; tray.Dispose();

            tips.Dispose();
            foreach (Image icon in icons) if (icon != null) icon.Dispose();
            regular.Dispose(); bold.Dispose();
        };
    }

    private void ConfigureHeaderButton(Button button, string text, string tooltip)
    {
        button.Text = text;
        button.Width = 23;
        button.Dock = DockStyle.Right;
        button.FlatStyle = FlatStyle.Standard;
        button.BackColor = Face;
        button.UseVisualStyleBackColor = false;
        button.TabStop = false;
        tips.SetToolTip(button, tooltip);
    }

    private void LoadSettings()
    {
        try
        {
            if (!File.Exists(settingsPath)) return;
            foreach (string line in File.ReadAllLines(settingsPath))
            {
                string[] pair = line.Split(new char[] { '=' }, 2);
                if (pair.Length != 2) continue;
                int n;
                if (pair[0] == "width" && int.TryParse(pair[1], out n) && n >= 0 && n < Widths.Length) widthIndex = n;
                if (pair[0] == "left") leftSide = pair[1] == "1";

                if (pair[0] == "secondary") secondary = pair[1] == "1";
                if (pair[0] == "browser") browserExecutable = pair[1];
                if (pair[0] == "browserSystem") browserUseSystem = pair[1] == "1";
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    private void SaveSettings()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.WriteAllLines(settingsPath, new string[] {
                "width=" + widthIndex, "left=" + (leftSide ? "1" : "0"),
                "secondary=" + (secondary ? "1" : "0"),
                "browser=" + browserExecutable,
                "browserSystem=" + (browserUseSystem ? "1" : "0")
            });
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    private Screen TargetScreen()
    {
        if (secondary)
            foreach (Screen s in Screen.AllScreens) if (!s.Primary) return s;
        foreach (Screen s in Screen.AllScreens) if (s.Primary) return s;
        return Screen.AllScreens[0];
    }
    private void Reposition()
    {
        Rectangle work = TargetScreen().WorkingArea;
        int height = Math.Min(HeightDefault, Math.Max(130, work.Height - 16));
        int width = expanded ? Math.Min(Widths[widthIndex], work.Width) : TabWidth;
        Bounds = new Rectangle(leftSide ? work.Left : work.Right - width,
            work.Top + Math.Max(0, (work.Height - height) / 2), width, height);
        tab.SetBounds(leftSide ? 0 : width - TabWidth, 0, TabWidth, height);
        content.SetBounds(leftSide ? TabWidth : 0, 0, Math.Max(0, width - TabWidth), height);
        tab.Text = leftSide ? (expanded ? "<" : ">") : (expanded ? ">" : "<");
        tips.SetToolTip(sideButton, leftSide ? "Mover para a direita" : "Mover para a esquerda");
        smallerButton.Enabled = widthIndex > 0;
        largerButton.Enabled = widthIndex < Widths.Length - 1;
        title.Text = " WinSidebar";
        int inside = content.ClientSize.Width;
        header.SetBounds(3, 3, Math.Max(0, inside - 6), 25);
        folders.SetBounds(3, Math.Max(0, height - 67), Math.Max(0, inside - 6), 61);
        status.SetBounds(5, Math.Max(0, folders.Top - 22), Math.Max(0, inside - 10), 19);
        tree.SetBounds(3, 34, Math.Max(0, inside - 6), Math.Max(35, status.Top - 37));
        int available = Math.Max(4, folders.ClientSize.Width - 8);
        int each = Math.Max(1, available / 4);
        for (int i = 0; i < 4; i++)
            shortcuts[i].SetBounds(4 + i * each, 25, (i == 3 ? available - i * each : each) - 3, 29);
    }
    private void Expand(bool show)
    {
        if (expanded == show) return;
        expanded = show;
        if (show) content.Visible = true;
        Reposition();
        if (show) RefreshWindows(true);
        else content.Visible = false;
    }
    private void RequestExit()
    {
        if (MessageBox.Show(this, "Deseja encerrar o WinSidebar?", "Encerrar WinSidebar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes) Close();
    }
    private void ChangeWidth(int direction)
    {
        int next = Math.Max(0, Math.Min(Widths.Length - 1, widthIndex + direction));
        if (next == widthIndex) return;
        widthIndex = next;
        Reposition();
        SaveSettings();
    }
    private void ToggleConfigure()
    {
        configureMode = !configureMode;
        folderTitle.Text = configureMode ? " CONFIGURANDO" : " PASTAS / WEB";
        configureButton.Text = configureMode ? "✓" : "⚙";
        tips.SetToolTip(configureButton, configureMode ? "Concluir configuração" : "Configurar atalhos");
        RefreshShortcutVisuals();
    }
    private void RefreshShortcutVisuals()
    {
        for (int i = 0; i < 4; i++)
        {
            shortcuts[i].Image = null;
            if (icons[i] != null) icons[i].Dispose();
            icons[i] = ShortcutStore.MakeIcon(entries[i]);
            shortcuts[i].Image = icons[i];
            shortcuts[i].AccessibleName = entries[i].Name;
            tips.SetToolTip(shortcuts[i], (configureMode ? "Editar: " : "Abrir: ") +
                entries[i].Name + "\n" + (entries[i].Target.Length == 0 ? "Não configurado" : entries[i].Target));
        }
    }
    private void EditShortcut(int slot)
    {
        using (ShortcutEditor editor = new ShortcutEditor(entries[slot], browserExecutable, browserUseSystem))
        {
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            ShortcutEntry[] candidate = new ShortcutEntry[4];
            for (int i = 0; i < 4; i++) candidate[i] = entries[i].Copy();
            candidate[slot] = editor.Result;
            try
            {
                // Never change the interface or V7 browser settings if this fails.
                ShortcutStore.Write(candidate);
                entries[slot] = editor.Result;
                browserExecutable = editor.BrowserExecutable;
                browserUseSystem = editor.BrowserUseSystem;
                SaveSettings();
                RefreshShortcutVisuals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Não foi possível salvar o atalho. Os dados anteriores foram preservados.\n\n" +
                    ex.Message, "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    private void OpenShortcut(int slot)
    {
        ShortcutEntry entry = entries[slot];
        if (entry.Target.Length == 0)
        {
            MessageBox.Show(this, "Atalho não configurado. Clique na engrenagem para escolher o destino.",
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (entry.Type == "folder" && !Directory.Exists(entry.Target))
        {
            MessageBox.Show(this, "Pasta não encontrada:\n" + entry.Target,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (entry.Type == "website" && !browserUseSystem && !File.Exists(browserExecutable))
        {
            MessageBox.Show(this, "Escolha um navegador no modo Configurar antes de abrir este site.\n" +
                "O navegador padrão não será usado automaticamente.", "WinSidebar",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        try
        {
            if (entry.Type == "website" && !browserUseSystem)
                Process.Start(new ProcessStartInfo(browserExecutable, "\"" + entry.Target + "\"") {
                    UseShellExecute = true });
            else
                Process.Start(new ProcessStartInfo(entry.Target) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Não foi possível abrir:\n" + entry.Target + "\n\n" + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    protected override CreateParams CreateParams
    {
        get { CreateParams p = base.CreateParams; p.ExStyle |= 0x00000080; return p; }
    }
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        hotkeyHandle = Handle;
        hotkeyErrors = "";
        registered.Clear();
        for (int i = 0; i < 4; i++)
        {
            int id = 9801 + i;
            if (Native.RegisterHotKey(Handle, id, ShiftNoRepeat, (uint)((int)Keys.F1 + i)))
                registered.Add(id);
            else hotkeyErrors += (hotkeyErrors.Length == 0 ? "" : ", ") + "Shift+F" + (i + 1);
        }
    }
    protected override void OnHandleDestroyed(EventArgs e)
    {
        foreach (int id in registered) Native.UnregisterHotKey(hotkeyHandle, id);
        registered.Clear(); hotkeyHandle = IntPtr.Zero;
        base.OnHandleDestroyed(e);
    }
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey)
        {
            int id = m.WParam.ToInt32();
            if (id == 9801) Expand(!expanded);
            else if (id == 9802) Step(-1);
            else if (id == 9803) Step(1);
            else if (id == 9804) ActivateSelected();
            return;
        }
        base.WndProc(ref m);
    }
    private List<WindowItem> Snapshot()
    {
        List<WindowItem> list = new List<WindowItem>();
        IntPtr shell = Native.GetShellWindow();
        uint ownPid = (uint)Process.GetCurrentProcess().Id;
        Dictionary<uint, string> names = new Dictionary<uint, string>();
        Native.EnumProc callback = delegate(IntPtr hwnd, IntPtr ignored)
        {
            if (hwnd == shell || hwnd == Handle || !Native.IsWindowVisible(hwnd) || Native.IsCloaked(hwnd)) return true;
            long style = Native.ExStyle(hwnd);
            bool app = (style & 0x00040000L) != 0;
            if (((style & 0x80L) != 0 || Native.GetWindow(hwnd, 4) != IntPtr.Zero) && !app) return true;
            int length = Native.GetWindowTextLength(hwnd);
            if (length < 1 || length > 16384) return true;
            StringBuilder sb = new StringBuilder(length + 1);
            if (Native.GetWindowText(hwnd, sb, sb.Capacity) == 0) return true;
            string name = sb.ToString().Trim();
            if (name.Length == 0) return true;
            if (name.Equals("Calculadora", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Calculator", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Configurações", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Settings", StringComparison.OrdinalIgnoreCase)) return true;
            uint pid;
            Native.GetWindowThreadProcessId(hwnd, out pid);
            if (pid == ownPid) return true;
            if (pid != 0)
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
            list.Add(new WindowItem { Handle = hwnd, Title = name,
                Monitor = Screen.FromHandle(hwnd).DeviceName, Minimized = Native.IsIconic(hwnd) });
            return true;
        };
        Native.EnumWindows(callback, IntPtr.Zero);
        return list;
    }
    private void RefreshWindows(bool force)
    {
        if (refreshing || IsDisposed) return;
        refreshing = true;
        try
        {
            List<WindowItem> items = Snapshot();
            StringBuilder sb = new StringBuilder();
            foreach (WindowItem w in items)
                sb.Append(w.Handle.ToInt64()).Append('|').Append(w.Title).Append('|')
                  .Append(w.Monitor).Append('|').Append(w.Minimized ? '1' : '0').Append(';');
            string current = sb.ToString();
            if (!force && signature == current) return;
            signature = current;
            IntPtr prior = firstRefresh ? Native.GetForegroundWindow() : selectedHandle;
            firstRefresh = false;
            tree.BeginUpdate(); internalSelection = true;
            try
            {
                tree.Nodes.Clear();
                TreeNode selected = null;
                TreeNode first = null;
                Screen[] monitors = Screen.AllScreens;
                for (int i = 0; i < monitors.Length; i++)
                {
                    Screen screen = monitors[i];
                    TreeNode root = new TreeNode("Monitor " + (i + 1) +
                        (screen.Primary ? " - principal" : " - secundário"));
                    root.NodeFont = bold;
                    foreach (WindowItem w in items)
                    {
                        if (w.Monitor != screen.DeviceName) continue;
                        TreeNode node = new TreeNode((w.Minimized ? "_ " : "") + w.Title);
                        node.Tag = w.Handle;
                        node.ToolTipText = w.Title;
                        root.Nodes.Add(node);
                        if (first == null) first = node;
                        if (w.Handle == prior) selected = node;
                    }
                    if (root.Nodes.Count > 0) { tree.Nodes.Add(root); root.Expand(); }
                }
                tree.SelectedNode = selected ?? first;
                selectedHandle = tree.SelectedNode != null && tree.SelectedNode.Tag is IntPtr
                    ? (IntPtr)tree.SelectedNode.Tag : IntPtr.Zero;
                if (tree.SelectedNode != null) tree.SelectedNode.EnsureVisible();
                if (hotkeyErrors.Length == 0) status.Text = items.Count + " janelas | Shift+F1-F4";
            }
            finally { internalSelection = false; tree.EndUpdate(); }
        }
        catch (Exception ex) { status.Text = "Erro: " + ex.GetType().Name; }
        finally { refreshing = false; }
    }
    private List<TreeNode> WindowNodes()
    {
        List<TreeNode> nodes = new List<TreeNode>();
        foreach (TreeNode root in tree.Nodes)
            foreach (TreeNode node in root.Nodes)
                if (node.Tag is IntPtr) nodes.Add(node);
        return nodes;
    }
    private void Step(int delta)
    {
        if (!expanded) Expand(true);
        List<TreeNode> nodes = WindowNodes();
        if (nodes.Count == 0) return;
        int index = -1;
        for (int i = 0; i < nodes.Count; i++)
            if ((IntPtr)nodes[i].Tag == selectedHandle) { index = i; break; }
        index = index < 0 ? 0 : (index + delta + nodes.Count) % nodes.Count;
        tree.SelectedNode = nodes[index];
        tree.SelectedNode.EnsureVisible();
        selectedHandle = (IntPtr)nodes[index].Tag;
    }
    private void ActivateSelected()
    {
        if (!expanded) Expand(true);
        if (selectedHandle != IntPtr.Zero) Activate(selectedHandle);
    }
    private void Activate(IntPtr hwnd)
    {
        if (!Native.IsWindow(hwnd)) { RefreshWindows(true); return; }
        if (Native.IsIconic(hwnd)) Native.ShowWindowAsync(hwnd, 9);
        Native.SetForegroundWindow(hwnd);
        Expand(false);
    }
}

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        bool first;
        using (System.Threading.Mutex mutex = new System.Threading.Mutex(true,
            @"Local\WinSidebarPublic", out first))
        {
            if (!first) { MessageBox.Show("WinSidebar já está aberto.", "WinSidebar"); return; }
            try
            {
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new SidebarWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Falha ao iniciar:\n" + ex.Message, "WinSidebar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { mutex.ReleaseMutex(); }
        }
    }
}
