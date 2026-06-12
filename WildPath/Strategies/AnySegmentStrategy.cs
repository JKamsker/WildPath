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

    internal override string? EvaluateFirst(
        string currentDirectory,
        PathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (_fileSystem is IFileSystemEntryEnumerable enumerable)
        {
            var visitor = new FirstDirectoryVisitor(child, token);
            enumerable.VisitDirectories(currentDirectory, ref visitor);
            return visitor.Result;
        }

        foreach (var directory in _fileSystem.EnumerateDirectories(currentDirectory))
        {
            if (token.IsCancellationRequested)
            {
                return null;
            }

            if (child == null)
            {
                return directory;
            }

            var result = child.EvaluateFirst(directory, token);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private struct FirstDirectoryVisitor : IFileSystemEntryVisitor
    {
        private readonly PathEvaluatorSegment? _child;
        private readonly CancellationToken _token;

        public FirstDirectoryVisitor(PathEvaluatorSegment? child, CancellationToken token)
        {
            _child = child;
            _token = token;
            Result = null;
        }

        public string? Result { get; private set; }

        public bool Visit(string path)
        {
            if (_token.IsCancellationRequested)
            {
                return false;
            }

            if (_child == null)
            {
                Result = path;
                return false;
            }

            Result = _child.EvaluateFirst(path, _token);
            return Result is null;
        }
    }

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
