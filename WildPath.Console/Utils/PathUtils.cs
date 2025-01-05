namespace WildPath.Console.Utils;

internal static partial class PathUtils
{
    internal static string MakeRelative(string fromPath, string toPath)
    {
        if(!fromPath.EndsWith("\\") || !fromPath.EndsWith("/"))
        {
            fromPath = fromPath + "\\";
        }
        
        var fromUri = new Uri(fromPath);
        var toUri = new Uri(toPath);

        if (fromUri.Scheme != toUri.Scheme)
        {
            return toPath;
        }

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath.Replace('/', '\\');
    }
}

static partial class PathUtils
{
    public static string MakeRelative(string path, string basePath, char separator)
    {
        if (string.IsNullOrEmpty(path)) 
            throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrEmpty(basePath)) 
            throw new ArgumentNullException(nameof(basePath));
        
        // // Get full paths and normalize directory separators
        // string absolutePath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        // string absoluteBasePath = Path.GetFullPath(basePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        
        // Split paths into parts
        var pathParts = path.Split(separator);
        var basePathParts = basePath.Split(separator);

        // Find common prefix
        int commonLength = 0;
        while (commonLength < pathParts.Length && 
               commonLength < basePathParts.Length && 
               string.Equals(pathParts[commonLength], basePathParts[commonLength], StringComparison.OrdinalIgnoreCase))
        {
            commonLength++;
        }
        
        if (commonLength == 0)
        {
            return path;
        }

        // Go up for remaining base path segments
        var relativePathParts = new System.Collections.Generic.List<string>();
        for (int i = commonLength; i < basePathParts.Length; i++)
        {
            relativePathParts.Add("..");
        }

        // Add remaining path segments
        for (int i = commonLength; i < pathParts.Length; i++)
        {
            relativePathParts.Add(pathParts[i]);
        }

        // Join with the specified separator
        return string.Join(separator.ToString(), relativePathParts);
    }
}
