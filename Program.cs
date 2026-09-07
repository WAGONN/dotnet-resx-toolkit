using WAGONN.DotNet.ResX.Toolkit.Commands;
using WAGONN.DotNet.ResX.Toolkit.UI;

namespace WAGONN.DotNet.ResX.Toolkit;

class Program
{
    private static readonly (string Label, string Desc)[] MenuItems =
    [
        ("Export  RESX  →  Excel",    "Export RESX file(s) or folder to Excel workbook(s) with optional culture filter"),
        ("Import  Excel →  RESX",     "Import Excel workbook(s) or folder to RESX resource file(s) with optional culture filter"),
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
            ConsoleHelper.WriteError("Usage: export <input.resx|input_dir> <output.xlsx|output_dir> [culture] [--culture <culture>]");
            return 1;
        }

        string inputPath  = args[1];
        string outputPath = args[2];
        string? culture   = ParseCultureArg(args, startIndex: 3);

        return ExportCommand.Run(inputPath, outputPath, culture);
    }

    static int HandleCliImport(string[] args)
    {
        if (args.Length < 3)
        {
            ConsoleHelper.WriteError("Usage: import <input.xlsx|input_dir> <output.resx|output_dir> [culture] [--culture <culture>]");
            return 1;
        }

        string inputPath  = args[1];
        string outputPath = args[2];
        string? culture   = ParseCultureArg(args, startIndex: 3);

        return ImportCommand.Run(inputPath, outputPath, culture);
    }

    private static string? ParseCultureArg(string[] args, int startIndex)
    {
        for (int i = startIndex; i < args.Length; i++)
        {
            if ((args[i] is "--culture" or "-c") && i + 1 < args.Length)
            {
                return args[i + 1];
            }
            if (!args[i].StartsWith('-'))
            {
                return args[i];
            }
        }
        return null;
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
        Console.WriteLine("    dotnet run -- export    <input.resx|input_dir>  <output.xlsx|output_dir>  [culture]");
        Console.WriteLine("    dotnet run -- import    <input.xlsx|input_dir>  <output.resx|output_dir>  [culture]");
        Console.WriteLine("    dotnet run -- validate  <file.resx|file.xlsx>");
        Console.WriteLine();
        Console.WriteLine("  OPTIONS");
        Console.WriteLine("    [culture] / -c / --culture <code >  Filter by culture (e.g. tr, en, de, tr-TR).");
        Console.WriteLine("                                        Omit to process all files.");
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
        Console.WriteLine("    # Single file export / import");
        Console.WriteLine("    dotnet run -- export    Resources.tr.resx   Translations_TR.xlsx");
        Console.WriteLine("    dotnet run -- import    Translations_DE.xlsx Resources.de.resx");
        Console.WriteLine();
        Console.WriteLine("    # Folder / Directory export (preserves subfolder hierarchy)");
        Console.WriteLine("    dotnet run -- export    Resources/Views     ExportedExcel/Views      tr");
        Console.WriteLine("    dotnet run -- export    Resources           ExportedExcel            TR");
        Console.WriteLine("    dotnet run -- export    Resources           ExportedExcel            --culture en");
        Console.WriteLine();
        Console.WriteLine("    # Folder / Directory import");
        Console.WriteLine("    dotnet run -- import    ExportedExcel       Resources                TR");
        Console.WriteLine();
        Console.WriteLine("    # Validation");
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

        string inputPath  = ConsoleHelper.Prompt("Input RESX file or folder (e.g. Resources or Resources.tr.resx)");
        string cleanInput = inputPath.Trim('"', '\'', ' ');

        string? culture = null;
        bool isDir = Directory.Exists(cleanInput) ||
                     (!File.Exists(cleanInput) && !cleanInput.EndsWith(".resx", StringComparison.OrdinalIgnoreCase));

        if (isDir)
        {
            string cultureInput = ConsoleHelper.Prompt("Culture filter (optional, e.g. TR, en, or press [Enter] for all)");
            if (!string.IsNullOrWhiteSpace(cultureInput))
                culture = cultureInput;
        }

        string outputPath = ConsoleHelper.Prompt(isDir
            ? "Output folder path (e.g. ExportedExcel)"
            : "Output XLSX file path (e.g. Translations_TR.xlsx)");

        Console.WriteLine();
        ExportCommand.Run(inputPath, outputPath, culture);
        ConsoleHelper.WaitForEnter();
    }

    static void RunInteractiveImport()
    {
        ConsoleHelper.PrintBanner();
        ConsoleHelper.PrintSection("EXCEL  →  RESX  IMPORT");
        Console.WriteLine();

        string inputPath  = ConsoleHelper.Prompt("Input Excel file or folder (e.g. ExportedExcel or Translations_DE.xlsx)");
        string cleanInput = inputPath.Trim('"', '\'', ' ');

        string? culture = null;
        bool isDir = Directory.Exists(cleanInput) ||
                     (!File.Exists(cleanInput) &&
                      !cleanInput.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) &&
                      !cleanInput.EndsWith(".xls", StringComparison.OrdinalIgnoreCase));

        if (isDir)
        {
            string cultureInput = ConsoleHelper.Prompt("Culture filter (optional, e.g. TR, en, or press [Enter] for all)");
            if (!string.IsNullOrWhiteSpace(cultureInput))
                culture = cultureInput;
        }

        string outputPath = ConsoleHelper.Prompt(isDir
            ? "Output folder path (e.g. Resources)"
            : "Output RESX file path (e.g. Resources.de.resx)");

        Console.WriteLine();
        ImportCommand.Run(inputPath, outputPath, culture);
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
