using System.Text;

namespace NRG.Matrix.Displays;

public class AnsiConsolePrintAll : IAnsiConsole
{
    private readonly StringBuilder _sb = new();
    private IAnsiConsoleChar?[] _buffer = [];
    private char[] _charBuffer = [];
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

    public void Display(IEnumerable<IAnsiConsoleChar> chars)
    {
        foreach (var c in chars)
        {
            Set(c.X, c.Y, c);
        }

        GenerateBuffer();

        // Write directly from StringBuilder's internal buffer to avoid ToString() allocation
        if (_sb.Length > 0)
        {
            // Ensure char buffer is large enough
            if (_charBuffer.Length < _sb.Length)
            {
                _charBuffer = new char[_sb.Length * 2];
            }

            _sb.CopyTo(0, _charBuffer, 0, _sb.Length);
            Console.Out.Write(_charBuffer, 0, _sb.Length);
        }
    }

    private void Initialize()
    {
        Width = Console.BufferWidth;
        Height = Console.BufferHeight;
        _buffer = new IAnsiConsoleChar[Height * Width];
        // Pre-allocate char buffer for typical console output
        _charBuffer = new char[Height * Width * 15]; // ~15 chars per cell max (ANSI codes)
    }

    private void Set(int x, int y, IAnsiConsoleChar c)
    {
        if (x >= Width || x < 0 || y < 0 || y >= Height)
        {
            return;
        }

        _buffer[y * Width + x] = c;
    }

    private void GenerateBuffer()
    {
        _sb.Clear();
        var lastY = -1;
        ReadOnlySpan<char> lastColor = [];
        for (var i = 0; i < _buffer.Length; i++)
        {
            var y = i / Width;
            if (y != lastY)
            {
                _sb.Append("\x1b[");
                AppendInt(_sb, y + 1);
                _sb.Append(";1H");
                lastY = y;
            }

            var c = _buffer[i];
            if (c is null)
            {
                _sb.Append(' ');
                continue;
            }

            ReadOnlySpan<char> currentColor = c.AnsiColor;
            if (!lastColor.SequenceEqual(currentColor))
            {
                _sb.Append("\x1b[")
                    .Append(c.AnsiColor)
                    .Append('m');
                lastColor = currentColor;
            }

            _sb.Append(c.Char);
        }

        _sb.Append("\x1b[37m");
        Array.Clear(_buffer);
    }

    /// <summary>
    /// Appends an integer without allocating a string.
    /// </summary>
    private static void AppendInt(StringBuilder sb, int value)
    {
        if (value < 0)
        {
            sb.Append('-');
            value = -value;
        }

        if (value == 0)
        {
            sb.Append('0');
            return;
        }

        // Count digits
        var temp = value;
        var digits = 0;
        while (temp > 0)
        {
            digits++;
            temp /= 10;
        }

        // Build number in reverse
        Span<char> buffer = stackalloc char[10];
        var pos = digits - 1;
        while (value > 0)
        {
            buffer[pos--] = (char)('0' + (value % 10));
            value /= 10;
        }

        for (var i = 0; i < digits; i++)
        {
            sb.Append(buffer[i]);
        }
    }
}