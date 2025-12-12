using System.Text;

namespace NRG.Matrix;

public static class MatrixConsole
{
    private static IMatrixConsoleChar[][] _buffer = [];
    private static IMatrixConsoleChar[][] _oldBuffer = [];
    private static int _width;
    private static int _height;

    public static void Initialize()
    {
        _width = Console.BufferWidth;
        _height = Console.BufferHeight;
        _oldBuffer = Clear();
        _buffer = Clear();
        Console.Write("\x1b[?25l\x1b[2J");
    }

    public static void ClearBuffer() => _buffer = Clear();

    private static ConsoleChar _clearChar = new(' ', new(MatrixConsoleColor.White));

    private static IMatrixConsoleChar[][] Clear(char c = ' ')
    {
        //IMatrixConsoleChar clearChar = new ConsoleChar(c, new(MatrixConsoleColor.White));
        var width = new IMatrixConsoleChar[_width];
        Array.Fill(width, _clearChar);
        var height = new IMatrixConsoleChar[_height][];
        for (var i = 0; i < height.Length; i++)
        {
            height[i] = (IMatrixConsoleChar[])width.Clone();
        }

        return height;
    }

    public static void Set(int x, int y, IMatrixConsoleChar c)
    {
        if (x >= _width || x < 0 || y < 0 || y >= _height)
        {
            return;
        }

        _buffer[y][x] = c;
    }

    //public static void DrawBuffer()
    //{
    //    //var debug = string.Join("\n", _buffer.Select(e => string.Join("", e.Select(e => e.Char))));
    //    var sb = new StringBuilder("\x1b[J");
    //    for (var i = 0; i < _buffer.Length; i++)
    //    {
    //        var line = ToLineString(_buffer[i]);
    //        var fullLine = $"\x1b[{i + 1};0H{line}";
    //        sb.Append(fullLine);
    //    }

    //    Console.Write(sb.ToString());
    //    Console.Write("\x1b[37m");
    //    _buffer = Clear();
    //}

    public static async Task DrawBufferChanged()
    {
        var changed = GetBufferChanges();
        var debug = string.Join("\n", changed.Select(e => string.Join("", e.Select(e => e ? '1' : '0'))));
        var sb = new StringBuilder();
        var lastColor = string.Empty;
        for (var y = 0; y < changed.Length; y++)
        {
            for (var x = 0; x < changed[y].Length; x++)
            {
                if (!changed[y][x])
                {
                    continue;
                }

                var c = _buffer[y][x];
                if (lastColor != c.AnsiConsoleColor)
                {
                    sb.Append($"\x1b[{c.AnsiConsoleColor}m");
                    lastColor = c.AnsiConsoleColor;
                }

                sb.Append($"\x1b[{y+1};{x}H{c.Char}");
            }
        }

        await Console.Out.WriteAsync($"{sb}\x1b[37m");
        //Console.Write($"{sb}\x1b[37m");
        _oldBuffer = _buffer;
        _buffer = Clear();
    }

    private static string ToLineString(IMatrixConsoleChar[] chars)
    {
        var sb = new StringBuilder();
        var lastColor = string.Empty;
        foreach (var c in chars)
        {
            if (lastColor != c.AnsiConsoleColor)
            {
                sb.Append($"\x1b[{c.AnsiConsoleColor}m");
                lastColor = c.AnsiConsoleColor;
            }

            sb.Append(c.Char);
        }

        return sb.ToString();
    }

    private static bool[][] GetBufferChanges()
    {
        var result = new bool[_buffer.Length][];
        for (var y = 0; y < _buffer.Length; y++)
        {
            result[y] = new bool[_buffer[y].Length];
            for (var x = 0; x < _buffer[y].Length; x++)
            {
                var a = _buffer[y][x];
                var b = _oldBuffer[y][x];
                var hasCharChanged = a.Char != b.Char;
                var hasColorChanged = a.AnsiConsoleColor != b.AnsiConsoleColor;
                result[y][x] = hasCharChanged || hasColorChanged;
            }
        }

        return result;
    }
}

public record ConsoleChar(char Char, BasicColor BasicColor) : IMatrixConsoleChar
{
    public string AnsiConsoleColor => BasicColor.AnsiConsoleColor;
}

public interface IMatrixConsoleChar : IChar, IConsoleColor;

public interface IChar
{
    public char Char { get; }
}

public record StaticChar(char Char) : IChar;

public interface IConsoleColor
{
    public string AnsiConsoleColor { get; }
}

public record RGB(byte R, byte G, byte B) : IConsoleColor
{
    public string AnsiConsoleColor { get; } = $"38;2;{R};{G};{B}";

    public RGB Luminos(int percentage)
    {
        var factor = (100 - percentage);
        return new((byte)(R * 100 / factor), (byte)(G * 100 / factor), (byte)(B * 100 / factor));
    }
}

public record BasicColor(MatrixConsoleColor Value) : IConsoleColor
{
    public string AnsiConsoleColor { get; } = ((int)Value).ToString();
}

public enum MatrixConsoleColor
{
    // Dark
    Black = 30,
    DarkRed = 31,
    DarkGreen = 32,
    DarkYellow = 33,
    DarkBlue = 34,
    DarkMagenta = 35,
    DarkCyan = 36,
    DarkGray = 37,
    // Bright
    Gray = 90,
    Red = 91,
    Green = 92,
    Yellow = 93,
    Blue = 94,
    Magenta = 95,
    Cyan = 96,
    White = 97,
}
