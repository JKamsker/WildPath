﻿using WildPath.Abstractions;

namespace WildPath.Strategies;

public class ParentSegmentStrategy : ISegmentStrategy
{
    private readonly string _segment;
    private readonly IFileSystem _fileSystem;

    public ParentSegmentStrategy(string segment, IFileSystem fileSystem)
    {
        if(!string.Equals(segment, "..", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("ParentSegmentStrategy can only be used with '..' segment", nameof(segment));
        }

        _segment = segment;
        _fileSystem = fileSystem;
    }

    public bool Matches(string path) => true;

    public IEnumerable<string> Evaluate(string currentDirectory, PathEvaluatorSegment? child)
    {
        var parentDirectory = _fileSystem.GetDirectoryName(currentDirectory);
        if (parentDirectory == null) yield break;

        if (child == null)
        {
            yield return parentDirectory;
            yield break;
        }

        foreach (var result in child.Evaluate(parentDirectory))
        {
            yield return result;
        }
    }
}