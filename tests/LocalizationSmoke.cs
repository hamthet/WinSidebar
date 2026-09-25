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
        Dictionary<string, string> chinese = ReadLocale(assembly, "zh-CN");
        Check(catalog != null && catalog.Count >= 100, "base catalog covers application-owned UI");
        Check(Localization.Current == "en-US", "English is the application default before profile initialization");
        Check(spanish != null && spanish.Count == catalog.Count, "Spanish key parity");
        Check(russian != null && russian.Count == catalog.Count, "Russian key parity");
        Check(chinese != null && chinese.Count == catalog.Count, "Simplified Chinese key parity");
        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            Check(item.Value.Count == 2 && item.Value.ContainsKey("en-US") && item.Value.ContainsKey("pt-BR"),
                "base translation parity: " + item.Key);
            string spanishText;
            string russianText;
            string chineseText;
            Check(spanish.TryGetValue(item.Key, out spanishText) && !string.IsNullOrWhiteSpace(spanishText),
                "Spanish entry: " + item.Key);
            Check(russian.TryGetValue(item.Key, out russianText) && !string.IsNullOrWhiteSpace(russianText),
                "Russian entry: " + item.Key);
            Check(chinese.TryGetValue(item.Key, out chineseText) && !string.IsNullOrWhiteSpace(chineseText),
                "Simplified Chinese entry: " + item.Key);
            foreach (string code in new[] { "en-US", "pt-BR", "es-ES", "ru-RU", "zh-CN" })
            {
                Localization.Select(code);
                string expected = code == "es-ES" ? spanishText : code == "ru-RU" ? russianText :
                    code == "zh-CN" ? chineseText : item.Value[code];
                Check(Localization.Text(item.Key) == expected, "embedded translation: " + item.Key + "/" + code);
            }
        }

        string directory = Path.Combine(Path.GetTempPath(), "WinSidebarLocaleSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "settings.ini");
        CultureInfo originalCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            foreach (string cultureName in new[] { "en-US", "pt-BR", "es-MX", "ru-RU", "zh-CN", "zh-TW" })
            {
                if (File.Exists(path)) File.Delete(path);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                Localization.Initialize(path);
                Check(Localization.Current == "en-US" && !Localization.SettingsFileExists &&
                    !Localization.SettingsReadFailed,
                    "new installation defaults to English regardless of Windows UI culture: " + cultureName);
            }

            File.WriteAllLines(path, new[] { "width=1", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "pt-BR" && Localization.SettingsFileExists &&
                !Localization.SettingsReadFailed,
                "readable legacy existing profile without language retains Portuguese");

            File.WriteAllLines(path, new[] { "width=1", "language=es-ES", "left=1" });
            using (FileStream locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Localization.Initialize(path);
                Check(Localization.SettingsFileExists && Localization.SettingsReadFailed &&
                    Localization.Current == "en-US",
                    "unreadable existing profile fails closed without assuming legacy Portuguese");
            }
            Localization.Initialize(path);
            Check(!Localization.SettingsReadFailed && Localization.Current == "es-ES",
                "settings-read guard clears after a successful restart/read");

            File.Delete(path);
            Localization.SaveInitialSelection(path, "es-ES");
            Localization.Initialize(path);
            Check(Localization.Current == "es-ES" &&
                Array.Exists(File.ReadAllLines(path), line => line == "language=es-ES"),
                "first-run language selection persists atomically");

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
            Check(Localization.Current == "ru-RU" && Localization.Text("windows.rename") == "Переименовать окно...",
                "saved Russian selection restores window commands");
            File.WriteAllLines(path, new[] { "width=1", "language=zh-CN", "left=1" });
            Localization.Initialize(path);
            Check(Localization.Current == "zh-CN" && Localization.Text("windows.rename") == "重命名窗口…" &&
                Localization.Text("windows.ignore") == "忽略此应用",
                "saved Simplified Chinese selection restores localized window commands");
            bool unsupportedRejected = false;
            try { Localization.Select("zh-TW"); }
            catch (ArgumentOutOfRangeException) { unsupportedRejected = true; }
            Check(unsupportedRejected, "unsupported Traditional Chinese cannot be selected");
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
