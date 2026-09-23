using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

// V8: generic shortcuts, independent of usernames and organization URLs.
internal sealed class ShortcutEntry
{
    internal string Name;
    internal string Type; // folder | website
    internal string Target;
    internal string Icon; // folder | download | archive | globe | star | briefcase | custom
    internal string IconFile;

    internal ShortcutEntry Copy()
    {
        return new ShortcutEntry { Name = Name, Type = Type, Target = Target,
            Icon = Icon, IconFile = IconFile };
    }
}

internal static class ShortcutStore
{
    internal const int MinimumEntries = 4;
    internal const int EntriesPerRow = 4;
    internal const int MaxEntries = 40;
    internal const long MaxFileBytes = 256L * 1024L;

    internal static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinSidebar");
    internal static readonly string FilePath = Path.Combine(Root, "shortcuts.xml");

    internal static ShortcutEntry[] Defaults()
    {
        return new ShortcutEntry[] {
            new ShortcutEntry { Name = Localization.Text("defaults.documents"), Type = "folder",
                Target = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Icon = "folder", IconFile = "" },
            new ShortcutEntry { Name = Localization.Text("defaults.downloads"), Type = "folder",
                Target = Native.DownloadsPath(), Icon = "download", IconFile = "" },
            new ShortcutEntry { Name = Localization.Text("defaults.archive"), Type = "folder",
                Target = "", Icon = "archive", IconFile = "" },
            new ShortcutEntry { Name = Localization.Text("defaults.website"), Type = "website",
                Target = "https://www.google.com/", Icon = "globe", IconFile = "" }
        };
    }

    internal static ShortcutEntry CreateEmpty(int index)
    {
        return new ShortcutEntry {
            Name = Localization.Text("defaults.shortcut_prefix") + " " + (index + 1),
            Type = "folder", Target = "", Icon = "folder", IconFile = ""
        };
    }

    private static void ValidateCount(int count)
    {
        if (count < MinimumEntries || count > MaxEntries || count % EntriesPerRow != 0)
            throw new InvalidDataException(Localization.Text("store.invalid_entry"));
    }

    internal static ShortcutEntry[] Read()
    {
        if (!File.Exists(FilePath)) return Defaults();
        if (new FileInfo(FilePath).Length > MaxFileBytes)
            throw new InvalidDataException(Localization.Text("store.too_large"));
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.DtdProcessing = DtdProcessing.Prohibit;
        settings.XmlResolver = null;
        XmlDocument xml = new XmlDocument();
        xml.XmlResolver = null;
        using (XmlReader reader = XmlReader.Create(FilePath, settings)) xml.Load(reader);
        if (xml.DocumentElement == null || xml.DocumentElement.Name != "shortcuts")
            throw new InvalidDataException(Localization.Text("store.unknown_version"));
        string version = xml.DocumentElement.GetAttribute("version");
        XmlNodeList elements = xml.DocumentElement.SelectNodes("item");
        if (version == "1")
        {
            if (elements.Count != MinimumEntries)
                throw new InvalidDataException(Localization.Text("store.four_entries"));
        }
        else if (version == "2") ValidateCount(elements.Count);
        else throw new InvalidDataException(Localization.Text("store.unknown_version"));
        ShortcutEntry[] result = new ShortcutEntry[elements.Count];
        bool[] seen = new bool[elements.Count];
        foreach (XmlNode node in elements)
        {
            XmlElement item = node as XmlElement;
            if (item == null) throw new InvalidDataException(Localization.Text("store.invalid_entry"));
            int id;
            if (!int.TryParse(item.GetAttribute("id"), out id) ||
                id < 0 || id >= result.Length || seen[id])
                throw new InvalidDataException(Localization.Text("store.invalid_id"));
            seen[id] = true;
            ShortcutEntry entry = new ShortcutEntry {
                Name = item.GetAttribute("name"),
                Type = item.GetAttribute("type"),
                Target = item.GetAttribute("target"),
                Icon = item.GetAttribute("icon"),
                IconFile = item.GetAttribute("iconFile")
            };
            Validate(entry);
            result[id] = entry;
        }
        return result;
    }

    internal static void Validate(ShortcutEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.Name) ||
            entry.Name.Length > 48 || entry.Target == null || entry.Target.Length > 2048 ||
            (entry.Type != "folder" && entry.Type != "website"))
            throw new InvalidDataException(Localization.Text("store.invalid_fields"));
        if (entry.Type == "folder")
        {
            if (entry.Target.Length > 0 && !Path.IsPathRooted(entry.Target))
                throw new InvalidDataException(Localization.Text("store.absolute_folder"));
        }
        else
        {
            Uri uri;
            if (!Uri.TryCreate(entry.Target, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidDataException(Localization.Text("store.valid_url"));
        }
        string[] allowed = { "folder", "download", "archive", "globe", "star", "briefcase", "custom" };
        if (Array.IndexOf(allowed, entry.Icon) < 0 || entry.IconFile == null || entry.IconFile.Length > 1024)
            throw new InvalidDataException(Localization.Text("store.invalid_icon"));
        if (entry.Icon == "custom" && (!Path.IsPathRooted(entry.IconFile) ||
            !File.Exists(entry.IconFile)))
            throw new InvalidDataException(Localization.Text("store.custom_icon_missing"));
    }

    internal static void Write(ShortcutEntry[] entries)
    {
        if (entries == null) throw new InvalidDataException(Localization.Text("store.invalid_entry"));
        ValidateCount(entries.Length);
        foreach (ShortcutEntry entry in entries) Validate(entry);
        Directory.CreateDirectory(Root);
        XmlDocument document = new XmlDocument();
        XmlElement root = document.CreateElement("shortcuts");
        // Keep the legacy format while exactly four entries exist; older builds can still read it.
        root.SetAttribute("version", entries.Length == MinimumEntries ? "1" : "2");
        document.AppendChild(root);
        for (int i = 0; i < entries.Length; i++)
        {
            XmlElement item = document.CreateElement("item");
            item.SetAttribute("id", i.ToString());
            item.SetAttribute("name", entries[i].Name);
            item.SetAttribute("type", entries[i].Type);
            item.SetAttribute("target", entries[i].Target);
            item.SetAttribute("icon", entries[i].Icon);
            item.SetAttribute("iconFile", entries[i].IconFile);
            root.AppendChild(item);
        }
        string temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.Indent = true;
            writerSettings.Encoding = new System.Text.UTF8Encoding(false);
            using (XmlWriter writer = XmlWriter.Create(temporary, writerSettings)) document.Save(writer);
            if (File.Exists(FilePath))
                File.Replace(temporary, FilePath, FilePath + ".bak", true);
            else
                File.Move(temporary, FilePath);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    internal static string CopyUserIcon(string source)
    {
        FileInfo file = new FileInfo(source);
        if (!file.Exists || file.Length > 1024 * 1024 || file.Length == 0)
            throw new InvalidDataException(Localization.Text("store.icon_size"));
        string extension = Path.GetExtension(source).ToLowerInvariant();
        if (extension != ".png" && extension != ".ico")
            throw new InvalidDataException(Localization.Text("store.icon_type"));
        string directory = Path.Combine(Root, "icons");
        Directory.CreateDirectory(directory);
        string destination = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".png");
        if (extension == ".ico")
        {
            using (Icon icon = new Icon(source))
            using (Bitmap image = icon.ToBitmap())
            using (Bitmap converted = new Bitmap(image, new Size(32, 32)))
                converted.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
        }
        else
        {
            using (Image image = Image.FromFile(source))
            {
                if (image.Width > 4096 || image.Height > 4096)
                    throw new InvalidDataException(Localization.Text("store.icon_dimensions"));
                using (Bitmap converted = new Bitmap(image, new Size(32, 32)))
                    converted.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        return destination;
    }

    internal static Image MakeIcon(ShortcutEntry entry)
    {
        if (entry.Icon == "custom")
        {
            try
            {
                using (Image original = Image.FromFile(entry.IconFile))
                    return new Bitmap(original, new Size(20, 20));
            }
            catch (Exception) { return IconArt.Draw(entry.Type == "website" ? 3 : 0); }
        }
        if (entry.Icon == "globe") return IconArt.Draw(3);
        if (entry.Icon == "download") return IconArt.Draw(1);
        if (entry.Icon == "archive") return IconArt.Draw(2);
        if (entry.Icon == "star") return SystemIcons.Information.ToBitmap();
        if (entry.Icon == "briefcase") return SystemIcons.Application.ToBitmap();
        return IconArt.Draw(0);
    }
}

internal sealed class ShortcutEditor : Form
{
    private static readonly string[] IconCodes = { "folder", "download", "archive", "globe", "star", "briefcase", "custom" };
    private readonly TextBox name = new TextBox();
    private readonly TextBox target = new TextBox();
    private readonly ComboBox type = new ComboBox();
    private readonly ComboBox icon = new ComboBox();
    private readonly TextBox iconFile = new TextBox();
    private readonly TextBox browser = new TextBox();
    private readonly CheckBox systemBrowser = new CheckBox();
    internal ShortcutEntry Result;
    internal string BrowserExecutable;
    internal bool BrowserUseSystem;
    private readonly ShortcutEntry initial;

    internal ShortcutEditor(ShortcutEntry entry, string browserExecutable, bool browserUseSystem)
    {
        initial = entry.Copy();
        BrowserExecutable = browserExecutable;
        BrowserUseSystem = browserUseSystem;
        Text = Localization.Text("editor.title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 347);
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = Color.FromArgb(212, 208, 200);
        Label intro = AddLabel(Localization.Text("editor.name"), 12, 14, 120);
        name.SetBounds(12, 33, 430, 22); name.MaxLength = 48;
        name.Text = entry.Name; Controls.Add(name);
        AddLabel(Localization.Text("editor.type"), 12, 62, 180);
        type.SetBounds(12, 81, 140, 23);
        type.DropDownStyle = ComboBoxStyle.DropDownList;
        type.Items.AddRange(new object[] { Localization.Text("editor.folder"), Localization.Text("defaults.website") });
        type.SelectedIndex = entry.Type == "website" ? 1 : 0;
        Controls.Add(type);
        AddLabel(Localization.Text("editor.target"), 12, 108, 180);
        target.SetBounds(12, 128, 332, 23); target.Text = entry.Target;
        target.MaxLength = 2048; Controls.Add(target);
        Button browse = AddButton(Localization.Text("editor.browse"), 350, 127, 92, 24);
        browse.Click += delegate {
            if (type.SelectedIndex == 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = Localization.Text("editor.folder_picker");
                    if (Directory.Exists(target.Text)) dialog.SelectedPath = target.Text;
                    if (dialog.ShowDialog(this) == DialogResult.OK) target.Text = dialog.SelectedPath;
                }
            }
            else MessageBox.Show(this, Localization.Text("editor.enter_url"), "WinSidebar");
        };
        AddLabel(Localization.Text("editor.icon"), 12, 158, 120);
        icon.SetBounds(12, 178, 175, 23);
        icon.DropDownStyle = ComboBoxStyle.DropDownList;
        icon.Items.AddRange(new object[] { Localization.Text("editor.icon_folder"), Localization.Text("editor.icon_download"), Localization.Text("editor.icon_archive"), Localization.Text("editor.icon_globe"), Localization.Text("editor.icon_star"), Localization.Text("editor.icon_briefcase"), Localization.Text("editor.icon_custom") });
        icon.SelectedIndex = Math.Max(0, Array.IndexOf(IconCodes, entry.Icon)); Controls.Add(icon);
        iconFile.SetBounds(193, 178, 151, 23); iconFile.ReadOnly = true;
        iconFile.Text = entry.IconFile; Controls.Add(iconFile);
        Button iconBrowse = AddButton("PNG/ICO...", 350, 177, 92, 24);
        iconBrowse.Click += delegate {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = Localization.Text("editor.images_filter");
                if (dialog.ShowDialog(this) == DialogResult.OK)
                { iconFile.Text = dialog.FileName; icon.SelectedIndex = Array.IndexOf(IconCodes, "custom"); }
            }
        };
        AddLabel(Localization.Text("editor.browser_only_sites"), 12, 208, 230);
        systemBrowser.SetBounds(12, 228, 240, 21);
        systemBrowser.Text = Localization.Text("editor.system_browser");
        systemBrowser.Checked = browserUseSystem;
        Controls.Add(systemBrowser);
        browser.SetBounds(12, 254, 332, 23);
        browser.Text = browserExecutable; browser.ReadOnly = true;
        Controls.Add(browser);
        Button browserBrowse = AddButton(Localization.Text("editor.choose_exe"), 350, 253, 92, 24);
        browserBrowse.Click += delegate {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = Localization.Text("editor.executables_filter");
                dialog.CheckFileExists = true;
                if (File.Exists(browser.Text)) dialog.FileName = browser.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                { browser.Text = dialog.FileName; systemBrowser.Checked = false; }
            }
        };
        type.SelectedIndexChanged += delegate { UpdateBrowserControls(browserBrowse); };
        systemBrowser.CheckedChanged += delegate { UpdateBrowserControls(browserBrowse); };
        UpdateBrowserControls(browserBrowse);
        Button cancel = AddButton(Localization.Text("editor.cancel"), 252, 304, 90, 27);
        cancel.DialogResult = DialogResult.Cancel;
        Button save = AddButton(Localization.Text("editor.save"), 350, 304, 92, 27);
        save.Click += delegate {
            try
            {
                ShortcutEntry changed = new ShortcutEntry {
                    Name = name.Text.Trim(), Type = type.SelectedIndex == 0 ? "folder" : "website",
                    Target = target.Text.Trim(), Icon = IconCodes[icon.SelectedIndex],
                    IconFile = iconFile.Text
                };
                if (changed.Icon == "custom" && changed.IconFile != initial.IconFile)
                    changed.IconFile = ShortcutStore.CopyUserIcon(changed.IconFile);
                ShortcutStore.Validate(changed);
                if (changed.Type == "website" && !systemBrowser.Checked &&
                    (!File.Exists(browser.Text) || !Path.GetExtension(browser.Text).Equals(".exe", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidDataException(Localization.Text("editor.choose_browser_error"));
                Result = changed;
                BrowserExecutable = browser.Text;
                BrowserUseSystem = systemBrowser.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, Localization.Text("editor.invalid_configuration"), MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        AcceptButton = save; CancelButton = cancel;
    }

    private void UpdateBrowserControls(Button browse)
    {
        bool site = type.SelectedIndex == 1;
        systemBrowser.Enabled = site;
        browser.Enabled = site && !systemBrowser.Checked;
        browse.Enabled = site;
    }
    private Label AddLabel(string text, int x, int y, int width)
    {
        Label label = new Label(); label.Text = text;
        label.SetBounds(x, y, width, 17); Controls.Add(label); return label;
    }
    private Button AddButton(string text, int x, int y, int width, int height)
    {
        Button button = new Button(); button.Text = text;
        button.SetBounds(x, y, width, height); Controls.Add(button); return button;
    }
}
