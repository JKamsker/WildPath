using System;

namespace WildPath.LiveInput.Utils;

internal static class PathUtils
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