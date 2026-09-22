using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

// Application-owned strings only. External window titles, user data, paths,
// process identities and serialized shortcut/icon identifiers are never translated.
internal static class Localization
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = ReadCatalog();
    internal static string Current { get; private set; } = "pt-BR";

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

        Dictionary<string, string> spanish;
        using (Stream stream = assembly.GetManifestResourceStream("WinSidebar.i18n.es-ES.json"))
        {
            if (stream == null) throw new InvalidDataException("Missing embedded Spanish localization catalog.");
            spanish = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
        }
        if (spanish == null || spanish.Count != catalog.Count)
            throw new InvalidDataException("Incomplete Spanish localization catalog.");

        foreach (KeyValuePair<string, Dictionary<string, string>> item in catalog)
        {
            Dictionary<string, string> entry = item.Value;
            string translated;
            if (string.IsNullOrWhiteSpace(item.Key) || entry == null ||
                !entry.ContainsKey("pt-BR") || !entry.ContainsKey("en-US") ||
                string.IsNullOrWhiteSpace(entry["pt-BR"]) ||
                string.IsNullOrWhiteSpace(entry["en-US"]) ||
                !spanish.TryGetValue(item.Key, out translated) || string.IsNullOrWhiteSpace(translated))
                throw new InvalidDataException("Incomplete localization entry: " + item.Key);
            entry.Add("es-ES", translated);
        }
        return catalog;
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
        // Existing profiles without a language field retain Portuguese.
        string chosen = "pt-BR";
        if (!File.Exists(settingsFile))
        {
            string os = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            chosen = os == "pt" ? "pt-BR" : os == "es" ? "es-ES" : "en-US";
        }
        else
        {
            try
            {
                foreach (string line in File.ReadAllLines(settingsFile))
                {
                    if (!line.StartsWith("language=", StringComparison.Ordinal)) continue;
                    string code = line.Substring("language=".Length).Trim();
                    if (code == "pt-BR" || code == "en-US" || code == "es-ES") chosen = code;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        Select(chosen);
    }

    internal static void Select(string code)
    {
        // Unimplemented locales are not shown or selectable.
        if (code != "pt-BR" && code != "en-US" && code != "es-ES")
            throw new ArgumentOutOfRangeException(nameof(code), "Unsupported application language.");
        Current = code;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(code);
    }
}
