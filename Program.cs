using WAGONN.DotNet.ResX.Toolkit.Commands;
using WAGONN.DotNet.ResX.Toolkit.UI;

namespace WAGONN.DotNet.ResX.Toolkit;

class Program
{
    private static readonly (string Label, string Desc)[] MenuItems =
    [
        ("Export  RESX  →  Excel",    "Read a RESX resource file and export all entries to a translated Excel workbook"),
        ("Import  Excel →  RESX",     "Read a translated Excel workbook and write a standards-compliant RESX file"),
        ("Validate  RESX  or  Excel", "Scan a file for broken HTML tags with exact row & key details"),
        ("Exit",                       "Quit toolkit"),
    ];

    static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        if (args.Length > 0)
            return RunCliMode(args);

        return RunInteractiveMode();
    }

    static int RunCliMode(string[] args)
    {
        return args[0].ToLowerInvariant() switch
        {
            "export" or "resx-to-excel" => HandleCliExport(args),
            "import" or "excel-to-resx" => HandleCliImport(args),
            "validate" or "check"        => HandleCliValidate(args),
            "--help" or "-h"             => HandleCliHelp(),
            _                            => HandleCliUnknown(args[0])
        };
    }

    static int HandleCliExport(string[] args)
    {
        if (args.Length < 3)
        {
            ConsoleHelper.WriteError("Usage: export <input.resx> <output.xlsx>");
            return 1;
        }
        return ExportCommand.Run(resxPath: args[1], excelPath: args[2]);
    }

    static int HandleCliImport(string[] args)
    {
        if (args.Length < 3)
        {
            ConsoleHelper.WriteError("Usage: import <input.xlsx> <output.resx>");
            return 1;
        }
        return ImportCommand.Run(excelPath: args[1], resxPath: args[2]);
    }

    static int HandleCliValidate(string[] args)
    {
        if (args.Length < 2)
        {
            ConsoleHelper.WriteError("Usage: validate <file.resx|file.xlsx>");
            return 1;
        }
        return ValidateCommand.Run(filePath: args[1]);
    }

    static int HandleCliHelp()
    {
        Console.WriteLine();
        Console.WriteLine("  WAGONN DotNet ResX Toolkit");
        Console.WriteLine();
        Console.WriteLine("  USAGE");
        Console.WriteLine("    dotnet run -- export    <input.resx>  <output.xlsx>");
        Console.WriteLine("    dotnet run -- import    <input.xlsx>  <output.resx>");
        Console.WriteLine("    dotnet run -- validate  <file.resx|file.xlsx>");
        Console.WriteLine();
        Console.WriteLine("  ALIASES");
        Console.WriteLine("    export    resx-to-excel");
        Console.WriteLine("    import    excel-to-resx");
        Console.WriteLine("    validate  check");
        Console.WriteLine();
        Console.WriteLine("  EXIT CODES");
        Console.WriteLine("    0  — Success / no issues found");
        Console.WriteLine("    1  — File not found or parse error");
        Console.WriteLine("    2  — Validation errors detected");
        Console.WriteLine();
        Console.WriteLine("  EXAMPLES");
        Console.WriteLine("    dotnet run -- export    Resources.tr.resx   Translations_TR.xlsx");
        Console.WriteLine("    dotnet run -- import    Translations_DE.xlsx Resources.de.resx");
        Console.WriteLine("    dotnet run -- validate  Translations_DE.xlsx");
        Console.WriteLine("    dotnet run -- validate  Resources.tr.resx");
        Console.WriteLine();
        return 0;
    }

    static int HandleCliUnknown(string command)
    {
        ConsoleHelper.WriteError($"Unknown command: '{command}'. Run with --help for usage.");
        return 1;
    }

    static int RunInteractiveMode()
    {
        while (true)
        {
            int choice = ConsoleHelper.ArrowMenu("SELECT OPERATION", MenuItems);

            switch (choice)
            {
                case 0:
                    RunInteractiveExport();
                    break;

                case 1:
                    RunInteractiveImport();
                    break;

                case 2:
                    RunInteractiveValidate();
                    break;

                case 3:
                case -1:
                    Console.Clear();
                    Console.WriteLine();
                    ConsoleHelper.WriteDim("  WAGONN DotNet ResX Toolkit terminated.");
                    Console.WriteLine();
                    return 0;
            }
        }
    }

    static void RunInteractiveExport()
    {
        ConsoleHelper.PrintBanner();
        ConsoleHelper.PrintSection("RESX  →  EXCEL  EXPORT");
        Console.WriteLine();

        string resxPath  = ConsoleHelper.Prompt("Input  RESX file path  (e.g. Resources.tr.resx)");
        string excelPath = ConsoleHelper.Prompt("Output XLSX file path  (e.g. Translations_TR.xlsx)");

        Console.WriteLine();
        ExportCommand.Run(resxPath, excelPath);
        ConsoleHelper.WaitForEnter();
    }

    static void RunInteractiveImport()
    {
        ConsoleHelper.PrintBanner();
        ConsoleHelper.PrintSection("EXCEL  →  RESX  IMPORT");
        Console.WriteLine();

        string excelPath = ConsoleHelper.Prompt("Input  XLSX file path  (e.g. Translations_DE.xlsx)");
        string resxPath  = ConsoleHelper.Prompt("Output RESX file path  (e.g. Resources.de.resx)");

        Console.WriteLine();
        ImportCommand.Run(excelPath, resxPath);
        ConsoleHelper.WaitForEnter();
    }

    static void RunInteractiveValidate()
    {
        ConsoleHelper.PrintBanner();
        ConsoleHelper.PrintSection("HTML TAG VALIDATION");
        Console.WriteLine();

        string filePath = ConsoleHelper.Prompt("File path to validate  (.resx or .xlsx)");

        Console.WriteLine();
        ValidateCommand.Run(filePath);
        ConsoleHelper.WaitForEnter();
    }
}
