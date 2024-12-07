namespace WildPath.Console;

class Program
{
    static void Main(string[] args)
    {
        PathResolver.Resolve("...\\WildPath.Tests\\**\\net8.0");
        
        
        var resolver = new PathResolver();
        var path = resolver.Resolve("...\\WildPath.Tests\\**\\net8.0");
        
        
        // var path = resolver.Resolve("...{1,3}\\**{1,3}\\:tagged(testhost.exe):\\fr");
        System.Console.WriteLine($"Path: {path}");
    }
}