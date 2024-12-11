using System.Diagnostics.CodeAnalysis;

using WildPath.Abstractions;
using WildPath.Extensions;

namespace WildPath.Strategies;

/// <summary>
/// Does parent directories contain a marker file?
/// </summary>
internal class TaggedSegmentStrategy : ISegmentStrategy
{
    private readonly string _marker;
    private readonly IFileSystem _fileSystem;

    public TaggedSegmentStrategy(string marker, IFileSystem fileSystem)
    {
        _marker = marker;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path)
    {
        // Always true as the marker is the deciding factor, not the folder name itself.
        return true;
    }

    public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child)
    {
        var directories = _fileSystem.EnumerateDirectories(currentDirectory);

        foreach (var directory in directories)
        {
            var markerPath = Path.Combine(directory, _marker);

            // Check if the marker exists
            if (!_fileSystem.EntryExists(markerPath))
            {
                continue;
            }

            if (child == null)
            {
                yield return directory;
            }
            else
            {
                foreach (var subDir in child.Evaluate(directory))
                {
                    yield return subDir;
                }
            }
        }
    }

    public static bool TryCreate(string segment, IFileSystem fileSystem, [NotNullWhen(true)]out ISegmentStrategy? strategy)
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