using System.Text.RegularExpressions;

namespace Application.Common.Helpers;

public static class FolderNameHelper
{
    public static string ToSafeFolderName(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "unnamed-book";

        string safe = title.ToLowerInvariant()
            .Replace("ı", "i").Replace("ğ", "g").Replace("ü", "u")
            .Replace("ş", "s").Replace("ö", "o").Replace("ç", "c");

        safe = Regex.Replace(safe, @"[^a-z0-9\s-]", "");
        safe = Regex.Replace(safe, @"\s+", " ").Trim().Replace(" ", "-");

        return string.IsNullOrWhiteSpace(safe) ? "unnamed-book" : safe;
    }
}