using WildPath.Abstractions;

namespace WildPath.Extensions;

public static class StrategyFactoryExtensions
{
    public static ISegmentStrategy Create(this IStrategyFactory factory, string segment)
    {
        if (factory.TryCreate(segment, out var strategy))
        {
            return strategy;
        }

        throw new InvalidOperationException($"No strategy found for segment '{segment}'.");
    }
}