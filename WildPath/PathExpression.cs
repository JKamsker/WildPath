using WildPath.Abstractions;

namespace WildPath;

public sealed class PathExpression
{
    private readonly string[] _segments;
    private readonly PathExpressionSegment[]? _interpretedSegments;
    private CachedSegment? _cache;

    private PathExpression(
        string rawPath,
        string[] segments,
        PathExpressionSegment[]? interpretedSegments = null)
    {
        RawPath = rawPath;
        _segments = segments;
        _interpretedSegments = interpretedSegments;
    }

    internal string RawPath { get; }

    public static PathExpression Create(string rawPath, string[] segments)
    {
        if (rawPath is null)
        {
            throw new ArgumentNullException(nameof(rawPath));
        }

        if (segments is null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        return new PathExpression(rawPath, segments);
    }

    public static PathExpression Create(string rawPath, PathExpressionSegment[] segments)
    {
        if (rawPath is null)
        {
            throw new ArgumentNullException(nameof(rawPath));
        }

        if (segments is null)
        {
            throw new ArgumentNullException(nameof(segments));
        }

        return new PathExpression(rawPath, ToRawSegments(segments), segments);
    }

    public static PathExpression Parse(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        return new PathExpression(path, Split(path));
    }

    internal static string[] Split(string path)
    {
        return path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
    }

    internal PathEvaluatorSegment? GetOrCreate(IFileSystem fileSystem, IStrategyFactory strategyFactory)
    {
        var cache = _cache;
        if (cache is not null && ReferenceEquals(cache.StrategyFactory, strategyFactory))
        {
            return cache.Segment;
        }

        var segment = CanUseInterpretedSegments(strategyFactory)
            ? PathEvaluatorSegment.FromExpressions(_interpretedSegments!, fileSystem)
            : PathEvaluatorSegment.FromExpressions(_segments, fileSystem, strategyFactory);
        _cache = new CachedSegment(strategyFactory, segment);
        return segment;
    }

    private bool CanUseInterpretedSegments(IStrategyFactory strategyFactory)
    {
        return _interpretedSegments is not null &&
               strategyFactory is Strategies.CompositeStrategyFactory { IsDefault: true };
    }

    private static string[] ToRawSegments(PathExpressionSegment[] segments)
    {
        var rawSegments = new string[segments.Length];
        for (var index = 0; index < segments.Length; index++)
        {
            rawSegments[index] = segments[index].RawValue;
        }

        return rawSegments;
    }

    private sealed class CachedSegment
    {
        public CachedSegment(IStrategyFactory strategyFactory, PathEvaluatorSegment? segment)
        {
            StrategyFactory = strategyFactory;
            Segment = segment;
        }

        public IStrategyFactory StrategyFactory { get; }

        public PathEvaluatorSegment? Segment { get; }
    }
}
