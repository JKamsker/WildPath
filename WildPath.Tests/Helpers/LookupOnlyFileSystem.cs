#if NET8_0
using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;

namespace WildPath.Tests;

internal sealed class LookupOnlyFileSystem :
    IFileSystem,
    IFileSystemEntryLookup,
    IFileSystemEntryEnumerable
{
    private readonly Dictionary<string, string[]> _directories;
    private readonly Dictionary<(string Directory, string EntryName), string> _entries;

    public LookupOnlyFileSystem(
        string currentDirectory,
        IEnumerable<(string Directory, string EntryName, string Path)> entries,
        IEnumerable<(string Directory, string[] Entries)>? directories = null)
    {
        CurrentDirectory = currentDirectory;
        _entries = new Dictionary<(string, string), string>();
        foreach (var entry in entries)
        {
            _entries.Add((entry.Directory, entry.EntryName), entry.Path);
        }

        _directories = new Dictionary<string, string[]>();
        if (directories is null)
        {
            return;
        }

        foreach (var directory in directories)
        {
            _directories.Add(directory.Directory, directory.Entries);
        }
    }

    public int CombineCount { get; private set; }

    public int EnumerableEnumerationCount { get; private set; }

    public int LookupCount { get; private set; }

    public int VisitorEnumerationCount { get; private set; }

    public char DirectorySeparatorChar => '\\';

    public string CurrentDirectory { get; }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        EnumerableEnumerationCount++;
        return _directories.TryGetValue(path, out var entries)
            ? entries
            : Array.Empty<string>();
    }

    public void VisitDirectories<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor
    {
        VisitorEnumerationCount++;
        VisitEntries(path, ref visitor);
    }

    public string? GetDirectoryName(string path) => null;

    public string Combine(params string[] paths)
    {
        CombineCount++;
        return string.Join(DirectorySeparatorChar.ToString(), paths);
    }

    public string? GetFileName(string path)
    {
        var separatorIndex = path.LastIndexOf(DirectorySeparatorChar);
        return separatorIndex < 0 ? path : path.Substring(separatorIndex + 1);
    }

    public bool FileExists(string filePath) => false;

    public bool DirectoryExists(string directoryPath)
    {
        return _entries.ContainsValue(directoryPath);
    }

    public bool EntryExists(string path) => DirectoryExists(path);

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        EnumerableEnumerationCount++;
        return Array.Empty<string>();
    }

    public void VisitFileSystemEntries<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor
    {
        VisitorEnumerationCount++;
        VisitEntries(path, ref visitor);
    }

    public bool TryGetFileSystemEntry(
        string directoryPath,
        string entryName,
        [NotNullWhen(true)] out string? entryPath)
    {
        LookupCount++;
        return _entries.TryGetValue((directoryPath, entryName), out entryPath);
    }

    private void VisitEntries<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor
    {
        if (!_directories.TryGetValue(path, out var entries))
        {
            return;
        }

        foreach (var entry in entries)
        {
            if (!visitor.Visit(entry))
            {
                return;
            }
        }
    }
}
#endif
