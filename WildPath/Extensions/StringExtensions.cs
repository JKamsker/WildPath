using System.Runtime.CompilerServices;
using System.Text;

namespace WildPath.Extensions;

internal static class StringExtensions
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

#if NET48 || NETSTANDARD2_0

    public static bool Contains(this string source, string value, StringComparison comparisonType)
    {
        return source.IndexOf(value, comparisonType) >= 0;
    }

    // EndsWith and StartsWith
    public static bool EndsWith(this string source, char value)
    {
        return source[^1] == value;
    }

    public static bool StartsWith(this string source, char value)
    {
        return source[0] == value;
    }

    public static unsafe string ConvertToString(this ReadOnlySpan<char> memory)
    {
        if (memory.IsEmpty)
        {
            return string.Empty;
        }

        fixed (char* ptr = memory)
        {
            return new string(ptr, 0, memory.Length);
        }
    }

    public static string Join(this string[] values, char separator)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }
        if (values.Length == 0)
        {
            return string.Empty;
        }

        var result = new StringBuilder();
        result.Append(values[0]);
        for (int i = 1; i < values.Length; i++)
        {
            result.Append(separator);
            result.Append(values[i]);
        }
        return result.ToString();
    }

    // Splits a string into a maximum number of substrings based on a specified delimiting string and, optionally, options.
    public static string[] Split(this string source, char separator, int count, StringSplitOptions options = StringSplitOptions.None)
    {
        return source.Split(new[] { separator }, count, options);
    }

#else

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ConvertToString(this ReadOnlySpan<char> memory)
    {
        return new string(memory);
    }

    public static string Join(this string[] values, char separator)
    {
        return string.Join(separator, values);
    }

#endif
}