
namespace WildPath.Abstractions;

public interface ISegmentStrategy
{
    bool Matches(string path);
    IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default);
}