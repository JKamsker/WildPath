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
        var markerPath = _fileSystem.Combine(path, _marker);

        // Check if the marker exists
        return _fileSystem.EntryExists(markerPath);
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