namespace WildPath.Console;

class Program
{
    static void Main(string[] args)
    {
        
        var resolver = new PathResolver();
        // var path = resolver.EvaluateExpression("...\\WildPath.Tests\\**\\net8.0");
        var path = resolver.EvaluateExpression("...{1,3}\\**{1,3}\\:tagged(testhost.exe):\\fr");
        System.Console.WriteLine($"Path: {path}");
    }
}