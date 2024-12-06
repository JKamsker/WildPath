namespace WildPath;

public interface IFileSystem
{
    IEnumerable<string> EnumerateDirectories(string path);
    string? GetDirectoryName(string path);
    char DirectorySeparatorChar { get; }
    string CurrentDirectory { get; }
    string Combine(params string[] paths);
    string? GetFileName(string path);
    
    bool FileExists(string filePath);
    bool DirectoryExists(string directoryPath);
    bool EntryExists(string path);
}