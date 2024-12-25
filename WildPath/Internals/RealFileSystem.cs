using WildPath.Abstractions;

namespace WildPath.Internals;

internal class RealFileSystem : IFileSystem
{
    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
    public string CurrentDirectory => Directory.GetCurrentDirectory();


    public static RealFileSystem Instance { get; } = new RealFileSystem();

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        // Fix for if path is 'C:'
        if (IsDriveLetterPath(path))
        {
            path += DirectorySeparatorChar;
        }

        try
        {
            return Directory.EnumerateDirectories(path);
        }
        catch (UnauthorizedAccessException e)
        {
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        // Fix for if path is 'C:'
        if (IsDriveLetterPath(path))
        {
            path += DirectorySeparatorChar;
        }

        try
        {
            return Directory.EnumerateFileSystemEntries(path);
        }
        catch (UnauthorizedAccessException e)
        {
            return Enumerable.Empty<string>();
        }
    }

    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public string Combine(params string[] paths) => Path.Combine(paths);


    public string? GetFileName(string path)
        => IsDriveLetterPath(path)
            ? path
            : Path.GetFileName(path);

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    public bool EntryExists(string path)
    {
        return FileExists(path) || DirectoryExists(path);
    }

    private static bool IsDriveLetterPath(string path)
    {
        return path.Length == 2 && path[1] == ':' && path[0] >= 'A' && path[0] <= 'Z';
    }
}