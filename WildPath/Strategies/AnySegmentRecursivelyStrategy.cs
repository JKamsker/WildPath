using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class AnySegmentRecursivelyStrategy : SegmentStrategyBase,  ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public AnySegmentRecursivelyStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }
    
    public override bool Matches(string path) => true;
    
    protected override IEnumerable<string> GetSource(string currentDirectory)
        => EnumerateAllSubdirectories(currentDirectory);

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = EnumerateAllSubdirectories(currentDirectory);
    //     foreach (var directory in directories)
    //     {
    //         if (token.IsCancellationRequested)
    //         {
    //             yield break;
    //         }
    //
    //         if (child == null)
    //         {
    //             yield return directory;
    //             continue;
    //         }
    //
    //         foreach (var subDir in child.Evaluate(directory, token))
    //         {
    //             if (token.IsCancellationRequested)
    //             {
    //                 yield break;
    //             }
    //             
    //             yield return subDir;
    //         }
    //     }
    // }

    private IEnumerable<string> EnumerateAllSubdirectories(string currentDirectory)
    {
        var stack = new Stack<string>();
        stack.Push(currentDirectory);

        while (stack.Count > 0)
        {
            var directory = stack.Pop();
            yield return directory;

            foreach (var subDirectory in _fileSystem.EnumerateDirectories(directory))
            {
                stack.Push(subDirectory);
            }
        }
    }
}