namespace PathResolver;

public class RealFileSystem : IFileSystem
{
    public static RealFileSystem Instance { get; } = new RealFileSystem();
    
    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);
    public string? GetDirectoryName(string path) => Path.GetDirectoryName(path);
    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
    public string Combine(params string[] paths) => Path.Combine(paths);
    
    public string CurrentDirectory => Directory.GetCurrentDirectory();
}