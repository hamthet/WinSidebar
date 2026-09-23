using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

internal sealed class SnippetEditor : Form
{
    private readonly TextBox name = new TextBox();
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
        ClientSize = new Size(480, 348);
        Font = new Font("Microsoft Sans Serif", 8.25f);
        BackColor = Color.FromArgb(212, 208, 200);

        AddLabel(Localization.Text("snippets.name"), 12, 14, 180);
        name.SetBounds(12, 33, 450, 22);
        name.MaxLength = SnippetStore.MaxNameLength;
        name.Text = entry.Name;
        Controls.Add(name);

        AddLabel(Localization.Text("snippets.content"), 12, 66, 180);
        content.SetBounds(12, 86, 450, 208);
        content.Multiline = true;
        content.AcceptsReturn = true;
        content.AcceptsTab = false;
        content.ScrollBars = ScrollBars.Vertical;
        content.MaxLength = SnippetStore.MaxContentLength;
        content.Text = entry.Content;
        Controls.Add(content);

        Button cancel = AddButton(Localization.Text("editor.cancel"), 270, 308, 90, 27);
        cancel.DialogResult = DialogResult.Cancel;

        Button save = AddButton(Localization.Text("editor.save"), 372, 308, 90, 27);
        save.Click += delegate {
            try
            {
                Result = SnippetStore.NormalizeAndValidate(new SnippetEntry {
                    Name = name.Text,
                    Content = content.Text
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
