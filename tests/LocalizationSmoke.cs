using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

internal static class LocalizationSmoke
{
    private static int checks;
    private static void Check(bool condition, string scenario)
    {
        checks++;
        if (!condition) throw new Exception("FAILED: " + scenario);
    }

    private static void Main()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Dictionary<string, Dictionary<string, string>> catalog;
        Dictionary<string, string> spanish;
        using (Stream resource = assembly.GetManifestResourceStream("WinSidebar.i18n.catalog.json"))
        {
            Check(resource != null, "English/Portuguese catalog embedded in executable");
            catalog = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(resource);
        }
        using (Stream resource = assembly.GetManifestResourceStream("WinSidebar.i18n.es-ES.json"))
        {
            Check(resource != null, "Spanish catalog embedded in executable");
            spanish = JsonSerializer.Deserialize<Dictionary<string, string>>(resource);
        }
        Check(catalog != null && catalog.Count >= 100, "base catalog covers the application");
        Check(spanish != null && spanish.Count == catalog.Count, "Spanish covers exactly the same keys");
        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            Check(item.Value.Count == 2 && item.Value.ContainsKey("en-US") && item.Value.ContainsKey("pt-BR"),
                "base translations remain complete: " + item.Key);
            string spanishText;
            Check(spanish.TryGetValue(item.Key, out spanishText) && !string.IsNullOrWhiteSpace(spanishText),
                "Spanish translation exists: " + item.Key);
            foreach (string code in new[] { "en-US", "pt-BR", "es-ES" })
            {
                Localization.Select(code);
                string expected = code == "es-ES" ? spanishText : item.Value[code];
                Check(Localization.Text(item.Key) == expected, "embedded lookup matches catalog: " + item.Key + "/" + code);
            }
        }

        string directory = Path.Combine(Path.GetTempPath(), "WinSidebarLocaleSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.ini");
        CultureInfo originalCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-ES");
            Localization.Initialize(path);
            Check(Localization.Current == "es-ES" && Localization.Text("sidebar.language") == "Idioma",
                "Spanish Windows UI language selects Spanish on first launch");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-MX");
            Localization.Initialize(path);
            Check(Localization.Current == "es-ES", "other Spanish Windows locales select available Spanish catalog");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            Localization.Initialize(path);
            Check(Localization.Current == "en-US", "unsupported Windows UI locale defaults to English on first launch");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR", "Portuguese Windows selects Portuguese on first launch");
            File.WriteAllLines(path, new[] { "width=1", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR", "existing settings without language retain Portuguese");
            File.WriteAllLines(path, new[] { "width=1", "language=en-US", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "en-US" && Localization.Text("sidebar.shortcuts") == " SHORTCUTS",
                "saved English selection restores English UI");
            File.WriteAllLines(path, new[] { "width=1", "language=pt-BR", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR" && Localization.Text("sidebar.shortcuts") == " ATALHOS",
                "saved Portuguese selection restores Portuguese UI");
            File.WriteAllLines(path, new[] { "width=1", "language=es-ES", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "es-ES" && Localization.Text("windows.rename") == "Cambiar nombre de ventana...",
                "saved Spanish selection restores translated window controls");
            bool unsupportedRejected = false;
            try { Localization.Select("ru-RU"); }
            catch (ArgumentOutOfRangeException) { unsupportedRejected = true; }
            Check(unsupportedRejected, "unimplemented languages are not selectable");
            bool missingRejected = false;
            try { Localization.Text("missing.test.key"); }
            catch (InvalidDataException) { missingRejected = true; }
            Check(missingRejected, "missing localization keys fail closed");
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalCulture;
            Directory.Delete(directory, true);
        }
        Console.WriteLine("PASS: " + checks + " runtime localization smoke checks.");
    }
}
