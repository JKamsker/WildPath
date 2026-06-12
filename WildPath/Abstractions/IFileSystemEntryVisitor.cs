namespace WildPath.Abstractions;

public interface IFileSystemEntryVisitor
{
    bool Visit(string path);
}
