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
}

public record CustomStrategyCall(string MethodName, StrategyCallParameterInfo[] Parameters);

/// <summary>
/// Parses a custom strategy call string into a <see cref="CustomStrategyCall"/> record.
/// Example: ":methodName(param1, param2):"
/// </summary>
internal partial class CustomStrategyCallParser
{
    private static readonly Regex _regex = MethodCallRegex();

    internal static CustomStrategyCall ExtractMethodCall(string input)
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