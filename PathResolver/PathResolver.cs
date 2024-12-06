namespace PathResolver;

public class PathResolver
{
    private readonly string _currentDir;
    private readonly IFileSystem _fileSystem;

    public char DirectorySeparatorChar { get; set; }

    public PathResolver(string? currentDir = null, IFileSystem? fileSystem = null)
    {
        fileSystem ??= RealFileSystem.Instance;
        
        _currentDir = currentDir ?? fileSystem.CurrentDirectory;
        _fileSystem = fileSystem;
        DirectorySeparatorChar = fileSystem.DirectorySeparatorChar;
    }

    public string EvaluateExpression(string path)
    {
        var segments = path.Split(DirectorySeparatorChar);
        return EvaluateExpression(segments);
    }

    public string EvaluateExpression(params string[] path)
    {
        var segment = PathEvaluatorSegment.FromExpressions(path, _fileSystem);
        if (segment == null)
        {
            throw new InvalidOperationException("Path is empty.");
        }

        var result = segment.Evaluate(_currentDir);
        if (result == null)
        {
            throw new DirectoryNotFoundException(
                $"Path '{string.Join(DirectorySeparatorChar.ToString(), path)}' not found.");
        }

        return result;
    }
}