using System.Net;
using System.Text.RegularExpressions;

namespace PathResolver.Strategies;

public class StrategyFactory
{
    private readonly IFileSystem _fileSystem;

    public StrategyFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }
    
    public ISegmentStrategy Create(string segment) => segment switch
    {
        ".." => new ParentSegmentStrategy(segment, _fileSystem),
        "..." => new ParentsSegmentStrategy(segment, _fileSystem),
        "*" => new AnySegmentStrategy(segment, _fileSystem),
        "**" => new AnySegmentRecursivelyStrategy(segment, _fileSystem),
        _ => segment.Contains('*')
            ? new WildcardSegmentStrategy(segment, _fileSystem)
            : new ExactMatchSegmentStrategy(segment, _fileSystem)
    };
}

public interface ISegmentStrategy
{
    bool Matches(string path);
    IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child);
}

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
        foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory)
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
        var allSubdirectories = EnumerateAllSubdirectories(currentDirectory);
        foreach (var directory in allSubdirectories)
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

public class ParentSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public ParentSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        if(!string.Equals(segment, "..", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ParentSegmentStrategy can only be used with '..' segment", nameof(segment));
        
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        var parentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
        if (parentDirectory == null) yield break;

        if (child == null)
        {
            yield return parentDirectory;
            yield break;
        }

        foreach (var result in child.Evaluate(parentDirectory))
        {
            yield return result;
        }
    }
}

public class ParentsSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public ParentsSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        if (!string.Equals(segment, "...", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ParentsSegmentStrategy can only be used with '...' segment", nameof(segment));

        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        while (currentDirectory != null)
        {
            if (child == null)
            {
                yield return currentDirectory;
                continue;
            }

            foreach (var result in child.Evaluate(currentDirectory))
            {
                yield return result;
            }

            currentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
        }
    }
}

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
