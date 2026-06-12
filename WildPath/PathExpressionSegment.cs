namespace WildPath;

public readonly struct PathExpressionSegment
{
    public PathExpressionSegment(PathExpressionSegmentKind kind, string value)
        : this(kind, value, value)
    {
    }

    public PathExpressionSegment(PathExpressionSegmentKind kind, string rawValue, string value)
        : this(kind, rawValue, value, string.Empty)
    {
    }

    public PathExpressionSegment(
        PathExpressionSegmentKind kind,
        string rawValue,
        string value,
        string secondValue)
    {
        Kind = kind;
        RawValue = rawValue;
        Value = value;
        SecondValue = secondValue;
    }

    public PathExpressionSegmentKind Kind { get; }

    public string RawValue { get; }

    public string Value { get; }

    public string SecondValue { get; }
}
