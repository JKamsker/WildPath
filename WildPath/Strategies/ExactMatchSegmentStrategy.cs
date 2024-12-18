using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class ExactMatchSegmentStrategy
    : SegmentStrategyBase, ISegmentStrategy, IParentSegmentAware
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    private IPathEvaluatorSegment _parentSegment;
    private bool _isRootSegment;

    IPathEvaluatorSegment IParentSegmentAware.ParentSegment
    {
        get => _parentSegment;
        set => UpdateParentSegment(value);
    }

    public ExactMatchSegmentStrategy(string segment, IFileSystem fileSystem)
        : base(fileSystem)
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public override bool Matches(string path)
    {
        var fileName = _fileSystem.GetFileName(path) ?? string.Empty;
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        return string.Equals(_segment, fileName, StringComparison.OrdinalIgnoreCase);
    }

    protected override IEnumerable<string> GetSource(string currentDirectory)
    {
        if (_isRootSegment)
        {
            return new[] { _segment };
        }

        return _fileSystem
            .EnumerateFileSystemEntries(currentDirectory);
    }

    // public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    // {
    //     var directories = GetSource(currentDirectory);
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

    private void UpdateParentSegment(IPathEvaluatorSegment value)
    {
        _parentSegment = value;
        _isRootSegment = IsRootDirectory(_segment);
    }

    private bool IsRootDirectory(string segment)
    {
        var isFirst = ((IParentSegmentAware)this).ParentSegment?.IsFirst ?? false;

        return isFirst
               && segment.Length == 2
               && segment[1] == ':'
               && segment[0] >= 'A' && segment[0] <= 'Z';
    }
}