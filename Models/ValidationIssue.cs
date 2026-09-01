namespace WAGONN.DotNet.ResX.Toolkit.Models;

public enum IssueSeverity
{
    Error,
    Warning,
}

public sealed record ValidationIssue
{
    public int RowNumber { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public IssueSeverity Severity { get; init; }
}
