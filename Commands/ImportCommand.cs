using System.Xml.Linq;
using MiniExcelLibs;
using WAGONN.DotNet.ResX.Toolkit.Models;
using WAGONN.DotNet.ResX.Toolkit.UI;

namespace WAGONN.DotNet.ResX.Toolkit.Commands;

public static class ImportCommand
{
    public static int Run(string excelPath, string resxPath)
    {
        excelPath = excelPath.Trim('"', '\'', ' ');
        resxPath  = resxPath.Trim('"', '\'', ' ');

        ConsoleHelper.WriteInfo("Input  ", excelPath);
        ConsoleHelper.WriteInfo("Output ", resxPath);
        Console.WriteLine();

        if (!File.Exists(excelPath))
        {
            ConsoleHelper.WriteError($"File not found: {excelPath}");
            return 1;
        }

        try
        {
            ConsoleHelper.WriteDim("  Reading Excel workbook...");
            var entries = MiniExcel.Query<ResxEntry>(excelPath).ToList();

            ConsoleHelper.WriteDim($"  Loaded {entries.Count} rows. Writing RESX document...");

            var outputDir = Path.GetDirectoryName(Path.GetFullPath(resxPath));
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            var doc = BuildResxDocument(entries);
            doc.Save(resxPath);

            int written = entries.Count(e => !string.IsNullOrWhiteSpace(e.Key));
            ConsoleHelper.WriteSuccess($"{written} entries imported → {resxPath}");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Import failed: {ex.Message}");
            return 1;
        }
    }

    private static XDocument BuildResxDocument(IEnumerable<ResxEntry> entries)
    {
        var root = new XElement("root",
            ResxHeader("resmimetype", "text/microsoft-resx"),
            ResxHeader("version",     "2.0"),
            ResxHeader("reader",      "System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"),
            ResxHeader("writer",      "System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
        );

        foreach (var entry in entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var dataElement = new XElement("data",
                new XAttribute("name", entry.Key),
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement("value", entry.Value ?? string.Empty)
            );

            if (!string.IsNullOrWhiteSpace(entry.Comment))
                dataElement.Add(new XElement("comment", entry.Comment));

            root.Add(dataElement);
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    private static XElement ResxHeader(string name, string value) =>
        new XElement("resheader",
            new XAttribute("name", name),
            new XElement("value", value));
}
