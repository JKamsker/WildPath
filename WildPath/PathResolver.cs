using WildPath.Abstractions;
using WildPath.Internals;

namespace WildPath;

public class PathResolver
{
    private readonly string _currentDir;
    private readonly IFileSystem _fileSystem;

    private static readonly PathResolver _default = new();

    public char DirectorySeparatorChar { get; set; }

    public PathResolver(string? currentDir = null, IFileSystem? fileSystem = null)
    {
        fileSystem ??= RealFileSystem.Instance;

        _currentDir = currentDir ?? fileSystem.CurrentDirectory;
        _fileSystem = fileSystem;
        DirectorySeparatorChar = fileSystem.DirectorySeparatorChar;
    }

    public static string Resolve(string expression)
    {
        return _default.EvaluateExpression(expression);
    }

    public static string Resolve(params string[] path)
    {
        return _default.EvaluateExpression(path);
    }

    public static IEnumerable<string> ResolveAll(string path)
    {
        return _default.EvaluateAll(path);
    }

    public static IEnumerable<string> ResolveAll(params string[] path)
    {
        return _default.EvaluateAll(path);
    }

    internal string EvaluateExpression(string path)
    {
        var segments = path.Split(DirectorySeparatorChar);
        return EvaluateExpression(segments);
    }

    internal string EvaluateExpression(params string[] path)
    {
        var result = EvaluateAll(path).FirstOrDefault();
        if (result == null)
        {
            throw new DirectoryNotFoundException(
                $"Path '{string.Join(DirectorySeparatorChar.ToString(), path)}' not found.");
        }

        return result;
    }

    internal IEnumerable<string> EvaluateAll(string path)
    {
        var segments = path.Split(DirectorySeparatorChar);
        return EvaluateAll(segments);
    }

    internal IEnumerable<string> EvaluateAll(string[] path)
    {
        var segment = PathEvaluatorSegment.FromExpressions(path, _fileSystem);
        if (segment == null)
        {
            throw new InvalidOperationException("Path is empty.");
        }

        return segment.Evaluate(_currentDir);
    }
}

/// <summary>
/// Provides facade extension methods for <see cref="PathResolver"/>.
/// </summary>
public static class PathResolverExtensions
{
    public static string Resolve(this PathResolver resolver, string expression)
    {
        return resolver.EvaluateExpression(expression);
    }

    public static string Resolve(this PathResolver resolver, params string[] path)
    {
        return resolver.EvaluateExpression(path);
    }
    
    public static IEnumerable<string> ResolveAll(this PathResolver resolver, string path)
    {
        return resolver.EvaluateAll(path);
    }
    
    public static IEnumerable<string> ResolveAll(this PathResolver resolver, params string[] path)
    {
        return resolver.EvaluateAll(path);
    }
}