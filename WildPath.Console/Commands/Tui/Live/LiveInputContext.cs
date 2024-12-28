// using Spectre.Console;
//
// namespace WildPath.Console.Commands.Tui;
//
// public class LiveInputContext
// {
//     private readonly LiveInput _input;
//     public LiveDisplayContext DisplayContext { get; }
//     public TuiState State { get; }
//     public Layout Layout { get; }
//
//     public LiveInputContext
//     (
//         LiveInput input,
//         LiveDisplayContext displayContext,
//         TuiState state,
//         Layout layout
//     )
//     {
//         _input = input;
//         DisplayContext = displayContext;
//         State = state;
//         Layout = layout;
//     }
//
//     public void Refresh()
//     {
//         _input.Refresh();
//     }
// }

using Spectre.Console;

namespace WildPath.Console.Commands.Tui.Live;

/// <summary>
/// Provides access to the current state and layout of LiveInput.
/// </summary>
public class LiveInputContext
{
    public LiveInputState State { get; }
    public Layout Layout { get; }

    public EventHandler<LiveInputState> OnEnter;
    public EventHandler<LiveInputState> OnInputChanged;

    public LiveInputContext(LiveInputState state, Layout layout)
    {
        State = state;
        Layout = layout;
        
        State.OnEnter += (_, s) => OnEnter?.Invoke(this, s);
        State.OnInputChanged += (_, s) => OnInputChanged?.Invoke(this, s);
    }
    
    public bool RefreshRequested { get; private set; }
    
    public void Refresh()
    {
        // Table updated...
        RefreshRequested = true;
    }
    
    public void ResetRefresh()
    {
        RefreshRequested = false;
    }
}
