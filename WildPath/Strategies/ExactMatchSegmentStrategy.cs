namespace WildPath.Strategies;

public class ExactMatchSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public ExactMatchSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => string.Equals(_segment, path, StringComparison.OrdinalIgnoreCase);

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        var directories = _fileSystem
            .EnumerateDirectories(currentDirectory)
            .Where(d => Matches(_fileSystem.GetFileName(d) ?? string.Empty));
        
        foreach (var directory in directories)
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