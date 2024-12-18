using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class AnySegmentStrategy : SegmentStrategyBase, ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public AnySegmentStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path) => true;

    protected override IEnumerable<string> GetSource(string currentDirectory)
        => _fileSystem.EnumerateDirectories(currentDirectory);

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory))
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
}