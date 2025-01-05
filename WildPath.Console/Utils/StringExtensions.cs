namespace WildPath.Console.Utils;

internal static class StringExtensions
{
    public static int CountExcept(this string str, params ReadOnlySpan<char> exceptions)
    {
        var count = 0;
        foreach (var c in str)
        {
            if (exceptions.Contains(c))
            {
                continue;
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// Counts the number of characters in the string excluding the specified exceptions.
    /// </summary>
    /// <param name="str">The source string.</param>
    /// <param name="exceptions">An array of substrings to exclude from the count.</param>
    /// <returns>The count of characters excluding the exceptions.</returns>
    public static int CountExcept(this string str, params ReadOnlySpan<string> exceptions)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (exceptions == null || exceptions.Length == 0)
            return str.Length; // No exceptions, count everything.

        int count = 0;
        int i = 0;

        while (i < str.Length)
        {
            bool matched = false;

            foreach (var exception in exceptions)
            {
                if 
                (
                    exception.Length > 0 
                    && i + exception.Length <= str.Length 
                    && str.AsSpan(i, exception.Length).SequenceEqual(exception))
                {
                    i += exception.Length; // Skip over the exception
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                count++;
                i++;
            }
        }

        return count;
    }
}
