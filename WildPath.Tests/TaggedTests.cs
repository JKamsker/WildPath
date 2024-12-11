namespace WildPath.Tests;

/// <summary>
/// Tests the tagged strategy
/// </summary>
public class TaggedTests
{
    [Fact]
    public void Folder_Containing_Marker_Folder_Produces_Correct_Path()
    {
        // Arrange
        var currentDir = "C:\\Test";
        var existingDirectories = new[]
        {
            "C:\\Test\\SubDir1\\SubSubDir1\\bin\\Debug\\kxd",
            "C:\\Test\\SubDir1\\SubSubDir1\\obj\\Debug\\net48",
            
            "C:\\Test\\SubDir2\\SubSubDir1\\bin\\Debug\\kxd",
            "C:\\Test\\SubDir2\\SubSubDir1\\obj\\Debug\\net48",
            "C:\\Test\\SubDir2\\SubSubDir1\\.marker",
            
            "C:\\Test\\SubDir3\\SubSubDir1\\bin\\Debug\\kxd",
            "C:\\Test\\SubDir3\\SubSubDir1\\obj\\Debug\\net48",
        };

        var mockFileSystem = new MockFileSystem(currentDir, existingDirectories);
        var evaluator = new PathResolver(fileSystem: mockFileSystem);

        // Act
        var result = evaluator.EvaluateExpression("**\\:tagged(.marker):\\bin\\Debug\\kxd");

        // Assert
        Assert.Equal("C:\\Test\\SubDir2\\SubSubDir1\\bin\\Debug\\kxd", result);
    }

    //
}