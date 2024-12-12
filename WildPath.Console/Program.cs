namespace WildPath.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        var cdir = System.IO.Directory.GetCurrentDirectory();

        // var res = PathResolver.Resolve(cdir + "\\...\\WildPath.Tests\\**\\net8.0\\*.dll");

        var resolved = PathResolver
            .ResolveAll("...\\**", TimeSpan.FromSeconds(1).ToCancellationToken());
        
        foreach (var path in resolved)
        {
            System.Console.WriteLine($"Path: {path}");
        }


        // var resolver = new PathResolver();
        // var path = resolver.Resolve("...\\WildPath.Tests\\**\\net8.0");
        //
        // // var path = resolver.Resolve("...{1,3}\\**{1,3}\\:tagged(testhost.exe):\\fr");
        // System.Console.WriteLine($"Path: {path}");
    }
}