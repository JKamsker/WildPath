// using System.Collections.Concurrent;
// using Spectre.Console;
// using Spectre.Console.Cli;
// using WildPath.Console.Commands.Tui.Live;
// using WildPath.Console.CustomStrategies;
//
// namespace WildPath.Console.Commands.Tui;
//
// public class TuiCommand : AsyncCommand<TuiCommand.Settings>
// {
//     public class Settings : CommandSettings
//     {
//     }
//
//     public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
//     {
//         var cancellationToken = CancellationTokenFactory.FromConsoleCancelKeyPress();
//
//         var table = new Table()
//             .Expand()
//             .ShowRowSeparators()
//             .Border(TableBorder.Rounded)
//             .AddColumn("Pattern")
//             .AddColumn("Matches")
//             .AddRow(string.Empty, string.Empty);
//
//         var input = new LiveInput(table);
//         await input.StartAsync(async ctx =>
//         {
//             var currentResults = new ConcurrentBag<string>();
//             var currentInputChangedCts = new CancellationTokenSource();
//
//             ctx.OnInputChanged += (__, s) =>
//             {
//                 currentInputChangedCts.Cancel();
//                 currentResults = new();
//                 currentInputChangedCts = new();
//
//                 // needTextFieldRender = true;
//
//                 table.UpdateCell(0, 0, s.Input);
//                 table.UpdateCell(0, 1, string.Empty);
//
//                 _ = ResolvePattern(s.Input, currentInputChangedCts.Token, path =>
//                 {
//                     currentResults.Add(path);
//                     table.UpdateCell(0, 1, string.Join("\n", currentResults));
//                     ctx.Refresh();
//                 });
//             };
//
//             ctx.OnEnter += (_, s) =>
//             {
//                 currentInputChangedCts.Cancel();
//                 // Add new row with input and results, reset input and results
//                 table.AddRow(s.Input, string.Join("\n", currentResults));
//
//                 table.UpdateCell(0, 0, string.Empty);
//                 table.UpdateCell(0, 1, string.Empty);
//
//                 currentResults = new();
//                 s.Reset();
//             };
//
//             while (!cancellationToken.IsCancellationRequested)
//             {
//                 await Task.Delay(1000);
//             }
//         });
//
//         return 0;
//     }
//
//     private Task ResolvePattern(string pattern, CancellationToken token, Action<string> onFind)
//     {
//         return Task.Run(() =>
//         {
//             try
//             {
//                 var resolver = PathResolver.Create(builder =>
//                 {
//                     builder.WithCustomStrategy<HasFileStrategy>("hasFile");
//                     builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
//                     builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
//                 });
//
//                 if (token.IsCancellationRequested)
//                 {
//                     return;
//                 }
//
//                 var paths = resolver
//                     .ResolveAll(pattern, token)
//                     .Take(10);
//
//                 foreach (var path in paths)
//                 {
//                     if (token.IsCancellationRequested)
//                     {
//                         return;
//                     }
//
//                     onFind(path);
//                 }
//             }
//             catch (Exception e)
//             {
//                 onFind($"Error: {e.Message}");
//             }
//         });
//     }
// }

using System.Collections.Concurrent;
using Spectre.Console;
using Spectre.Console.Cli;
using WildPath.Console.Commands.Tui.Live;
using WildPath.Console.CustomStrategies;

namespace WildPath.Console.Commands.Tui;

public class TuiCommand : AsyncCommand<TuiCommand.Settings>
{
    public class Settings : CommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var cancellationToken = CancellationTokenFactory.FromConsoleCancelKeyPress();

        var table = CreateTable();
        await AnsiConsole.Console.LiveInput(table)
            .StartAsync(async ctx => await HandleLiveInput(ctx, table, cancellationToken));

        return 0;
    }

    private Table CreateTable()
    {
        return new Table()
            .Expand()
            .ShowRowSeparators()
            .Border(TableBorder.Rounded)
            .AddColumn("Pattern")
            .AddColumn("Matches")
            .AddRow(string.Empty, string.Empty);
    }

    private async Task HandleLiveInput
    (
        LiveInputContext ctx,
        Table table,
        CancellationToken cancellationToken
    )
    {
        var resolver = new PatternResolver();
        var currentResults = new ConcurrentBag<string>();
        var inputChangedCts = new CancellationTokenSource();

        ctx.OnInputChanged += (_, s) =>
        {
            ResetOnInputChanged(ref inputChangedCts, table, currentResults, s.Input);

            _ = resolver.ResolveAsync(s.Input, inputChangedCts.Token, path =>
            {
                currentResults.Add(path);
                table.UpdateCell(0, 1, string.Join("\n", currentResults));
                ctx.Refresh();
            });
        };

        ctx.OnEnter += (_, s) => { FinalizeInput(table, currentResults, s); };

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }

    private void ResetOnInputChanged
    (
        ref CancellationTokenSource cts,
        Table table,
        ConcurrentBag<string> results,
        string input
    )
    {
        cts.Cancel();
        cts.Dispose();
        cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        results.Clear();

        table.UpdateCell(0, 0, input);
        table.UpdateCell(0, 1, string.Empty);
    }

    private void FinalizeInput(Table table, ConcurrentBag<string> results, LiveInputState state)
    {
        table.AddRow(state.Input, string.Join("\n", results));

        table.UpdateCell(0, 0, string.Empty);
        table.UpdateCell(0, 1, string.Empty);

        results.Clear();
        state.Reset();
    }
}

// Refactored PatternResolver Class
public class PatternResolver
{
    public Task ResolveAsync(string pattern, CancellationToken token, Action<string> onFind)
    {
        return Task.Run(() =>
        {
            try
            {
                var resolver = PathResolver.Create(builder =>
                {
                    builder.WithCustomStrategy<HasFileStrategy>("hasFile");
                    builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
                    builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
                });

                if (token.IsCancellationRequested) return;

                var paths = resolver.ResolveAll(pattern, token).Take(10);

                foreach (var path in paths)
                {
                    if (token.IsCancellationRequested) return;

                    onFind(path);
                }
            }
            catch (Exception e)
            {
                onFind($"Error: {e.Message}");
            }
        }, token);
    }
}
