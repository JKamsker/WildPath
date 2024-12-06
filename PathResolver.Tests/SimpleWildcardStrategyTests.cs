using PathResolver.Strategies;

namespace PathResolver.Tests;

public class SimpleWildcardStrategyTests
{
    [Fact]
    public void Matches_Wildcard_At_The_End()
    {
        // Arrange
        _ = SimpleWildcardStrategy.TryCreate("Hello*", null, out var strategy)
            ? true
            : throw new Exception("Failed to create strategy.");

        // Act
        var result = strategy.Matches("Hello World");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_Wildcard_At_The_Beginning()
    {
        // Arrange
        _ = SimpleWildcardStrategy.TryCreate("*World", null, out var strategy)
            ? true
            : throw new Exception("Failed to create strategy.");

        // Act
        var result = strategy.Matches("Hello World");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_Wildcard_At_The_Beginning_And_End()
    {
        // Arrange
        _ = SimpleWildcardStrategy.TryCreate("*Wor*", null, out var strategy)
            ? true
            : throw new Exception("Failed to create strategy.");

        // Act
        var result = strategy.Matches("Hello World");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_In_The_Middle()
    {
        // Arrange
        _ = SimpleWildcardStrategy.TryCreate("Hel*rld", null, out var strategy)
            ? true
            : throw new Exception("Failed to create strategy.");

        // Act
        var result = strategy.Matches("Hello Beautiful World");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryCreate_Too_Many_Wildcards_Fails()
    {
        var result = SimpleWildcardStrategy.TryCreate("He*llo*Wor*", null, out var strategy);
        Assert.False(result, "Patterns with three wildcards should fail creation.");
    }

    [Fact]
    public void Matches_No_Wildcard_Fails()
    {
        var result = SimpleWildcardStrategy.TryCreate("Hello", null, out var strategy);
        Assert.False(result, "Patterns without wildcards should succeed creation.");
    }
}