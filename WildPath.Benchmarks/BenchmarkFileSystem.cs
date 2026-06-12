using System.Diagnostics.CodeAnalysis;
using WildPath.Abstractions;

namespace WildPath.Benchmarks;

internal sealed class BenchmarkFileSystem :
    IFileSystem,
    IFileSystemEntryLookup,
    IFileSystemEntryEnumerable
{
    private readonly Dictionary<string, string[]> _directories;
    private readonly Dictionary<(string Directory, string EntryName), string> _entries;

    private BenchmarkFileSystem(
        IEnumerable<(string Directory, string EntryName, string Path)> entries,
        IEnumerable<(string Directory, string[] Entries)> directories)
    {
        _entries = new Dictionary<(string, string), string>();
        foreach (var entry in entries)
        {
            _entries.Add((entry.Directory, entry.EntryName), entry.Path);
        }

        _directories = new Dictionary<string, string[]>();
        foreach (var directory in directories)
        {
            _directories.Add(directory.Directory, directory.Entries);
        }
    }

    public char DirectorySeparatorChar => '\\';

    public string CurrentDirectory => "C:";

    public static BenchmarkFileSystem Create()
    {
        return new BenchmarkFileSystem(
            entries:
            [
                new("C:", "Test", @"C:\Test"),
                new(@"C:\Test", "SubDir1", @"C:\Test\SubDir1"),
                new(@"C:\Test\SubDir1", "SubSubDir1", @"C:\Test\SubDir1\SubSubDir1"),
                new("C:", "Branch", @"C:\Branch"),
                new(@"C:\Branch", "leaf", @"C:\Branch\leaf"),
                new(@"C:\WithMarker", ".marker", @"C:\WithMarker\.marker"),
                new(@"C:\WithMarker", "leaf", @"C:\WithMarker\leaf")
            ],
            directories:
            [
                new(
                    "C:",
                    [
                        @"C:\Test",
                        @"C:\other",
                        @"C:\leaf-one",
                        @"C:\leaf",
                        @"C:\Branch",
                        @"C:\WithoutMarker",
                        @"C:\WithMarker"
                    ]),
                new(@"C:\Branch", [@"C:\Branch\leaf"]),
                new(@"C:\WithMarker", [@"C:\WithMarker\.marker", @"C:\WithMarker\leaf"])
            ]);
    }

    public string Combine(params string[] paths) => string.Join(DirectorySeparatorChar.ToString(), paths);

    public bool DirectoryExists(string directoryPath)
    {
        if (_directories.ContainsKey(directoryPath))
        {
            return true;
        }

        foreach (var entry in _entries.Values)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(entry, directoryPath))
            {
                return true;
            }
        }

        return false;
    }

    public bool EntryExists(string path) => DirectoryExists(path);

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return EnumerateStoredEntries(path);
    }

    public IEnumerable<string> EnumerateFileSystemEntries(string path)
    {
        return EnumerateStoredEntries(path);
    }

    public bool FileExists(string filePath) => false;

    public string? GetDirectoryName(string path) => null;

    public string? GetFileName(string path)
    {
        var separatorIndex = path.LastIndexOf(DirectorySeparatorChar);
        return separatorIndex < 0 ? path : path.Substring(separatorIndex + 1);
    }

    public bool TryGetFileSystemEntry(
        string directoryPath,
        string entryName,
        [NotNullWhen(true)] out string? entryPath)
    {
        return _entries.TryGetValue((directoryPath, entryName), out entryPath);
    }

    public void VisitDirectories<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor
    {
        VisitStoredEntries(path, ref visitor);
    }

    public void VisitFileSystemEntries<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor
    {
        VisitStoredEntries(path, ref visitor);
    }

    private IEnumerable<string> EnumerateStoredEntries(string path)
    {
        return _directories.TryGetValue(path, out var entries)
            ? entries
            : Array.Empty<string>();
    }

    private void VisitStoredEntries<TVisitor>(string path, ref TVisitor visitor)
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
