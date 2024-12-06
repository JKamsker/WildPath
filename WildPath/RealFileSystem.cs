namespace WildPath;

public class RealFileSystem : IFileSystem
{
    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
    public string CurrentDirectory => Directory.GetCurrentDirectory();


    public static RealFileSystem Instance { get; } = new RealFileSystem();

    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    public string Combine(params string[] paths) => Path.Combine(paths);


    public string? GetFileName(string path) => Path.GetFileName(path);

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
}