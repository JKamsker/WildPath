
namespace WildPath.Abstractions;

public interface IPathEvaluatorSegment
{
    IEnumerable<string> Evaluate(string currentDirectory);
    bool Matches(string path);

    internal bool IsFirst { get; }
}