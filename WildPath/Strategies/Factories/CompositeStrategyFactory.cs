using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;

namespace WildPath.Strategies;

public class CompositeStrategyFactory(params IStrategyFactory[] factories) : IStrategyFactory
{
    public bool IsDefault
        => factories is [StrategyFactory, DefaultStrategyFactory];
    
    
    public static CompositeStrategyFactory CreateDefault(IFileSystem fileSystem)
    {
        return new CompositeStrategyFactory(
            new StrategyFactory(fileSystem),
            new DefaultStrategyFactory(fileSystem)
        );
    }

    public bool TryCreate(string segment, [NotNullWhen(true)] out ISegmentStrategy? strategy)
    {
        foreach (var factory in factories)
        {
            if (factory.TryCreate(segment, out var strat))
            {
                strategy = strat;
                return true;
            }
        }

        strategy = null;
        return false;
    }
}
