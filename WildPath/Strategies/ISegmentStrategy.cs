namespace WildPath.Strategies;

public interface ISegmentStrategy
{
    bool Matches(string path);
    IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child);
}