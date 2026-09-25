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
    [DllImport("user32.dll")] internal static extern bool EnumChildWindows(IntPtr hwnd, EnumProc callback, IntPtr state);
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
        // The display label is localized, but the fallback filesystem component is not.
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
    private const int WmMouseActivate = 0x0021;
    private const uint NoRepeat = 0x4000;
    private const uint ShiftNoRepeat = 0x4004;
    private const uint CtrlAltNoRepeat = 0x4003;
    private const int ShortcutHotkeyBase = 9801;
    private const int SnippetHotkeyBase = 9811;
    private const int OpenSidebarHotkeyId = 9829;
    private static readonly int[] Widths = { 211, 260, 324 }; // one-way cycle
    private static readonly int[] Heights = { HeightDefault, 640, 780 }; // one-way cycle, clamped to work area
    private static readonly Color Face = Color.FromArgb(212, 208, 200);
    private static readonly Color Navy = Color.FromArgb(0, 0, 128);

    private readonly Font regular = new Font("Microsoft Sans Serif", 8.25f);
    private readonly Font bold = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
    private readonly Panel content = new Panel();
    private readonly Panel header = new Panel();
    private readonly Panel folders = new Panel();
    private readonly Panel shortcutBody = new Panel();
    private readonly Panel snippetsPanel = new Panel();
    private readonly Panel snippetHeader = new Panel();
    private readonly Panel snippetBody = new Panel();
    private readonly Label snippetTitle = new Label();
    private readonly Label title = new Label();
    private readonly Label status = new Label();
    private readonly Button tab = new Button();

    private readonly Button sideButton = new Button();
    private readonly Button horizontalSizeButton = new Button();
    private readonly Button verticalSizeButton = new Button();
    private readonly Button addShortcutRowButton = new Button();
    private readonly Button removeShortcutRowButton = new Button();
    private readonly Button addSnippetButton = new Button();
    private readonly Button removeSnippetButton = new Button();
    private readonly Label folderTitle = new Label();
    private readonly Panel folderHeader = new Panel();
    private readonly Button closeButton = new Button();
    private readonly VerticalTree tree = new VerticalTree();
    private readonly NotifyIcon tray = new NotifyIcon();
    private readonly Timer poll = new Timer();
    private readonly Timer foregroundPoll = new Timer();
    private readonly ToolTip tips = new ToolTip();
    private readonly Button[] shortcuts = new Button[ShortcutStore.MaxEntries];
    private readonly Image[] icons = new Image[ShortcutStore.MaxEntries];
    private ShortcutEntry[] entries;
    private readonly Button[] snippetPasteButtons = new Button[SnippetStore.MaxSlots];
    private readonly Button[] snippetEditButtons = new Button[SnippetStore.MaxSlots];
    private SnippetEntry[] snippetEntries;
    private readonly WindowManagement windows = new WindowManagement();
    private readonly TextInjector textInjector = new TextInjector();
    private readonly ContextMenuStrip shortcutEditRouter = new ContextMenuStrip();
    private readonly Button restoreDefaultsButton = new Button();
    private readonly Button restoreSnippetDefaultsButton = new Button();
    private readonly List<int> registered = new List<int>();
    private readonly string settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinSidebar", "settings.ini");

    private ToolStripMenuItem monitorPrimaryItem;
    private ToolStripMenuItem monitorSecondaryItem;
    private ToolStripMenuItem ignoredMenuItem;
    private ToolStripMenuItem exitMenuItem;
    private ToolStripMenuItem languageMenu;
    private ToolStripMenuItem englishItem;
    private ToolStripMenuItem portugueseItem;
    private ToolStripMenuItem spanishItem;
    private ToolStripMenuItem russianItem;
    private ToolStripMenuItem chineseItem;
    private int widthIndex;
    private int heightIndex;
    private bool leftSide;
    private bool secondary;
    private bool expanded;
    private bool refreshing;
    private bool internalSelection;
    private bool firstRefresh = true;
    private IntPtr selectedHandle = IntPtr.Zero;
    private IntPtr lastExternalForeground = IntPtr.Zero;
    private IntPtr hotkeyHandle = IntPtr.Zero;
    private string signature = "";

    private string hotkeyErrors = "";
    private string browserExecutable = "";
    private bool browserUseSystem;
    private bool settingsReadFailed;
    private string configurationError = "";

    internal SidebarWindow()
    {
        browserUseSystem = true;
        settingsReadFailed = Localization.SettingsReadFailed;
        LoadSettings();
        if (settingsReadFailed)
            configurationError = Localization.Text("config.settings_unreadable") +
                Localization.Text("config.original_preserved");
        try { entries = ShortcutStore.Read(); }
        catch (Exception ex)
        {
            entries = ShortcutStore.Defaults();
            configurationError = Localization.Text("config.shortcuts_unreadable") +
                Localization.Text("config.original_preserved") + ex.Message;
        }
        try { snippetEntries = SnippetStore.Read(); }
        catch (Exception)
        {
            snippetEntries = SnippetStore.Defaults();
            configurationError += Localization.Text("config.snippets_unreadable") +
                Localization.Text("config.original_preserved");
        }
        try { windows.Load(); }
        catch (Exception ex) { configurationError += Localization.Text("config.ignored_unreadable") + ex.Message; }
        textInjector.Failed += delegate(TextInjectionFailure failure) {
            if (!IsDisposed) ShowTextInjectionFailure(failure);
        };
        textInjector.Completed += delegate {
            if (!IsDisposed) status.Text = Localization.Text("snippets.pasted");
        };
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
        content.MouseEnter += delegate { RememberExternalForeground(); };
        Controls.Add(content);
        tab.FlatStyle = FlatStyle.Standard;
        tab.BackColor = Face;
        tab.TabStop = false;
        tab.Click += delegate { Expand(!expanded); };
        Controls.Add(tab);
        tips.SetToolTip(tab, Localization.Text("sidebar.tab_hint"));

        header.BackColor = Navy;
        content.Controls.Add(header);
        title.Text = " WinSidebar";
        title.ForeColor = Color.White;
        title.Font = bold;
        title.TextAlign = ContentAlignment.MiddleLeft;
        title.AutoEllipsis = true;
        title.Dock = DockStyle.Fill;
        header.Controls.Add(title);

        ConfigureHeaderButton(closeButton, "X", Localization.Text("sidebar.exit_hint"));
        closeButton.FlatStyle = FlatStyle.Flat;
        closeButton.FlatAppearance.BorderColor = Color.White;
        closeButton.BackColor = Color.FromArgb(185, 38, 38);
        closeButton.ForeColor = Color.White;
        closeButton.AccessibleName = Localization.Text("sidebar.exit_accessible");
        ConfigureHeaderButton(horizontalSizeButton, "⇔", Localization.Text("sidebar.next_width"));
        horizontalSizeButton.AccessibleName = Localization.Text("sidebar.next_width");
        ConfigureHeaderButton(verticalSizeButton, "⇕", Localization.Text("sidebar.next_height"));
        verticalSizeButton.AccessibleName = Localization.Text("sidebar.next_height");
        ConfigureHeaderButton(sideButton, "↔", Localization.Text("sidebar.other_edge"));
        sideButton.AccessibleName = Localization.Text("sidebar.move_accessible");
        closeButton.Click += delegate { RequestExit(); };
        horizontalSizeButton.Click += delegate { CycleWidth(); };
        verticalSizeButton.Click += delegate { CycleHeight(); };
        sideButton.Click += delegate { leftSide = !leftSide; Reposition(); SaveSettings(); };
        // Visual order: move edge, cycle width, cycle height, exit.
        header.Controls.Add(closeButton);
        header.Controls.Add(verticalSizeButton);
        header.Controls.Add(horizontalSizeButton);
        header.Controls.Add(sideButton);
        closeButton.BringToFront();
        verticalSizeButton.BringToFront();
        horizontalSizeButton.BringToFront();
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
        // Tree rows have their own context route. Otherwise WM_CONTEXTMENU can
        // bubble to content and replace the window actions with the global menu.
        // Do not show a drop-down while the native context message is in flight.
        tree.NodeMouseClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Button == MouseButtons.Right && e.Node != null && e.Node.Tag is IntPtr) {
                tree.SelectedNode = e.Node;
                selectedHandle = (IntPtr)e.Node.Tag;
            }
        };
        ContextMenuStrip windowContextRouter = new ContextMenuStrip();
        tree.ContextMenuStrip = windowContextRouter;
        windowContextRouter.Opening += delegate(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true; // Router is never itself shown, including on empty rows.
            Point clicked = tree.PointToClient(Cursor.Position);
            TreeNode node = tree.GetNodeAt(clicked);
            if (node == null || !(node.Tag is IntPtr)) {
                // The tree router canceled the native menu. For an empty area or
                // monitor heading, display the existing global menu after WM_CONTEXTMENU.
                tree.BeginInvoke((MethodInvoker)delegate {
                    if (IsDisposed || tree.IsDisposed) return;
                    ContextMenuStrip generalMenu = content.ContextMenuStrip;
                    if (generalMenu != null && !generalMenu.IsDisposed)
                        generalMenu.Show(tree, clicked);
                });
                return;
            }
            tree.SelectedNode = node;
            IntPtr hwnd = (IntPtr)node.Tag;
            selectedHandle = hwnd;
            tree.BeginInvoke((MethodInvoker)delegate {
                if (IsDisposed || tree.IsDisposed || !Native.IsWindow(hwnd)) return;
                windows.ShowWindowMenu(this, tree, hwnd, clicked, delegate { RefreshWindows(true); });
            });
        };
        tree.NodeMouseDoubleClick += delegate(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is IntPtr) Activate((IntPtr)e.Node.Tag);
        };
        tree.KeyDown += delegate(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Apps || (e.Shift && e.KeyCode == Keys.F10)) && tree.SelectedNode != null && tree.SelectedNode.Tag is IntPtr) {
                TreeNode selected = tree.SelectedNode;
                IntPtr hwnd = (IntPtr)selected.Tag;
                Point position = new Point(selected.Bounds.Left + 12, selected.Bounds.Bottom);
                tree.BeginInvoke((MethodInvoker)delegate {
                    if (IsDisposed || tree.IsDisposed || !Native.IsWindow(hwnd)) return;
                    windows.ShowWindowMenu(this, tree, hwnd, position, delegate { RefreshWindows(true); });
                });
                e.Handled = true; e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter) { ActivateSelected(); e.Handled = true; e.SuppressKeyPress = true; }
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
        folderTitle.Text = Localization.Text("sidebar.shortcuts");
        folderTitle.BackColor = Navy;
        folderTitle.ForeColor = Color.White;
        folderTitle.Font = bold;
        folderTitle.TextAlign = ContentAlignment.MiddleLeft;
        folderTitle.Dock = DockStyle.Fill;
        folderHeader.Controls.Add(folderTitle);
        shortcutBody.AutoScroll = true;
        shortcutBody.BackColor = Face;
        folders.Controls.Add(shortcutBody);

        addShortcutRowButton.Text = "+";
        addShortcutRowButton.Dock = DockStyle.Right;
        addShortcutRowButton.Width = 27;
        addShortcutRowButton.Height = 20;
        addShortcutRowButton.BackColor = Face;
        addShortcutRowButton.FlatStyle = FlatStyle.Standard;
        addShortcutRowButton.AccessibleName = Localization.Text("sidebar.add_shortcut_row");
        addShortcutRowButton.Click += delegate { AddShortcutRow(); };
        folderHeader.Controls.Add(addShortcutRowButton);
        addShortcutRowButton.BringToFront();
        tips.SetToolTip(addShortcutRowButton, Localization.Text("sidebar.add_shortcut_row"));

        removeShortcutRowButton.Text = "-";
        removeShortcutRowButton.Dock = DockStyle.Right;
        removeShortcutRowButton.Width = 27;
        removeShortcutRowButton.Height = 20;
        removeShortcutRowButton.BackColor = Face;
        removeShortcutRowButton.FlatStyle = FlatStyle.Standard;
        removeShortcutRowButton.AccessibleName = Localization.Text("sidebar.remove_shortcut_row");
        removeShortcutRowButton.Click += delegate { RemoveShortcutRow(); };
        folderHeader.Controls.Add(removeShortcutRowButton);
        removeShortcutRowButton.BringToFront();
        tips.SetToolTip(removeShortcutRowButton, Localization.Text("sidebar.remove_shortcut_row"));

        ConfigureShortcutHeaderButton(restoreDefaultsButton, "↺",
            Localization.Text("sidebar.restore_defaults"), delegate { RestoreShortcutDefaults(); });

        shortcutEditRouter.Opening += delegate(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            Button source = shortcutEditRouter.SourceControl as Button;
            if (source == null || !(source.Tag is int)) return;
            int slot = (int)source.Tag;
            BeginInvoke((MethodInvoker)delegate {
                if (!IsDisposed && slot >= 0 && slot < entries.Length) EditShortcut(slot);
            });
        };
        EnsureShortcutControls();
        RefreshShortcutVisuals();

        snippetsPanel.BackColor = Face;
        snippetsPanel.BorderStyle = BorderStyle.Fixed3D;
        content.Controls.Add(snippetsPanel);
        snippetHeader.BackColor = Navy;
        snippetHeader.Dock = DockStyle.Top;
        snippetHeader.Height = 20;
        snippetsPanel.Controls.Add(snippetHeader);
        snippetTitle.Text = Localization.Text("snippets.title");
        snippetTitle.BackColor = Navy;
        snippetTitle.ForeColor = Color.White;
        snippetTitle.Font = bold;
        snippetTitle.TextAlign = ContentAlignment.MiddleLeft;
        snippetTitle.Dock = DockStyle.Fill;
        snippetHeader.Controls.Add(snippetTitle);

        addSnippetButton.Text = "+";
        addSnippetButton.Dock = DockStyle.Right;
        addSnippetButton.Width = 27;
        addSnippetButton.Height = 20;
        addSnippetButton.BackColor = Face;
        addSnippetButton.FlatStyle = FlatStyle.Standard;
        addSnippetButton.AccessibleName = Localization.Text("snippets.add_row");
        addSnippetButton.Click += delegate { AddSnippetRow(); };
        snippetHeader.Controls.Add(addSnippetButton);
        addSnippetButton.BringToFront();
        tips.SetToolTip(addSnippetButton, Localization.Text("snippets.add_row"));

        removeSnippetButton.Text = "-";
        removeSnippetButton.Dock = DockStyle.Right;
        removeSnippetButton.Width = 27;
        removeSnippetButton.Height = 20;
        removeSnippetButton.BackColor = Face;
        removeSnippetButton.FlatStyle = FlatStyle.Standard;
        removeSnippetButton.AccessibleName = Localization.Text("snippets.remove_row");
        removeSnippetButton.Click += delegate { RemoveSnippetRow(); };
        snippetHeader.Controls.Add(removeSnippetButton);
        removeSnippetButton.BringToFront();
        tips.SetToolTip(removeSnippetButton, Localization.Text("snippets.remove_row"));

        restoreSnippetDefaultsButton.Text = "↺";
        restoreSnippetDefaultsButton.Dock = DockStyle.Right;
        restoreSnippetDefaultsButton.Width = 27;
        restoreSnippetDefaultsButton.Height = 20;
        restoreSnippetDefaultsButton.BackColor = Face;
        restoreSnippetDefaultsButton.FlatStyle = FlatStyle.Standard;
        restoreSnippetDefaultsButton.AccessibleName = Localization.Text("snippets.restore_defaults");
        restoreSnippetDefaultsButton.Click += delegate { RestoreSnippetDefaults(); };
        snippetHeader.Controls.Add(restoreSnippetDefaultsButton);
        restoreSnippetDefaultsButton.BringToFront();
        tips.SetToolTip(restoreSnippetDefaultsButton, Localization.Text("snippets.restore_defaults"));

        snippetBody.AutoScroll = true;
        snippetBody.BackColor = Face;
        snippetsPanel.Controls.Add(snippetBody);
        EnsureSnippetControls();
        RefreshSnippetVisuals();

        ContextMenuStrip menu = new ContextMenuStrip();
        ToolStripMenuItem main = new ToolStripMenuItem(Localization.Text("sidebar.primary_monitor"));
        ToolStripMenuItem other = new ToolStripMenuItem(Localization.Text("sidebar.secondary_monitor"));
        ToolStripMenuItem exit = new ToolStripMenuItem(Localization.Text("sidebar.exit_menu"));
        main.Click += delegate { secondary = false; Reposition(); SaveSettings(); RefreshWindows(true); };
        other.Click += delegate { secondary = true; Reposition(); SaveSettings(); RefreshWindows(true); };
        exit.Click += delegate { RequestExit(); };
        menu.Items.Add(main);
        menu.Items.Add(other);
        ToolStripMenuItem manageIgnored = new ToolStripMenuItem(Localization.Text("sidebar.manage_ignored"));
        manageIgnored.Click += delegate { windows.ManageIgnored(this, delegate { RefreshWindows(true); }); };
        monitorPrimaryItem = main;
        monitorSecondaryItem = other;
        ignoredMenuItem = manageIgnored;
        exitMenuItem = exit;
        languageMenu = new ToolStripMenuItem(Localization.Text("sidebar.language"));
        englishItem = new ToolStripMenuItem("English");
        portugueseItem = new ToolStripMenuItem("Português (Brasil)");
        spanishItem = new ToolStripMenuItem("Español");
        russianItem = new ToolStripMenuItem("\u0420\u0443\u0441\u0441\u043A\u0438\u0439");
        chineseItem = new ToolStripMenuItem("\u7B80\u4F53\u4E2D\u6587");
        englishItem.Click += delegate { ChangeLanguage("en-US"); };
        portugueseItem.Click += delegate { ChangeLanguage("pt-BR"); };
        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };
        russianItem.Click += delegate { ChangeLanguage("ru-RU"); };
        chineseItem.Click += delegate { ChangeLanguage("zh-CN"); };
        languageMenu.DropDownItems.Add(englishItem);
        languageMenu.DropDownItems.Add(portugueseItem);
        languageMenu.DropDownItems.Add(spanishItem);
        languageMenu.DropDownItems.Add(russianItem);
        languageMenu.DropDownItems.Add(chineseItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(manageIgnored);
        menu.Items.Add(languageMenu);
        menu.Items.Add(exit);
        tab.ContextMenuStrip = menu;
        content.ContextMenuStrip = menu;
        tray.Icon = SystemIcons.Application;
        tray.Text = "WinSidebar";
        tray.ContextMenuStrip = menu;
        tray.Visible = true;
        tray.DoubleClick += delegate { Expand(!expanded); };
        ApplyLanguage();
        poll.Interval = 1800;
        poll.Tick += delegate { if (expanded) RefreshWindows(false); };
        poll.Start();
        foregroundPoll.Interval = 100;
        foregroundPoll.Tick += delegate { RememberExternalForeground(); };
        foregroundPoll.Start();
        Shown += delegate
        {
            Reposition();
            RefreshWindows(true);
            if (configurationError.Length > 0)
                MessageBox.Show(this, configurationError, "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            if (hotkeyErrors.Length > 0)
            {
                status.Text = Localization.Text("sidebar.hotkeys_unavailable") + hotkeyErrors;
                tray.BalloonTipTitle = "WinSidebar";
                tray.BalloonTipText = Localization.Text("sidebar.hotkeys_in_use") + hotkeyErrors;
                tray.ShowBalloonTip(6000);
            }
        };
        FormClosed += delegate
        {
            poll.Stop(); poll.Dispose();
            foregroundPoll.Stop(); foregroundPoll.Dispose();
            tray.Visible = false; tray.Dispose();

            windowContextRouter.Dispose();
            shortcutEditRouter.Dispose();
            textInjector.Dispose();
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

    private void ConfigureShortcutHeaderButton(Button button, string caption, string tooltip, Action action)
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

    private void ChangeLanguage(string code)
    {
        if (Localization.Current == code) return;
        string oldLanguage = Localization.Current;
        Localization.Select(code);
        try { SaveSettingsStrict(); }
        catch (Exception ex)
        {
            Localization.Select(oldLanguage);
            MessageBox.Show(this, Localization.Text("sidebar.language_save_failed") + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        ApplyLanguage();
        signature = "";
        RefreshWindows(true);
    }

    private void ApplyLanguage()
    {
        tips.SetToolTip(tab, Localization.Text("sidebar.tab_hint"));
        tips.SetToolTip(closeButton, Localization.Text("sidebar.exit_hint"));
        closeButton.AccessibleName = Localization.Text("sidebar.exit_accessible");
        tips.SetToolTip(horizontalSizeButton, Localization.Text("sidebar.next_width"));
        horizontalSizeButton.AccessibleName = Localization.Text("sidebar.next_width");
        tips.SetToolTip(verticalSizeButton, Localization.Text("sidebar.next_height"));
        verticalSizeButton.AccessibleName = Localization.Text("sidebar.next_height");
        tips.SetToolTip(sideButton, Localization.Text("sidebar.other_edge"));
        sideButton.AccessibleName = Localization.Text("sidebar.move_accessible");
        folderTitle.Text = Localization.Text("sidebar.shortcuts");
        addShortcutRowButton.AccessibleName = Localization.Text("sidebar.add_shortcut_row");
        tips.SetToolTip(addShortcutRowButton, Localization.Text("sidebar.add_shortcut_row"));
        removeShortcutRowButton.AccessibleName = Localization.Text("sidebar.remove_shortcut_row");
        tips.SetToolTip(removeShortcutRowButton, Localization.Text("sidebar.remove_shortcut_row"));
        restoreDefaultsButton.AccessibleName = Localization.Text("sidebar.restore_defaults");
        tips.SetToolTip(restoreDefaultsButton, Localization.Text("sidebar.restore_defaults"));
        monitorPrimaryItem.Text = Localization.Text("sidebar.primary_monitor");
        monitorSecondaryItem.Text = Localization.Text("sidebar.secondary_monitor");
        ignoredMenuItem.Text = Localization.Text("sidebar.manage_ignored");
        exitMenuItem.Text = Localization.Text("sidebar.exit_menu");
        languageMenu.Text = Localization.Text("sidebar.language");
        englishItem.Checked = Localization.Current == "en-US";
        portugueseItem.Checked = Localization.Current == "pt-BR";
        spanishItem.Checked = Localization.Current == "es-ES";
        russianItem.Checked = Localization.Current == "ru-RU";
        chineseItem.Checked = Localization.Current == "zh-CN";
        if (hotkeyErrors.Length > 0)
        {
            status.Text = Localization.Text("sidebar.hotkeys_unavailable") + hotkeyErrors;
            tray.BalloonTipText = Localization.Text("sidebar.hotkeys_in_use") + hotkeyErrors;
        }
        snippetTitle.Text = Localization.Text("snippets.title");
        addSnippetButton.AccessibleName = Localization.Text("snippets.add_row");
        tips.SetToolTip(addSnippetButton, Localization.Text("snippets.add_row"));
        removeSnippetButton.AccessibleName = Localization.Text("snippets.remove_row");
        tips.SetToolTip(removeSnippetButton, Localization.Text("snippets.remove_row"));
        restoreSnippetDefaultsButton.AccessibleName = Localization.Text("snippets.restore_defaults");
        tips.SetToolTip(restoreSnippetDefaultsButton, Localization.Text("snippets.restore_defaults"));
        RefreshShortcutVisuals();
        RefreshSnippetVisuals();
        Reposition();
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
                if (pair[0] == "height" && int.TryParse(pair[1], out n) && n >= 0 && n < Heights.Length) heightIndex = n;
                if (pair[0] == "left") leftSide = pair[1] == "1";

                if (pair[0] == "secondary") secondary = pair[1] == "1";
                if (pair[0] == "browser") browserExecutable = pair[1];
                if (pair[0] == "browserSystem") browserUseSystem = pair[1] == "1";
            }
        }
        catch (IOException) { settingsReadFailed = true; }
        catch (UnauthorizedAccessException) { settingsReadFailed = true; }
    }
    private void SaveSettings()
    {
        if (settingsReadFailed) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            File.WriteAllLines(settingsPath, new string[] {
                "width=" + widthIndex, "height=" + heightIndex,
                "left=" + (leftSide ? "1" : "0"),
                "secondary=" + (secondary ? "1" : "0"),
                "browser=" + browserExecutable,
                "browserSystem=" + (browserUseSystem ? "1" : "0"),
                "language=" + Localization.Current
            });
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    // Unlike legacy auto-save, explicit Save reports errors and replaces the file atomically.
    private void SaveSettingsStrict()
    {
        if (settingsReadFailed)
            throw new InvalidDataException(Localization.Text("config.settings_unreadable"));
        Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
        string temporary = settingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temporary, new string[] {
                "width=" + widthIndex, "height=" + heightIndex,
                "left=" + (leftSide ? "1" : "0"),
                "secondary=" + (secondary ? "1" : "0"),
                "browser=" + browserExecutable,
                "browserSystem=" + (browserUseSystem ? "1" : "0"),
                "language=" + Localization.Current
            });
            if (File.Exists(settingsPath)) File.Replace(temporary, settingsPath, settingsPath + ".bak", true);
            else File.Move(temporary, settingsPath);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private sealed class FileSnapshot
    {
        internal bool Existed;
        internal byte[] Bytes;
    }

    private static FileSnapshot SnapshotFile(string path)
    {
        bool existed = File.Exists(path);
        byte[] bytes = existed ? File.ReadAllBytes(path) : null;
        return new FileSnapshot { Existed = existed, Bytes = bytes };
    }

    private static void RestoreFile(string path, FileSnapshot snapshot)
    {
        // A null snapshot means snapshot acquisition failed. Never delete or replace
        // an original file when rollback evidence was not captured successfully.
        if (snapshot == null) return;
        if (!snapshot.Existed) { if (File.Exists(path)) File.Delete(path); }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, snapshot.Bytes);
        }
    }


    private void RestoreShortcutDefaults()
    {
        if (MessageBox.Show(this, Localization.Text("sidebar.restore_shortcuts_question"),
            "WinSidebar", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

        FileSnapshot oldFile = null;
        ShortcutEntry[] previous = new ShortcutEntry[entries.Length];
        for (int i = 0; i < entries.Length; i++) previous[i] = entries[i].Copy();
        try
        {
            oldFile = SnapshotFile(ShortcutStore.FilePath);
            ShortcutEntry[] defaults = ShortcutStore.Defaults();
            ShortcutStore.Write(defaults);
            entries = defaults;
            EnsureShortcutControls();
            RefreshShortcutVisuals();
            Reposition();
            MessageBox.Show(this, Localization.Text("sidebar.restore_shortcuts_success"),
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            entries = previous;
            EnsureShortcutControls();
            try { RestoreFile(ShortcutStore.FilePath, oldFile); } catch (Exception) { }
            RefreshShortcutVisuals();
            Reposition();
            MessageBox.Show(this, Localization.Text("sidebar.save_shortcut_failed") + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RestoreSnippetDefaults()
    {
        if (MessageBox.Show(this, Localization.Text("snippets.restore_question"),
            "WinSidebar", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

        FileSnapshot oldFile = null;
        SnippetEntry[] previous = new SnippetEntry[snippetEntries.Length];
        for (int i = 0; i < snippetEntries.Length; i++) previous[i] = snippetEntries[i].Copy();
        try
        {
            oldFile = SnapshotFile(SnippetStore.FilePath);
            SnippetEntry[] defaults = SnippetStore.Defaults();
            SnippetStore.Write(defaults);
            snippetEntries = defaults;
            EnsureSnippetControls();
            ReloadHotkeys();
            RefreshSnippetVisuals();
            Reposition();
            MessageBox.Show(this, Localization.Text("snippets.restore_success"),
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception)
        {
            snippetEntries = previous;
            EnsureSnippetControls();
            try { RestoreFile(SnippetStore.FilePath, oldFile); } catch (Exception) { }
            RefreshSnippetVisuals();
            Reposition();
            MessageBox.Show(this, Localization.Text("snippets.save_failed"),
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
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
        int height = Math.Min(Heights[heightIndex], Math.Max(130, work.Height - 16));
        int width = expanded ? Math.Min(Widths[widthIndex], work.Width) : TabWidth;
        Bounds = new Rectangle(leftSide ? work.Left : work.Right - width,
            work.Top + Math.Max(0, (work.Height - height) / 2), width, height);
        tab.SetBounds(leftSide ? 0 : width - TabWidth, 0, TabWidth, height);
        content.SetBounds(leftSide ? TabWidth : 0, 0, Math.Max(0, width - TabWidth), height);
        tab.Text = leftSide ? (expanded ? "<" : ">") : (expanded ? ">" : "<");
        tips.SetToolTip(sideButton, leftSide ? Localization.Text("sidebar.move_right") : Localization.Text("sidebar.move_left"));
        horizontalSizeButton.Enabled = true;
        verticalSizeButton.Enabled = true;
        title.Text = " WinSidebar";
        int inside = content.ClientSize.Width;
        header.SetBounds(3, 3, Math.Max(0, inside - 6), 25);
        int shortcutRows = Math.Max(1,
            (entries.Length + ShortcutStore.EntriesPerRow - 1) / ShortcutStore.EntriesPerRow);
        int shortcutFullHeight = 30 + shortcutRows * 31;
        int snippetFullHeight = 26 + snippetEntries.Length * 23;
        const int minimumTreeHeight = 70;
        int requiredHeight = 34 + minimumTreeHeight + 22 +
            shortcutFullHeight + 4 + snippetFullHeight + 6;
        int maximumHeight = Math.Max(130, work.Height - 16);
        int desiredHeight = Math.Max(Heights[heightIndex], requiredHeight);
        height = Math.Min(desiredHeight, maximumHeight);
        if (Height != height || Top != work.Top + Math.Max(0, (work.Height - height) / 2))
        {
            Bounds = new Rectangle(leftSide ? work.Left : work.Right - width,
                work.Top + Math.Max(0, (work.Height - height) / 2), width, height);
            tab.SetBounds(leftSide ? 0 : width - TabWidth, 0, TabWidth, height);
            content.SetBounds(leftSide ? TabWidth : 0, 0, Math.Max(0, width - TabWidth), height);
            inside = content.ClientSize.Width;
            header.SetBounds(3, 3, Math.Max(0, inside - 6), 25);
        }

        bool constrained = height < requiredHeight;
        int shortcutBlockHeight = shortcutFullHeight;
        int snippetBlockHeight = snippetFullHeight;
        if (constrained)
        {
            int extraHeight = Math.Max(0, height - HeightDefault);
            int shortcutVisibleRows = Math.Min(shortcutRows, 1 + extraHeight / 90);
            int snippetVisibleRows = Math.Min(snippetEntries.Length, 4 + extraHeight / 45);
            shortcutBlockHeight = 30 + shortcutVisibleRows * 31;
            snippetBlockHeight = 26 + snippetVisibleRows * 23;
        }

        snippetsPanel.SetBounds(3, Math.Max(0, height - snippetBlockHeight - 6),
            Math.Max(0, inside - 6), snippetBlockHeight);
        folders.SetBounds(3, Math.Max(0, snippetsPanel.Top - shortcutBlockHeight - 4),
            Math.Max(0, inside - 6), shortcutBlockHeight);
        status.SetBounds(5, Math.Max(0, folders.Top - 22), Math.Max(0, inside - 10), 19);
        tree.SetBounds(3, 34, Math.Max(0, inside - 6), Math.Max(35, status.Top - 37));

        shortcutBody.SetBounds(0, 20, Math.Max(0, folders.ClientSize.Width),
            Math.Max(0, folders.ClientSize.Height - 20));
        shortcutBody.AutoScroll = constrained && shortcutBlockHeight < shortcutFullHeight;
        shortcutBody.AutoScrollMinSize = shortcutBody.AutoScroll
            ? new Size(0, shortcutRows * 31 + 4) : Size.Empty;
        if (!shortcutBody.AutoScroll) shortcutBody.AutoScrollPosition = Point.Empty;
        int available = Math.Max(4, shortcutBody.ClientSize.Width - 8);
        int each = Math.Max(1, available / ShortcutStore.EntriesPerRow);
        for (int i = 0; i < entries.Length; i++)
        {
            int row = i / ShortcutStore.EntriesPerRow;
            int column = i % ShortcutStore.EntriesPerRow;
            int cellWidth = column == ShortcutStore.EntriesPerRow - 1
                ? available - column * each : each;
            shortcuts[i].SetBounds(4 + column * each, 4 + row * 31,
                Math.Max(1, cellWidth - 3), 29);
        }

        snippetBody.SetBounds(0, 20, Math.Max(0, snippetsPanel.ClientSize.Width),
            Math.Max(0, snippetsPanel.ClientSize.Height - 20));
        snippetBody.AutoScroll = constrained && snippetBlockHeight < snippetFullHeight;
        snippetBody.AutoScrollMinSize = snippetBody.AutoScroll
            ? new Size(0, snippetEntries.Length * 23 + 4) : Size.Empty;
        if (!snippetBody.AutoScroll) snippetBody.AutoScrollPosition = Point.Empty;
        int snippetWidth = Math.Max(0, snippetBody.ClientSize.Width);
        int editWidth = 29;
        int rowLeft = 4;
        int rowRight = Math.Max(rowLeft, snippetWidth - 4);
        for (int i = 0; i < snippetEntries.Length; i++)
        {
            int y = 2 + i * 23;
            int editX = Math.Max(rowLeft, rowRight - editWidth);
            snippetPasteButtons[i].SetBounds(rowLeft, y, Math.Max(1, editX - rowLeft - 2), 22);
            snippetEditButtons[i].SetBounds(editX, y, editWidth, 22);
        }
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
        if (MessageBox.Show(this, Localization.Text("sidebar.exit_question"), Localization.Text("sidebar.exit_accessible"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2) == DialogResult.Yes) Close();
    }
    private void CycleWidth()
    {
        widthIndex = (widthIndex + 1) % Widths.Length;
        Reposition();
        SaveSettings();
    }

    private void CycleHeight()
    {
        heightIndex = (heightIndex + 1) % Heights.Length;
        Reposition();
        SaveSettings();
    }
    private void EnsureShortcutControls()
    {
        for (int i = 0; i < ShortcutStore.MaxEntries; i++)
        {
            if (i < entries.Length)
            {
                if (shortcuts[i] == null)
                {
                    int slot = i;
                    Button button = new Button();
                    button.FlatStyle = FlatStyle.Standard;
                    button.BackColor = Face;
                    button.UseVisualStyleBackColor = false;
                    button.Text = "";
                    button.ImageAlign = ContentAlignment.MiddleCenter;
                    button.Tag = slot;
                    button.ContextMenuStrip = shortcutEditRouter;
                    button.Click += delegate { OpenShortcut(slot); };
                    shortcutBody.Controls.Add(button);
                    shortcuts[i] = button;
                }
                shortcuts[i].Visible = true;
            }
            else if (shortcuts[i] != null)
            {
                shortcuts[i].Visible = false;
                shortcuts[i].Image = null;
                if (icons[i] != null) { icons[i].Dispose(); icons[i] = null; }
            }
        }
        addShortcutRowButton.Enabled = entries.Length < ShortcutStore.MaxEntries;
        removeShortcutRowButton.Enabled = entries.Length > ShortcutStore.MinimumEntries;
    }

    private void EnsureSnippetControls()
    {
        for (int i = 0; i < SnippetStore.MaxSlots; i++)
        {
            if (i < snippetEntries.Length)
            {
                if (snippetPasteButtons[i] == null)
                {
                    int slot = i;
                    Button paste = new Button();
                    paste.FlatStyle = FlatStyle.Standard;
                    paste.BackColor = Face;
                    paste.UseVisualStyleBackColor = false;
                    paste.TextAlign = ContentAlignment.MiddleLeft;
                    paste.AutoEllipsis = true;
                    paste.MouseEnter += delegate { RememberExternalForeground(); };
                    paste.Click += delegate { PasteSnippet(slot, true); };
                    snippetBody.Controls.Add(paste);
                    snippetPasteButtons[i] = paste;

                    Button edit = new Button();
                    edit.Text = "⚙";
                    edit.FlatStyle = FlatStyle.Standard;
                    edit.BackColor = Face;
                    edit.UseVisualStyleBackColor = false;
                    edit.Click += delegate { EditSnippet(slot); };
                    snippetBody.Controls.Add(edit);
                    snippetEditButtons[i] = edit;
                }
                snippetPasteButtons[i].Visible = true;
                snippetEditButtons[i].Visible = true;
            }
            else
            {
                if (snippetPasteButtons[i] != null) snippetPasteButtons[i].Visible = false;
                if (snippetEditButtons[i] != null) snippetEditButtons[i].Visible = false;
            }
        }
        addSnippetButton.Enabled = snippetEntries.Length < SnippetStore.MaxSlots;
        removeSnippetButton.Enabled = snippetEntries.Length > SnippetStore.MinimumSlots;
    }

    private void AddShortcutRow()
    {
        if (entries.Length >= ShortcutStore.MaxEntries) return;
        ShortcutEntry[] candidate = new ShortcutEntry[entries.Length + ShortcutStore.EntriesPerRow];
        for (int i = 0; i < entries.Length; i++) candidate[i] = entries[i].Copy();
        for (int i = entries.Length; i < candidate.Length; i++) candidate[i] = ShortcutStore.CreateEmpty(i);
        try
        {
            ShortcutStore.Write(candidate);
            entries = candidate;
            EnsureShortcutControls();
            RefreshShortcutVisuals();
            Reposition();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, Localization.Text("sidebar.save_shortcut_failed") + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AddSnippetRow()
    {
        if (snippetEntries.Length >= SnippetStore.MaxSlots) return;
        SnippetEntry[] candidate = new SnippetEntry[snippetEntries.Length + 1];
        for (int i = 0; i < snippetEntries.Length; i++) candidate[i] = snippetEntries[i].Copy();
        candidate[candidate.Length - 1] = SnippetStore.CreateDefault(candidate.Length - 1);
        try
        {
            SnippetStore.Write(candidate);
            snippetEntries = candidate;
            EnsureSnippetControls();
            ReloadHotkeys();
            RefreshSnippetVisuals();
            Reposition();
        }
        catch (Exception)
        {
            MessageBox.Show(this, Localization.Text("snippets.save_failed"), "WinSidebar",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RemoveShortcutRow()
    {
        if (entries.Length <= ShortcutStore.MinimumEntries) return;
        int nextLength = entries.Length - ShortcutStore.EntriesPerRow;
        ShortcutEntry[] candidate = new ShortcutEntry[nextLength];
        for (int i = 0; i < nextLength; i++) candidate[i] = entries[i].Copy();
        try
        {
            ShortcutStore.Write(candidate);
            entries = candidate;
            EnsureShortcutControls();
            RefreshShortcutVisuals();
            Reposition();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, Localization.Text("sidebar.save_shortcut_failed") + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RemoveSnippetRow()
    {
        if (snippetEntries.Length <= SnippetStore.MinimumSlots) return;
        int nextLength = snippetEntries.Length - 1;
        SnippetEntry[] candidate = new SnippetEntry[nextLength];
        for (int i = 0; i < nextLength; i++) candidate[i] = snippetEntries[i].Copy();
        try
        {
            SnippetStore.Write(candidate);
            snippetEntries = candidate;
            EnsureSnippetControls();
            ReloadHotkeys();
            RefreshSnippetVisuals();
            Reposition();
        }
        catch (Exception)
        {
            MessageBox.Show(this, Localization.Text("snippets.save_failed"), "WinSidebar",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RefreshShortcutVisuals()
    {
        EnsureShortcutControls();
        for (int i = 0; i < entries.Length; i++)
        {
            string hotkey = i < 4 ? "F" + (i + 1) : "";
            shortcuts[i].Image = null;
            if (icons[i] != null) icons[i].Dispose();
            icons[i] = ShortcutStore.MakeIcon(entries[i]);
            shortcuts[i].Image = icons[i];
            shortcuts[i].AccessibleName = entries[i].Name +
                (hotkey.Length == 0 ? "" : " — " + hotkey);
            tips.SetToolTip(shortcuts[i], Localization.Text("sidebar.open_prefix") + entries[i].Name +
                (hotkey.Length == 0 ? "" : "\n" + hotkey) +
                "\n" + Localization.Text("sidebar.edit_right_click") + "\n" +
                (entries[i].Target.Length == 0 ? Localization.Text("sidebar.not_configured") : entries[i].Target));
        }
    }

    private void RefreshSnippetVisuals()
    {
        EnsureSnippetControls();
        for (int i = 0; i < snippetEntries.Length; i++)
        {
            string name = snippetEntries[i].Name;
            string hotkey = snippetEntries[i].Hotkey ?? "";
            snippetPasteButtons[i].Text = name;
            snippetPasteButtons[i].AccessibleName =
                Localization.Text("snippets.paste_hint_prefix") + name +
                (hotkey.Length == 0 ? "" : " — " + hotkey);
            tips.SetToolTip(snippetPasteButtons[i],
                Localization.Text("snippets.paste_hint_prefix") + name +
                (hotkey.Length == 0 ? "" : "\n" + hotkey));
            snippetEditButtons[i].AccessibleName =
                Localization.Text("snippets.configure_hint_prefix") + name;
            tips.SetToolTip(snippetEditButtons[i],
                Localization.Text("snippets.configure_hint_prefix") + name);
        }
    }

    private void EditSnippet(int slot)
    {
        using (SnippetEditor editor = new SnippetEditor(snippetEntries[slot]))
        {
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            int preferredX = leftSide ? Right + 12 : Left - editor.Width - 12;
            int editorX = Math.Max(workArea.Left,
                Math.Min(preferredX, Math.Max(workArea.Left, workArea.Right - editor.Width)));
            int preferredY = Top + (Height - editor.Height) / 2;
            int editorY = Math.Max(workArea.Top,
                Math.Min(preferredY, Math.Max(workArea.Top, workArea.Bottom - editor.Height)));
            editor.StartPosition = FormStartPosition.Manual;
            editor.Location = new Point(editorX, editorY);
            editor.TopMost = true;
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            if (!string.IsNullOrEmpty(editor.Result.Hotkey))
            {
                for (int i = 0; i < snippetEntries.Length; i++)
                {
                    if (i == slot) continue;
                    if (string.Equals(snippetEntries[i].Hotkey, editor.Result.Hotkey,
                        StringComparison.Ordinal))
                    {
                        MessageBox.Show(this, Localization.Text("snippets.hotkey_duplicate"), "WinSidebar",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }

            SnippetEntry[] candidate = new SnippetEntry[snippetEntries.Length];
            for (int i = 0; i < candidate.Length; i++) candidate[i] = snippetEntries[i].Copy();
            candidate[slot] = editor.Result;
            try
            {
                SnippetStore.Write(candidate);
                snippetEntries[slot] = editor.Result;
                ReloadHotkeys();
                RefreshSnippetVisuals();
            }
            catch (Exception)
            {
                MessageBox.Show(this, Localization.Text("snippets.save_failed"), "WinSidebar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void PasteSnippet(int slot, bool restoreExternalFocus)
    {
        if (slot < 0 || slot >= snippetEntries.Length || textInjector.IsBusy) return;
        SnippetEntry entry = snippetEntries[slot];
        if (entry == null || string.IsNullOrEmpty(entry.Content))
        {
            status.Text = Localization.Text("snippets.empty");
            return;
        }

        IntPtr target = restoreExternalFocus ? lastExternalForeground : Native.GetForegroundWindow();
        TextInjectionFailure failure;
        int functionNumber = SnippetStore.HotkeyFunctionNumber(entry.Hotkey);
        Keys trigger = restoreExternalFocus || functionNumber == 0
            ? Keys.None : (Keys)((int)Keys.F1 + functionNumber - 1);
        if (!textInjector.Begin(target, trigger, entry.Content, restoreExternalFocus, out failure) &&
            failure != TextInjectionFailure.Busy)
            ShowTextInjectionFailure(failure);
    }

    private void RememberExternalForeground()
    {
        IntPtr hwnd = Native.GetForegroundWindow();
        if (hwnd == IntPtr.Zero || hwnd == Native.GetShellWindow() ||
            !Native.IsWindow(hwnd) || !Native.IsWindowVisible(hwnd)) return;
        uint pid;
        Native.GetWindowThreadProcessId(hwnd, out pid);
        if (pid != (uint)Process.GetCurrentProcess().Id)
            lastExternalForeground = hwnd;
    }

    private void ShowTextInjectionFailure(TextInjectionFailure failure)
    {
        if (failure == TextInjectionFailure.None || failure == TextInjectionFailure.Busy) return;
        string key;
        MessageBoxIcon icon = MessageBoxIcon.Warning;
        if (failure == TextInjectionFailure.EmptyText)
        {
            status.Text = Localization.Text("snippets.empty");
            return;
        }
        if (failure == TextInjectionFailure.ModifierTimeout) key = "snippets.modifier_timeout";
        else if (failure == TextInjectionFailure.TargetChanged) key = "snippets.target_changed";
        else if (failure == TextInjectionFailure.FocusFailed) key = "snippets.focus_failed";
        else if (failure == TextInjectionFailure.ClipboardPrepareFailed) key = "snippets.clipboard_failed";
        else if (failure == TextInjectionFailure.SendInputFailed) key = "snippets.send_failed";
        else if (failure == TextInjectionFailure.ClipboardRestoreFailed) key = "snippets.clipboard_restore_failed";
        else key = "snippets.target_unavailable";
        MessageBox.Show(this, Localization.Text(key), "WinSidebar",
            MessageBoxButtons.OK, icon);
    }

    private void EditShortcut(int slot)
    {
        using (ShortcutEditor editor = new ShortcutEditor(entries[slot], browserExecutable, browserUseSystem))
        {
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            int preferredX = leftSide ? Right + 12 : Left - editor.Width - 12;
            int editorX = Math.Max(workArea.Left, Math.Min(preferredX, Math.Max(workArea.Left, workArea.Right - editor.Width)));
            int preferredY = Top + (Height - editor.Height) / 2;
            int editorY = Math.Max(workArea.Top, Math.Min(preferredY, Math.Max(workArea.Top, workArea.Bottom - editor.Height)));
            editor.StartPosition = FormStartPosition.Manual;
            editor.Location = new Point(editorX, editorY);
            editor.TopMost = true;
            if (editor.ShowDialog(this) != DialogResult.OK) return;
            ShortcutEntry[] candidate = new ShortcutEntry[entries.Length];
            for (int i = 0; i < entries.Length; i++) candidate[i] = entries[i].Copy();
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
                MessageBox.Show(this, Localization.Text("sidebar.save_shortcut_failed") +
                    ex.Message, "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
    private void OpenShortcut(int slot)
    {
        ShortcutEntry entry = entries[slot];
        if (entry.Target.Length == 0)
        {
            MessageBox.Show(this, Localization.Text("sidebar.shortcut_missing"),
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (entry.Type == "folder" && !Directory.Exists(entry.Target))
        {
            MessageBox.Show(this, Localization.Text("sidebar.folder_missing") + entry.Target,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        if (entry.Type == "website" && !browserUseSystem && !File.Exists(browserExecutable))
        {
            MessageBox.Show(this, Localization.Text("sidebar.choose_browser") +
                Localization.Text("sidebar.default_browser_not_used"), "WinSidebar",
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
            MessageBox.Show(this, Localization.Text("sidebar.open_failed") + entry.Target + "\n\n" + ex.Message,
                "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    protected override CreateParams CreateParams
    {
        get { CreateParams p = base.CreateParams; p.ExStyle |= 0x00000080; return p; }
    }
    private void ReloadHotkeys()
    {
        if (hotkeyHandle == IntPtr.Zero) return;
        foreach (int id in registered) Native.UnregisterHotKey(hotkeyHandle, id);
        registered.Clear();
        hotkeyErrors = "";

        for (int i = 0; i < 4; i++)
        {
            int id = ShortcutHotkeyBase + i;
            if (Native.RegisterHotKey(hotkeyHandle, id, NoRepeat, (uint)((int)Keys.F1 + i)))
                registered.Add(id);
            else hotkeyErrors += (hotkeyErrors.Length == 0 ? "" : ", ") + "F" + (i + 1);
        }

        for (int i = 0; i < snippetEntries.Length; i++)
        {
            string hotkey = snippetEntries[i].Hotkey ?? "";
            int functionNumber = SnippetStore.HotkeyFunctionNumber(hotkey);
            if (functionNumber == 0) continue;
            int id = SnippetHotkeyBase + i;
            if (Native.RegisterHotKey(hotkeyHandle, id, ShiftNoRepeat,
                (uint)((int)Keys.F1 + functionNumber - 1)))
                registered.Add(id);
            else hotkeyErrors += (hotkeyErrors.Length == 0 ? "" : ", ") + hotkey;
        }

        if (Native.RegisterHotKey(hotkeyHandle, OpenSidebarHotkeyId, CtrlAltNoRepeat, (uint)Keys.Y))
            registered.Add(OpenSidebarHotkeyId);
        else hotkeyErrors += (hotkeyErrors.Length == 0 ? "" : ", ") + "AltGr+Y";

        if (hotkeyErrors.Length > 0)
            status.Text = Localization.Text("sidebar.hotkeys_unavailable") + hotkeyErrors;
        else if (expanded)
        {
            signature = "";
            RefreshWindows(true);
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        hotkeyHandle = Handle;
        ReloadHotkeys();
    }
    protected override void OnHandleDestroyed(EventArgs e)
    {
        foreach (int id in registered) Native.UnregisterHotKey(hotkeyHandle, id);
        registered.Clear(); hotkeyHandle = IntPtr.Zero;
        base.OnHandleDestroyed(e);
    }
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmMouseActivate)
            RememberExternalForeground();

        if (m.Msg == WmHotkey)
        {
            int id = m.WParam.ToInt32();
            if (id == OpenSidebarHotkeyId)
            {
                Expand(!expanded);
            }
            else if (id >= ShortcutHotkeyBase && id < ShortcutHotkeyBase + 4)
                OpenShortcut(id - ShortcutHotkeyBase);
            else if (id >= SnippetHotkeyBase && id < SnippetHotkeyBase + SnippetStore.MaxSlots)
            {
                int slot = id - SnippetHotkeyBase;
                if (slot >= 0 && slot < snippetEntries.Length)
                    PasteSnippet(slot, false);
            }
            return;
        }
        base.WndProc(ref m);
    }
    private List<WindowItem> Snapshot()
    {
        List<WindowItem> list = new List<WindowItem>();
        IntPtr shell = Native.GetShellWindow();
        uint ownPid = (uint)Process.GetCurrentProcess().Id;
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
            uint pid;
            Native.GetWindowThreadProcessId(hwnd, out pid);
            if (pid == ownPid) return true;
            if (windows.IsIgnored(hwnd)) return true;
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
            List<IntPtr> live = new List<IntPtr>();
            foreach (WindowItem w in items) live.Add(w.Handle);
            windows.RetainAliases(live);
            StringBuilder sb = new StringBuilder();
            foreach (WindowItem w in items)
                sb.Append(w.Handle.ToInt64()).Append('|').Append(windows.DisplayTitle(w.Handle, w.Title)).Append('|')
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
                    TreeNode root = new TreeNode(Localization.Text("sidebar.monitor_prefix") + (i + 1) +
                        (screen.Primary ? Localization.Text("sidebar.primary_suffix") : Localization.Text("sidebar.secondary_suffix")));
                    root.NodeFont = bold;
                    foreach (WindowItem w in items)
                    {
                        if (w.Monitor != screen.DeviceName) continue;
                        string displayTitle = windows.DisplayTitle(w.Handle, w.Title);
                        TreeNode node = new TreeNode((w.Minimized ? "_ " : "") + displayTitle);
                        node.Tag = w.Handle;
                        node.ToolTipText = displayTitle == w.Title ? w.Title : displayTitle + " — " + w.Title;
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
                if (hotkeyErrors.Length == 0) status.Text = items.Count + Localization.Text("sidebar.window_count_suffix");
            }
            finally { internalSelection = false; tree.EndUpdate(); }
        }
        catch (Exception ex) { status.Text = Localization.Text("sidebar.error_prefix") + ex.GetType().Name; }
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
        string settingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinSidebar", "settings.ini");
        bool firstUse = !File.Exists(settingsFile);
        Localization.Initialize(settingsFile);

        bool first;
        using (System.Threading.Mutex mutex = new System.Threading.Mutex(true,
            @"Local\WinSidebarPublic", out first))
        {
            if (!first) { MessageBox.Show(Localization.Text("startup.already_running"), "WinSidebar"); return; }
            try
            {
                Application.SetCompatibleTextRenderingDefault(false);
                if (firstUse)
                {
                    using (FirstRunLanguageDialog dialog = new FirstRunLanguageDialog(Localization.Current))
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            try { Localization.SaveInitialSelection(settingsFile, dialog.SelectedCode); }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Localization.Text("sidebar.language_save_failed") + ex.Message,
                                    "WinSidebar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                Application.Run(new SidebarWindow());
            }
            catch (Exception ex)
            {
                MessageBox.Show(Localization.Text("startup.failed") + ex.Message, "WinSidebar",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { mutex.ReleaseMutex(); }
        }
    }
}
