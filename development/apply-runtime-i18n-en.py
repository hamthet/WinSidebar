#!/usr/bin/env python3
"""Apply the English/PT runtime integration only to the audited source baseline.

Development-only script; do not ship or merge it into final main. It changes
application-owned string literals, never persisted tokens or process identities.
"""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
catalog = json.loads((ROOT / 'i18n/catalog.json').read_text(encoding='utf-8'))
by_pt = {}
for key, translations in catalog.items():
    pt = translations['pt-BR']
    assert translations['en-US'], key
    if pt in by_pt:
        assert catalog[by_pt[pt]]['en-US'] == translations['en-US'], (key, by_pt[pt])
    else:
        by_pt[pt] = key

source = ROOT / 'src/WinSidebar.cs'
editor = ROOT / 'src/ShortcutConfig.cs'
manager = ROOT / 'src/WindowManagement.cs'
s = source.read_text(encoding='utf-8')
e = editor.read_text(encoding='utf-8')
w = manager.read_text(encoding='utf-8')

def once(text, before, after, label):
    count = text.count(before)
    if count != 1:
        raise SystemExit(f'{label}: expected exactly one source anchor, found {count}: {before[:96]!r}')
    return text.replace(before, after, 1)

# Preserve original serialized icon codes and Windows Downloads folder paths.
e = once(e, 'Name = "Downloads", Type = "folder",',
         'Name = Localization.Text("defaults.downloads"), Type = "folder",', 'Downloads default')
e = once(e, '    private readonly TextBox name = new TextBox();',
         '    private static readonly string[] IconCodes = { "folder", "download", "archive", "globe", "star", "briefcase", "custom" };\n    private readonly TextBox name = new TextBox();', 'icon codes')
e = once(e, 'icon.Items.AddRange(new object[] { "folder", "download", "archive", "globe", "star", "briefcase", "custom" });',
         'icon.Items.AddRange(new object[] { Localization.Text("editor.icon_folder"), Localization.Text("editor.icon_download"), Localization.Text("editor.icon_archive"), Localization.Text("editor.icon_globe"), Localization.Text("editor.icon_star"), Localization.Text("editor.icon_briefcase"), Localization.Text("editor.icon_custom") });', 'localized icon labels')
e = once(e, 'icon.SelectedItem = entry.Icon;',
         'icon.SelectedIndex = Math.Max(0, Array.IndexOf(IconCodes, entry.Icon));', 'icon selection')
e = once(e, 'icon.SelectedItem = "custom";',
         'icon.SelectedIndex = Array.IndexOf(IconCodes, "custom");', 'custom icon selection')
e = once(e, 'Icon = (string)icon.SelectedItem,',
         'Icon = IconCodes[icon.SelectedIndex],', 'stable icon serialization')

# Localization initializes before any default shortcuts are created; for an old
# settings.ini without language=, the first language remains Brazilian Portuguese.
s = once(s, '        browserUseSystem = !File.Exists(settingsPath); // primeira instalação: navegador padrão\n        LoadSettings();',
         '        Localization.Initialize(settingsPath);\n        browserUseSystem = !File.Exists(settingsPath); // primeira instalação: navegador padrão\n        LoadSettings();', 'startup initialization')
s = once(s, '    private int widthIndex;', '''    private ToolStripMenuItem monitorPrimaryItem;
    private ToolStripMenuItem monitorSecondaryItem;
    private ToolStripMenuItem ignoredMenuItem;
    private ToolStripMenuItem exitMenuItem;
    private ToolStripMenuItem languageMenu;
    private ToolStripMenuItem englishItem;
    private ToolStripMenuItem portugueseItem;
    private int widthIndex;''', 'menu item fields')
s = once(s, '        exit.Click += delegate { RequestExit(); };', '''        exit.Click += delegate { RequestExit(); };
        monitorPrimaryItem = main;
        monitorSecondaryItem = other;
        ignoredMenuItem = manageIgnored;
        exitMenuItem = exit;''', 'menu references') if False else s
# The manageIgnored local is declared after menu.Items.Add(other); bind it there.
s = once(s, '        manageIgnored.Click += delegate { windows.ManageIgnored(this, delegate { RefreshWindows(true); }); };',
         '''        manageIgnored.Click += delegate { windows.ManageIgnored(this, delegate { RefreshWindows(true); }); };
        monitorPrimaryItem = main;
        monitorSecondaryItem = other;
        ignoredMenuItem = manageIgnored;
        exitMenuItem = exit;
        languageMenu = new ToolStripMenuItem(Localization.Text("sidebar.language"));
        englishItem = new ToolStripMenuItem("English");
        portugueseItem = new ToolStripMenuItem("Português (Brasil)");
        englishItem.Click += delegate { ChangeLanguage("en-US"); };
        portugueseItem.Click += delegate { ChangeLanguage("pt-BR"); };
        languageMenu.DropDownItems.Add(englishItem);
        languageMenu.DropDownItems.Add(portugueseItem);''', 'language menu')
s = once(s, '        menu.Items.Add(manageIgnored);\n        menu.Items.Add(exit);',
         '        menu.Items.Add(manageIgnored);\n        menu.Items.Add(languageMenu);\n        menu.Items.Add(exit);', 'language menu placement')
s = once(s, '        tray.DoubleClick += delegate { Expand(!expanded); };',
         '        tray.DoubleClick += delegate { Expand(!expanded); };\n        ApplyLanguage();', 'initial full UI refresh')
s = once(s, '    private void LoadSettings()\n    {', '''    private void ChangeLanguage(string code)
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
        tips.SetToolTip(smallerButton, Localization.Text("sidebar.shrink"));
        smallerButton.AccessibleName = Localization.Text("sidebar.shrink");
        tips.SetToolTip(largerButton, Localization.Text("sidebar.expand"));
        largerButton.AccessibleName = Localization.Text("sidebar.expand");
        tips.SetToolTip(sideButton, Localization.Text("sidebar.other_edge"));
        sideButton.AccessibleName = Localization.Text("sidebar.move_accessible");
        folderTitle.Text = Localization.Text(configureMode ? "sidebar.configuring" : "sidebar.shortcuts");
        configureButton.AccessibleName = Localization.Text("sidebar.configure");
        tips.SetToolTip(configureButton, Localization.Text(configureMode ? "sidebar.finish_editing" : "sidebar.configure_mode"));
        savePreferencesButton.AccessibleName = Localization.Text("sidebar.save_preferences");
        tips.SetToolTip(savePreferencesButton, Localization.Text("sidebar.save_preferences"));
        restoreDefaultsButton.AccessibleName = Localization.Text("sidebar.restore_defaults");
        tips.SetToolTip(restoreDefaultsButton, Localization.Text("sidebar.restore_defaults"));
        monitorPrimaryItem.Text = Localization.Text("sidebar.primary_monitor");
        monitorSecondaryItem.Text = Localization.Text("sidebar.secondary_monitor");
        ignoredMenuItem.Text = Localization.Text("sidebar.manage_ignored");
        exitMenuItem.Text = Localization.Text("sidebar.exit_menu");
        languageMenu.Text = Localization.Text("sidebar.language");
        englishItem.Checked = Localization.Current == "en-US";
        portugueseItem.Checked = Localization.Current == "pt-BR";
        if (hotkeyErrors.Length > 0)
        {
            status.Text = Localization.Text("sidebar.hotkeys_unavailable") + hotkeyErrors;
            tray.BalloonTipText = Localization.Text("sidebar.hotkeys_in_use") + hotkeyErrors;
        }
        RefreshShortcutVisuals();
        Reposition();
    }

    private void LoadSettings()
    {''', 'language change and control refresh')
# Update both existing settings-save paths; do not modify serialized shortcut data.
save = '"browserSystem=" + (browserUseSystem ? "1" : "0")'
assert s.count(save) == 2, ('settings writers', s.count(save))
s = s.replace(save, save + ',\n                "language=" + Localization.Current')
# The singleton warning is displayed before SidebarWindow is constructed.
s = once(s, '    private static void Main()\n    {\n        bool first;',
         '''    private static void Main()
    {
        Localization.Initialize(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WinSidebar", "settings.ini"));
        bool first;''', 'singleton locale setup')

# Each translated literal in existing UI is replaced with a stable, unlocalized
# key. Unrecognized literals remain untouched and are exposed by the coverage
# report, rather than being guessed or translated as process IDs / URLs.
pattern = re.compile(r'"(?:\\.|[^"\\])*"')
used = set()
def localize(text):
    def swap(match):
        raw = match.group(0)
        try:
            value = json.loads(raw)
        except ValueError:
            return raw
        key = by_pt.get(value)
        if not key:
            return raw
        used.add(key)
        return 'Localization.Text(' + json.dumps(key) + ')'
    return pattern.sub(swap, text)

s = localize(s)
e = localize(e)
w = localize(w)
# Generic translation of the current default icon names is separate from their
# persistent `folder` / `download` / etc. IDs above.

# Changes to window alias or ignore behavior are explicitly OUTSIDE this patch.
# These files are committed only if every anchored change succeeded.
source.write_text(s, encoding='utf-8')
editor.write_text(e, encoding='utf-8')
manager.write_text(w, encoding='utf-8')

# Scan remaining C# string literals for untranslated Portuguese. These are
# *potential* omissions requiring review; the baseline may include comments and
# technical names, so do not falsely call this a UI acceptance test.
pt_words = re.compile(r'(?:[ãõçáàâéêíóôú] |[ãõçáàâéêíóôú]|\b(?:atalhos?|configurações?|preferências?|navegador|janela|janelas|aplicativos?|ignorado|ignorar|restaurar|salvar|escolha|pasta|arquivos?|erro|carregada|preservado|recolher|principal|secundário)\b)', re.I)
left = set()
for name, text in [('WinSidebar.cs', s), ('ShortcutConfig.cs', e), ('WindowManagement.cs', w)]:
    for match in pattern.finditer(text):
        try:
            value = json.loads(match.group(0))
        except ValueError:
            continue
        if pt_words.search(value):
            left.add((name, value))
report = '\n'.join(f'{name}: {value!r}' for name, value in sorted(left))
(ROOT / 'development/i18n-untranslated-candidates.txt').write_text(
    '# Candidate untranslated literals; review manually. Empty means none found by this heuristic.\n' + report + '\n', encoding='utf-8')
print(f'Localized {len(used)} catalog keys; {len(left)} untranslated candidates:')
print(report or '(none)')
