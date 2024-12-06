namespace PathResolver.Tests;

public class UnitTest1
{
    private static string[] GetExistingDirectories() =>
    [
        "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd",
        "C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48",
        "C:\\Test\\SubDir2\\SubSubDir2"
    ];

    [Theory]
    // Simple subpath
    [InlineData("C:\\Test", "SubDir1\\SubSubDir1", "C:\\Test\\SubDir1\\SubSubDir1")]
    
    // Go up with wildcard
    [InlineData(
        "C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48",
        "...\\**\\kxd",
        "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd"
    )]
    
    // Go up until we hit the parent
    [InlineData("C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48", "...\\SubDir1", "C:\\Test\\SubDir1")]
    [InlineData("C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48", "...\\Sub*", "C:\\Test\\SubDir1\\SubSubDir1")]
    [InlineData("C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48", "...\\*Dir2", "C:\\Test\\SubDir2")]
    [InlineData("C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48", "...\\Sub*Dir2", "C:\\Test\\SubDir2")]
    [InlineData("C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48", "...\\*ub*Dir2", "C:\\Test\\SubDir2")]
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
}