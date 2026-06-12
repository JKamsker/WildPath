using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace WildPath.SourceGenerator;

internal static class PathSegmentSource
{
    public static string Create(string rawPath)
    {
        var segments = rawPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var builder = new StringBuilder("new global::WildPath.PathExpressionSegment[] { ");

        for (var index = 0; index < segments.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(", ");
            }

            AppendSegment(builder, segments[index]);
        }

        builder.Append(" }");
        return builder.ToString();
    }

    private static void AppendSegment(StringBuilder builder, string segment)
    {
        var kind = GetKind(segment, out var value, out var secondValue);
        builder.Append("new global::WildPath.PathExpressionSegment(")
            .Append("global::WildPath.PathExpressionSegmentKind.")
            .Append(kind)
            .Append(", ")
            .Append(SymbolDisplay.FormatLiteral(segment, true));

        if (!string.Equals(segment, value, StringComparison.Ordinal))
        {
            builder.Append(", ")
                .Append(SymbolDisplay.FormatLiteral(value, true));

            if (secondValue.Length > 0)
            {
                builder.Append(", ")
                    .Append(SymbolDisplay.FormatLiteral(secondValue, true));
            }
        }

        builder.Append(')');
    }

    private static string GetKind(string segment, out string value, out string secondValue)
    {
        value = segment;
        secondValue = string.Empty;
        return segment switch
        {
            ".." => "Parent",
            "..." => "Parents",
            "*" => "Any",
            "**" => "AnyRecursive",
            _ => GetComplexKind(segment, out value, out secondValue)
        };
    }

    private static string GetComplexKind(string segment, out string value, out string secondValue)
    {
        secondValue = string.Empty;
        if (TryGetTaggedMarker(segment, out value))
        {
            return "Tagged";
        }

        if (TryGetSimpleWildcardKind(segment, out var kind, out value, out secondValue))
        {
            return kind;
        }

        value = segment;
        if (segment.Contains("*"))
        {
            return "WildcardPattern";
        }

        return IsRuntimeInterpreted(segment)
            ? "Runtime"
            : "Exact";
    }

    private static bool TryGetSimpleWildcardKind(
        string segment,
        out string kind,
        out string value,
        out string secondValue)
    {
        kind = string.Empty;
        value = string.Empty;
        secondValue = string.Empty;

        var parts = segment.Split('*');
        if (parts.Length is < 2 or > 3 ||
            (parts.Length == 3 && parts[2].Length > 0))
        {
            return false;
        }

        var startsWithWildcard = segment.StartsWith("*", StringComparison.Ordinal);
        var endsWithWildcard = segment.EndsWith("*", StringComparison.Ordinal);
        var partOne = parts[0];
        var partTwo = parts.Length > 1 ? parts[1] : string.Empty;
        var partThree = parts.Length > 2 ? parts[2] : string.Empty;
        var count = (partOne.Length > 0 ? 1 : 0) +
                    (partTwo.Length > 0 ? 1 : 0) +
                    (partThree.Length > 0 ? 1 : 0);

        if (count is not (1 or 2))
        {
            return false;
        }

        if (startsWithWildcard && endsWithWildcard && count == 1)
        {
            kind = "SimpleWildcardContains";
            value = partTwo;
            return true;
        }

        if (count == 1 && startsWithWildcard)
        {
            kind = "SimpleWildcardEndsWith";
            value = partTwo;
            return true;
        }

        if (count == 1 && endsWithWildcard)
        {
            kind = "SimpleWildcardStartsWith";
            value = partOne;
            return true;
        }

        if (count == 2)
        {
            kind = "SimpleWildcardStartsAndEnds";
            value = partOne;
            secondValue = partTwo;
            return partOne.Length > 0 && partTwo.Length > 0;
        }

        return false;
    }

    private static bool IsRuntimeInterpreted(string segment)
    {
        return (segment.StartsWith(":", StringComparison.Ordinal) &&
                segment.EndsWith(":", StringComparison.Ordinal));
    }

    private static bool TryGetTaggedMarker(string segment, out string marker)
    {
        const string start = ":tagged(";
        const string end = "):";

        if (segment.StartsWith(start, StringComparison.Ordinal) &&
            segment.EndsWith(end, StringComparison.Ordinal))
        {
            marker = segment.Substring(start.Length, segment.Length - start.Length - end.Length);
            return true;
        }

        marker = string.Empty;
        return false;
    }
}
