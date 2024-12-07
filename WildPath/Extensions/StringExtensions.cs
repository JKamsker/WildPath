namespace WildPath.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Gets a substring between two strings. The first on is at the start and the second one is at the end.
    /// </summary>
    public static bool TryTrimStartAndEnd(this string input, string start, string end, out string result)
    {
        if (string.IsNullOrEmpty(input))
        {
            result = string.Empty;
            return false;
        }

        if (input.StartsWith(start) && input.EndsWith(end))
        {
            // result = input.Substring(start.Length, input.Length - start.Length - end.Length);
            result = input[start.Length..^end.Length];
            return true;
        }

        result = string.Empty;
        return false;
    }
}