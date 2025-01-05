using System.Collections.Concurrent;
using System.Diagnostics;
using JKToolKit.Spectre.LiveInput;
using Spectre.Console;
using Spectre.Console.Cli;
using WildPath.Console.CustomStrategies;
using WildPath.Console.Utils;

namespace WildPath.Console.Commands.Tui;

public class TuiCommand : AsyncCommand<TuiCommand.Settings>
{
    public class Settings : CommandSettings
    {
        // Current Directory
        [CommandOption("-C|--current-directory")]
        public string CurrentDirectory { get; set; } = System.IO.Directory.GetCurrentDirectory();

        // Custom directory separator
        [CommandOption("-s|--separator")] public char Separator { get; set; } = System.IO.Path.DirectorySeparatorChar;

        // Make relative path
        [CommandOption("-r|--relative")] public bool? MakeRelative { get; set; } //= true;
    }

    private Settings? _settings;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        _settings = settings;

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
        var currentResults = new ConcurrentBag<string>();
        var inputChangedCts = new CancellationTokenSource();

        ctx.OnInputChanged += (_, s) =>
        {
            ResetOnInputChanged(ref inputChangedCts, table, currentResults, s.Input);

            _ = ResolvePatternAsync(s.Input, inputChangedCts.Token, path =>
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

    private Task ResolvePatternAsync(string pattern, CancellationToken token, Action<string> onFind)
    {
        return Task.Run(() =>
        {
            if (_settings is null) return;

            try
            {
                var resolver = PathResolver.Create(builder =>
                {
                    builder.WithPathSeparator(_settings.Separator);
                    builder.WithCustomStrategy<HasFileStrategy>("hasFile");
                    builder.WithCustomStrategy<HasDirectoryStrategy>("hasDirectory");
                    builder.WithCustomStrategy<JsonFileStrategy>("hasJson");
                });

                if (token.IsCancellationRequested) return;

                var paths = resolver
                    .ResolveAll(pattern, token)
                    .Distinct()
                    .Take(10);

                foreach (var path in paths)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    if (_settings.MakeRelative is true)
                    {
                        var result = PathUtils.MakeRelative(path, _settings.CurrentDirectory, _settings.Separator);
                        onFind(result);
                    }
                    else if (_settings.MakeRelative is false)
                    {
                        onFind(path);
                    }
                    else
                    {
                        // Default behavior: make relative but intelligently
                        var relativePath =
                            PathUtils.MakeRelative(path, _settings.CurrentDirectory, _settings.Separator);

                        var shouldPickAbsolute =
                            path.Length < relativePath.Length
                            || relativePath.CountExcept("..", _settings.Separator.ToString()) == 0;

                        string result;
                        if (shouldPickAbsolute)
                        {
                            result = path;
                        }
                        else
                        {
                            result = relativePath;
                            if (!result.Contains(_settings.Separator))
                            {
                                result = string.Concat(".", _settings.Separator, result);
                            }
                        }

                        // Debug.WriteLine
                        // (
                        //     $"Base:\t\t{_settings.CurrentDirectory}\n" +
                        //     $"Absolute:\t{path}\n" +
                        //     $"Relative:\t{relativePath}\n" +
                        //     $"Result:\t\t{result}\n" +
                        //     $"====================\n"
                        // );

                        onFind(result);
                    }
                }
            }
            catch (Exception e)
            {
                onFind($"Error: {e.Message}");
            }
        }, token);
    }
}
