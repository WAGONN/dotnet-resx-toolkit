using System.Xml.Linq;
using MiniExcelLibs;
using WAGONN.DotNet.ResX.Toolkit.Models;
using WAGONN.DotNet.ResX.Toolkit.UI;

namespace WAGONN.DotNet.ResX.Toolkit.Commands;

public static class ExportCommand
{
    public static int Run(string resxPath, string excelPath)
    {
        resxPath  = resxPath.Trim('"', '\'', ' ');
        excelPath = excelPath.Trim('"', '\'', ' ');

        ConsoleHelper.WriteInfo("Input  ", resxPath);
        ConsoleHelper.WriteInfo("Output ", excelPath);
        Console.WriteLine();

        if (!File.Exists(resxPath))
        {
            ConsoleHelper.WriteError($"File not found: {resxPath}");
            return 1;
        }

        try
        {
            ConsoleHelper.WriteDim("  Reading RESX entries...");
            var entries = ReadResxEntries(resxPath);

            ConsoleHelper.WriteDim($"  Parsed {entries.Count} entries. Writing Excel workbook...");

            var outputDir = Path.GetDirectoryName(Path.GetFullPath(excelPath));
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            MiniExcel.SaveAs(excelPath, entries, overwriteFile: true);

            ConsoleHelper.WriteSuccess($"{entries.Count} entries exported → {excelPath}");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Export failed: {ex.Message}");
            return 1;
        }
    }

    private static List<ResxEntry> ReadResxEntries(string resxPath)
    {
        var xdoc    = XDocument.Load(resxPath);
        var entries = new List<ResxEntry>();

        foreach (var dataElement in xdoc.Root!.Elements("data"))
        {
            entries.Add(new ResxEntry
            {
                Key     = dataElement.Attribute("name")?.Value  ?? string.Empty,
                Value   = dataElement.Element("value")?.Value   ?? string.Empty,
                Comment = dataElement.Element("comment")?.Value ?? string.Empty,
            });
        }

        return entries;
    }
}
