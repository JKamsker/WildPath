﻿using JKToolKit.Spectre.AutoCompletion.Completion;
using JKToolKit.Spectre.AutoCompletion.Integrations;
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
        // args = new string[]
        // {
        //     "ls",
        //     "**\\*.json",
        //     "-C",
        //     "C:\\Users\\W31rd0"
        // };

        LogCommand(args);

        var consoleApp = new Spectre.Console.Cli.CommandApp();
        consoleApp.Configure(config =>
        {
            config.AddAutoCompletion(conf => conf.AddPowershell(pconf =>
            {
                pconf.Aliases.Add("wpls", ["ls"]);
            }));

            // ls
            config.AddCommand<LsCommand>("ls")
                .WithDescription("List files and directories in the current directory.");
        });


        consoleApp.Run(args);
    }

    private static void LogCommand(string[] args)
    {
        var dir = "C:\\Users\\W31rd0\\source\\repos\\JKamsker\\tmp";
        if (!System.IO.Directory.Exists(dir))
        {
            return;
        }

        var command = string.Join(" ", args);
        var currentDirectory = System.IO.Directory.GetCurrentDirectory();
        var logPath = System.IO.Path.Combine(dir, "commands.log");

        System.IO.File.AppendAllText(logPath, $"{DateTime.Now} - {currentDirectory} - {command}\n");
    }

    private static void Test()
    {
        var resolver = PathResolver.Create(builder =>
        {
            builder.WithCustomStrategy<HasFileStrategy>("hasFile");
            builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
            builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
        });

        var pathx = resolver.Resolve(".\\Temp\\V7Modified.dll");

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