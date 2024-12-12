namespace WildPath.Abstractions;

public interface IPathEvaluatorSegment
{
    IEnumerable<string> Evaluate(string currentDirectory, CancellationToken token = default);


    bool Matches(string path);

    internal bool IsFirst { get; }
}