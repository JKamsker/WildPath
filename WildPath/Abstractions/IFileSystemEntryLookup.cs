using System.Diagnostics.CodeAnalysis;

namespace WildPath.Abstractions;

public interface IFileSystemEntryLookup
{
    bool TryGetFileSystemEntry(
        string directoryPath,
        string entryName,
        [NotNullWhen(true)] out string? entryPath);
}
