using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;
using WildPath.Extensions;

namespace WildPath.Strategies;

/// <summary>
/// Does parent directories contain a marker file?
/// </summary>
internal class TaggedSegmentStrategy(string marker, IFileSystem fileSystem)
    : SegmentStrategyBase(fileSystem), ISegmentStrategy
{
    private readonly string _marker = marker;
    private readonly IFileSystem _fileSystem = fileSystem;

    public override bool Matches(string path)
    {
        return HasMarker(path);
    }

    public override IEnumerable<string> Evaluate(
        string currentDirectory,
        IPathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory))
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }

            if (!HasMarker(directory))
            {
                continue;
            }

            if (child == null)
            {
                yield return directory;
                continue;
            }

            foreach (var subDir in child.Evaluate(directory, token))
            {
                if (token.IsCancellationRequested)
                {
                    yield break;
                }

                yield return subDir;
            }
        }
    }

    internal override string? EvaluateFirst(
        string currentDirectory,
        PathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (_fileSystem is IFileSystemEntryEnumerable enumerable)
        {
            var visitor = new FirstTaggedDirectoryVisitor(this, child, token);
            enumerable.VisitDirectories(currentDirectory, ref visitor);
            return visitor.Result;
        }

        foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory))
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }

            if (!HasMarker(directory))
            {
                continue;
            }

            if (child == null)
            {
                return directory;
            }

            var result = child.EvaluateFirst(directory, token);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = _fileSystem.EnumerateDirectories(currentDirectory);
    //
    //     foreach (var directory in directories)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //
    //         var markerPath = _fileSystem.Combine(directory, _marker);
    //
    //         // Check if the marker exists
    //         if (!_fileSystem.EntryExists(markerPath))
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

    protected override IEnumerable<string> GetSource(string currentDirectory)
        => _fileSystem.EnumerateDirectories(currentDirectory);

    private bool HasMarker(string directory)
    {
        if (_fileSystem is IFileSystemEntryLookup lookup)
        {
            return lookup.TryGetFileSystemEntry(directory, _marker, out _);
        }

        var markerPath = _fileSystem.Combine(directory, _marker);
        return _fileSystem.EntryExists(markerPath);
    }

    private struct FirstTaggedDirectoryVisitor : IFileSystemEntryVisitor
    {
        private readonly TaggedSegmentStrategy _strategy;
        private readonly PathEvaluatorSegment? _child;
        private readonly CancellationToken _token;

        public FirstTaggedDirectoryVisitor(
            TaggedSegmentStrategy strategy,
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

            if (!_strategy.HasMarker(path))
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

    public static bool TryCreate(string segment, IFileSystem fileSystem, [NotNullWhen(true)] out ISegmentStrategy? strategy)
    {
        if (segment.TryTrimStartAndEnd(":tagged(", "):", out var marker))
        {
            strategy = new TaggedSegmentStrategy(marker, fileSystem);
            return true;
        }

        strategy = default;
        return false;
    }
}
