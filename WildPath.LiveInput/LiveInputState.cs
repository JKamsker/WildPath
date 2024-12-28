using System;
using System.Text;
using WildPath.LiveInput.Utils;

namespace WildPath.LiveInput;

public class LiveInputState
{
    public EventHandler<LiveInputState>? OnEnter { get; set; }
    public EventHandler<LiveInputState>? OnCursorPosChanged { get; set; }
    public EventHandler<LiveInputState>? OnInputChanged { get; set; }

    private string _inputCache = string.Empty;
    private int? _cursorPosition;

    public InputMode Mode { get; private set; } = InputMode.Insert;

    public string Input
    {
        get
        {
            if (_inputCache.Length != InputBuffer.Length)
            {
                _inputCache = InputBuffer.ToString();
            }

            return _inputCache;
        }
    }

    public int? CursorPosition
    {
        get => _cursorPosition;
        private set
        {
            _cursorPosition = value;
            OnCursorPosChanged?.Invoke(this, this);
        }
    }

    public StringBuilder InputBuffer { get; private set; } = new();

    public string RenderWithCursor() //char cursorChar = '*'
    {
        if (CursorPosition is null)
        {
            return Mode == InputMode.Insert
                ? $"{Input}[underline]_[/]"
                : Input;
        }

        var cursorPosition = CursorPosition.Value;
        var input = InputBuffer.Copy();
        // input[cursorPosition] = cursorChar;
        if (Mode == InputMode.Insert)
        {
            // Bounds check
            if (cursorPosition >= input.Length)
            {
                cursorPosition = input.Length - 1;
            }

            var charAtCursor = input[cursorPosition];

            // "\x1B[4m"
            // "\x1B[0m"

            input.Remove(cursorPosition, 1);
            // input.Insert(cursorPosition, Markup.Escape($"\x1B[4m{charAtCursor}\x1B[0m"));
            input.Insert(cursorPosition, $"[underline]{charAtCursor}[/]");
        }
        else
        {
            input[cursorPosition] = '*';
        }

        return input.ToString();
    }

    public void AddInput(char c)
    {
        if (CursorPosition is null)
        {
            InputBuffer.Append(c);
            OnInputChanged?.Invoke(this, this);
            return;
        }

        // InputBuffer.Insert(CursorPosition.Value, c);
        if (Mode == InputMode.Insert)
        {
            InputBuffer.Insert(CursorPosition.Value, c);
        }
        else
        {
            InputBuffer[CursorPosition.Value] = c;
        }

        CursorPosition++;
        OnInputChanged?.Invoke(this, this);
    }

    public void PopInput()
    {
        if (InputBuffer.Length <= 0)
        {
            return;
        }

        _inputCache = string.Empty;

        if (CursorPosition is not { } curpos)
        {
            InputBuffer.Remove(InputBuffer.Length - 1, 1);
            OnInputChanged?.Invoke(this, this);
            return;
        }

        var newPos = curpos - 1;
        if (newPos >= 0)
        {
            InputBuffer.Remove(newPos, 1);
        }

        // CursorPosition = newPos < 0 ? null : newPos;
        CursorPosition = Math.Max(0, newPos);
        OnInputChanged?.Invoke(this, this);
    }

    public void EnterPressed()
    {
        if (InputBuffer.Length == 0)
        {
            return;
        }

        // var output = InputBuffer.ToString();
        // InputBuffer.Clear();
        // _inputCache = string.Empty;
        // CursorPosition = null;

        OnEnter?.Invoke(this, this);
    }

    public void Reset()
    {
        InputBuffer.Clear();
        _inputCache = string.Empty;
        CursorPosition = null;
    }

    /// <summary>
    /// Moves the cursor to the left.
    /// </summary>
    public void MoveLeft()
    {
        if (CursorPosition is null)
        {
            CursorPosition = InputBuffer.Length - 1;
        }
        else
        {
            CursorPosition = Math.Max(0, CursorPosition.Value - 1);
        }
    }

    /// <summary>
    /// Moves the cursor to the right.
    /// </summary>
    public void MoveRight()
    {
        if (CursorPosition is null)
        {
            return;
        }

        // CursorPosition = Math.Min(InputBuffer.Length - 1, CursorPosition.Value + 1);
        var newPos = CursorPosition.Value + 1;

        //set null if cursor is at the end
        CursorPosition = newPos >= InputBuffer.Length ? null : newPos;
    }
}
