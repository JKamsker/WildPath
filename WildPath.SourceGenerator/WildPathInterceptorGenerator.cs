using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WildPath.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public sealed class WildPathInterceptorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var calls = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name.Identifier.ValueText: "Resolve" or "ResolveAll"
                    }
                },
                transform: static (syntaxContext, token) => PathCallCandidate.Create(syntaxContext, token))
            .Where(static candidate => candidate is not null)
            .Select(static (candidate, _) => candidate!);

        context.RegisterSourceOutput(
            calls.Collect(),
            static (sourceContext, candidates) => InterceptorSourceEmitter.Emit(sourceContext, candidates));
    }
}
