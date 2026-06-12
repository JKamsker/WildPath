using WildPath.Abstractions;
using WildPath.Extensions;
using WildPath.Internals;
using WildPath.Strategies;

namespace WildPath;

internal class PathEvaluatorSegment : IPathEvaluatorSegment
{
    private readonly ISegmentStrategy _strategy;
    private readonly PathEvaluatorSegment? _child;
    private readonly IFileSystem _fileSystem;

    public string RawToken { get; }

    public bool IsFirst { get; }

    private PathEvaluatorSegment(
        string segment,
        PathEvaluatorSegment? child,
        IFileSystem fileSystem,
        bool isFirst,
        IStrategyFactory? strategyFactory = null
    )
    {
        _child = child;
        _fileSystem = fileSystem;

        RawToken = segment;
        IsFirst = isFirst;

        _strategy = (strategyFactory ?? StrategyFactory.Default)
            .Create(segment)
            .Initialize(this);
    }

    private PathEvaluatorSegment(
        PathExpressionSegment segment,
        PathEvaluatorSegment? child,
        IFileSystem fileSystem,
        bool isFirst
    )
    {
        _child = child;
        _fileSystem = fileSystem;

        RawToken = segment.RawValue;
        IsFirst = isFirst;

        _strategy = CreateInterpretedStrategy(segment, fileSystem).Initialize(this);
    }

    // public bool Matches(string path) => _strategy.Matches(path);

    public IEnumerable<string> Evaluate(string currentDirectory, CancellationToken token = default)
    {
        return _strategy.Evaluate(currentDirectory, _child, token);
    }

    public string? EvaluateFirst(string currentDirectory, CancellationToken token = default)
    {
        if (_strategy is SegmentStrategyBase strategy)
        {
            return strategy.EvaluateFirst(currentDirectory, _child, token);
        }

        return _strategy.Evaluate(currentDirectory, _child, token).FirstOrDefault();
    }

    public IEnumerable<PathEvaluatorSegment> EnumerateChildren()
    {
        var current = this;
        while (current != null)
        {
            yield return current;
            current = current._child;
        }
    }

    public static PathEvaluatorSegment? FromExpressions(
        string[] path,
        IFileSystem? fileSystem = null,
        IStrategyFactory? strategyFactory = null
    )
    {
        fileSystem ??= RealFileSystem.Instance;
        strategyFactory ??= new CompositeStrategyFactory(
            new StrategyFactory(fileSystem),
            new DefaultStrategyFactory(fileSystem)
        );

        PathEvaluatorSegment? currentSegment = null;
        for (var index = path.Length - 1; index >= 0; index--)
        {
            var isFirst = index == 0;
            currentSegment = new PathEvaluatorSegment
            (
                segment: path[index], 
                child: currentSegment, 
                fileSystem: fileSystem,
                isFirst: isFirst, 
                strategyFactory: strategyFactory
            );
        }

        return currentSegment;
    }

    public static PathEvaluatorSegment? FromExpressions(
        PathExpressionSegment[] path,
        IFileSystem? fileSystem = null
    )
    {
        fileSystem ??= RealFileSystem.Instance;

        PathEvaluatorSegment? currentSegment = null;
        for (var index = path.Length - 1; index >= 0; index--)
        {
            var isFirst = index == 0;
            currentSegment = new PathEvaluatorSegment
            (
                segment: path[index],
                child: currentSegment,
                fileSystem: fileSystem,
                isFirst: isFirst
            );
        }

        return currentSegment;
    }

    private static ISegmentStrategy CreateInterpretedStrategy(
        PathExpressionSegment segment,
        IFileSystem fileSystem)
    {
        return segment.Kind switch
        {
            PathExpressionSegmentKind.Parent => new ParentSegmentStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.Parents => new ParentsSegmentStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.Any => new AnySegmentStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.AnyRecursive => new AnySegmentRecursivelyStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.Tagged => new TaggedSegmentStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.SimpleWildcardStartsWith => SimpleWildcardStrategy.CreateInterpreted(segment, fileSystem),
            PathExpressionSegmentKind.SimpleWildcardEndsWith => SimpleWildcardStrategy.CreateInterpreted(segment, fileSystem),
            PathExpressionSegmentKind.SimpleWildcardContains => SimpleWildcardStrategy.CreateInterpreted(segment, fileSystem),
            PathExpressionSegmentKind.SimpleWildcardStartsAndEnds => SimpleWildcardStrategy.CreateInterpreted(segment, fileSystem),
            PathExpressionSegmentKind.WildcardPattern => new WildcardSegmentStrategy(segment.Value, fileSystem),
            PathExpressionSegmentKind.Exact => new ExactMatchSegmentStrategy(segment.Value, fileSystem),
            _ => CompositeStrategyFactory.CreateDefault(fileSystem).Create(segment.RawValue)
        };
    }
}
