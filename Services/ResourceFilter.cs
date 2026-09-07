namespace WAGONN.DotNet.ResX.Toolkit.Services;

public static class ResourceFilter
{
    /// <summary>
    /// Checks whether a given file path matches the specified culture filter.
    /// Supports .resx and .xlsx localization file conventions (e.g. Index.tr.resx, Index.cshtml.tr.resx, Index.tr-TR.resx).
    /// If culture is null, empty, "*", or "all", all files match.
    /// </summary>
    public static bool MatchesCulture(string filePath, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture) || culture.Trim() is "*" or "all" or "ALL")
            return true;

        culture = culture.Trim().TrimStart('.');
        string fileName = Path.GetFileName(filePath);
        string withoutExt = Path.GetFileNameWithoutExtension(fileName);

        // e.g. for "Index.tr.resx" -> withoutExt is "Index.tr" -> cultureExt is "tr"
        // for "Index.cshtml.tr.resx" -> withoutExt is "Index.cshtml.tr" -> cultureExt is "tr"
        // for "Index.tr-TR.resx" -> withoutExt is "Index.tr-TR" -> cultureExt is "tr-TR"
        // for "tr.resx" -> withoutExt is "tr" -> cultureExt is ""
        string cultureExt = Path.GetExtension(withoutExt).TrimStart('.');

        if (string.IsNullOrEmpty(cultureExt))
        {
            return string.Equals(withoutExt, culture, StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(cultureExt, culture, StringComparison.OrdinalIgnoreCase))
            return true;

        // If user specified "tr", also match "tr-TR", "tr-CY", etc.
        if (cultureExt.StartsWith(culture + "-", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
