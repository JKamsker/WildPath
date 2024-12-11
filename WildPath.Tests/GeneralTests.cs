namespace WildPath.Tests;

public class GeneralTests
{
    private static string[] GetExistingDirectories() =>
    [
        "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd",
        "C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48",
        "C:\\Test\\SubDir2\\SubSubDir2"
    ];

    private const string DebugDir = "C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48";

    [Theory]
    // Simple subpath
    [InlineData("C:\\Test", "SubDir1\\SubSubDir1", "C:\\Test\\SubDir1\\SubSubDir1")]

    // Go up with wildcard
    [InlineData(DebugDir, "...\\**\\kxd", "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd")]

    // Go up until we hit the parent
    [InlineData(DebugDir, "...\\SubDir1", "C:\\Test\\SubDir1")]
    [InlineData(DebugDir, "...\\Sub*", "C:\\Test\\SubDir1\\SubSubDir1")]
    [InlineData(DebugDir, "...\\*Dir2", "C:\\Test\\SubDir2")]
    [InlineData(DebugDir, "...\\Sub*Dir2", "C:\\Test\\SubDir2")]
    [InlineData(DebugDir, "...\\*ub*Dir2", "C:\\Test\\SubDir2")]
    public void EvaluateExpression_ReturnsCorrectDirectory_WhenSingleMatch(
        string currentdir,
        string expression,
        string expectation
    )
    {
        // Arrange
        var existingDirectories = GetExistingDirectories();

        var mockFileSystem = new MockFileSystem(currentdir, existingDirectories);
        var evaluator = new PathResolver(fileSystem: mockFileSystem);

        // Act
        var result = evaluator.EvaluateExpression(expression);

        // Assert
        Assert.Equal(expectation, result);
    }
    
    // Rooted path
    [Fact]
    public void RootedDirectory_ReturnsCorrectDirectory_Ignores_CurrentDir()
    {
        // Arrange
        var currentDir = "C:\\Test";
        var existingDirectories = GetExistingDirectories();

        var mockFileSystem = new MockFileSystem(currentDir, existingDirectories);
        var evaluator = new PathResolver(fileSystem: mockFileSystem);

        // Act
        var result = evaluator.EvaluateExpression("C:\\Test\\SubDir1\\SubSubDir*");

        // Assert
        Assert.Equal("C:\\Test\\SubDir1\\SubSubDir1", result);
    }
}