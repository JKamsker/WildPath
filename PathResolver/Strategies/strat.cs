using System.Text.RegularExpressions;

namespace PathResolver.Strategies;

public interface ISegmentStrategy
{
    bool Matches(string path);
    IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem);
}

public class WildcardSegmentStrategy : ISegmentStrategy
{
    private readonly string _pattern;

    public WildcardSegmentStrategy(string segment)
    {
        _pattern = "^" + Regex.Escape(segment).Replace("\\*", ".*") + "$";
    }

    public bool Matches(string path) => Regex.IsMatch(path, _pattern);

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        foreach (var directory in fileSystem.EnumerateDirectories(currentDirectory)
                                           .Where(d => Matches(Path.GetFileName(d) ?? string.Empty)))
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

public class StructuralSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;

    public StructuralSegmentStrategy(string segment)
    {
        _segment = segment;
    }

    public bool Matches(string path) => true; // Matches any path structurally

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        if (_segment == "..")
        {
            var parentDirectory = fileSystem.GetDirectoryName(currentDirectory);
            if (parentDirectory == null) yield break;

            if (child == null)
                yield return parentDirectory;
            else
            {
                foreach (var result in child.Evaluate(parentDirectory))
                {
                    yield return result;
                }
            }
        }
        else if (_segment == "...")
        {
            while (currentDirectory != null)
            {
                if (child == null)
                    yield return currentDirectory;
                else
                {
                    foreach (var result in child.Evaluate(currentDirectory))
                    {
                        yield return result;
                    }
                }

                currentDirectory = fileSystem.GetDirectoryName(currentDirectory);
            }
        }
    }
}

public class ExactMatchSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;

    public ExactMatchSegmentStrategy(string segment)
    {
        _segment = segment;
    }

    public bool Matches(string path) => string.Equals(_segment, path, StringComparison.OrdinalIgnoreCase);

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        foreach (var directory in fileSystem.EnumerateDirectories(currentDirectory)
                                           .Where(d => Matches(Path.GetFileName(d) ?? string.Empty)))
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