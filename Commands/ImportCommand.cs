using System.Xml.Linq;
using MiniExcelLibs;
using WAGONN.DotNet.ResX.Toolkit.Models;
using WAGONN.DotNet.ResX.Toolkit.UI;

using WAGONN.DotNet.ResX.Toolkit.Services;

namespace WAGONN.DotNet.ResX.Toolkit.Commands;

public static class ImportCommand
{
    public static int Run(string inputPath, string outputPath, string? culture = null)
    {
        inputPath  = inputPath.Trim('"', '\'', ' ');
        outputPath = outputPath.Trim('"', '\'', ' ');
        if (!string.IsNullOrWhiteSpace(culture))
            culture = culture.Trim('"', '\'', ' ');

        if (Directory.Exists(inputPath))
        {
            return ImportDirectory(inputPath, outputPath, culture);
        }

        if (File.Exists(inputPath))
        {
            return ImportSingleFile(inputPath, outputPath);
        }

        ConsoleHelper.WriteError($"File or directory not found: {inputPath}");
        return 1;
    }

    private static int ImportDirectory(string inputDir, string outputDir, string? culture)
    {
        inputDir  = Path.GetFullPath(inputDir);
        outputDir = Path.GetFullPath(outputDir);

        ConsoleHelper.WriteInfo("Input Dir ", inputDir);
        ConsoleHelper.WriteInfo("Output Dir", outputDir);
        if (!string.IsNullOrWhiteSpace(culture))
            ConsoleHelper.WriteInfo("Culture   ", culture);
        Console.WriteLine();

        var excelFiles = Directory.EnumerateFiles(inputDir, "*.xlsx", SearchOption.AllDirectories)
            .Where(f => !Path.GetFileName(f).StartsWith("~$") && ResourceFilter.MatchesCulture(f, culture))
            .OrderBy(f => f)
            .ToList();

        if (excelFiles.Count == 0)
        {
            string filterMsg = !string.IsNullOrWhiteSpace(culture)
                ? $" matching culture '{culture}'"
                : string.Empty;
            ConsoleHelper.WriteWarning($"No Excel files found in '{inputDir}'{filterMsg}.");
            return 0;
        }

        ConsoleHelper.WriteDim($"  Found {excelFiles.Count} matching Excel file(s). Importing to RESX...");
        Console.WriteLine();

        int successFiles = 0;
        int errorCount   = 0;
        int totalEntries = 0;

        foreach (var file in excelFiles)
        {
            string relativePath = Path.GetRelativePath(inputDir, file);
            string targetRel    = Path.ChangeExtension(relativePath, ".resx");
            string targetPath   = Path.Combine(outputDir, targetRel);

            try
            {
                var entries = MiniExcel.Query<ResxEntry>(file).ToList();

                var targetSubDir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetSubDir))
                    Directory.CreateDirectory(targetSubDir);

                var doc = BuildResxDocument(entries);
                doc.Save(targetPath);

                int written = entries.Count(e => !string.IsNullOrWhiteSpace(e.Key));
                ConsoleHelper.WriteDim($"  [✓] {relativePath} → {targetRel} ({written} entries)");
                totalEntries += written;
                successFiles++;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"  [✗] Failed to import '{relativePath}': {ex.Message}");
                errorCount++;
            }
        }

        if (errorCount == 0)
        {
            ConsoleHelper.WriteSuccess($"Successfully imported {successFiles} file(s) ({totalEntries} total entries) → {outputDir}");
            return 0;
        }

        ConsoleHelper.WriteWarning($"Completed with issues: {successFiles} file(s) succeeded, {errorCount} failed.");
        return 1;
    }

    private static int ImportSingleFile(string excelPath, string resxPath)
    {
        ConsoleHelper.WriteInfo("Input  ", excelPath);
        ConsoleHelper.WriteInfo("Output ", resxPath);
        Console.WriteLine();

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
