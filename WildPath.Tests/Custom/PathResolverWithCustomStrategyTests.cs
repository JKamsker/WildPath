namespace WildPath.Tests.Custom;

public class PathResolverWithCustomStrategyTests
{
    [Fact]
    public void Test()
    {
        var mockFs = new MockFileSystem(
            currentDirectory: "C:/",
            fsEntries:
            [
                "C:/a/c",
                "C:/a/b/test.txt",
            ],
            directorySeparatorChar: '/'
        );

        var resolver = PathResolver.Create(builder =>
        {
            builder.WithCustomStrategy<HasFileStrategy>("hasFile");
            builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
            builder.WithFileSystem(mockFs);
            builder.WithCurrentDirectory("C:/");
        });

        var result = resolver.Resolve("**/:hasFile(test.txt):");
        Assert.Equal("C:/a/b", result);
    }
}