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

    private static Dictionary<string, string> ReadLocale(Assembly assembly, string code)
    {
        using (Stream resource = assembly.GetManifestResourceStream("WinSidebar.i18n." + code + ".json"))
        {
            Check(resource != null, code + " catalog embedded in executable");
            return JsonSerializer.Deserialize<Dictionary<string, string>>(resource);
        }
    }

    private static void Main()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Dictionary<string, Dictionary<string, string>> catalog;
        using (Stream resource = assembly.GetManifestResourceStream("WinSidebar.i18n.catalog.json"))
        {
            Check(resource != null, "English/Portuguese catalog embedded in executable");
            catalog = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(resource);
        }
        Dictionary<string, string> spanish = ReadLocale(assembly, "es-ES");
        Dictionary<string, string> russian = ReadLocale(assembly, "ru-RU");
        Check(catalog != null && catalog.Count >= 100, "base catalog covers application-owned UI");
        Check(spanish != null && spanish.Count == catalog.Count, "Spanish key parity");
        Check(russian != null && russian.Count == catalog.Count, "Russian key parity");
        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            Check(item.Value.Count == 2 && item.Value.ContainsKey("en-US") && item.Value.ContainsKey("pt-BR"),
                "base translation parity: " + item.Key);
            string spanishText;
            string russianText;
            Check(spanish.TryGetValue(item.Key, out spanishText) && !string.IsNullOrWhiteSpace(spanishText),
                "Spanish entry: " + item.Key);
            Check(russian.TryGetValue(item.Key, out russianText) && !string.IsNullOrWhiteSpace(russianText),
                "Russian entry: " + item.Key);
            foreach (string code in new[] { "en-US", "pt-BR", "es-ES", "ru-RU" })
            {
                Localization.Select(code);
                string expected = code == "es-ES" ? spanishText : code == "ru-RU" ? russianText : item.Value[code];
                Check(Localization.Text(item.Key) == expected, "embedded translation: " + item.Key + "/" + code);
            }
        }

        string directory = Path.Combine(Path.GetTempPath(), "WinSidebarLocaleSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.ini");
        CultureInfo originalCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("es-MX");
            Localization.Initialize(path);
            Check(Localization.Current == "es-ES", "Spanish Windows variants select Spanish");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-RU");
            Localization.Initialize(path);
            Check(Localization.Current == "ru-RU" && Localization.Text("sidebar.language") == "Язык",
                "Russian Windows selects Russian on first launch");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ru-KZ");
            Localization.Initialize(path);
            Check(Localization.Current == "ru-RU", "other Russian Windows locales select Russian");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("zh-CN");
            Localization.Initialize(path);
            Check(Localization.Current == "en-US", "unimplemented Chinese defaults to English");
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR", "Portuguese Windows selects Portuguese");
            File.WriteAllLines(path, new[] { "width=1", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR", "existing profile without language retains Portuguese");
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
                "saved Spanish selection restores window commands");
            File.WriteAllLines(path, new[] { "width=1", "language=ru-RU", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "ru-RU" && Localization.Text("windows.rename") == "Переименовать окно..." &&
                Localization.Text("windows.ignore") == "Игнорировать это приложение",
                "saved Russian selection restores localized window commands");
            bool unsupportedRejected = false;
            try { Localization.Select("zh-CN"); }
            catch (ArgumentOutOfRangeException) { unsupportedRejected = true; }
            Check(unsupportedRejected, "unimplemented Chinese cannot be selected");
            bool missingRejected = false;
            try { Localization.Text("missing.test.key"); }
            catch (InvalidDataException) { missingRejected = true; }
            Check(missingRejected, "missing translation keys fail closed");
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalCulture;
            Directory.Delete(directory, true);
        }
        Console.WriteLine("PASS: " + checks + " runtime localization smoke checks.");
    }
}
