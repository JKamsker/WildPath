using System.Text.RegularExpressions;

using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class WildcardSegmentStrategy : ISegmentStrategy
{
    private readonly string _pattern;
    private readonly IFileSystem _fileSystem;
    private readonly Regex _regex;

    public WildcardSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        _pattern = "^" + Regex.Escape(segment).Replace("\\*", ".*") + "$";
        _regex = new Regex(_pattern, RegexOptions.Compiled);
        
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => _regex.IsMatch(path);

    public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    {
        var directories = _fileSystem
            .EnumerateFileSystemEntries(currentDirectory);

        foreach (var directory in directories)
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }
            
            if (!Matches(_fileSystem.GetFileName(directory) ?? string.Empty))
            {
                continue;
            }

            if (child == null)
            {
                yield return directory;
            }
            else
            {
                foreach (var subDir in child.Evaluate(directory, token))
                {
                    if (token.IsCancellationRequested)
                    {
                        yield break;
                    }

                    yield return subDir;
                }
            }
        }
    }
}