using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;

namespace WildPath.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class PathResolutionBenchmarks
{
    private readonly string _complexWildcardPath = "l*e*f";
    private readonly string _exactPath = @"Test\SubDir1\SubSubDir1";
    private readonly string _recursivePath = @"**\leaf";
    private readonly string _simpleWildcardPath = "leaf*";
    private readonly string _taggedPath = @":tagged(.marker):\leaf";

    private PathExpression _complexWildcardExpression = null!;
    private PathExpression _exactExpression = null!;
    private PathExpression _recursiveExpression = null!;
    private PathResolver _resolver = null!;
    private PathExpression _simpleWildcardExpression = null!;
    private PathExpression _taggedExpression = null!;

    [GlobalSetup]
    public void Setup()
    {
        _resolver = new PathResolver(fileSystem: BenchmarkFileSystem.Create());
        _exactExpression = PathExpression.Create(
            _exactPath,
            [
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "Test"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "SubDir1"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "SubSubDir1")
            ]);
        _recursiveExpression = PathExpression.Create(
            _recursivePath,
            [
                new PathExpressionSegment(PathExpressionSegmentKind.AnyRecursive, "**"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "leaf")
            ]);
        _simpleWildcardExpression = PathExpression.Create(
            _simpleWildcardPath,
            [
                new PathExpressionSegment(
                    PathExpressionSegmentKind.SimpleWildcardStartsWith,
                    "leaf*",
                    "leaf")
            ]);
        _complexWildcardExpression = PathExpression.Create(
            _complexWildcardPath,
            [
                new PathExpressionSegment(PathExpressionSegmentKind.WildcardPattern, "l*e*f")
            ]);
        _taggedExpression = PathExpression.Create(
            _taggedPath,
            [
                new PathExpressionSegment(
                    PathExpressionSegmentKind.Tagged,
                    ":tagged(.marker):",
                    ".marker"),
                new PathExpressionSegment(PathExpressionSegmentKind.Exact, "leaf")
            ]);

        WarmUpExpressionCaches();
    }

    [BenchmarkCategory("Exact")]
    [Benchmark]
    public string Exact_DynamicString() => _resolver.Resolve(_exactPath);

    [BenchmarkCategory("Exact")]
    [Benchmark]
    public string Exact_GeneratedLiteral() => _resolver.Resolve(@"Test\SubDir1\SubSubDir1");

    [BenchmarkCategory("Exact")]
    [Benchmark]
    public string Exact_CompiledExpression() => _resolver.Resolve(_exactExpression);

    [BenchmarkCategory("Recursive")]
    [Benchmark]
    public string Recursive_DynamicString() => _resolver.Resolve(_recursivePath);

    [BenchmarkCategory("Recursive")]
    [Benchmark]
    public string Recursive_GeneratedLiteral() => _resolver.Resolve(@"**\leaf");

    [BenchmarkCategory("Recursive")]
    [Benchmark]
    public string Recursive_CompiledExpression() => _resolver.Resolve(_recursiveExpression);

    [BenchmarkCategory("SimpleWildcard")]
    [Benchmark]
    public string SimpleWildcard_DynamicString() => _resolver.Resolve(_simpleWildcardPath);

    [BenchmarkCategory("SimpleWildcard")]
    [Benchmark]
    public string SimpleWildcard_GeneratedLiteral() => _resolver.Resolve("leaf*");

    [BenchmarkCategory("SimpleWildcard")]
    [Benchmark]
    public string SimpleWildcard_CompiledExpression() => _resolver.Resolve(_simpleWildcardExpression);

    [BenchmarkCategory("ComplexWildcard")]
    [Benchmark]
    public string ComplexWildcard_DynamicString() => _resolver.Resolve(_complexWildcardPath);

    [BenchmarkCategory("ComplexWildcard")]
    [Benchmark]
    public string ComplexWildcard_GeneratedLiteral() => _resolver.Resolve("l*e*f");

    [BenchmarkCategory("ComplexWildcard")]
    [Benchmark]
    public string ComplexWildcard_CompiledExpression() => _resolver.Resolve(_complexWildcardExpression);

    [BenchmarkCategory("Tagged")]
    [Benchmark]
    public string Tagged_DynamicString() => _resolver.Resolve(_taggedPath);

    [BenchmarkCategory("Tagged")]
    [Benchmark]
    public string Tagged_GeneratedLiteral() => _resolver.Resolve(@":tagged(.marker):\leaf");

    [BenchmarkCategory("Tagged")]
    [Benchmark]
    public string Tagged_CompiledExpression() => _resolver.Resolve(_taggedExpression);

    private void WarmUpExpressionCaches()
    {
        _ = _resolver.Resolve(_exactExpression);
        _ = _resolver.Resolve(_recursiveExpression);
        _ = _resolver.Resolve(_simpleWildcardExpression);
        _ = _resolver.Resolve(_complexWildcardExpression);
        _ = _resolver.Resolve(_taggedExpression);
    }
}
