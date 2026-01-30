using System.Text;

namespace NRG.Matrix.Displays;

public class AnsiConsolePrintAll : IAnsiConsole
{
    private readonly StringBuilder _sb = new();
    private IAnsiConsoleChar?[] _buffer = [];
    public int Width;
    public int Height;

    public AnsiConsolePrintAll()
    {
        // Needed to display special characters.
        Console.OutputEncoding = Encoding.UTF8;
        Console.Write("\x1b[?25l");
        Initialize();
    }

    public bool HasResolutionChanged(out int width, out int height)
    {
        var hasChanged = Width != Console.BufferWidth || Height != Console.BufferHeight;
        if (hasChanged)
        {
            Initialize();
        }

        width = Width;
        height = Height;

        return hasChanged;
    }

    public Task Display(IEnumerable<IAnsiConsoleChar> chars)
    {
        foreach (var c in chars)
        {
            Set(c.X, c.Y, c);
        }

        var buffer = GenerateBuffer();
        Console.Write(buffer);

        return Task.CompletedTask;
    }

    private void Initialize()
    {
        Width = Console.BufferWidth;
        Height = Console.BufferHeight;
        _buffer = new IAnsiConsoleChar[Height * Width];
    }

    private void Set(int x, int y, IAnsiConsoleChar c)
    {
        if (x >= Width || x < 0 || y < 0 || y >= Height)
        {
            return;
        }

        _buffer[y * Width + x] = c;
    }

    private StringBuilder GenerateBuffer()
    {
        _sb.Clear();
        //var debugBuffer = string.Join("\n", _buffer.Select(e => e?.Char));
        var lastY = -1;
        var lastColor = string.Empty;
        for (var i = 0; i < _buffer.Length; i++)
        {
            var y = (int)(i / Width);
            if (y != lastY)
            {
                _sb.Append($"\x1b[{y + 1};1H");
                lastY = y;
            }

            var c = _buffer[i];
            if (c is null)
            {
                _sb.Append(' ');
                continue;
            }

            if (lastColor != c.AnsiColor)
            {
                _sb.Append($"\x1b[{c.AnsiColor}m");
                lastColor = c.AnsiColor;
            }

            _sb.Append(c.Char);
        }

        _sb.Append("\x1b[37m");
        Array.Clear(_buffer);
        return _sb;
    }
}

