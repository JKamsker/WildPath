namespace WildPath;

internal static class SpanExtensions
{
    /// <summary>
    /// Splits a span by a separator, advances the input to the end of the separator
    /// </summary>
    public static ReadOnlySpan<T> CutUntil<T>(this ref ReadOnlySpan<T> input, T separator)
        where T : IEquatable<T>?
    {
        if (input.IsEmpty)
        {
            return ReadOnlySpan<T>.Empty;
        }

        var index = input.IndexOf(separator);
        if (index == -1)
        {
            var result1 = input;
            input = ReadOnlySpan<T>.Empty;
            return result1;
        }

        var result2 = input.Slice(0, index);
        input = input.Slice(index + 1);
        return result2;
    }

    public static bool StartsWith<T>(this ReadOnlySpan<T> input, T value)
    {
        return !input.IsEmpty && input[0]?.Equals(value) == true;
    }

    public static bool EndsWith<T>(this ReadOnlySpan<T> input, T value)
    {
        return !input.IsEmpty && input[^1]?.Equals(value) == true;
    }
}

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