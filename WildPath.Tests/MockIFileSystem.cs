using WildPath.Abstractions;

namespace WildPath.Tests;

public class MockFileSystem : IFileSystem
{
    private readonly HashSet<string> _directories;
    public char DirectorySeparatorChar { get; }
    public string CurrentDirectory { get; }

    public MockFileSystem(
        string currentDirectory,
        IEnumerable<string> existingDirectories,
        char? directorySeparatorChar = null
    )
    {
        // Normalize and store all directories in a HashSet for O(1) lookups.
        // Ensuring all directories do not end with a trailing slash for consistency.
        _directories = new HashSet<string>(
            existingDirectories.Select(d => NormalizeDirectoryPath(d)),
            StringComparer.OrdinalIgnoreCase
        );

        DirectorySeparatorChar = directorySeparatorChar ?? Path.DirectorySeparatorChar;
        CurrentDirectory = currentDirectory;
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        var normalizedPath = NormalizeDirectoryPath(path);

        // An "immediate subdirectory" is a directory that:
        // 1. Starts with the given path + directory separator
        // 2. Has exactly one more directory name after the given path
        //    i.e. "C:\Test" -> "C:\Test\SubDir1" is immediate but "C:\Test\SubDir1\Nested" is not

        var prefix = normalizedPath + DirectorySeparatorChar;
        int prefixLength = prefix.Length;

        var result = _directories
                .Where(dir => dir.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Select(dir =>
                {
                    var remainder = dir.AsSpan()[prefixLength..];
                    // The directory is immediate if it contains no further directory separators
                    // after the prefix.
                    // return remainder.Contains(DirectorySeparatorChar) ? null : dir;

                    var separatorIndex = remainder.IndexOf(DirectorySeparatorChar);
                    return separatorIndex == -1 ? dir : dir.Substring(0, prefixLength + separatorIndex);
                })
                .Where(dir => dir != null)! // filter out non-immediate directories
                ;
        
        return result;
    }

    public string? GetDirectoryName(string path)
    {
        var normalizedPath = NormalizeDirectoryPath(path);

        if (string.IsNullOrEmpty(normalizedPath))
        {
            return null;
        }

        // Find the last directory separator
        int lastSeparatorIndex = normalizedPath.LastIndexOf(DirectorySeparatorChar);
        if (lastSeparatorIndex <= 0)
        {
            // This means either there's no parent directory or it's the root
            return null;
        }

        // Return everything up to (but not including) the last separator
        return normalizedPath.Substring(0, lastSeparatorIndex);
    }

    public string Combine(params string[] paths)
    {
        return string.Join(DirectorySeparatorChar, paths);
    }

    public string GetFileName(string path)
    {
        var normalizedPath = NormalizeDirectoryPath(path);

        // Find the last directory separator
        int lastSeparatorIndex = normalizedPath.LastIndexOf(DirectorySeparatorChar);
        if (lastSeparatorIndex == -1)
        {
            // This means there's no directory separator
            return normalizedPath;
        }

        // Return everything after the last separator
        return normalizedPath.Substring(lastSeparatorIndex + 1);
    }

    public bool FileExists(string filePath)
    {
        var normalizedPath = NormalizeDirectoryPath(filePath);
        return _directories.Contains(normalizedPath);
    }

    public bool DirectoryExists(string directoryPath)
    {
        var normalizedPath = NormalizeDirectoryPath(directoryPath);
        if (_directories.Contains(normalizedPath))
        {
            return true;
        }
        
        var combinedPath = string.Concat(normalizedPath, DirectorySeparatorChar);
        foreach (var dir in _directories)
        {
            if (dir.StartsWith(combinedPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
    
    // FileSystemEntryExists
    public bool EntryExists(string path)
    {
        return DirectoryExists(path);
    }

    private string NormalizeDirectoryPath(string path)
    {
        // Remove trailing directory separators
        var trimmed = path.TrimEnd(DirectorySeparatorChar, '/', '\\');
        // Optionally, you could run something like Path.GetFullPath or handle platform differences.
        return trimmed;
    }
}