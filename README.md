# WAGONN.DotNet.ResX.Toolkit

> **RESX ↔ Excel Localization Toolkit** — A professional CLI and interactive TUI toolkit by [WAGONN](https://wagonn.net) for converting .NET RESX resource files to Excel workbooks, importing them back, and validating HTML tag integrity across both formats.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![Release & Publish](https://github.com/WAGONN/dotnet-resx-toolkit/actions/workflows/release.yml/badge.svg)](https://github.com/WAGONN/dotnet-resx-toolkit/actions/workflows/release.yml)
[![GitHub Repo](https://img.shields.io/badge/GitHub-dotnet--resx--toolkit-181717.svg?logo=github)](https://github.com/WAGONN/dotnet-resx-toolkit)

Built for teams who send resource files to translation bureaus via Excel and need to reimport them cleanly as valid RESX files — with built-in validation to catch broken markup before it ships.

---

## Features

| Feature | Description |
|---|---|
| **RESX → Excel** | Export all keys, values, and comments into a structured `.xlsx` workbook |
| **Excel → RESX** | Import a translated workbook back into a standards-compliant `.resx` file |
| **HTML Validation** | Scan any `.resx` or `.xlsx` for broken HTML tags — mismatched pairs, orphan closes, unclosed opens — with exact row numbers and keys |
| **Flicker-free TUI** | Arrow-key navigable terminal menu; banner renders once, only changed lines repaint |
| **CLI mode** | Pipe-friendly, scriptable; distinct exit codes for CI/CD integration |
| **Safe output** | Destination directories are auto-created if they don't exist |
| **.NET 10 · Cross-platform** | Runs natively on macOS, Linux, and Windows |

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Installation & Quick Start

Clone the repository and run:

```sh
git clone https://github.com/WAGONN/dotnet-resx-toolkit.git
cd dotnet-resx-toolkit
dotnet run
```

---

## Usage

### 1. Interactive Mode (TUI)

Launch the interactive arrow-key menu:

```sh
dotnet run
# or
dotnet run --project WAGONN.DotNet.ResX.Toolkit
```

Use `↑` / `↓` to navigate, `Enter` to confirm, `q` to quit.

### 2. CLI Mode

```sh
# Export RESX → Excel
dotnet run -- export <input.resx> <output.xlsx>

# Import Excel → RESX
dotnet run -- import <input.xlsx> <output.resx>

# Validate HTML tags in a RESX or Excel file
dotnet run -- validate <file.resx|file.xlsx>

# Help
dotnet run -- --help
```

**Aliases:** `export` = `resx-to-excel` · `import` = `excel-to-resx` · `validate` = `check`

### Examples

```sh
dotnet run -- export    Resources.tr.resx    Translations_TR.xlsx
dotnet run -- import    Translations_DE.xlsx Resources.de.resx
dotnet run -- validate  Translations_DE.xlsx
dotnet run -- validate  Resources.tr.resx
```

---

## Exit Codes

| Code | Meaning |
|------|---------|
| `0` | Success — operation completed / no issues found |
| `1` | File not found or parse error |
| `2` | Validation completed with at least one **error** |

---

## Excel Format

The exported workbook contains three columns:

| Key | Value | Comment |
|-----|-------|---------|
| `Greeting` | `Hello <b>World</b>` | `Used on homepage` |
| `Farewell` | `Goodbye` | |

All three columns are preserved on re-import. Rows with an empty `Key` are skipped.

---

## HTML Validation

The validator runs a stack-based analysis on every entry value and detects four categories of issues:

| Severity | Type | Example |
|----------|------|---------|
| `ERROR` | **Orphan closing tag** | `text</i> more` — no matching open |
| `ERROR` | **Mismatched pair** | `<b>text</i>` — wrong tag name |
| `ERROR` | **Misordered nesting** | `<b><i>text</b></i>` — crossing pairs |
| `WARNING` | **Unclosed opening tag** | `<span class="x">text` — never closed |

Self-closing and void elements (`<br/>`, `<hr/>`, `<img>`, `<input>`, etc.) are correctly ignored.

### Sample Output

```
  [ERROR]  Row 4     │  Mismatch
           │  Misordered tags: </b> closes a tag from position 0, but <i> opened at position 8 is still open in between.
           │  Value: <b>Bold <i>italic</b></i>

  [WARN]   Row 8     │  UnclosedSpan
           │  Unclosed opening tag <span> at position 7 — no corresponding </span> found.
           │  Value: Price: <span class="price">$99.99 (tax included)

──────────────────────────────────────────────────────────────────────

[ERROR] Validation complete — 3 error(s), 2 warning(s) across 5 issue(s) in 8 entries.
```

---

## Project Structure

```
dotnet-resx-toolkit/
├── WAGONN.DotNet.ResX.Toolkit.csproj  # Project & package definition
├── Program.cs                         # Entry point — interactive TUI + CLI dispatch
├── Models/
│   ├── ResxEntry.cs                   # Resource entry (Key / Value / Comment)
│   └── ValidationIssue.cs            # Validation finding (RowNumber, Key, Detail, Severity)
├── Commands/
│   ├── ExportCommand.cs               # RESX → Excel
│   ├── ImportCommand.cs               # Excel → RESX
│   └── ValidateCommand.cs             # HTML tag validation for RESX and Excel
├── Services/
│   └── HtmlTagValidator.cs            # Stack-based HTML tag parser
└── UI/
    └── ConsoleHelper.cs               # TUI: banner, flicker-free arrow-key menu, styled output
```

---

## License

MIT © [WAGONN](https://wagonn.net)

