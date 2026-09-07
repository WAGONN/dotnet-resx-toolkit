using System.Xml.Linq;
using MiniExcelLibs;
using WAGONN.DotNet.ResX.Toolkit.Models;
using WAGONN.DotNet.ResX.Toolkit.UI;

using WAGONN.DotNet.ResX.Toolkit.Services;

namespace WAGONN.DotNet.ResX.Toolkit.Commands;

public static class ExportCommand
{
    public static int Run(string inputPath, string outputPath, string? culture = null)
    {
        inputPath  = inputPath.Trim('"', '\'', ' ');
        outputPath = outputPath.Trim('"', '\'', ' ');
        if (!string.IsNullOrWhiteSpace(culture))
            culture = culture.Trim('"', '\'', ' ');

        if (Directory.Exists(inputPath))
        {
            return ExportDirectory(inputPath, outputPath, culture);
        }

        if (File.Exists(inputPath))
        {
            return ExportSingleFile(inputPath, outputPath);
        }

        ConsoleHelper.WriteError($"File or directory not found: {inputPath}");
        return 1;
    }

    private static int ExportDirectory(string inputDir, string outputDir, string? culture)
    {
        inputDir  = Path.GetFullPath(inputDir);
        outputDir = Path.GetFullPath(outputDir);

        ConsoleHelper.WriteInfo("Input Dir ", inputDir);
        ConsoleHelper.WriteInfo("Output Dir", outputDir);
        if (!string.IsNullOrWhiteSpace(culture))
            ConsoleHelper.WriteInfo("Culture   ", culture);
        Console.WriteLine();

        var resxFiles = Directory.EnumerateFiles(inputDir, "*.resx", SearchOption.AllDirectories)
            .Where(f => ResourceFilter.MatchesCulture(f, culture))
            .OrderBy(f => f)
            .ToList();

        if (resxFiles.Count == 0)
        {
            string filterMsg = !string.IsNullOrWhiteSpace(culture)
                ? $" matching culture '{culture}'"
                : string.Empty;
            ConsoleHelper.WriteWarning($"No RESX files found in '{inputDir}'{filterMsg}.");
            return 0;
        }

        ConsoleHelper.WriteDim($"  Found {resxFiles.Count} matching RESX file(s). Exporting to Excel...");
        Console.WriteLine();

        int successFiles = 0;
        int errorCount   = 0;
        int totalEntries = 0;

        foreach (var file in resxFiles)
        {
            string relativePath = Path.GetRelativePath(inputDir, file);
            string targetRel    = Path.ChangeExtension(relativePath, ".xlsx");
            string targetPath   = Path.Combine(outputDir, targetRel);

            try
            {
                var entries = ReadResxEntries(file);

                var targetSubDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetSubDir))
                    Directory.CreateDirectory(targetSubDir);

                MiniExcel.SaveAs(targetPath, entries, overwriteFile: true);

                ConsoleHelper.WriteDim($"  [✓] {relativePath} → {targetRel} ({entries.Count} entries)");
                totalEntries += entries.Count;
                successFiles++;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"  [✗] Failed to export '{relativePath}': {ex.Message}");
                errorCount++;
            }
        }

        if (errorCount == 0)
        {
            ConsoleHelper.WriteSuccess($"Successfully exported {successFiles} file(s) ({totalEntries} total entries) → {outputDir}");
            return 0;
        }

        ConsoleHelper.WriteWarning($"Completed with issues: {successFiles} file(s) succeeded, {errorCount} failed.");
        return 1;
    }

    private static int ExportSingleFile(string resxPath, string excelPath)
    {
        ConsoleHelper.WriteInfo("Input  ", resxPath);
        ConsoleHelper.WriteInfo("Output ", excelPath);
        Console.WriteLine();

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
