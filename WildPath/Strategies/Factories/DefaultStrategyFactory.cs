using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;

namespace WildPath.Strategies;

public class DefaultStrategyFactory : IStrategyFactory
{
    private readonly IFileSystem _fileSystem;

    public DefaultStrategyFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public bool TryCreate(string segment, out ISegmentStrategy strategy)
    {
        strategy = new ExactMatchSegmentStrategy(segment, _fileSystem);
        return true;
    }
}