using System.Diagnostics.CodeAnalysis;

namespace WildPath.Abstractions;

public interface IStrategyFactory
{
    bool TryCreate(string segment, [NotNullWhen(true)] out ISegmentStrategy? strategy);
}