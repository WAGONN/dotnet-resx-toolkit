using System.Xml.Linq;
using MiniExcelLibs;
using WAGONN.DotNet.ResX.Toolkit.Models;
using WAGONN.DotNet.ResX.Toolkit.Services;
using WAGONN.DotNet.ResX.Toolkit.UI;

namespace WAGONN.DotNet.ResX.Toolkit.Commands;

public static class ValidateCommand
{
    public static int Run(string filePath)
    {
        filePath = filePath.Trim('"', '\'', ' ');

        if (!File.Exists(filePath))
        {
            ConsoleHelper.WriteError($"File not found: {filePath}");
            return 1;
        }

        string ext = Path.GetExtension(filePath).ToLowerInvariant();

        return ext switch
        {
            ".resx"          => ValidateResx(filePath),
            ".xlsx" or ".xls" => ValidateExcel(filePath),
            _ => HandleUnsupportedExtension(ext)
        };
    }

    private static int ValidateResx(string path)
    {
        ConsoleHelper.WriteInfo("Source  ", path);
        ConsoleHelper.WriteInfo("Format  ", "RESX (XML resource file)");
        Console.WriteLine();

        XDocument xdoc;
        try
        {
            xdoc = XDocument.Load(path);
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Cannot parse RESX file as XML: {ex.Message}");
            return 1;
        }

        var allIssues = new List<ValidationIssue>();
        int rowIndex  = 0;

        foreach (var dataElement in xdoc.Root!.Elements("data"))
        {
            rowIndex++;
            string key   = dataElement.Attribute("name")?.Value ?? $"<unnamed #{rowIndex}>";
            string value = dataElement.Element("value")?.Value  ?? string.Empty;

            var issues = HtmlTagValidator.Validate(value);
            foreach (var issue in issues)
            {
                allIssues.Add(issue with { RowNumber = rowIndex, Key = key });
            }
        }

        PrintReport(allIssues, rowIndex, path);
        return allIssues.Any(i => i.Severity == IssueSeverity.Error) ? 2 : 0;
    }

    private static int ValidateExcel(string path)
    {
        ConsoleHelper.WriteInfo("Source  ", path);
        ConsoleHelper.WriteInfo("Format  ", "Excel workbook (.xlsx)");
        Console.WriteLine();

        List<ResxEntry> entries;
        try
        {
            entries = MiniExcel.Query<ResxEntry>(path).ToList();
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Cannot read Excel file: {ex.Message}");
            return 1;
        }

        var allIssues = new List<ValidationIssue>();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            int rowNumber = i + 2;
            string key    = string.IsNullOrWhiteSpace(entry.Key) ? $"<empty key, row {rowNumber}>" : entry.Key;

            var issues = HtmlTagValidator.Validate(entry.Value);
            foreach (var issue in issues)
            {
                allIssues.Add(issue with { RowNumber = rowNumber, Key = key });
            }
        }

        PrintReport(allIssues, entries.Count, path);
        return allIssues.Any(i => i.Severity == IssueSeverity.Error) ? 2 : 0;
    }

    private static void PrintReport(IReadOnlyList<ValidationIssue> issues, int totalRows, string path)
    {
        int errors   = issues.Count(i => i.Severity == IssueSeverity.Error);
        int warnings = issues.Count(i => i.Severity == IssueSeverity.Warning);

        ConsoleHelper.PrintSeparator();
        ConsoleHelper.WriteDim($"  Scanned {totalRows} entries in {Path.GetFileName(path)}");
        ConsoleHelper.PrintSeparator();
        Console.WriteLine();

        if (issues.Count == 0)
        {
            ConsoleHelper.WriteSuccess("All entries passed HTML tag validation. No issues found.");
            return;
        }

        var grouped = issues
            .OrderBy(i => i.RowNumber)
            .ThenBy(i => i.Severity);

        foreach (var issue in grouped)
        {
            ConsoleHelper.WriteIssue(issue);
        }

        Console.WriteLine();
        ConsoleHelper.PrintSeparator();

        if (errors > 0)
            ConsoleHelper.WriteError($"Validation complete — {errors} error(s), {warnings} warning(s) across {issues.Count} issue(s) in {totalRows} entries.");
        else
            ConsoleHelper.WriteWarning($"Validation complete — 0 errors, {warnings} warning(s) across {issues.Count} issue(s) in {totalRows} entries.");
    }

    private static int HandleUnsupportedExtension(string ext)
    {
        ConsoleHelper.WriteError($"Unsupported file type '{ext}'. Supported: .resx, .xlsx, .xls");
        return 1;
    }
}
