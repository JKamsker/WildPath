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
    private readonly Func<string, bool> _matcher;

    public bool IsValid { get; }


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
        if (TryCreateMatcher(out _matcher))
        {
            IsValid = true;
        }
    }


    public override bool Matches(string path)
    {
        var fileName = _fileSystem.GetFileName(path) ?? string.Empty;
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        return _matcher(fileName);
    }

    protected override IEnumerable<string> GetSource(string currentDirectory)
    {
        return _fileSystem
            .EnumerateFileSystemEntries(currentDirectory);
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

    public static bool TryCreate(string segment, IFileSystem fileSystem, out ISegmentStrategy strategy)
    {
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

        // test: make a delegate for this
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
        return result.IsValid;
    }

    private bool TryCreateMatcher(out Func<string, bool> matcher)
    {
        matcher = default;

        if (_startsWithWildcard && _endsWithWildcard)
        {
            matcher = path => path.Contains(_partTwo, StringComparison.OrdinalIgnoreCase);
            return true;
        }

        switch (_count)
        {
            case 1 when _startsWithWildcard:
                matcher = path => path.EndsWith(_partTwo, StringComparison.OrdinalIgnoreCase);
                return true;
            case 1 when _endsWithWildcard:
                matcher = path => path.StartsWith(_partOne, StringComparison.OrdinalIgnoreCase);
                return true;
            case 2:
                // Pattern looks like "He*lo"
                // path should start with prefix and end with suffix
                matcher = path => path.StartsWith(_partOne, StringComparison.OrdinalIgnoreCase) &&
                                  path.EndsWith(_partTwo, StringComparison.OrdinalIgnoreCase);
                return true;
            default:
                // throw new InvalidOperationException("Invalid wildcard usage");
                return false;
        }
    }
}