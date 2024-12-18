using WildPath.Abstractions;
using WildPath.Internals;
using WildPath.Strategies;
using WildPath.Strategies.Custom;

namespace WildPath;

public class PathResolverBuilder
{
    private IFileSystem? _fileSystem;
    private string? _currentDir;
    private Dictionary<string, Type> _strategies = new();

    public PathResolverBuilder()
    {
    }

    public PathResolverBuilder WithFileSystem(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
        return this;
    }

    public PathResolverBuilder WithCurrentDirectory(string currentDir)
    {
        _currentDir = currentDir;
        return this;
    }

    public PathResolverBuilder WithCustomStrategy<TStrategy>(string methodName)
        where TStrategy : ICustomStrategy
    {
        _strategies.Add(methodName, typeof(TStrategy));
        return this;
    }

    public PathResolver Build()
    {
        IStrategyFactory? factory = null;
        IFileSystem fileSystem = _fileSystem ?? RealFileSystem.Instance;

        if (_strategies.Count == 0)
        {
            factory = CompositeStrategyFactory.CreateDefault(fileSystem);
        }
        else
        {
            var customFactory = new CustomStrategyFactory(fileSystem);
            foreach (var (methodName, strategyType) in _strategies)
            {
                customFactory.AddStrategy(methodName, strategyType);
            }

            factory = customFactory;
        }


        return new PathResolver
        (
            currentDir: _currentDir,
            fileSystem: fileSystem,
            strategyFactory: factory
        );
    }
}