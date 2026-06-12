namespace WildPath.Abstractions;

public interface IFileSystemEntryEnumerable
{
    void VisitDirectories<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor;

    void VisitFileSystemEntries<TVisitor>(string path, ref TVisitor visitor)
        where TVisitor : struct, IFileSystemEntryVisitor;
}
