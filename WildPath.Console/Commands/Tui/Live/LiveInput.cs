using Spectre.Console;
using Spectre.Console.Rendering;
using WildPath.Console.Utils;

namespace WildPath.Console.Commands.Tui.Live;

/// <summary>
/// Handles live TUI input and UI rendering, delegating business logic to an external handler.
/// </summary>
public class LiveInput
{
    private readonly IAnsiConsole _console;
    private readonly IRenderable _table;
    private readonly CancellationTokenSource _cts = new();
    private readonly Debouncer _debouncer = new(TimeSpan.FromMilliseconds(500));

    private bool _isCursorVisible = true;
    private bool _needTableRender = true;
    private bool _needTextFieldRender = true;

    private readonly LiveInputState _state;

    public LiveInput(IAnsiConsole console, IRenderable table)
    {
        _console = console;
        _table = table;
        _state = new LiveInputState();
    }

    public async Task StartAsync(Func<LiveInputContext, Task> onRun)
    {
        var layout = new Layout("Root")
            .SplitRows(
                new Layout("Top"),
                new Layout("Bottom")
                {
                    Size = 4
                }
            );

        layout["Top"].Update(_table);


        // Setup event handlers for TuiState
        // _state.OnEnter += (_, s) => _needTableRender = true;
        _state.OnInputChanged += (_, s) => _needTextFieldRender = true;
        _state.OnCursorPosChanged += (_, s) => _needTextFieldRender = true;

        await _console.Live(layout).StartAsync(async ctx =>
        {
            try
            {
                var liveContext = new LiveInputContext(_state, layout);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CaptureUserInputAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignore
                    }
                });

                // Delegate control to the user-defined handler
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await onRun(liveContext);
                    }
                    finally
                    {
                        _cts?.Cancel();
                    }
                });

                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_debouncer.TryCall())
                    {
                        _isCursorVisible = !_isCursorVisible;
                        _needTextFieldRender = true;
                    }

                    if (_needTextFieldRender)
                    {
                        var value = _isCursorVisible ? _state.Input : _state.RenderWithCursor();
                        layout["Bottom"].Update(CreateTextField(value));
                    }

                    if (_needTextFieldRender || liveContext.RefreshRequested)
                    {
                        ctx.Refresh();
                        liveContext.ResetRefresh();
                        _needTextFieldRender = false;
                    }

                    await Task.Delay(50, _cts.Token); // Prevent excessive updates
                }

                await task;
            }
            catch (TaskCanceledException)
            {
            }
        });
    }

    private static Panel CreateTextField(string value) =>
        new Panel(new Markup(value))
            .Expand()
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green);

    private async Task CaptureUserInputAsync()
    {
        while (!_cts.Token.IsCancellationRequested)
        {
            if (!_console.Input.IsKeyAvailable())
            {
                await Task.Delay(50, _cts.Token);
                continue;
            }

            if (_cts.Token.IsCancellationRequested) return;
            var keyResult = (await _console.Input.ReadKeyAsync(true, _cts.Token));

            if (keyResult is not { } key || _cts.Token.IsCancellationRequested)
            {
                return;
            }

            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    _state.MoveLeft();
                    break;
                case ConsoleKey.RightArrow:
                    _state.MoveRight();
                    break;
                case ConsoleKey.Enter:
                    _state.EnterPressed();
                    break;
                case ConsoleKey.Backspace:
                    _state.PopInput();
                    break;
                default:
                    if (!char.IsControl(key.KeyChar))
                        _state.AddInput(key.KeyChar);
                    break;
            }
        }
    }
}
