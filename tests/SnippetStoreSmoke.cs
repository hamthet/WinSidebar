using System;
using System.IO;
using System.Text;
using System.Text.Json;

internal static class SnippetStoreSmoke
{
    private static int checks;

    private static void Check(bool condition, string scenario)
    {
        checks++;
        if (!condition) throw new Exception("FAILED: " + scenario);
    }

    private static void ExpectInvalid(Action action, string scenario)
    {
        bool rejected = false;
        try { action(); }
        catch (InvalidDataException) { rejected = true; }
        Check(rejected, scenario);
    }

    private static string ItemJson(int id, string name, string content)
    {
        return JsonSerializer.Serialize(new { id = id, name = name, content = content });
    }

    private static void Main()
    {
        string directory = Path.Combine(Path.GetTempPath(),
            "WinSidebarSnippetStoreSmoke-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "snippets.json");

        try
        {
            SnippetEntry[] defaults = SnippetStore.Read(path);
            Check(defaults.Length == 4, "missing file yields four defaults");
            for (int i = 0; i < 4; i++)
            {
                Check(defaults[i].Name == "Script " + (i + 1), "default name " + i);
                Check(defaults[i].Content == "", "default content empty " + i);
            }

            SnippetEntry[] first = SnippetStore.Defaults();
            first[0].Name = "  Resposta multilíngue  ";
            first[0].Content = "  início\r\nPortuguês: ação\r\nРусский: тест\r\n简体中文：测试\r\nEmoji: 🙂\r\nfim  ";
            first[1].Content = "\r\n";
            first[2].Content = new string('x', SnippetStore.MaxContentLength);
            SnippetStore.Write(path, first);

            SnippetEntry[] roundTrip = SnippetStore.Read(path);
            Check(roundTrip[0].Name == "Resposta multilíngue", "names are canonicalized by trimming");
            Check(roundTrip[0].Content == first[0].Content, "multiline Unicode content round-trips exactly");
            Check(roundTrip[1].Content == "\r\n", "content whitespace is never trimmed");
            Check(roundTrip[2].Content.Length == SnippetStore.MaxContentLength, "maximum content length accepted");

            SnippetEntry[] second = SnippetStore.Read(path);
            second[3].Name = new string('N', SnippetStore.MaxNameLength);
            second[3].Content = "second version";
            SnippetStore.Write(path, second);
            Check(File.Exists(path + ".bak"), "atomic replacement creates a backup");
            SnippetEntry[] backup = SnippetStore.Read(path + ".bak");
            Check(backup[3].Content == "", "backup preserves previous generation");
            Check(SnippetStore.Read(path)[3].Content == "second version", "replacement writes new generation");

            SnippetEntry[] badCount = new SnippetEntry[3];
            ExpectInvalid(delegate { SnippetStore.Write(path, badCount); }, "writer rejects non-four-slot array");

            SnippetEntry[] badName = SnippetStore.Defaults();
            badName[0].Name = new string('n', SnippetStore.MaxNameLength + 1);
            ExpectInvalid(delegate { SnippetStore.Write(path, badName); }, "writer rejects oversized name");

            SnippetEntry[] emptyName = SnippetStore.Defaults();
            emptyName[0].Name = "   ";
            ExpectInvalid(delegate { SnippetStore.Write(path, emptyName); }, "writer rejects blank name");

            SnippetEntry[] badContent = SnippetStore.Defaults();
            badContent[0].Content = new string('x', SnippetStore.MaxContentLength + 1);
            ExpectInvalid(delegate { SnippetStore.Write(path, badContent); }, "writer rejects oversized content");

            File.WriteAllText(path,
                "{\"version\":2,\"items\":[" +
                ItemJson(0, "Script 1", "") + "," + ItemJson(1, "Script 2", "") + "," +
                ItemJson(2, "Script 3", "") + "," + ItemJson(3, "Script 4", "") + "]}",
                new UTF8Encoding(false));
            ExpectInvalid(delegate { SnippetStore.Read(path); }, "reader rejects unknown version");

            File.WriteAllText(path,
                "{\"version\":1,\"items\":[" +
                ItemJson(0, "Script 1", "") + "," + ItemJson(0, "Duplicate", "") + "," +
                ItemJson(2, "Script 3", "") + "," + ItemJson(3, "Script 4", "") + "]}",
                new UTF8Encoding(false));
            ExpectInvalid(delegate { SnippetStore.Read(path); }, "reader rejects duplicate slot ids");

            File.WriteAllText(path,
                "{\"version\":1,\"items\":[" +
                ItemJson(0, "Script 1", "") + "," + ItemJson(1, "Script 2", "") + "," +
                ItemJson(2, "Script 3", "") + "]}",
                new UTF8Encoding(false));
            ExpectInvalid(delegate { SnippetStore.Read(path); }, "reader rejects missing slot");

            using (FileStream oversized = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                oversized.SetLength(SnippetStore.MaxFileBytes + 1);
            ExpectInvalid(delegate { SnippetStore.Read(path); }, "reader rejects oversized physical file");
        }
        finally
        {
            try { Directory.Delete(directory, true); } catch (Exception) { }
        }

        Console.WriteLine("PASS: " + checks + " snippet-store smoke checks.");
    }
}
