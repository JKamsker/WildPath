#if NET8_0
namespace WildPath.Tests;

public class PathExpressionAllocationTests
{
    [Fact]
    public void ResolveCompiledRootPath_DoesNotAllocateOnWarmCall()
    {
        var expression = PathExpression.Create(
            "C:",
            new[]
            {
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "C:")
            });

        _ = PathResolver.Resolve(expression);
        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = PathResolver.Resolve(expression);
            }
        });

        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void ResolveCompiledExactPath_UsesLookupWithoutEnumerating()
    {
        var fileSystem = new LookupOnlyFileSystem(
            currentDirectory: "C:",
            entries:
            [
                new("C:", "Test", @"C:\Test"),
                new(@"C:\Test", "SubDir1", @"C:\Test\SubDir1"),
                new(@"C:\Test\SubDir1", "SubSubDir1", @"C:\Test\SubDir1\SubSubDir1")
            ]);
        var resolver = new PathResolver(fileSystem: fileSystem);
        var expression = PathExpression.Create(
            @"Test\SubDir1\SubSubDir1",
            [
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "Test"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "SubDir1"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "SubSubDir1")
            ]);

        var result = resolver.Resolve(expression);

        Assert.Equal(@"C:\Test\SubDir1\SubSubDir1", result);
        Assert.Equal(3, fileSystem.LookupCount);
        Assert.Equal(0, fileSystem.EnumerableEnumerationCount);
        Assert.Equal(0, fileSystem.VisitorEnumerationCount);

        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = resolver.Resolve(expression);
            }
        });
        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void ResolveCompiledTaggedPath_UsesLookupWithoutCombiningMarkerPath()
    {
        var fileSystem = new LookupOnlyFileSystem(
            currentDirectory: "C:",
            entries:
            [
                new(@"C:\WithMarker", ".marker", @"C:\WithMarker\.marker"),
                new(@"C:\WithMarker", "leaf", @"C:\WithMarker\leaf")
            ],
            directories:
            [
                new("C:", [@"C:\WithoutMarker", @"C:\WithMarker"])
            ]);
        var resolver = new PathResolver(fileSystem: fileSystem);
        var expression = PathExpression.Create(
            @":tagged(.marker):\leaf",
            [
                new PathExpressionSegment(
                    PathExpressionSegmentKind.Tagged,
                    ":tagged(.marker):",
                    ".marker"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "leaf")
            ]);

        var result = resolver.Resolve(expression);

        Assert.Equal(@"C:\WithMarker\leaf", result);
        Assert.Equal(0, fileSystem.EnumerableEnumerationCount);
        Assert.Equal(1, fileSystem.VisitorEnumerationCount);
        Assert.Equal(3, fileSystem.LookupCount);
        Assert.Equal(0, fileSystem.CombineCount);

        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = resolver.Resolve(expression);
            }
        });
        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void ResolveCompiledRecursivePath_ReusesLookupForChildMatch()
    {
        var fileSystem = new LookupOnlyFileSystem(
            currentDirectory: "C:",
            entries:
            [
                new(@"C:\Branch", "leaf", @"C:\Branch\leaf")
            ],
            directories:
            [
                new("C:", [@"C:\Branch"])
            ]);
        var resolver = new PathResolver(fileSystem: fileSystem);
        var expression = PathExpression.Create(
            @"**\leaf",
            [
                new PathExpressionSegment(PathExpressionSegmentKind.AnyRecursive, "**"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "leaf")
            ]);

        var result = resolver.Resolve(expression);

        Assert.Equal(@"C:\Branch\leaf", result);
        Assert.Equal(0, fileSystem.EnumerableEnumerationCount);
        Assert.Equal(2, fileSystem.VisitorEnumerationCount);
        Assert.Equal(2, fileSystem.LookupCount);

        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = resolver.Resolve(expression);
            }
        });
        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void ResolveCompiledSimpleWildcardPath_DoesNotAllocateOnWarmCall()
    {
        var fileSystem = new LookupOnlyFileSystem(
            currentDirectory: "C:",
            entries: [],
            directories:
            [
                new("C:", [@"C:\other", @"C:\leaf-one"])
            ]);
        var resolver = new PathResolver(fileSystem: fileSystem);
        var expression = PathExpression.Create(
            "leaf*",
            [
                new PathExpressionSegment(
                    PathExpressionSegmentKind.SimpleWildcardStartsWith,
                    "leaf*",
                    "leaf")
            ]);

        var result = resolver.Resolve(expression);

        Assert.Equal(@"C:\leaf-one", result);
        Assert.Equal(0, fileSystem.EnumerableEnumerationCount);
        Assert.Equal(1, fileSystem.VisitorEnumerationCount);

        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = resolver.Resolve(expression);
            }
        });
        Assert.Equal(0, allocatedBytes);
    }

    [Fact]
    public void ResolveCompiledComplexWildcardPath_DoesNotAllocateOnWarmCall()
    {
        var fileSystem = new LookupOnlyFileSystem(
            currentDirectory: "C:",
            entries: [],
            directories:
            [
                new("C:", [@"C:\other", @"C:\leaf"])
            ]);
        var resolver = new PathResolver(fileSystem: fileSystem);
        var expression = PathExpression.Create(
            "l*e*f",
            [
                new PathExpressionSegment(
                    PathExpressionSegmentKind.WildcardPattern,
                    "l*e*f")
            ]);

        var result = resolver.Resolve(expression);

        Assert.Equal(@"C:\leaf", result);
        Assert.Equal(0, fileSystem.EnumerableEnumerationCount);
        Assert.Equal(1, fileSystem.VisitorEnumerationCount);

        var allocatedBytes = MeasureAllocatedBytes(() =>
        {
            for (var index = 0; index < 1_000; index++)
            {
                _ = resolver.Resolve(expression);
            }
        });
        Assert.Equal(0, allocatedBytes);
    }

    private static long MeasureAllocatedBytes(Action action)
    {
        action();

        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

}
#endif
