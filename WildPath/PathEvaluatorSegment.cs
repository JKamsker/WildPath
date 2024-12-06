using System.ComponentModel.DataAnnotations;
using PathResolver.Strategies;

namespace PathResolver;

public class PathEvaluatorSegment
{
    private readonly ISegmentStrategy _strategy;
    private readonly PathEvaluatorSegment? _child;
    private readonly IFileSystem _fileSystem;

    public PathEvaluatorSegment(string segment, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        _child = child;
        _fileSystem = fileSystem;

        _strategy = new StrategyFactory(fileSystem).Create(segment);
    }
    
    public bool Matches(string path) => _strategy.Matches(path);

    public IEnumerable<string> Evaluate(string currentDirectory)
        => _strategy.Evaluate(currentDirectory, _child);

    public static PathEvaluatorSegment? FromExpressions(string[] path, IFileSystem fileSystem)
    {
        PathEvaluatorSegment? currentSegment = null;
        foreach (var element in path.Reverse())
        {
            currentSegment = new PathEvaluatorSegment(element, currentSegment, fileSystem);
        }

        return currentSegment;
    }
}