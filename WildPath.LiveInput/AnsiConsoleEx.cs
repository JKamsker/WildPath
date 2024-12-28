using Spectre.Console;
using Spectre.Console.Rendering;

namespace WildPath.LiveInput;

public static class AnsiConsoleEx
{
    public static LiveInput LiveInput(IRenderable target)
        => AnsiConsole.Console.LiveInput(target);

    public static LiveInput LiveInput(this IAnsiConsole console, IRenderable target)
    {
        var input = new LiveInput(console, target);
        return input;
    }
}
