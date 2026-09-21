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
    internal static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinSidebar");
    internal static readonly string FilePath = Path.Combine(Root, "shortcuts.xml");

    internal static ShortcutEntry[] Defaults()
    {
        return new ShortcutEntry[] {
            new ShortcutEntry { Name = "Pasta local", Type = "folder",
                Target = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Icon = "folder", IconFile = "" },
            new ShortcutEntry { Name = "Downloads", Type = "folder",
                Target = Native.DownloadsPath(), Icon = "download", IconFile = "" },
            new ShortcutEntry { Name = "Acervo", Type = "folder",
                Target = "", Icon = "archive", IconFile = "" },
            new ShortcutEntry { Name = "Site", Type = "website",
                Target = "https://www.google.com/", Icon = "globe", IconFile = "" }
        };
    }

    internal static ShortcutEntry[] Read()
    {
        if (!File.Exists(FilePath)) return Defaults();
        if (new FileInfo(FilePath).Length > 65536)
            throw new InvalidDataException("Configuração de atalhos excessivamente grande.");
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.DtdProcessing = DtdProcessing.Prohibit;
        settings.XmlResolver = null;
        XmlDocument xml = new XmlDocument();
        xml.XmlResolver = null;
        using (XmlReader reader = XmlReader.Create(FilePath, settings)) xml.Load(reader);
        if (xml.DocumentElement == null || xml.DocumentElement.Name != "shortcuts" ||
            xml.DocumentElement.GetAttribute("version") != "1")
            throw new InvalidDataException("Versão de configuração de atalhos desconhecida.");
        XmlNodeList elements = xml.DocumentElement.SelectNodes("item");
        if (elements.Count != 4)
            throw new InvalidDataException("A configuração precisa conter quatro atalhos.");
        ShortcutEntry[] result = new ShortcutEntry[4];
        bool[] seen = new bool[4];
        foreach (XmlNode node in elements)
        {
            XmlElement item = node as XmlElement;
            if (item == null) throw new InvalidDataException("Atalho inválido.");
            int id;
            if (!int.TryParse(item.GetAttribute("id"), out id) ||
                id < 0 || id > 3 || seen[id])
                throw new InvalidDataException("Identificador de atalho inválido.");
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
            throw new InvalidDataException("Nome, tipo ou destino de atalho inválido.");
        if (entry.Type == "folder")
        {
            if (entry.Target.Length > 0 && !Path.IsPathRooted(entry.Target))
                throw new InvalidDataException("A pasta precisa ter um caminho absoluto.");
        }
        else
        {
            Uri uri;
            if (!Uri.TryCreate(entry.Target, UriKind.Absolute, out uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                throw new InvalidDataException("Informe uma URL HTTP ou HTTPS válida.");
        }
        string[] allowed = { "folder", "download", "archive", "globe", "star", "briefcase", "custom" };
        if (Array.IndexOf(allowed, entry.Icon) < 0 || entry.IconFile == null || entry.IconFile.Length > 1024)
            throw new InvalidDataException("Ícone inválido.");
        if (entry.Icon == "custom" && (!Path.IsPathRooted(entry.IconFile) ||
            !File.Exists(entry.IconFile)))
            throw new InvalidDataException("O arquivo do ícone personalizado não foi encontrado.");
    }

    internal static void Write(ShortcutEntry[] entries)
    {
        if (entries == null || entries.Length != 4)
            throw new InvalidDataException("São necessários quatro atalhos.");
        foreach (ShortcutEntry entry in entries) Validate(entry);
        Directory.CreateDirectory(Root);
        XmlDocument document = new XmlDocument();
        XmlElement root = document.CreateElement("shortcuts");
        root.SetAttribute("version", "1");
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
            throw new InvalidDataException("Ícone inexistente ou maior que 1 MB.");
        string extension = Path.GetExtension(source).ToLowerInvariant();
        if (extension != ".png" && extension != ".ico")
            throw new InvalidDataException("Somente PNG e ICO são aceitos.");
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
                    throw new InvalidDataException("Dimensões do ícone muito grandes.");
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
        Text = "WinSidebar — configurar atalho";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false; MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(460, 347);
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = Color.FromArgb(212, 208, 200);
        Label intro = AddLabel("Nome", 12, 14, 120);
        name.SetBounds(12, 33, 430, 22); name.MaxLength = 48;
        name.Text = entry.Name; Controls.Add(name);
        AddLabel("Tipo", 12, 62, 180);
        type.SetBounds(12, 81, 140, 23);
        type.DropDownStyle = ComboBoxStyle.DropDownList;
        type.Items.AddRange(new object[] { "Pasta", "Site" });
        type.SelectedIndex = entry.Type == "website" ? 1 : 0;
        Controls.Add(type);
        AddLabel("Destino / URL", 12, 108, 180);
        target.SetBounds(12, 128, 332, 23); target.Text = entry.Target;
        target.MaxLength = 2048; Controls.Add(target);
        Button browse = AddButton("Procurar...", 350, 127, 92, 24);
        browse.Click += delegate {
            if (type.SelectedIndex == 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Selecione a pasta do atalho";
                    if (Directory.Exists(target.Text)) dialog.SelectedPath = target.Text;
                    if (dialog.ShowDialog(this) == DialogResult.OK) target.Text = dialog.SelectedPath;
                }
            }
            else MessageBox.Show(this, "Digite uma URL HTTP/HTTPS no campo Destino.", "WinSidebar");
        };
        AddLabel("Ícone", 12, 158, 120);
        icon.SetBounds(12, 178, 175, 23);
        icon.DropDownStyle = ComboBoxStyle.DropDownList;
        icon.Items.AddRange(new object[] { "folder", "download", "archive", "globe", "star", "briefcase", "custom" });
        icon.SelectedItem = entry.Icon; Controls.Add(icon);
        iconFile.SetBounds(193, 178, 151, 23); iconFile.ReadOnly = true;
        iconFile.Text = entry.IconFile; Controls.Add(iconFile);
        Button iconBrowse = AddButton("PNG/ICO...", 350, 177, 92, 24);
        iconBrowse.Click += delegate {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Imagens PNG/ICO|*.png;*.ico";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                { iconFile.Text = dialog.FileName; icon.SelectedItem = "custom"; }
            }
        };
        AddLabel("Navegador (somente sites)", 12, 208, 230);
        systemBrowser.SetBounds(12, 228, 240, 21);
        systemBrowser.Text = "Usar navegador padrão do Windows";
        systemBrowser.Checked = browserUseSystem;
        Controls.Add(systemBrowser);
        browser.SetBounds(12, 254, 332, 23);
        browser.Text = browserExecutable; browser.ReadOnly = true;
        Controls.Add(browser);
        Button browserBrowse = AddButton("Selecionar .exe", 350, 253, 92, 24);
        browserBrowse.Click += delegate {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Executáveis|*.exe";
                dialog.CheckFileExists = true;
                if (File.Exists(browser.Text)) dialog.FileName = browser.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                { browser.Text = dialog.FileName; systemBrowser.Checked = false; }
            }
        };
        type.SelectedIndexChanged += delegate { UpdateBrowserControls(browserBrowse); };
        systemBrowser.CheckedChanged += delegate { UpdateBrowserControls(browserBrowse); };
        UpdateBrowserControls(browserBrowse);
        Button cancel = AddButton("Cancelar", 252, 304, 90, 27);
        cancel.DialogResult = DialogResult.Cancel;
        Button save = AddButton("Salvar", 350, 304, 92, 27);
        save.Click += delegate {
            try
            {
                ShortcutEntry changed = new ShortcutEntry {
                    Name = name.Text.Trim(), Type = type.SelectedIndex == 0 ? "folder" : "website",
                    Target = target.Text.Trim(), Icon = (string)icon.SelectedItem,
                    IconFile = iconFile.Text
                };
                if (changed.Icon == "custom" && changed.IconFile != initial.IconFile)
                    changed.IconFile = ShortcutStore.CopyUserIcon(changed.IconFile);
                ShortcutStore.Validate(changed);
                if (changed.Type == "website" && !systemBrowser.Checked &&
                    (!File.Exists(browser.Text) || !Path.GetExtension(browser.Text).Equals(".exe", StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidDataException("Escolha um navegador .exe ou marque 'Usar navegador padrão'.");
                Result = changed;
                BrowserExecutable = browser.Text;
                BrowserUseSystem = systemBrowser.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Configuração inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
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
