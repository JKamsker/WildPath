using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class ExactMatchSegmentStrategy : ISegmentStrategy, IParentSegmentAware
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
    {
        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => string.Equals(_segment, path, StringComparison.OrdinalIgnoreCase);

    public IEnumerable<string> Evaluate(string currentDirectory, IPathEvaluatorSegment? child, CancellationToken token = default)
    {
        IEnumerable<string> directories = Array.Empty<string>();
        if (_isRootSegment)
        {
            directories = new[] { _segment };
        }
        else
        {
            directories = _fileSystem
                .EnumerateFileSystemEntries(currentDirectory);
        }

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
                continue;
            }

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