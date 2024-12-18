using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using WildPath.Extensions;

namespace WildPath.Internals;

/// <summary>
/// Represents information about a parameter in a custom strategy call.
/// </summary>
public record StrategyCallParameterInfo(string? Name, string Value, int Position = -1)
{
    // implicit to string conversion
    public static implicit operator string(StrategyCallParameterInfo parameter) => parameter.Value;

    public bool IsNamed => !string.IsNullOrWhiteSpace(Name);

    public StrategyCallParameterInfo EnsureUnnamed()
    {
        if (IsNamed)
        {
            throw new InvalidOperationException("Parameter is named.");
        }

        return this;
    }
}

public record CustomStrategyCall(string MethodName, StrategyCallParameterInfo[] Parameters)
{
    public bool TryGetParameter(string name, [NotNullWhen(true)] out StrategyCallParameterInfo? parameter)
    {
        parameter = Parameters.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        return parameter is not null;
    }

    /// <summary>
    /// Gets the parameter at the specified position.
    /// </summary>
    /// <param name="position">The position of the parameter. (Named parameters are not counted.)</param>
    /// <returns>The unnamed parameter at the specified position.</returns>
    /// <exception cref="ArgumentException">Thrown when the parameter is not found.</exception>
    public StrategyCallParameterInfo GetUnnamedParameter(int position)
    {
        var index = 0;
        foreach (var current in Parameters)
        {
            if (current.IsNamed)
            {
                continue;
            }

            if (index == position)
            {
                return current;
            }

            index++;
        }

        throw new ArgumentException("Parameter not found.");
    }
}

/// <summary>
/// Parses a custom strategy call string into a <see cref="CustomStrategyCall"/> record.
/// Example: ":methodName(param1, param2):"
/// </summary>
internal partial class CustomStrategyCallParser
{
    private static readonly Regex _regex = MethodCallRegex();

    internal static CustomStrategyCall ExtractMethodCallOld(string input)
    {
        var match = _regex.Match(input);
        if (!match.Success)
            throw new ArgumentException("Input string is not in the expected format.");

        // Extract the method name
        var methodName = match.Groups["method"].Value;

        // Extract the parameters string
        var paramsString = match.Groups["params"].Value;

        // Parse parameters into StrategyCallParameterInfo array
        var parameters = ParseParameters(paramsString);

        return new(methodName, parameters);
    }

    private static StrategyCallParameterInfo[] ParseParameters(string paramsString)
    {
        if (string.IsNullOrWhiteSpace(paramsString))
        {
            return Array.Empty<StrategyCallParameterInfo>();
        }

        return paramsString
            .Split(',')
            .Select(param => param.Trim())
            .Select((param, position) => ParseParameter(param, position))
            .ToArray();
    }

    private static StrategyCallParameterInfo ParseParameter(string param, int position)
    {
        if (!param.Contains(':'))
        {
            return new StrategyCallParameterInfo(null, param, position);
        }

        // Named parameter (key: value)
        var keyValue = param.Split(':', 2).Select(s => s.Trim()).ToArray();
        if (keyValue.Length == 2)
        {
            return new StrategyCallParameterInfo(keyValue[0], keyValue[1], position);
        }

        throw new ArgumentException($"Invalid named parameter format: {param}");
    }

    private static StrategyCallParameterInfo ParseParameter(ReadOnlySpan<char> param, int position)
    {
        if (!param.Contains(':'))
        {
            return new StrategyCallParameterInfo(null, param.ConvertToString(), position);
        }

        var firstPart = param.CutUntil(':', '\\').Trim();
        var secondPart = param.CutUntil(':', '\\').Trim();

        if (firstPart.Length > 0 && secondPart.Length > 0)
        {
            return new StrategyCallParameterInfo(firstPart.ConvertToString(), secondPart.ConvertToString(), position);
        }

        throw new ArgumentException($"Invalid named parameter format: {param.ConvertToString()}");
    }

#if NET48 || NETSTANDARD2_0
    private static Regex MethodCallRegex()
    {
        return new Regex(@":(?<method>\w+)\((?<params>.*?)\):", RegexOptions.Compiled | RegexOptions.Singleline);
    }
#else
    [GeneratedRegex(@":(?<method>\w+)\((?<params>.*?)\):", RegexOptions.Singleline)]
    private static partial Regex MethodCallRegex();
#endif
}