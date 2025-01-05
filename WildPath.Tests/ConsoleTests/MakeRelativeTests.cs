#if NET8_0_OR_GREATER
using WildPath.Console.Utils;

namespace WildPath.Tests.ConsoleTests;

public class MakeRelativeTests
{
    [Fact]
    public void MakeRelative_SubPath()
    {
        // Arrange
        var basePath = @"C:\tmp";
        var path = @"C:\tmp\WildPath.Console\Commands\Tui";

        // Act
        var result = PathUtils.MakeRelative(path, basePath, '\\');

        // Assert
        Assert.Equal("WildPath.Console\\Commands\\Tui", result);
    }
    
    [Fact]
    public void MakeRelative_ParentPath()
    {
        // Arrange
        var basePath = @"C:\tmp\WildPath.Console\Commands\Tui";
        var path = @"C:\tmp";

        // Act
        var result = PathUtils.MakeRelative(path, basePath, '\\');

        // Assert
        Assert.Equal("..\\..\\..", result);
    }
    
    [Fact]
    public void MakeRelative_ParentPath_DifferentChild()
    {
        // Arrange
        var basePath = @"C:\tmp\WildPath.Console\Commands\Tui";
        var path = @"C:\tmp\SomeOtherFolder";

        // Act
        var result = PathUtils.MakeRelative(path, basePath, '\\');

        // Assert
        Assert.Equal("..\\..\\..\\SomeOtherFolder", result);
    }
    
    // [Fact]
    // public void MakeRelative_SamePath()
    // {
    //     // Arrange
    //     var basePath = @"C:\tmp\WildPath.Console\Commands\Tui";
    //     var path = @"C:\tmp\WildPath.Console\Commands\Tui";
    //
    //     // Act
    //     var result = PathUtils.MakeRelative(path, basePath, '\\');
    //
    //     // Assert
    //     Assert.Equal(".", result);
    // }
    
    [Fact]
    public void MakeRelative_SamePath_Sibling()
    {
        // Arrange
        var basePath = @"C:\tmp\WildPath.Console\Commands\Tui";
        var path = @"C:\tmp\WildPath.Console\Commands\SomeOtherCommand";

        // Act
        var result = PathUtils.MakeRelative(path, basePath, '\\');

        // Assert
        Assert.Equal("..\\SomeOtherCommand", result);
    }
}

#endif
