using System.Text;

namespace WildPath.LiveInput.Utils;

internal static class StringBuilderExtensions
{
    // Copy
    public static StringBuilder Copy(this StringBuilder sb)
    {
        var copy = new StringBuilder(sb.Length);
        copy.Append(sb);
        return copy;
    }
}
