using System.ComponentModel;

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

    public static string Resolve(string expression, CancellationToken token = default)
    {
        return _default.EvaluateExpression(expression, token);
    }

    public static string Resolve(string[] pathSegments, CancellationToken token = default)
    {
        return _default.EvaluateExpression(pathSegments, token);
    }

    public static IEnumerable<string> ResolveAll(string path, CancellationToken token = default)
    {
        return _default.EvaluateAll(path, token);
    }

    public static IEnumerable<string> ResolveAll(string[] pathSegments, CancellationToken token = default)
    {
        return _default.EvaluateAll(pathSegments, token);
    }

    internal string EvaluateExpression(string path, CancellationToken token = default)
    {
        var segments = path.Split(DirectorySeparatorChar);
        return EvaluateExpression(segments, token);
    }

    internal string EvaluateExpression(string[] path, CancellationToken token = default)
    {
        var result = EvaluateAll(path, token).FirstOrDefault();
        if (result == null)
        {
            throw new DirectoryNotFoundException(
                $"Path '{string.Join(DirectorySeparatorChar.ToString(), path)}' not found.");
        }

        return result;
    }

    internal IEnumerable<string> EvaluateAll(string path, CancellationToken token = default)
    {
        var segments = path.Split(DirectorySeparatorChar);
        return EvaluateAll(segments, token);
    }

    internal IEnumerable<string> EvaluateAll(string[] pathSegments, CancellationToken token = default)
    {
        var segment = PathEvaluatorSegment.FromExpressions(pathSegments, _fileSystem);
        if (segment == null)
        {
            throw new InvalidOperationException("Path is empty.");
        }

        return segment.Evaluate(_currentDir, token);
    }
}


[EditorBrowsable(EditorBrowsableState.Never)]
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