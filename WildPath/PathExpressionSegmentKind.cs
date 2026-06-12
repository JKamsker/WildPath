namespace WildPath;

public enum PathExpressionSegmentKind
{
    Runtime = 0,
    Exact,
    Parent,
    Parents,
    Any,
    AnyRecursive,
    Tagged,
    SimpleWildcardStartsWith,
    SimpleWildcardEndsWith,
    SimpleWildcardContains,
    SimpleWildcardStartsAndEnds,
    WildcardPattern
}
