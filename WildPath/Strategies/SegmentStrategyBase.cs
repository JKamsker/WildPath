using WildPath.Abstractions;

namespace WildPath.Strategies;

public abstract class SegmentStrategyBase : ISegmentStrategy
{
    private readonly IFileSystem _fileSystem;

    public SegmentStrategyBase(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public abstract bool Matches(string path);
    protected abstract IEnumerable<string> GetSource(string currentDirectory);

    public virtual IEnumerable<string> Evaluate(
        string currentDirectory,
        IPathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        var fsEntries = GetSource(currentDirectory);

        foreach (var fsEntry in fsEntries)
        {
            if (token.IsCancellationRequested)
            {
                yield break;
            }
            
            if(!Matches(fsEntry))
            {
                continue;
            }

            if (child == null)
            {
                yield return fsEntry;
                continue;
            }

            foreach (var subDir in child.Evaluate(fsEntry, token))
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