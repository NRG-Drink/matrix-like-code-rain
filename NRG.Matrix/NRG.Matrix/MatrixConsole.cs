using System.Text;

namespace NRG.Matrix;

public static class MatrixConsole
{
    private static IMatrixConsoleChar[][] _buffer = [];
    private static int _width;
    private static int _height;

    public static void Initialize()
    {
        _width = Console.BufferWidth;
        _height = Console.BufferHeight;
        Clear();
        Console.Write("\x1b[?25l");
    }

    public static void Clear(char c = ' ')
    {
        IMatrixConsoleChar clearChar = new ConsoleChar(c, new(MatrixConsoleColor.White));
        var width = new IMatrixConsoleChar[_width];
        Array.Fill(width, clearChar);
        var height = new IMatrixConsoleChar[_height][];
        for (var i = 0; i < height.Length; i++)
        {
            height[i] = (IMatrixConsoleChar[])width.Clone();
        }

        _buffer = height;
    }

    public static void Set(int x, int y, IMatrixConsoleChar c)
    {
        if (x >= _width || x < 0 || y < 0 || y >= _height)
        {
            return;
        }

        _buffer[y][x] = c;
    }

    public static void DrawBuffer()
    {
        //var debug = string.Join("\n", _buffer.Select(e => string.Join("", e.Select(e => e.Char))));
        var sb = new StringBuilder();
        for (var i = 0; i < _buffer.Length; i++)
        {
            var line = ToLineString(_buffer[i]);
            var l = $"\x1b[{i + 1};0H{line}";
            sb.Append(l);
            //Console.Write($"\x1b[{i+1};0H{line}");
            //Console.Write($"\x1b[{i + 1};0H");
            //Console.Write(line);
        }

        Console.Write("\x1b[37m");
        Console.Write(sb.ToString());
    }

    private static string ToLineString(IMatrixConsoleChar[] chars)
    {
        if (Console.BufferWidth != chars.Length)
        {

        }
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
