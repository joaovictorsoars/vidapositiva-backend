using System.Text.RegularExpressions;

namespace VidaPositiva.Api.Extensions.String;

public static class StringExtensions
{
    public static string NormalizeWhitespaces(this string input)
    {
        return string.IsNullOrWhiteSpace(input) ? string.Empty : Regex.Replace(input, @"\s+", " ").Trim();
    }
}