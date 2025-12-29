using Spectre.Console;
using Spectre.Console.Cli;
using WildPath.Console.CustomStrategies;
using WildPath.Console.Utils;

namespace WildPath.Console;

internal class LsCommand : Command<LsCommand.LsSettings>
{
    public class LsSettings : CommandSettings
    {
        // positional path argument
        [CommandArgument(0, "[path]")] public string? Path { get; set; }

        // [CommandOption("-l|--long")]
        // public bool Long { get; set; }

        [CommandOption("-n|--limit")] public int? Limit { get; set; }

        // -a --absolute bool switch
        [CommandOption("-a|--absolute")] public bool Absolute { get; set; }

        // --relative bool switch
        [CommandOption("--relative")] public bool Relative { get; set; }
        
        // set current directory
        [CommandOption("-C|--directory")] 
        public string? Directory { get; set; }
    }

    public override int Execute(CommandContext context, LsSettings settings)
    {
        string? customCurrentDirectory = null;
        if(!string.IsNullOrWhiteSpace(settings.Directory))
        {
            customCurrentDirectory = settings.Directory;
        }
        
        var resolver = PathResolver.Create(builder =>
        {
            builder.WithCustomStrategy<HasFileStrategy>("hasFile");
            builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
            builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
            
            if(customCurrentDirectory != null)
            {
                builder.WithCurrentDirectory(customCurrentDirectory);
            }
        });

        var expression = string.IsNullOrWhiteSpace(settings.Path) ? "." : settings.Path;
        var paths = resolver
            .ResolveAll(expression, TimeSpan.FromSeconds(1).ToCancellationToken())
            .Take(settings.Limit ?? 10);

        foreach (var path in paths)
        {
            if (settings.Absolute)
            {
                AnsiConsole.MarkupLine($"[green]{path}[/]");
                continue;
            }

            var relativeToCurrent = PathUtils.MakeRelative(customCurrentDirectory ?? System.IO.Directory.GetCurrentDirectory(), path);
            var shortPath = relativeToCurrent.Length <= path.Length || settings.Relative
                ? relativeToCurrent
                : path;

            AnsiConsole.MarkupLine($"[green]{shortPath}[/]");
        }

        return 0;
    }
}

// Install command for powershell
// used like this: (wpcli install pwsh) | pwsh
// Inserts following code into the profile:
/*
function wpls {
    if (-not (Get-Command wpcli -ErrorAction SilentlyContinue)) {
        Write-Error "wpcli is not installed or not found in PATH."
        return
    }

    $command = "wpcli ls $args"
    Invoke-Expression $command
}
*/
