using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

// All application-owned strings use stable keys. Never translate user data, external
// window titles, filesystem paths, serialized identifiers or process identities.
internal static class Localization
{
    private static readonly Dictionary<string, Dictionary<string, string>> Catalog = ReadCatalog();
    internal static string Current { get; private set; } = "pt-BR";

    private static Dictionary<string, Dictionary<string, string>> ReadCatalog()
    {
        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("WinSidebar.i18n.catalog.json"))
        {
            if (stream == null) throw new InvalidDataException("Missing embedded localization catalog.");
            Dictionary<string, Dictionary<string, string>> catalog =
                JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream);
            if (catalog == null || catalog.Count == 0) throw new InvalidDataException("Empty localization catalog.");
            foreach (KeyValuePair<string, Dictionary<string, string>> entry in catalog)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value == null ||
                    !entry.Value.ContainsKey("pt-BR") || !entry.Value.ContainsKey("en-US") ||
                    string.IsNullOrWhiteSpace(entry.Value["pt-BR"]) ||
                    string.IsNullOrWhiteSpace(entry.Value["en-US"]))
                    throw new InvalidDataException("Incomplete localization entry: " + entry.Key);
            }
            return catalog;
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
        // Existing installations with no language setting retain Portuguese.
        string chosen = "pt-BR";
        if (!File.Exists(settingsFile))
        {
            string os = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            chosen = os == "pt" ? "pt-BR" : "en-US";
        }
        else
        {
            try
            {
                foreach (string line in File.ReadAllLines(settingsFile))
                {
                    if (!line.StartsWith("language=", StringComparison.Ordinal)) continue;
                    string code = line.Substring("language=".Length).Trim();
                    if (code == "pt-BR" || code == "en-US") chosen = code;
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        Select(chosen);
    }

    internal static void Select(string code)
    {
        // More locales become selectable only when their complete catalogs ship.
        if (code != "pt-BR" && code != "en-US")
            throw new ArgumentOutOfRangeException(nameof(code), "Unsupported application language.");
        Current = code;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(code);
    }
}
