namespace WildPath.Internals;

internal partial class CustomStrategyCallParser
{
    /// <summary>
    /// Parses a custom strategy call string into a <see cref="CustomStrategyCall"/> record.
    /// Example: ":methodName(param1, param2):"
    /// </summary>
    internal static CustomStrategyCall ExtractMethodCall(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input string is not in the expected format.");
        }

        var startsWithColon = input.StartsWith(':');
        var endsWithBracketAndColon = input.EndsWith("):");
        if (!startsWithColon || !endsWithBracketAndColon)
        {
            throw new ArgumentException("Input string is not in the expected format.");
        }
        
        var inputSpan = input.AsSpan();

        var bracketStart = input.IndexOf('(');
        var methodName = inputSpan[1..bracketStart].Trim().ConvertToString();
        
        var parameterStart = bracketStart + 1;
        var parameterEnd = input.Length - 2;

        var parameterPart = inputSpan[parameterStart..parameterEnd];

        var parameters = new List<StrategyCallParameterInfo>();
        var index = 0;
        while (true)
        {
            var commaIndex = parameterPart.IndexOfUnescaped(',', '\\');
            if (commaIndex != -1)
            {
                var parameter = parameterPart[..commaIndex].Trim();
                var parameterInfo = ParseParameter(parameter, index++);
                parameters.Add(parameterInfo);

                parameterPart = parameterPart[(commaIndex + 1)..];
            }
            else
            {
                if (parameterPart.Length > 0 || parameters.Count > 0)
                {
                    var lastParameter = parameterPart.Trim();
                    var lastParameterInfo = ParseParameter(lastParameter, index);
                    parameters.Add(lastParameterInfo);
                }

                break;
            }
        }

        return new(methodName, parameters.ToArray());
    }
}