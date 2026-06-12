using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace WildPath.SourceGenerator;

internal sealed class PathCallCandidate : IEquatable<PathCallCandidate>
{
    private const string PathResolverName = "global::WildPath.PathResolver";
    private const string PathResolverExtensionsName = "global::WildPath.PathResolverExtensions";

    public PathCallCandidate(
        string methodName,
        CallKind callKind,
        string rawPath,
        string segmentsSource,
        int interceptVersion,
        string interceptData,
        string displayLocation)
    {
        MethodName = methodName;
        CallKind = callKind;
        RawPath = rawPath;
        SegmentsSource = segmentsSource;
        InterceptVersion = interceptVersion;
        InterceptData = interceptData;
        DisplayLocation = displayLocation;
    }

    public string MethodName { get; }

    public CallKind CallKind { get; }

    public string RawPath { get; }

    public string SegmentsSource { get; }

    public int InterceptVersion { get; }

    public string InterceptData { get; }

    public string DisplayLocation { get; }

    public static PathCallCandidate? Create(GeneratorSyntaxContext context, CancellationToken token)
    {
        if (context.Node is not InvocationExpressionSyntax invocation ||
            context.SemanticModel.GetOperation(invocation, token) is not IInvocationOperation operation)
        {
            return null;
        }

        var method = operation.TargetMethod;
        if (method.Name is not ("Resolve" or "ResolveAll"))
        {
            return null;
        }

        var containingType = method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var callKind = containingType switch
        {
            PathResolverName => CallKind.Static,
            PathResolverExtensionsName => CallKind.Extension,
            _ => (CallKind?)null
        };

        if (callKind is null || TryGetPathLiteral(operation, out var rawPath) == false)
        {
            return null;
        }

#pragma warning disable RSEXPERIMENTAL002
        var location = context.SemanticModel.GetInterceptableLocation(invocation);
#pragma warning restore RSEXPERIMENTAL002
        if (location is null)
        {
            return null;
        }

        return new PathCallCandidate(
            method.Name,
            callKind.Value,
            rawPath,
            PathSegmentSource.Create(rawPath),
            location.Version,
            location.Data,
            location.GetDisplayLocation());
    }

    public bool Equals(PathCallCandidate? other)
    {
        return other is not null &&
               MethodName == other.MethodName &&
               CallKind == other.CallKind &&
               RawPath == other.RawPath &&
               SegmentsSource == other.SegmentsSource &&
               InterceptVersion == other.InterceptVersion &&
               InterceptData == other.InterceptData &&
               DisplayLocation == other.DisplayLocation;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PathCallCandidate);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = StringComparer.Ordinal.GetHashCode(MethodName);
            hash = (hash * 397) ^ CallKind.GetHashCode();
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(RawPath);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(SegmentsSource);
            hash = (hash * 397) ^ InterceptVersion.GetHashCode();
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(InterceptData);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(DisplayLocation);
            return hash;
        }
    }

    private static bool TryGetPathLiteral(IInvocationOperation operation, out string rawPath)
    {
        foreach (var argument in operation.Arguments)
        {
            if (argument.Parameter?.Type.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            if (argument.Value.ConstantValue is { HasValue: true, Value: string value })
            {
                rawPath = value;
                return true;
            }
        }

        rawPath = string.Empty;
        return false;
    }
}
