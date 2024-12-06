namespace WildPath.Strategies;

public class AnySegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public AnySegmentStrategy(string segment, IFileSystem fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory))
        {
            if (child == null)
            {
                yield return directory;
                continue;
            }

            foreach (var subDir in child.Evaluate(directory))
            {
                yield return subDir;
            }
        }
    }
}