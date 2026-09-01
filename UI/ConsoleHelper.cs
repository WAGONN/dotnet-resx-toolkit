using WAGONN.DotNet.ResX.Toolkit.Models;

namespace WAGONN.DotNet.ResX.Toolkit.UI;

public static class ConsoleHelper
{
    private const string Primary   = "\x1b[38;5;37m";
    private const string Secondary = "\x1b[38;5;66m";
    private const string Success   = "\x1b[38;5;71m";
    private const string Warning   = "\x1b[38;5;178m";
    private const string Error     = "\x1b[38;5;124m";
    private const string Muted     = "\x1b[38;5;243m";
    private const string Highlight = "\x1b[1;37m";
    private const string TitleFg   = "\x1b[1;38;5;37m";
    private const string Border    = "\x1b[38;5;66m";
    private const string VerFg     = "\x1b[38;5;36m";
    private const string SectionFg = "\x1b[1;38;5;66m";
    private const string InfoFg    = "\x1b[38;5;109m";
    private const string Reset     = "\x1b[0m";

    private const string AppVersion  = "1.0.0";
    private const string AppTitle    = "DOTNET RESX TOOLKIT";
    private const string AppSubtitle = "RESX ↔ Excel Localization Toolkit";

    private const string BoxBorder = "──────────────────────────────────────────────────────────────────────";

    public static void PrintBanner(string? sdkVersion = null)
    {
        Console.Clear();
        string sdk = sdkVersion ?? GetDotnetVersion();

        Emit($"{Border}┌{BoxBorder}┐{Reset}");
        Emit($"{Border}│{Reset}  {TitleFg}{"WAGONN  /  " + AppTitle,-59}{Reset} {VerFg}v{AppVersion}{Border}  │{Reset}");
        Emit($"{Border}├{BoxBorder}┤{Reset}");
        EmitRow("Toolkit", AppSubtitle, 24, 41);
        EmitRow(".NET SDK", sdk, 24, 41);
        Emit($"{Border}└{BoxBorder}┘{Reset}");
        Console.WriteLine();
    }

    private static void EmitRow(string label, string value, int lw, int vw) =>
        Emit($"{Border}│{Reset}  {Muted}{label,-24}{Reset} : {Highlight}{value,-41}{Border}│{Reset}");

    public static void PrintHeader(string title)
    {
        Emit($"{Primary}# {title}{Reset}");
        Emit($"{Muted}{BoxBorder}{Reset}");
    }

    public static void PrintSeparator() => Emit($"{Muted}{BoxBorder}{Reset}");

    public static void PrintSection(string label) => Emit($"{SectionFg}  [ {label} ]{Reset}");

    public static int ArrowMenu(
        string title,
        IReadOnlyList<(string Label, string Desc)> items,
        string? sdkVersion = null)
    {
        int selected = 0;
        int count    = items.Count;

        HideCursor();
        try
        {
            PrintBanner(sdkVersion);
            PrintHeader(title);
            Console.WriteLine();

            int menuTop = Console.CursorTop;
            DrawMenuRegion(items, selected, menuTop);

            while (true)
            {
                var key = ReadKey();

                int next = key switch
                {
                    Key.Up    => (selected - 1 + count) % count,
                    Key.Down  => (selected + 1) % count,
                    _         => selected
                };

                if (key == Key.Enter) return selected;
                if (key == Key.Quit)  return -1;

                if (next != selected)
                {
                    selected = next;
                    Console.SetCursorPosition(0, menuTop);
                    DrawMenuRegion(items, selected, menuTop);
                }
            }
        }
        finally
        {
            ShowCursor();
        }
    }

    private static void DrawMenuRegion(
        IReadOnlyList<(string Label, string Desc)> items,
        int selected,
        int menuTop)
    {
        Console.SetCursorPosition(0, menuTop);

        for (int i = 0; i < items.Count; i++)
        {
            if (i == selected)
                Console.WriteLine($"\x1b[2K  \x1b[1;38;5;30m▸ {Highlight}{items[i].Label}{Reset}");
            else
                Console.WriteLine($"\x1b[2K    {Muted}{items[i].Label}{Reset}");
        }

        Console.WriteLine($"\x1b[2K");
        Console.WriteLine($"\x1b[2K{Muted}{BoxBorder}{Reset}");
        Console.WriteLine($"\x1b[2K  {InfoFg}ℹ  {items[selected].Desc}{Reset}");
        Console.WriteLine($"\x1b[2K{Muted}{BoxBorder}{Reset}");
        Console.WriteLine($"\x1b[2K  {Muted}[↑/↓] Navigate    [Enter] Confirm    [q] Quit{Reset}");
        Console.Write($"\x1b[2K");
    }

    public static void WriteSuccess(string message)
    {
        Console.WriteLine();
        Emit($"{Success}[SUCCESS] {message}{Reset}");
    }

    public static void WriteError(string message)
    {
        Console.WriteLine();
        Emit($"{Error}[ERROR] {message}{Reset}");
    }

    public static void WriteWarning(string message)
    {
        Console.WriteLine();
        Emit($"{Warning}[WARNING] {message}{Reset}");
    }

    public static void WriteInfo(string label, string value) =>
        Emit($"  {Muted}{label}:{Reset} {Highlight}{value}{Reset}");

    public static void WriteDim(string message) => Emit($"{Muted}{message}{Reset}");

    public static string Prompt(string label)
    {
        Console.Write($"  {Primary}{label}:{Reset} ");
        return Console.ReadLine()?.Trim('"', '\'', ' ') ?? string.Empty;
    }

    public static bool Confirm(string question)
    {
        Console.Write($"  {Warning}{question} (y/N):{Reset} ");
        var input = Console.ReadLine()?.Trim();
        return input is "y" or "Y";
    }

    public static void WaitForEnter()
    {
        Console.WriteLine();
        Console.Write($"  {Muted}Press [Enter] to continue...{Reset}");
        Console.ReadLine();
    }

    public static void WriteIssue(ValidationIssue issue)
    {
        string badge = issue.Severity == IssueSeverity.Error
            ? $"{Error}[ERROR]  {Reset}"
            : $"{Warning}[WARN]   {Reset}";

        string severityColor = issue.Severity == IssueSeverity.Error ? Error : Warning;

        Emit($"  {badge}{Muted}Row {issue.RowNumber,-5}{Reset} {Secondary}│{Reset}  {Highlight}{issue.Key}{Reset}");
        Emit($"           {Secondary}│{Reset}  {severityColor}{issue.Detail}{Reset}");

        const int maxLen = 120;
        string preview = issue.Value.Length > maxLen
            ? issue.Value[..maxLen] + " …"
            : issue.Value;

        preview = preview.Replace("\r\n", " ↵ ").Replace("\n", " ↵ ").Replace("\r", " ↵ ");

        Emit($"           {Secondary}│{Reset}  {Muted}Value: {preview}{Reset}");
        Console.WriteLine();
    }

    private static void Emit(string line) => Console.WriteLine(line);

    private static string GetDotnetVersion()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("dotnet", "--version")
            {
                RedirectStandardOutput = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            return proc?.StandardOutput.ReadLine()?.Trim() ?? "N/A";
        }
        catch { return "N/A"; }
    }

    private static void HideCursor()
    {
        try { Console.CursorVisible = false; } catch { }
    }

    private static void ShowCursor()
    {
        try { Console.CursorVisible = true; } catch { }
    }

    private enum Key { Up, Down, Enter, Quit, Other }

    private static Key ReadKey()
    {
        var k = Console.ReadKey(intercept: true);

        if (k.Key == ConsoleKey.UpArrow)    return Key.Up;
        if (k.Key == ConsoleKey.DownArrow)  return Key.Down;
        if (k.Key == ConsoleKey.Enter)      return Key.Enter;
        if (k.KeyChar is 'q' or 'Q')        return Key.Quit;

        if (k.Key == ConsoleKey.Escape)
        {
            if (Console.KeyAvailable)
            {
                var k2 = Console.ReadKey(intercept: true);
                if (Console.KeyAvailable)
                {
                    var k3 = Console.ReadKey(intercept: true);
                    if (k2.KeyChar == '[')
                        return k3.KeyChar == 'A' ? Key.Up : k3.KeyChar == 'B' ? Key.Down : Key.Other;
                }
            }
            return Key.Quit;
        }

        return Key.Other;
    }
}
