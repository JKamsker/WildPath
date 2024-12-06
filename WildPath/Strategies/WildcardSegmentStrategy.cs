using System.Text.RegularExpressions;

namespace PathResolver.Strategies;

public class WildcardSegmentStrategy : ISegmentStrategy
{
    private readonly string _pattern;
    private readonly IFileSystem _fileSystem;

    public WildcardSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        _pattern = "^" + Regex.Escape(segment).Replace("\\*", ".*") + "$";
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => Regex.IsMatch(path, _pattern);

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        var directories = _fileSystem
            .EnumerateDirectories(currentDirectory)
            .Where(d => Matches(_fileSystem.GetFileName(d) ?? string.Empty));

        foreach (var directory in directories)
        {
            if (child == null)
                yield return directory;
            else
            {
                foreach (var subDir in child.Evaluate(directory))
                {
                    yield return subDir;
                }
            }
        }
    }
}