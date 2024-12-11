using System.Net;

using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class StrategyFactory
{
    private readonly IFileSystem _fileSystem;

    public StrategyFactory(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public ISegmentStrategy Create(string segment) => segment switch
    {
        ".." => new ParentSegmentStrategy(segment, _fileSystem),
        "..." => new ParentsSegmentStrategy(segment, _fileSystem),
        "*" => new AnySegmentStrategy(segment, _fileSystem),
        "**" => new AnySegmentRecursivelyStrategy(segment, _fileSystem),
        _ => CreateStrategy(segment)
    };

    private ISegmentStrategy CreateStrategy(string segment)
    {
        if (TryCreateWildcardStrategy(segment, out var strategy))
        {
            return strategy;
        }

        if (TaggedSegmentStrategy.TryCreate(segment, _fileSystem, out strategy))
        {
            return strategy;
        }


        return new ExactMatchSegmentStrategy(segment, _fileSystem);
    }

    private bool TryCreateWildcardStrategy(string segment, out ISegmentStrategy strategy)
    {
        if (!segment.Contains('*'))
        {
            strategy = null;
            return false;
        }

        if (SimpleWildcardStrategy.TryCreate(segment, _fileSystem, out var simpleWildcardStrategy))
        {
            strategy = simpleWildcardStrategy;
            return true;
        }

        strategy = new WildcardSegmentStrategy(segment, _fileSystem);
        return true;
    }
}