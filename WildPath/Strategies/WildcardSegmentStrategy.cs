using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class WildcardSegmentStrategy : SegmentStrategyBase, ISegmentStrategy
{
    private readonly string _pattern;
    private readonly IFileSystem _fileSystem;

    public WildcardSegmentStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        _pattern = segment;
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path)
    {
        var fileName = GetFileName(path);
        return !fileName.IsEmpty && MatchesPattern(fileName, _pattern.AsSpan());
    }

    protected override IEnumerable<string> GetSource(string currentDirectory)
        => _fileSystem.EnumerateFileSystemEntries(currentDirectory);

    internal override string? EvaluateFirst(
        string currentDirectory,
        PathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (_fileSystem is IFileSystemEntryEnumerable enumerable)
        {
            var visitor = new FirstWildcardEntryVisitor(this, child, token);
            enumerable.VisitFileSystemEntries(currentDirectory, ref visitor);
            return visitor.Result;
        }

        foreach (var entry in _fileSystem.EnumerateFileSystemEntries(currentDirectory))
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }

            if (!Matches(entry))
            {
                continue;
            }

            if (child == null)
            {
                return entry;
            }

            var result = child.EvaluateFirst(entry, token);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static bool MatchesPattern(ReadOnlySpan<char> value, ReadOnlySpan<char> pattern)
    {
        var valueIndex = 0;
        var patternIndex = 0;
        var starIndex = -1;
        var matchedAfterStar = 0;

        while (valueIndex < value.Length)
        {
            if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                starIndex = patternIndex++;
                matchedAfterStar = valueIndex;
                continue;
            }

            if (patternIndex < pattern.Length &&
                char.ToUpperInvariant(pattern[patternIndex]) == char.ToUpperInvariant(value[valueIndex]))
            {
                patternIndex++;
                valueIndex++;
                continue;
            }

            if (starIndex != -1)
            {
                patternIndex = starIndex + 1;
                valueIndex = ++matchedAfterStar;
                continue;
            }

            return false;
        }

        while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
        {
            patternIndex++;
        }

        return patternIndex == pattern.Length;
    }

    private ReadOnlySpan<char> GetFileName(string path)
    {
        var span = TrimTrailingSeparators(path.AsSpan());
        var separatorIndex = span.LastIndexOfAny(
            _fileSystem.DirectorySeparatorChar,
            System.IO.Path.DirectorySeparatorChar,
            System.IO.Path.AltDirectorySeparatorChar);
        return separatorIndex < 0
            ? span
            : span[(separatorIndex + 1)..];
    }

    private ReadOnlySpan<char> TrimTrailingSeparators(ReadOnlySpan<char> path)
    {
        var length = path.Length;
        while (length > 0 && IsDirectorySeparator(path[length - 1]))
        {
            length--;
        }

        return path[..length];
    }

    private bool IsDirectorySeparator(char value)
    {
        return value == _fileSystem.DirectorySeparatorChar ||
               value == System.IO.Path.DirectorySeparatorChar ||
               value == System.IO.Path.AltDirectorySeparatorChar;
    }

    private struct FirstWildcardEntryVisitor : IFileSystemEntryVisitor
    {
        private readonly WildcardSegmentStrategy _strategy;
        private readonly PathEvaluatorSegment? _child;
        private readonly CancellationToken _token;

        public FirstWildcardEntryVisitor(
            WildcardSegmentStrategy strategy,
            PathEvaluatorSegment? child,
            CancellationToken token)
        {
            _strategy = strategy;
            _child = child;
            _token = token;
            Result = null;
        }

        public string? Result { get; private set; }

        public bool Visit(string path)
        {
            if (_token.IsCancellationRequested)
            {
                return false;
            }

            if (!_strategy.Matches(path))
            {
                return true;
            }

            if (_child == null)
            {
                Result = path;
                return false;
            }

            Result = _child.EvaluateFirst(path, _token);
            return Result is null;
        }
    }
}
