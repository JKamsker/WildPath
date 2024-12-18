using WildPath.Abstractions;
using WildPath.Strategies.Custom;

namespace WildPath.Tests.Custom;

public class CustomStrategyFactoryTests
{
    [Fact]
    public void TryCreate_ValidSegment_CreatesCustomStrategySuccessfully()
    {
        // Arrange
        var fileSystemMock = new MockFileSystem();
        var factory = new CustomStrategyFactory(fileSystemMock);

        var expectedMarker = "test-marker";
        var segment = $":HasFileStrategy({expectedMarker}):";

        factory.AddStrategy<HasFileStrategy>("HasFileStrategy");

        // Act
        var success = factory.TryCreate(segment, out var strategy);

        // Assert
        Assert.True(success);
        Assert.NotNull(strategy);
        Assert.IsType<HasFileStrategy>(strategy);

        var hasFileStrategy = (HasFileStrategy)strategy!;
        Assert.Equal(expectedMarker, hasFileStrategy.Marker);
    }

    [Fact]
    public void TryCreate_InvalidSegment_ReturnsFalse()
    {
        // Arrange
        var fileSystemMock = new MockFileSystem();
        var factory = new CustomStrategyFactory(fileSystemMock);
        var segment = "InvalidSegment";

        // Act
        var success = factory.TryCreate(segment, out var strategy);

        // Assert
        Assert.False(success);
        Assert.Null(strategy);
    }

    [Fact]
    public void TryCreate_UnknownStrategy_ReturnsFalse()
    {
        // Arrange
        var fileSystemMock = new MockFileSystem();
        var factory = new CustomStrategyFactory(fileSystemMock);
        var segment = ":UnknownStrategy(param):";

        // Act
        var success = factory.TryCreate(segment, out var strategy);

        // Assert
        Assert.False(success);
        Assert.Null(strategy);
    }

    [Fact]
    public void TryCreate_StrategyWithInvalidConstructorParameters_ReturnsFalse()
    {
        // Arrange
        var fileSystemMock = new MockFileSystem();
        var factory = new CustomStrategyFactory(fileSystemMock);

        factory.AddStrategy<InvalidConstructorStrategy>("InvalidConstructorStrategy");
        var segment = ":InvalidConstructorStrategy(param):";

        // Act
        var success = factory.TryCreate(segment, out var strategy);

        // Assert
        Assert.False(success);
        Assert.Null(strategy);
    }

    public class InvalidConstructorStrategy : ICustomStrategy
    {
        public InvalidConstructorStrategy(string invalidParam)
        {
        }

        public bool Matches(string path)
        {
            return false;
        }

        public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
        {
            yield break;
        }
    }
}