using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

internal sealed class SnippetEditor : Form
{
    private readonly TextBox name = new TextBox();
    private readonly ComboBox hotkey = new ComboBox();
    private readonly TextBox content = new TextBox();
    internal SnippetEntry Result;

    internal SnippetEditor(SnippetEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        Text = Localization.Text("snippets.editor_title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 394);
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = Color.FromArgb(212, 208, 200);

        AddLabel(Localization.Text("snippets.name"), 12, 14, 180);
        name.SetBounds(12, 33, 450, 22);
        name.MaxLength = SnippetStore.MaxNameLength;
        name.Text = entry.Name;
        Controls.Add(name);

        AddLabel(Localization.Text("snippets.hotkey"), 12, 66, 220);
        hotkey.DropDownStyle = ComboBoxStyle.DropDownList;
        hotkey.SetBounds(12, 84, 220, 24);
        hotkey.Items.Add(Localization.Text("snippets.hotkey_none"));
        for (int i = 1; i <= SnippetStore.MaxHotkeyFunction; i++)
            hotkey.Items.Add("Shift+F" + i);
        int selected = 0;
        for (int i = 1; i < hotkey.Items.Count; i++)
            if (string.Equals(hotkey.Items[i].ToString(), entry.Hotkey, StringComparison.Ordinal))
                selected = i;
        hotkey.SelectedIndex = selected;
        Controls.Add(hotkey);

        AddLabel(Localization.Text("snippets.content"), 12, 120, 180);
        content.SetBounds(12, 140, 450, 202);
        content.Multiline = true;
        content.AcceptsReturn = true;
        content.AcceptsTab = false;
        content.ScrollBars = ScrollBars.Vertical;
        content.MaxLength = SnippetStore.MaxContentLength;
        content.Text = entry.Content;
        Controls.Add(content);

        Button cancel = AddButton(Localization.Text("editor.cancel"), 270, 354, 90, 27);
        cancel.DialogResult = DialogResult.Cancel;

        Button save = AddButton(Localization.Text("editor.save"), 372, 354, 90, 27);
        save.Click += delegate {
            try
            {
                Result = SnippetStore.NormalizeAndValidate(new SnippetEntry {
                    Name = name.Text,
                    Content = content.Text,
                    Hotkey = hotkey.SelectedIndex <= 0 ? "" : hotkey.SelectedItem.ToString()
                });
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (InvalidDataException)
            {
                MessageBox.Show(this, Localization.Text("snippets.invalid_name"), "WinSidebar",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                name.Focus();
                name.SelectAll();
            }
        };

        AcceptButton = save;
        CancelButton = cancel;
    }

    private void AddLabel(string text, int x, int y, int width)
    {
        Label label = new Label();
        label.Text = text;
        label.SetBounds(x, y, width, 17);
        Controls.Add(label);
    }

    private Button AddButton(string text, int x, int y, int width, int height)
    {
        Button button = new Button();
        button.Text = text;
        button.SetBounds(x, y, width, height);
        Controls.Add(button);
        return button;
    }
}
