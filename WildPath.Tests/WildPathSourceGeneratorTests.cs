#if NET8_0
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using WildPath.SourceGenerator;

namespace WildPath.Tests;

public class WildPathSourceGeneratorTests
{
    [Fact]
    public void GeneratesInterceptorsForLiteralStaticCalls()
    {
        const string source = """
using System.Collections.Generic;
using System.Threading;
using WildPath;

internal class Consumer
{
    public string Resolve(CancellationToken token)
        => PathResolver.Resolve(@"root\**\leaf", token);

    public IEnumerable<string> ResolveAll()
        => PathResolver.ResolveAll(@"root\**\leaf");

    public string ResolveWithInstance(PathResolver resolver, CancellationToken token)
        => resolver.Resolve(@"root\**\leaf", token);

    public IEnumerable<string> ResolveAllWithInstance(PathResolver resolver)
        => resolver.ResolveAll(@"root\**\leaf");

    public string ResolveTagged()
        => PathResolver.Resolve(@"**\:tagged(.marker):\leaf");

    public string ResolveCustom()
        => PathResolver.Resolve(@":hasFile(test.txt):");

    public string ResolveSimpleWildcard()
        => PathResolver.Resolve(@"leaf*");

    public string ResolveSimpleWildcardWithPrefixAndSuffix()
        => PathResolver.Resolve(@"le*f*");

    public string ResolveComplexWildcard()
        => PathResolver.Resolve(@"l*e*f");
}
""";

        var generated = RunGenerator(source);

        Assert.Contains("global::WildPath.PathExpression.Create", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.Exact", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.AnyRecursive", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.Tagged, \":tagged(.marker):\", \".marker\"", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.Runtime, \":hasFile(test.txt):\"", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.SimpleWildcardStartsWith, \"leaf*\", \"leaf\"", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.SimpleWildcardStartsAndEnds, \"le*f*\", \"le\", \"f\"", generated);
        Assert.Contains("global::WildPath.PathExpressionSegmentKind.WildcardPattern, \"l*e*f\"", generated);
        Assert.Contains("global::System.Runtime.CompilerServices.InterceptsLocation", generated);
        Assert.Contains("global::WildPath.PathResolver.Resolve(__path0, token)", generated);
        Assert.Contains("global::WildPath.PathResolver.ResolveAll(__path0, token)", generated);
        Assert.Contains("this global::WildPath.PathResolver resolver", generated);
        Assert.Contains("global::WildPath.PathResolverExtensions.Resolve(resolver, __path0, token)", generated);
        Assert.Contains("global::WildPath.PathResolverExtensions.ResolveAll(resolver, __path0, token)", generated);
    }

    private static string RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Preview)
            .WithFeatures(new[]
            {
                new KeyValuePair<string, string>("InterceptorsNamespaces", "WildPath.Generated"),
                new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "WildPath.Generated")
            });
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            "WildPath.SourceGenerator.Tests",
            new[] { syntaxTree },
            GetReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new WildPathInterceptorGenerator().AsSourceGenerator() },
            parseOptions: parseOptions);

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var updatedCompilation,
            out var generatorDiagnostics);
        var result = driver.GetRunResult();

        Assert.Empty(generatorDiagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.Empty(result.Diagnostics.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.Empty(updatedCompilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        var tree = Assert.Single(result.GeneratedTrees, generatedTree => generatedTree.FilePath.EndsWith("WildPathInterceptors.g.cs"));
        return tree.GetText().ToString();
    }

    private static MetadataReference[] GetReferences()
    {
        var trustedPlatformAssemblies = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("Trusted platform assemblies were unavailable.");

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path))
            .Append(MetadataReference.CreateFromFile(typeof(PathResolver).Assembly.Location))
            .ToArray();
    }
}
#endif
