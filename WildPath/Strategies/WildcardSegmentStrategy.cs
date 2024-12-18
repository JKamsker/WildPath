using System.Text.RegularExpressions;

using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class WildcardSegmentStrategy : SegmentStrategyBase, ISegmentStrategy
{
    private readonly string _pattern;
    private readonly IFileSystem _fileSystem;
    private readonly Regex _regex;

    public WildcardSegmentStrategy(string segment, IFileSystem fileSystem) 
        : base(fileSystem)
    {
        _pattern = "^" + Regex.Escape(segment).Replace("\\*", ".*") + "$";
        _regex = new Regex(_pattern, RegexOptions.Compiled);
        
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path)
    {
        var fileName = _fileSystem.GetFileName(path) ?? string.Empty;
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }
        
        return _regex.IsMatch(fileName);
    }

    protected override IEnumerable<string> GetSource(string currentDirectory) 
        => _fileSystem.EnumerateFileSystemEntries(currentDirectory);

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = _fileSystem
    //         .EnumerateFileSystemEntries(currentDirectory);
    //
    //     foreach (var directory in directories)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //         
    //         if (!Matches(directory))
    //         {
    //             continue;
    //         }
    //
    //         if (child == null)
    //         {
    //             yield return directory;
    //         }
    //         else
    //         {
    //             foreach (var subDir in child.Evaluate(directory, token))
    //             {
    //                 if (token.IsCancellationRequested)
    //                 {
    //                     yield break;
    //                 }
    //
    //                 yield return subDir;
    //             }
    //         }
    //     }
    // }
}