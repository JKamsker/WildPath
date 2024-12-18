using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class ParentSegmentStrategy : SegmentStrategyBase, ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public ParentSegmentStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        if(!string.Equals(segment, "..", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("ParentSegmentStrategy can only be used with '..' segment", nameof(segment));
        }

        _segment = segment;
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path) => true;

    protected override IEnumerable<string> GetSource(string currentDirectory)
    {
        var parentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
        if (parentDirectory == null)
        {
            yield break;
        }
        
        yield return parentDirectory;
    }
    
    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     if (token.IsCancellationRequested)
    //     {
    //         yield break;
    //     }
    //     
    //     var parentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
    //     if (parentDirectory == null)
    //     {
    //         yield break;
    //     }
    //
    //     if (child == null)
    //     {
    //         yield return parentDirectory;
    //         yield break;
    //     }
    //
    //     foreach (var result in child.Evaluate(parentDirectory, token))
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //         
    //         yield return result;
    //     }
    // }
}