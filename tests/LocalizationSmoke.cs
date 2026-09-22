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
        using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinSidebar.i18n.catalog.json"))
        {
            Check(resource != null, "catalog is embedded in executable");
            Dictionary<string, Dictionary<string, string>> catalog =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(resource);
            Check(catalog.Count >= 100, "both language catalogs cover the application");
            foreach (KeyValuePair<string, Dictionary<string, string>> entry in catalog)
            {
                Check(entry.Value.Count == 2 && entry.Value.ContainsKey("en-US") && entry.Value.ContainsKey("pt-BR"),
                    "each entry has exactly English and Portuguese: " + entry.Key);
                foreach (string code in new[] { "en-US", "pt-BR" })
                {
                    Localization.Select(code);
                    Check(Localization.Text(entry.Key) == entry.Value[code], "embedded lookup matches catalog: " + entry.Key + "/" + code);
                }
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
