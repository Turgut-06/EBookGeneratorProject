using System.Text.RegularExpressions;
using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class ContactCleanerService : IContactCleanerService
{
    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"(?:\+?90\s?)?(?:\(?0?[0-9]{3}\)?[\s.-]?)?[0-9]{3}[\s.-]?[0-9]{2}[\s.-]?[0-9]{2}",
        RegexOptions.Compiled);

    public string CleanContactInfo(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        var cleaned = EmailRegex.Replace(text, "[EMAIL REMOVED]");
        cleaned = PhoneRegex.Replace(cleaned, "[PHONE REMOVED]");
        return cleaned;
    }
}