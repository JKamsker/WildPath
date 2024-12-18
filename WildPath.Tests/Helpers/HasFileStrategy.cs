using WildPath.Abstractions;
using WildPath.Internals;
using WildPath.Strategies.Custom;

namespace WildPath.Tests.Custom;

/// <summary>
/// Called via ...\WildPath.Console\:MyCustomStrategy(param1, param2)
/// </summary>
public class HasFileStrategy : ICustomStrategy
{
    private readonly IFileSystem _fileSystem;
    internal readonly string Marker;

    public HasFileStrategy(CustomStrategyCall call, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        if (call.Parameters.Length != 1)
        {
            throw new ArgumentException("Expected 1 parameter.");
        }

        Marker = call.Parameters[0];
    }

    public bool Matches(string path)
    {
        return true;
    }
    
    public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    {
        var directories = _fileSystem.EnumerateDirectories(currentDirectory);

        foreach (var directory in directories)
        {
            token.ThrowIfCancellationRequested();

            var markerPath = _fileSystem.Combine(directory, Marker);
            
            // Check if the marker exists
            if (!_fileSystem.FileExists(markerPath))
            {
                continue;
            }

            if (child == null)
            {
                yield return directory;
            }
            else
            {
                foreach (var subDir in child.Evaluate(directory, token))
                {
                    yield return subDir;
                }
            }
        }
    }
}

// HasDirectoryStrategy.cs
// :hasDirectory(directoryMarker):