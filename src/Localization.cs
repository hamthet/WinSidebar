using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

// Application-owned strings only. Never translate user data, external window titles,
// paths, process identities or persisted icon/shortcut identifiers.
internal static class Localization
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = ReadCatalog();
    internal static string Current { get; private set; } = "en-US";

    private static Dictionary<string, Dictionary<string, string>> ReadCatalog()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Dictionary<string, Dictionary<string, string>> catalog;
        using (Stream stream = assembly.GetManifestResourceStream("WinSidebar.i18n.catalog.json"))
        {
            if (stream == null) throw new InvalidDataException("Missing embedded localization catalog.");
            catalog = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream);
        }
        if (catalog == null || catalog.Count == 0)
            throw new InvalidDataException("Empty localization catalog.");

        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            Dictionary<string, string> entry = item.Value;
            if (string.IsNullOrWhiteSpace(item.Key) || entry == null ||
                entry.Count != 2 || !entry.ContainsKey("pt-BR") || !entry.ContainsKey("en-US") ||
                string.IsNullOrWhiteSpace(entry["pt-BR"]) || string.IsNullOrWhiteSpace(entry["en-US"]))
                throw new InvalidDataException("Incomplete localization entry: " + item.Key);
        }
        MergeLocale(assembly, catalog, "es-ES");
        MergeLocale(assembly, catalog, "ru-RU");
        MergeLocale(assembly, catalog, "zh-CN");
        return catalog;
    }

    private static void MergeLocale(Assembly assembly,
        Dictionary<string, Dictionary<string, string>> catalog, string code)
    {
        Dictionary<string, string> translations;
        using (Stream stream = assembly.GetManifestResourceStream("WinSidebar.i18n." + code + ".json"))
        {
            if (stream == null) throw new InvalidDataException("Missing embedded localization catalog: " + code);
            translations = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
        }
        if (translations == null || translations.Count != catalog.Count)
            throw new InvalidDataException("Incomplete localization catalog: " + code);
        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            string value;
            if (!translations.TryGetValue(item.Key, out value) || string.IsNullOrWhiteSpace(value))
                throw new InvalidDataException("Incomplete localization entry: " + code + "/" + item.Key);
            item.Value.Add(code, value);
        }
    }

    internal static string Text(string key)
    {
        Dictionary<string, string> entry;
        string value;
        if (!Catalog.TryGetValue(key, out entry) || !entry.TryGetValue(Current, out value))
            throw new InvalidDataException("Missing localization: " + Current + "/" + key);
        return value;
    }

    internal static void Initialize(string settingsFile)
    {
        bool existingProfile = File.Exists(settingsFile);
        // English is the product default for new installations. Profiles created
        // before language persistence existed retain Portuguese unless they already
        // contain an explicit supported language= value.
        string chosen = existingProfile ? "pt-BR" : "en-US";
        if (existingProfile)
        {
            try
            {
                foreach (string line in File.ReadAllLines(settingsFile))
                {
                    if (!line.StartsWith("language=", StringComparison.Ordinal)) continue;
                    string code = line.Substring("language=".Length).Trim();
                    if (code == "pt-BR" || code == "en-US" || code == "es-ES" ||
                        code == "ru-RU" || code == "zh-CN") chosen = code;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        Select(chosen);
    }

    internal static void SaveInitialSelection(string settingsFile, string code)
    {
        Select(code);
        Directory.CreateDirectory(Path.GetDirectoryName(settingsFile));
        string temporary = settingsFile + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllLines(temporary, new[] { "language=" + code });
            if (File.Exists(settingsFile))
                File.Replace(temporary, settingsFile, settingsFile + ".bak", true);
            else
                File.Move(temporary, settingsFile);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static void Select(string code)
    {
        // Only catalogs actually embedded in the application can be selected.
        if (code != "pt-BR" && code != "en-US" && code != "es-ES" &&
            code != "ru-RU" && code != "zh-CN")
            throw new ArgumentOutOfRangeException(nameof(code), "Unsupported application language.");
        Current = code;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(code);
    }
}
