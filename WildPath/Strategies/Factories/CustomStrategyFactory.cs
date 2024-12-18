using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using WildPath.Abstractions;
using WildPath.Internals;

namespace WildPath.Strategies.Custom;

public class CustomStrategyFactory : IStrategyFactory
{
    private readonly IFileSystem _fileSystem;
    private Dictionary<string, Type> _strategies = new();

    public CustomStrategyFactory(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem;
    }

    public bool TryCreate(string segment, [NotNullWhen(true)] out ISegmentStrategy? strategy)
    {
        var startsAndEndsWithColon = segment.StartsWith(':') && segment.EndsWith(':');
        if (!startsAndEndsWithColon)
        {
            strategy = default;
            return false;
        }

        var call = CustomStrategyCallParser.ExtractMethodCall(segment);
        if (!_strategies.TryGetValue(call.MethodName, out var strategyType))
        {
            strategy = default;
            return false;
        }

        var constructor = GetConstructor(strategyType);
        if (constructor is null)
        {
            strategy = default;
            return false;
        }

        var parameterInfos = constructor.GetParameters();
        var parameters = new object[parameterInfos.Length];
        for (var i = 0; i < parameterInfos.Length; i++)
        {
            var parameterInfo = parameterInfos[i];
            if (parameterInfo.ParameterType == typeof(CustomStrategyCall))
            {
                parameters[i] = call;
            }
            else if (parameterInfo.ParameterType == typeof(IFileSystem))
            {
                parameters[i] = _fileSystem;
            }
        }

        strategy = (ICustomStrategy)constructor.Invoke(parameters);
        return true;
    }

    private ConstructorInfo? GetConstructor(Type strategyType)
    {
        // Prefer constructors with CustomStrategyCall and IFileSystem or one of them.
        // Accept empty constructors.
        // Decline constructors with other parameters.
        var constructors = strategyType.GetConstructors()
            .OrderByDescending(c =>
            {
                var parameters = c.GetParameters();
                var score = 0;
                foreach (var parameter in parameters)
                {
                    if (parameter.ParameterType == typeof(CustomStrategyCall))
                    {
                        score += 1;
                    }
                    else if (parameter.ParameterType == typeof(IFileSystem))
                    {
                        score += 1;
                    }
                }

                return score;
            });

        foreach (var constructor in constructors)
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                return constructor;
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(CustomStrategyCall))
            {
                return constructor;
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(IFileSystem))
            {
                return constructor;
            }

            if (parameters.Length != 2)
            {
                continue;
            }

            var hasCustomStrategyCall = parameters.Any(p => p.ParameterType == typeof(CustomStrategyCall));
            var hasFileSystem = parameters.Any(p => p.ParameterType == typeof(IFileSystem));
            if (hasCustomStrategyCall && hasFileSystem)
            {
                return constructor;
            }
        }

        return null;
    }

    public void AddStrategy<T>(string name)
        where T : ICustomStrategy
    {
        _strategies.Add(name, typeof(T));
    }
    
    internal void AddStrategy(string name, Type strategyType)
    {
        _strategies.Add(name, strategyType);
    }
}