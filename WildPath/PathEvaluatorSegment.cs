using WildPath.Abstractions;
using WildPath.Extensions;
using WildPath.Strategies;

namespace WildPath;

internal class PathEvaluatorSegment : IPathEvaluatorSegment
{
    private readonly ISegmentStrategy _strategy;
    private readonly PathEvaluatorSegment? _child;
    private readonly IFileSystem _fileSystem;

    public string RawToken { get; }

    public bool IsFirst { get; }

    private PathEvaluatorSegment(string segment, PathEvaluatorSegment? child, IFileSystem fileSystem, bool isFirst)
    {
        _child = child;
        _fileSystem = fileSystem;
        
        RawToken = segment;
        IsFirst = isFirst;

        _strategy = new StrategyFactory(fileSystem)
            .Create(segment)
            .Initialize(this);
    }

    public bool Matches(string path) => _strategy.Matches(path);

    public IEnumerable<string> Evaluate(string currentDirectory)
    {
        return _strategy.Evaluate(currentDirectory, _child);
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

    public static PathEvaluatorSegment? FromExpressions(string[] path, IFileSystem fileSystem)
    {
        PathEvaluatorSegment? currentSegment = null;
        foreach (var (element, i) in path.Reverse().Select((x, i) => (x, i)))
        {
            var isFirst = i == path.Length - 1;
            currentSegment = new PathEvaluatorSegment(element, currentSegment, fileSystem, isFirst);
        }

        return currentSegment;
    }
}