using WildPath.Abstractions;

namespace WildPath.Strategies;

internal class AnySegmentRecursivelyStrategy : SegmentStrategyBase, ISegmentStrategy
{
    [ThreadStatic]
    private static Stack<string>? _cachedFirstResultStack;

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

    internal override string? EvaluateFirst(
        string currentDirectory,
        PathEvaluatorSegment? child,
        CancellationToken token = default
    )
    {
        if (token.IsCancellationRequested)
        {
            return null;
        }

        if (child == null)
        {
            return currentDirectory;
        }

        var stack = RentFirstResultStack();
        stack.Push(currentDirectory);

        try
        {
            while (stack.Count > 0)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                var directory = stack.Pop();
                var result = child.EvaluateFirst(directory, token);
                if (result is not null)
                {
                    return result;
                }

                if (_fileSystem is IFileSystemEntryEnumerable enumerable)
                {
                    var visitor = new PushDirectoryVisitor(stack);
                    enumerable.VisitDirectories(directory, ref visitor);
                }
                else
                {
                    foreach (var subDirectory in _fileSystem.EnumerateDirectories(directory))
                    {
                        stack.Push(subDirectory);
                    }
                }
            }

            return null;
        }
        finally
        {
            ReturnFirstResultStack(stack);
        }
    }

    private static Stack<string> RentFirstResultStack()
    {
        var stack = _cachedFirstResultStack;
        if (stack is null)
        {
            return new Stack<string>();
        }

        _cachedFirstResultStack = null;
        return stack;
    }

    private static void ReturnFirstResultStack(Stack<string> stack)
    {
        stack.Clear();
        _cachedFirstResultStack = stack;
    }

    private struct PushDirectoryVisitor : IFileSystemEntryVisitor
    {
        private readonly Stack<string> _stack;

        public PushDirectoryVisitor(Stack<string> stack)
        {
            _stack = stack;
        }

        public bool Visit(string path)
        {
            _stack.Push(path);
            return true;
        }
    }

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
