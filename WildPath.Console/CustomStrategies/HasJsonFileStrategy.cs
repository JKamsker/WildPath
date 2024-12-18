using Newtonsoft.Json.Linq;
using WildPath.Abstractions;
using WildPath.Internals;
using WildPath.Strategies.Custom;

namespace WildPath.Console.CustomStrategies;

/// <summary>
/// :hasJson(myJson.json, a.b.c, 123)
/// </summary>
public class JsonFileStrategy : ICustomStrategy
{
    private readonly IFileSystem _fileSystem;
    internal readonly string Marker;
    internal readonly string JsonPath;
    internal readonly string Expectation;

    public JsonFileStrategy(CustomStrategyCall call, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;

        if (call.Parameters.Length != 3)
        {
            throw new ArgumentException("Expected 3 parameters.");
        }

        JsonPath = call.Parameters[0];
        Marker = call.Parameters[1];
        Expectation = call.Parameters[2];
    }


    public bool Matches(string path)
    {
        return true;
    }

    public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    {
        var directories = _fileSystem.EnumerateDirectories(currentDirectory);

        foreach (var directory in directories)
        {
            token.ThrowIfCancellationRequested();

            // Check if the JSON file exists
            var jsonPath = _fileSystem.Combine(directory, JsonPath);
            if (!_fileSystem.FileExists(jsonPath))
            {
                continue;
            }

            var isValid = ValidateFile(jsonPath);
            if (isValid)
            {
                yield return _fileSystem.Combine(directory, jsonPath);
            }
        }
    }

    private bool ValidateFile(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var jObject = JObject.Parse(json);

        var tokens = jObject.SelectTokens(Marker);
        foreach (var token in tokens)
        {
            if (string.Equals(token.ToString(), Expectation, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}