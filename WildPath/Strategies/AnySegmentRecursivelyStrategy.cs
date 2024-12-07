using WildPath.Abstractions;

namespace WildPath.Strategies;

public class AnySegmentRecursivelyStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public AnySegmentRecursivelyStrategy(string segment, IFileSystem fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }
    
    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        var directories = EnumerateAllSubdirectories(currentDirectory);
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

    private IEnumerable<string> EnumerateAllSubdirectories(string currentDirectory)
    {
        var stack = new Stack<string>();
        stack.Push(currentDirectory);

        while (stack.Count > 0)
        {
            var directory = stack.Pop();
            yield return directory;

            foreach (var subDirectory in _fileSystem.EnumerateDirectories(directory))
            {
                stack.Push(subDirectory);
            }
        }
    }
}