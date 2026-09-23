using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

internal sealed class SnippetEntry
{
    internal string Name;
    internal string Content;

    internal SnippetEntry Copy()
    {
        return new SnippetEntry { Name = Name, Content = Content };
    }
}

internal static class SnippetStore
{
    internal const int MinimumSlots = 4;
    internal const int HotkeySlots = 4;
    internal const int MaxSlots = 8;
    internal const int MaxNameLength = 48;
    internal const int MaxContentLength = 65536;
    internal const long MaxFileBytes = 2L * 1024L * 1024L;

    internal static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WinSidebar");
    internal static readonly string FilePath = Path.Combine(Root, "snippets.json");

    private sealed class Document
    {
        // Explicit public constructors keep System.Text.Json deserialization
        // independent from non-public-constructor behavior.
        public Document() { }
        public int version { get; set; }
        public List<Item> items { get; set; }
    }

    private sealed class Item
    {
        public Item() { }
        public int id { get; set; }
        public string name { get; set; }
        public string content { get; set; }
    }

    internal static SnippetEntry CreateDefault(int index)
    {
        return new SnippetEntry { Name = "Script " + (index + 1), Content = "" };
    }

    internal static SnippetEntry[] Defaults()
    {
        SnippetEntry[] result = new SnippetEntry[MinimumSlots];
        for (int i = 0; i < result.Length; i++) result[i] = CreateDefault(i);
        return result;
    }

    private static void ValidateCount(int count)
    {
        if (count < MinimumSlots || count > MaxSlots)
            throw new InvalidDataException("Snippet file contains an invalid number of slots.");
    }

    internal static SnippetEntry[] Read()
    {
        return Read(FilePath);
    }

    internal static SnippetEntry[] Read(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Snippet path is required.", nameof(path));
        if (!File.Exists(path)) return Defaults();

        FileInfo file = new FileInfo(path);
        if (file.Length <= 0 || file.Length > MaxFileBytes)
            throw new InvalidDataException("Snippet file has an invalid size.");

        Document document;
        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            document = JsonSerializer.Deserialize<Document>(stream);

        if (document == null || document.items == null)
            throw new InvalidDataException("Snippet file has an unsupported structure or version.");
        if (document.version == 1)
        {
            if (document.items.Count != MinimumSlots)
                throw new InvalidDataException("Snippet file has an unsupported structure or version.");
        }
        else if (document.version == 2) ValidateCount(document.items.Count);
        else throw new InvalidDataException("Snippet file has an unsupported structure or version.");

        SnippetEntry[] result = new SnippetEntry[document.items.Count];
        bool[] seen = new bool[document.items.Count];
        foreach (Item item in document.items)
        {
            if (item == null || item.id < 0 || item.id >= result.Length || seen[item.id])
                throw new InvalidDataException("Snippet file contains an invalid slot id.");
            seen[item.id] = true;
            result[item.id] = NormalizeAndValidate(new SnippetEntry {
                Name = item.name,
                Content = item.content
            });
        }
        for (int i = 0; i < result.Length; i++)
            if (!seen[i] || result[i] == null)
                throw new InvalidDataException("Snippet file contains missing or duplicate slots.");
        return result;
    }

    internal static void Write(SnippetEntry[] entries)
    {
        Write(FilePath, entries);
    }

    internal static void Write(string path, SnippetEntry[] entries)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Snippet path is required.", nameof(path));
        if (entries == null) throw new InvalidDataException("Snippet slots are required.");
        ValidateCount(entries.Length);

        Document document = new Document {
            version = entries.Length == MinimumSlots ? 1 : 2,
            items = new List<Item>(entries.Length)
        };
        for (int i = 0; i < entries.Length; i++)
        {
            SnippetEntry normalized = NormalizeAndValidate(entries[i]);
            document.items.Add(new Item { id = i, name = normalized.Name, content = normalized.Content });
        }

        string directory = Path.GetDirectoryName(Path.GetFullPath(path));
        Directory.CreateDirectory(directory);
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        string backup = path + ".bak";
        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(document, options);
            byte[] bytes = new UTF8Encoding(false).GetBytes(json);
            if (bytes.LongLength > MaxFileBytes)
                throw new InvalidDataException("Serialized snippet file is too large.");
            File.WriteAllBytes(temporary, bytes);
            if (File.Exists(path)) File.Replace(temporary, path, backup, true);
            else File.Move(temporary, path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static SnippetEntry NormalizeAndValidate(SnippetEntry entry)
    {
        if (entry == null || entry.Name == null || entry.Content == null)
            throw new InvalidDataException("Snippet name and content are required.");

        string name = entry.Name.Trim();
        if (name.Length == 0 || name.Length > MaxNameLength)
            throw new InvalidDataException("Snippet name length is invalid.");
        if (entry.Content.Length > MaxContentLength)
            throw new InvalidDataException("Snippet content is too large.");

        return new SnippetEntry { Name = name, Content = entry.Content };
    }
}
