using WildPath.Console.CustomStrategies;
using WildPath.Strategies.Custom;

namespace WildPath.Console;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

class Program
{
    static async Task Main(string[] args)
    {
        // JsonDemo.Test();

        var resolver = PathResolver.Create(builder =>
        {
            builder.WithCustomStrategy<HasFileStrategy>("hasFile");
            builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
            builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
        });

        var path = resolver.Resolve
        (
            "...\\WildPath.Tests\\ressources\\**\\:hasJson(myJson.json, $..Products[?(@.Price >= 50)].Name, Anvil):"
            , CancellationToken.None
        );
        
        // **\zip(myfile.zip, **\*.json)
        // **\zip(myfile.zip)\*.json
        
        Console.WriteLine(path);
    }

    private static async Task Demo()
    {
        AnsiConsole.Write(new FigletText("WildPath Shell").Centered().Color(Color.Green));
        AnsiConsole.MarkupLine("[bold yellow]Type a path expression to evaluate, or type 'exit' to quit.[/]");

        CancellationTokenSource cts = default;

        // Listen to Ctrl+C and cancel the evaluation task if needed
        Console.CancelKeyPress += (_, args) =>
        {
            if (cts is null || cts.IsCancellationRequested)
            {
                return;
            }

            args.Cancel = true;
            cts?.Cancel();
        };

        while (true)
        {
            try
            {
                // Capture the user's input
                string? input = AnsiConsole.Ask<string>("Type path expression:");
                cts = TimeSpan.FromSeconds(1).ToCancellationTokenSource();

                // End process
                if (string.Equals(input.Trim(), "exit", StringComparison.InvariantCultureIgnoreCase))
                {
                    cts.Cancel();
                    break;
                }


                var resolved = PathResolver
                    .ResolveAll(input, cts.Token)
                    .Take(10);

                foreach (var item in resolved)
                {
                    AnsiConsole.MarkupLine($"[green]Resolved:[/] {item}");
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }
    }
}