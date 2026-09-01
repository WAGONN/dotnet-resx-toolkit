using System.Text.RegularExpressions;
using WAGONN.DotNet.ResX.Toolkit.Models;

namespace WAGONN.DotNet.ResX.Toolkit.Services;

public static class HtmlTagValidator
{
    private static readonly HashSet<string> VoidElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "area", "base", "br", "col", "embed", "hr", "img", "input",
        "link", "meta", "param", "source", "track", "wbr"
    };

    private static readonly Regex TagPattern = new(
        @"<(/?)([A-Za-z][A-Za-z0-9]*)(?:\s[^>]*)?(/?)\s*>",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex CommentPattern = new(
        @"<!--.*?-->",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public static IReadOnlyList<ValidationIssue> Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        var issues = new List<ValidationIssue>();
        string cleaned = CommentPattern.Replace(value, string.Empty);
        var stack = new Stack<(string Tag, int Position)>();

        foreach (Match m in TagPattern.Matches(cleaned))
        {
            bool isClose     = m.Groups[1].Value == "/";
            bool isSelfClose = m.Groups[3].Value == "/";
            string tag       = m.Groups[2].Value.ToLowerInvariant();
            int pos          = m.Index;

            if (isSelfClose || VoidElements.Contains(tag))
                continue;

            if (isClose)
            {
                if (stack.Count == 0)
                {
                    issues.Add(MakeIssue(value,
                        $"Orphan closing tag </{tag}> at position {pos} — no matching opening tag exists.",
                        IssueSeverity.Error));
                }
                else
                {
                    var (top, topPos) = stack.Peek();
                    if (top == tag)
                    {
                        stack.Pop();
                    }
                    else
                    {
                        var deeper = stack.FirstOrDefault(s => s.Tag == tag);
                        if (deeper.Tag != null)
                        {
                            issues.Add(MakeIssue(value,
                                $"Misordered tags: </{tag}> closes a tag from position {deeper.Position}, " +
                                $"but <{top}> opened at position {topPos} is still open in between.",
                                IssueSeverity.Error));

                            while (stack.Count > 0 && stack.Peek().Tag != tag)
                                stack.Pop();
                            if (stack.Count > 0) stack.Pop();
                        }
                        else
                        {
                            issues.Add(MakeIssue(value,
                                $"Mismatched closing tag </{tag}> at position {pos} — " +
                                $"expected </{top}> to close the tag opened at position {topPos}.",
                                IssueSeverity.Error));
                        }
                    }
                }
            }
            else
            {
                stack.Push((tag, pos));
            }
        }

        foreach (var (tag, pos) in stack)
        {
            issues.Add(MakeIssue(value,
                $"Unclosed opening tag <{tag}> at position {pos} — no corresponding </{tag}> found.",
                IssueSeverity.Warning));
        }

        return issues;
    }

    private static ValidationIssue MakeIssue(string value, string detail, IssueSeverity severity) =>
        new() { Value = value, Detail = detail, Severity = severity };
}
