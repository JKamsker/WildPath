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
        ".." => new ParentSegmentStrategy(segment),
        "..." => new ParentsSegmentStrategy(segment),
        "*" => new AnySegmentStrategy(segment),
        "**" => new AnySegmentRecursivelyStrategy(segment),
        _ => segment.Contains('*')
            ? new WildcardSegmentStrategy(segment)
            : new ExactMatchSegmentStrategy(segment)
    };
}

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

/// <summary>
/// Segment is only **, which matches any number of directories of any name. 
/// </summary>
public class AnySegmentRecursivelyStrategy : ISegmentStrategy
{
    private readonly string _segment;

    public AnySegmentRecursivelyStrategy(string segment)
    {
        _segment = segment;
    }
    
    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        var allSubdirectories = EnumerateAllSubdirectories(currentDirectory, fileSystem);
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

    private IEnumerable<string> EnumerateAllSubdirectories(string currentDirectory, IFileSystem fileSystem)
    {
        var stack = new Stack<string>();
        stack.Push(currentDirectory);

        while (stack.Count > 0)
        {
            var directory = stack.Pop();
            yield return directory;

            foreach (var subDirectory in fileSystem.EnumerateDirectories(directory))
            {
                stack.Push(subDirectory);
            }
        }
    }
}

/// <summary>
/// Segment is only *, which matches any single directory of any name.
/// </summary>
public class AnySegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;

    public AnySegmentStrategy(string segment)
    {
        _segment = segment;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        foreach (var directory in fileSystem.EnumerateDirectories(currentDirectory))
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

    public ParentSegmentStrategy(string segment)
    {
        if(!string.Equals(segment, "..", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ParentSegmentStrategy can only be used with '..' segment", nameof(segment));
        
        _segment = segment;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
    {
        var parentDirectory = fileSystem.GetDirectoryName(currentDirectory);
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

    public ParentsSegmentStrategy(string segment)
    {
        if (!string.Equals(segment, "...", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("ParentsSegmentStrategy can only be used with '...' segment", nameof(segment));

        _segment = segment;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
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

            currentDirectory = fileSystem.GetDirectoryName(currentDirectory);
        }
    }
}
//
// public class StructuralSegmentStrategy : ISegmentStrategy
// {
//     private readonly string _segment;
//
//     public StructuralSegmentStrategy(string segment)
//     {
//         _segment = segment;
//     }
//
//     public bool Matches(string path) => true; // Matches any path structurally
//
//     public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child, IFileSystem fileSystem)
//     {
//         if (_segment == "..")
//         {
//             var parentDirectory = fileSystem.GetDirectoryName(currentDirectory);
//             if (parentDirectory == null) yield break;
//
//             if (child == null)
//             {
//                 yield return parentDirectory;
//                 yield break;
//             }
//
//             foreach (var result in child.Evaluate(parentDirectory))
//             {
//                 yield return result;
//             }
//         }
//         else if (_segment == "...")
//         {
//             while (currentDirectory != null)
//             {
//                 if (child == null)
//                 {
//                     yield return currentDirectory;
//                     continue;
//                 }
//
//                 foreach (var result in child.Evaluate(currentDirectory))
//                 {
//                     yield return result;
//                 }
//
//                 currentDirectory = fileSystem.GetDirectoryName(currentDirectory);
//             }
//         }
//     }
// }

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
        var directories = fileSystem
            .EnumerateDirectories(currentDirectory)
            .Where(d => Matches(fileSystem.GetFileName(d) ?? string.Empty));
        
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