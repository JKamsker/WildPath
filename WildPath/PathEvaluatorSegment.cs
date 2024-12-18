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

    // public bool Matches(string path) => _strategy.Matches(path);

    public IEnumerable<string> Evaluate(string currentDirectory, CancellationToken token = default)
    {
        return _strategy.Evaluate(currentDirectory, _child, token);
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
        foreach (var (element, i) in path.Reverse().Select((x, i) => (x, i)))
        {
            var isFirst = i == path.Length - 1;
            currentSegment = new PathEvaluatorSegment
            (
                segment: element, 
                child: currentSegment, 
                fileSystem: fileSystem,
                isFirst: isFirst, 
                strategyFactory: strategyFactory
            );
        }

        return currentSegment;
    }
}