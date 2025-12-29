using System.ComponentModel;
using WildPath.Abstractions;
using WildPath.Internals;
using WildPath.Strategies;

namespace WildPath;

public class PathResolver
{
    private readonly string _currentDir;
    private readonly IFileSystem _fileSystem;

    private static readonly PathResolver _default = new();
    private readonly IStrategyFactory _strategyFactory;

    public char? DirectorySeparatorChar { get; set; }

    public PathResolver(string? currentDir = null, IFileSystem? fileSystem = null, IStrategyFactory? strategyFactory = null)
    {
        fileSystem ??= RealFileSystem.Instance;

        _currentDir = currentDir ?? fileSystem.CurrentDirectory;
        _fileSystem = fileSystem;
        DirectorySeparatorChar = fileSystem.DirectorySeparatorChar;

        if (strategyFactory is not null)
        {
            _strategyFactory = new CompositeStrategyFactory
            (
                new StrategyFactory(fileSystem),
                strategyFactory,
                new DefaultStrategyFactory(fileSystem)
            );
        }
        else
        {
            _strategyFactory = new CompositeStrategyFactory
            (
                new StrategyFactory(fileSystem),
                new DefaultStrategyFactory(fileSystem)
            );
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="PathResolver"/> class.
    /// </summary>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The created instance of the <see cref="PathResolver"/> class.</returns>
    public static PathResolver Create(Action<PathResolverBuilder> configure)
    {
        var builder = new PathResolverBuilder();
        configure(builder);
        return builder.Build();
    }

    /// <summary>
    /// Resolves the path expression and returns the first result.
    /// </summary>
    /// <param name="expression">The path expression to resolve.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The first path that matches the expression.</returns>
    public static string Resolve(string expression, CancellationToken token = default)
    {
        return _default.EvaluateExpression(expression, token);
    }

    /// <summary>
    /// Resolves the path expression and returns the first result.
    /// </summary>
    /// <param name="pathSegments">Segments of the path expression to resolve.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The first path that matches the expression.</returns>
    public static string Resolve(string[] pathSegments, CancellationToken token = default)
    {
        return _default.EvaluateExpression(pathSegments, token);
    }

    /// <summary>
    /// Resolves the path expression and returns all results.
    /// </summary>
    /// <param name="path">The path expression to resolve.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>All paths that match the expression.</returns>
    public static IEnumerable<string> ResolveAll(string path, CancellationToken token = default)
    {
        return _default.EvaluateAll(path, token);
    }

    /// <summary>
    /// Resolves the path expression and returns all results.
    /// </summary>
    /// <param name="pathSegments">Segments of the path expression to resolve.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>All paths that match the expression.</returns>
    public static IEnumerable<string> ResolveAll(string[] pathSegments, CancellationToken token = default)
    {
        return _default.EvaluateAll(pathSegments, token);
    }

    internal string EvaluateExpression(string path, CancellationToken token = default)
    {
        var segments = Split(path);
        return EvaluateExpression(segments, token);
    }

    internal string EvaluateExpression(string[] path, CancellationToken token = default)
    {
        var result = EvaluateAll(path, token).FirstOrDefault();
        if (result == null)
        {
            var reassembledPath = string.Join((DirectorySeparatorChar ?? System.IO.Path.DirectorySeparatorChar).ToString(), path);
            throw new DirectoryNotFoundException($"Path '{reassembledPath}' not found.");
        }

        return result;
    }

    internal IEnumerable<string> EvaluateAll(string path, CancellationToken token = default)
    {
        var segments = Split(path);
        return EvaluateAll(segments, token);
    }

    internal IEnumerable<string> EvaluateAll(string[] pathSegments, CancellationToken token = default)
    {
        var segment = PathEvaluatorSegment.FromExpressions(pathSegments, _fileSystem, _strategyFactory);
        if (segment == null)
        {
            throw new InvalidOperationException("Path is empty.");
        }

        return segment.Evaluate(_currentDir, token);
    }

    private string[] Split(string path)
    {
        if (DirectorySeparatorChar.HasValue)
        {
            return path.Split(DirectorySeparatorChar.Value);
        }

        return path.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
    }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class PathResolverExtensions
{
    /// <summary>
    /// Resolves the path expression and returns the first result.
    /// </summary>
    /// <param name="resolver">The path resolver.</param>
    /// <param name="expression">The path expression to resolve.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The first path that matches the expression.</returns>
    public static string Resolve(this PathResolver resolver, string expression, CancellationToken token = default)
    {
        return resolver.EvaluateExpression(expression, token);
    }

    /// <summary>
    /// Resolves the path expression and returns the first result.
    /// </summary>
    /// <param name="resolver">The path resolver.</param>
    /// <param name="path">Segments of the path expression to resolve.</param>
    /// <returns>The first path that matches the expression.</returns>
    public static string Resolve(this PathResolver resolver, params string[] path)
    {
        return resolver.EvaluateExpression(path);
    }

    /// <summary>
    /// Resolves the path expression and returns all results.
    /// </summary>
    /// <param name="resolver">The path resolver.</param>
    /// <param name="path">The path expression to resolve.</param>
    /// <returns>All paths that match the expression.</returns>
    public static IEnumerable<string> ResolveAll(this PathResolver resolver, string path, CancellationToken token = default)
    {
        return resolver.EvaluateAll(path, token);
    }

    /// <summary>
    /// Resolves the path expression and returns all results.
    /// </summary>
    /// <param name="resolver">The path resolver.</param>
    /// <param name="path">Segments of the path expression to resolve.</param>
    /// <returns>All paths that match the expression.</returns>
    public static IEnumerable<string> ResolveAll(this PathResolver resolver, params string[] path)
    {
        return resolver.EvaluateAll(path);
    }
}