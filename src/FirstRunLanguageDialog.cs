using System;
using System.Drawing;
using System.Windows.Forms;

internal sealed class FirstRunLanguageDialog : Form
{
    private readonly ComboBox language = new ComboBox();
    internal string SelectedCode { get; private set; }

    internal FirstRunLanguageDialog(string suggestedCode)
    {
        Text = "WinSidebar — Language / Idioma";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ControlBox = false;
        ShowInTaskbar = true;
        TopMost = true;
        ClientSize = new Size(360, 142);
        Font = new Font("Microsoft Sans Serif", 9f);

        Label prompt = new Label();
        prompt.Text = "Choose your language / Escolha seu idioma";
        prompt.SetBounds(18, 18, 320, 22);
        Controls.Add(prompt);

        language.DropDownStyle = ComboBoxStyle.DropDownList;
        language.SetBounds(18, 48, 324, 26);
        language.Items.AddRange(new object[] {
            "English",
            "Português (Brasil)",
            "Español",
            "Русский",
            "简体中文"
        });
        language.SelectedIndex = CodeToIndex(suggestedCode);
        Controls.Add(language);

        Button ok = new Button();
        ok.Text = "OK";
        ok.SetBounds(252, 94, 90, 28);
        ok.Click += delegate {
            SelectedCode = IndexToCode(language.SelectedIndex);
            DialogResult = DialogResult.OK;
            Close();
        };
        Controls.Add(ok);
        AcceptButton = ok;
    }

    private static int CodeToIndex(string code)
    {
        if (code == "pt-BR") return 1;
        if (code == "es-ES") return 2;
        if (code == "ru-RU") return 3;
        if (code == "zh-CN") return 4;
        return 0;
    }

    private static string IndexToCode(int index)
    {
        if (index == 1) return "pt-BR";
        if (index == 2) return "es-ES";
        if (index == 3) return "ru-RU";
        if (index == 4) return "zh-CN";
        return "en-US";
    }
}
