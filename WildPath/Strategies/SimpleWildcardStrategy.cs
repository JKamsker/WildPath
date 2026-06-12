using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;
using WildPath.Extensions;

namespace WildPath.Strategies;

/// <summary>
/// "*World" or "Hello*" or "*llo*" will match "Hello World"
/// Uses no regex, just string operations
/// </summary>
internal class SimpleWildcardStrategy : SegmentStrategyBase, ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;
    private readonly string _partOne;
    private readonly string _partTwo;
    private readonly int _count;
    private readonly bool _startsWithWildcard;
    private readonly bool _endsWithWildcard;

    public bool IsValid => true;


    /// <summary>
    /// Dumb constructor without any logic
    /// </summary>
    private SimpleWildcardStrategy(
        string segment,
        IFileSystem fileSystem,
        string partOne,
        string partTwo,
        int count,
        bool startsWithWildcard,
        bool endsWithWildcard
    ) : base(fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
        _partOne = partOne;
        _partTwo = partTwo;
        _count = count;
        _startsWithWildcard = startsWithWildcard;
        _endsWithWildcard = endsWithWildcard;
    }


    public override bool Matches(string path)
    {
        var fileName = GetFileName(path);
        if (fileName.IsEmpty)
        {
            return false;
        }

        return MatchesFileName(fileName);
    }

    protected override IEnumerable<string> GetSource(string currentDirectory)
    {
        return _fileSystem
            .EnumerateFileSystemEntries(currentDirectory);
    }

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

    private struct FirstWildcardEntryVisitor : IFileSystemEntryVisitor
    {
        private readonly SimpleWildcardStrategy _strategy;
        private readonly PathEvaluatorSegment? _child;
        private readonly CancellationToken _token;

        public FirstWildcardEntryVisitor(
            SimpleWildcardStrategy strategy,
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

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = _fileSystem
    //         .EnumerateFileSystemEntries(currentDirectory);
    //
    //     foreach (var directory in directories)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //
    //         if (!Matches(_fileSystem.GetFileName(directory) ?? string.Empty))
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

    public static bool TryCreate(
        string segment,
        IFileSystem fileSystem,
        [NotNullWhen(true)] out ISegmentStrategy? strategy
    )
    {
        if (!segment.Contains('*'))
        {
            strategy = null;
            return false;
        }

        var segSpan = segment.AsSpan();
        var partOne = segSpan.CutUntil('*');
        var partTwo = segSpan.CutUntil('*');
        var partThree = segSpan.CutUntil('*');

        var startsWithWildcard = segment.StartsWith('*');
        var endsWithWildcard = segment.EndsWith('*');

        var count = (!partOne.IsEmpty ? 1 : 0)
                    + (!partTwo.IsEmpty ? 1 : 0)
                    + (!partThree.IsEmpty ? 1 : 0);

        if (!partThree.IsEmpty || count is not (1 or 2))
        {
            strategy = null;
            return false;
        }

        var result = new SimpleWildcardStrategy
        (
            segment: segment,
            fileSystem: fileSystem,
            partOne: partOne.ConvertToString(),
            partTwo: partTwo.ConvertToString(),
            count: count,
            startsWithWildcard,
            endsWithWildcard
        );

        strategy = result;
        return true;
    }

    internal static ISegmentStrategy CreateInterpreted(
        PathExpressionSegment segment,
        IFileSystem fileSystem)
    {
        return segment.Kind switch
        {
            PathExpressionSegmentKind.SimpleWildcardStartsWith => new SimpleWildcardStrategy(
                segment.RawValue,
                fileSystem,
                segment.Value,
                string.Empty,
                count: 1,
                startsWithWildcard: false,
                endsWithWildcard: true),
            PathExpressionSegmentKind.SimpleWildcardEndsWith => new SimpleWildcardStrategy(
                segment.RawValue,
                fileSystem,
                string.Empty,
                segment.Value,
                count: 1,
                startsWithWildcard: true,
                endsWithWildcard: false),
            PathExpressionSegmentKind.SimpleWildcardContains => new SimpleWildcardStrategy(
                segment.RawValue,
                fileSystem,
                string.Empty,
                segment.Value,
                count: 1,
                startsWithWildcard: true,
                endsWithWildcard: true),
            PathExpressionSegmentKind.SimpleWildcardStartsAndEnds => new SimpleWildcardStrategy(
                segment.RawValue,
                fileSystem,
                segment.Value,
                segment.SecondValue,
                count: 2,
                startsWithWildcard: false,
                endsWithWildcard: false),
            _ => throw new ArgumentOutOfRangeException(nameof(segment))
        };
    }

    private bool MatchesFileName(ReadOnlySpan<char> path)
    {
        if (_startsWithWildcard && _endsWithWildcard)
        {
            return path.Contains(_partTwo.AsSpan(), StringComparison.OrdinalIgnoreCase);
        }

        return _count switch
        {
            1 when _startsWithWildcard => path.EndsWith(_partTwo.AsSpan(), StringComparison.OrdinalIgnoreCase),
            1 when _endsWithWildcard => path.StartsWith(_partOne.AsSpan(), StringComparison.OrdinalIgnoreCase),
            2 => path.StartsWith(_partOne.AsSpan(), StringComparison.OrdinalIgnoreCase) &&
                 path.EndsWith(_partTwo.AsSpan(), StringComparison.OrdinalIgnoreCase),
            _ => false
        };
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
}
