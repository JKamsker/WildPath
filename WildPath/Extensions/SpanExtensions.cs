namespace WildPath.Extensions;

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

    public static ReadOnlySpan<T> CutUntil<T>(this ref ReadOnlySpan<T> input, T separator, T escapeChar)
        where T : IEquatable<T>
    {
        if (input.IsEmpty)
        {
            return ReadOnlySpan<T>.Empty;
        }

        var index = input.IndexOfUnescaped(separator, escapeChar);
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

    // ReadOnlySpan<char> contains
    public static bool Contains(this ReadOnlySpan<char> span, char value)
    {
        return span.IndexOf(value) != -1;
    }

    // Similar to IndexOf, but ignores escape characters
    public static int IndexOfUnescaped<T>(this ReadOnlySpan<T> span, T value, T escapeChar)
        where T : IEquatable<T>
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i].Equals(escapeChar))
            {
                i++;
                continue;
            }

            if (span[i].Equals(value))
            {
                return i;
            }
        }

        return -1;
    }
}