using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class ExactMatchSegmentStrategy
    : SegmentStrategyBase, ISegmentStrategy, IParentSegmentAware
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    private IPathEvaluatorSegment _parentSegment = default!;
    private bool _isRootSegment;

    IPathEvaluatorSegment IParentSegmentAware.ParentSegment
    {
        get => _parentSegment;
        set => UpdateParentSegment(value);
    }

    public ExactMatchSegmentStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path)
    {
        if(_segment == ".")
        {
            return true;
        }

        var fileName = GetFileName(path);
        if (fileName.IsEmpty)
        {
            return false;
        }

        return fileName.Equals(_segment.AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<string> GetSource(string currentDirectory)
    {
        if (_isRootSegment)
        {
            return new[] { _segment };
        }
        
        if(_segment == ".")
        {
            return new[] { currentDirectory };
        }

        return _fileSystem
            .EnumerateFileSystemEntries(currentDirectory);
    }

    public override IEnumerable<string> Evaluate(
        string currentDirectory,
        IPathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (token.IsCancellationRequested)
        {
            yield break;
        }

        if (_isRootSegment)
        {
            foreach (var result in EvaluateKnownEntry(_segment, child, token))
            {
                yield return result;
            }

            yield break;
        }

        if (_segment == ".")
        {
            foreach (var result in EvaluateKnownEntry(currentDirectory, child, token))
            {
                yield return result;
            }

            yield break;
        }

        if (_fileSystem is IFileSystemEntryLookup lookup &&
            lookup.TryGetFileSystemEntry(currentDirectory, _segment, out var entry))
        {
            foreach (var result in EvaluateKnownEntry(entry, child, token))
            {
                yield return result;
            }

            yield break;
        }

        foreach (var result in base.Evaluate(currentDirectory, child, token))
        {
            yield return result;
        }
    }

    internal override string? EvaluateFirst(
        string currentDirectory,
        PathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (_isRootSegment)
        {
            return child?.EvaluateFirst(_segment, token) ?? _segment;
        }

        if (_segment == ".")
        {
            return child?.EvaluateFirst(currentDirectory, token) ?? currentDirectory;
        }

        if (_fileSystem is IFileSystemEntryLookup lookup &&
            lookup.TryGetFileSystemEntry(currentDirectory, _segment, out var exactEntry))
        {
            return child?.EvaluateFirst(exactEntry, token) ?? exactEntry;
        }

        if (_fileSystem is IFileSystemEntryEnumerable enumerable)
        {
            var visitor = new FirstExactEntryVisitor(this, child, token);
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

            return child?.EvaluateFirst(entry, token) ?? entry;
        }

        return null;
    }

    private struct FirstExactEntryVisitor : IFileSystemEntryVisitor
    {
        private readonly ExactMatchSegmentStrategy _strategy;
        private readonly PathEvaluatorSegment? _child;
        private readonly CancellationToken _token;

        public FirstExactEntryVisitor(
            ExactMatchSegmentStrategy strategy,
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

    private static IEnumerable<string> EvaluateKnownEntry(
        string entry,
        IPathEvaluatorSegment? child,
        CancellationToken token)
    {
        if (child == null)
        {
            yield return entry;
            yield break;
        }

        foreach (var result in child.Evaluate(entry, token))
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }

            yield return result;
        }
    }

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = GetSource(currentDirectory);
    //
    //     foreach (var directory in directories)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //
    //         if (!Matches(directory))
    //         {
    //             continue;
    //         }
    //
    //         if (child == null)
    //         {
    //             yield return directory;
    //             continue;
    //         }
    //
    //         foreach (var subDir in child.Evaluate(directory, token))
    //         {
    //             if (token.IsCancellationRequested)
    //             {
    //                 yield break;
    //             }
    //
    //             yield return subDir;
    //         }
    //     }
    // }

    private void UpdateParentSegment(IPathEvaluatorSegment value)
    {
        _parentSegment = value;
        _isRootSegment = IsRootDirectory(_segment);
    }

    private bool IsRootDirectory(string segment)
    {
        var isFirst = ((IParentSegmentAware)this).ParentSegment?.IsFirst ?? false;

        return isFirst
               && segment.Length == 2
               && segment[1] == ':'
               && segment[0] >= 'A' && segment[0] <= 'Z';
    }

    private ReadOnlySpan<char> GetFileName(string path)
    {
        var span = TrimTrailingSeparators(path.AsSpan());
        if (span.Length == 2 && span[1] == ':')
        {
            return span;
        }

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
}
