using System.Text.RegularExpressions;

namespace PathResolver;

public class PathEvaluatorSegmentOld
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;
    private PathEvaluatorSegmentOld? _child;

    public PathEvaluatorSegmentOld(string segment, PathEvaluatorSegmentOld? child, IFileSystem fileSystem)
    {
        _segment = segment;
        _child = child;
        _fileSystem = fileSystem;

        // var pattern = "^" + Regex.Escape(_segment).Replace("\\*", ".*") + "$";
    }

    public bool Matches(string path)
    {
        if (string.Equals(_segment, "*"))
        {
            return true;
        }

        // No matching needed for .. since it's structural
        if (string.Equals(_segment, ".."))
        {
            return true;
        }

        // ... means any number of parent directories
        if (string.Equals(_segment, "..."))
        {
            return true;
        }

        // If the segment includes a wildcard, let's handle it.
        if (_segment.Contains("*"))
        {
            return MatchWildcard(path);
        }

        return string.Equals(_segment, path, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchWildcard(string path)
    {
        var pathSpan = path.AsSpan();
        var segSpan = _segment.AsSpan();
        var segSpanTmp = segSpan;

        var partOne = segSpanTmp.CutUntil('*');
        var partTwo = segSpanTmp.CutUntil('*');
        var partThree = segSpanTmp.CutUntil('*');

        var count = (!partOne.IsEmpty ? 1 : 0)
                    + (!partTwo.IsEmpty ? 1 : 0)
                    + (!partThree.IsEmpty ? 1 : 0);


        if (!partThree.IsEmpty)
        {
            var pattern = "^" + Regex.Escape(_segment).Replace("\\*", ".*") + "$";
            return Regex.IsMatch(path, pattern);
        }

        if (count == 1)
        {
            if (segSpan.StartsWith('*'))
            {
                return pathSpan.EndsWith(partTwo);
            }
            else if (segSpan.EndsWith('*'))
            {
                return pathSpan.StartsWith(partOne);
            }

            throw new InvalidOperationException("Invalid wildcard usage");
        }
        else if (count == 2)
        {
            // Pattern looks like "He*lo"
            var prefix = partOne;
            var suffix = partTwo;

            // path should start with prefix and end with suffix
            var res = pathSpan.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
                      pathSpan.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);

            return res;
        }

        throw new InvalidOperationException("Invalid wildcard usage");
    }

    public string? Evaluate(string currentDirectory)
    {
        if (_segment == "..")
        {
            var parentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
            if (parentDirectory == null)
            {
                return null; // Reached the root
            }

            return _child?.Evaluate(parentDirectory) ?? parentDirectory;
        }

        IEnumerable<string> directories;
        if (_segment == "...")
        {
            directories = EnumerateParents(currentDirectory);
        }
        else if (_segment == "**")
        {
            directories = EnumerateAllSubdirectories(currentDirectory);
        }
        else
        {
            directories = _fileSystem
                .EnumerateDirectories(currentDirectory)
                .Where(d => Matches(Path.GetFileName(d) ?? string.Empty));
        }

        foreach (var directory in directories)
        {
            if (_child == null)
            {
                return directory;
            }

            var subDirectory = _child.Evaluate(directory);
            if (subDirectory != null)
            {
                return subDirectory;
            }
        }

        return null;
    }

    private IEnumerable<string> EnumerateParents(string currentDirectory)
    {
        while (currentDirectory != null)
        {
            yield return currentDirectory;
            currentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
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

    public static PathEvaluatorSegment? FromExpressions(string[] path, IFileSystem fileSystem)
    {
        PathEvaluatorSegment? currentSegment = null;
        foreach (var element in path.Reverse())
        {
            if (currentSegment == null)
            {
                currentSegment = new PathEvaluatorSegment(element, null, fileSystem);
            }
            else
            {
                currentSegment = new PathEvaluatorSegment(element, currentSegment, fileSystem);
            }
        }

        return currentSegment;
    }
}