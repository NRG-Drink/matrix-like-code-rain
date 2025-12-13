using System.Diagnostics;
using System.Text;

namespace NRG.Matrix;

public static class MatrixConsole
{
    private readonly static StringBuilder _sb = new();
    private  static IMatrixConsoleChar?[] _buffer = [];
    private static int _width;
    private static int _height;

    public static void Initialize()
    {
        _width = Console.BufferWidth;
        _height = Console.BufferHeight;
        _buffer = new IMatrixConsoleChar[_height * _width];
        Console.Write("\x1b[?25l\x1b[2J");
    }

    public static void Set(int x, int y, IMatrixConsoleChar c)
    {
        if (x >= _width || x < 0 || y < 0 || y >= _height)
        {
            return;
        }

        _buffer[y * _width + x] = c;
    }

    public static StringBuilder DrawBuffer()
    {
        _sb.Clear();
        //var debugBuffer = string.Join("\n", _buffer.Select(e => e?.Char));
        var lastColor = string.Empty;
        var chunks = _buffer.Chunk(_width);
        var y = 0;
        foreach (var ch in chunks)
        {
            y++;
            _sb.Append($"\x1b[{y};1H");
            for (var x = 0; x < _width; x++)
            {
                var c = ch[x];
                if (c is null)
                {
                    _sb.Append(' ');
                    continue;
                }

                if (lastColor != c.AnsiConsoleColor)
                {
                    _sb.Append($"\x1b[{c.AnsiConsoleColor}m");
                    lastColor = c.AnsiConsoleColor;
                }

                _sb.Append(c.Char);
            }
        }

        _sb.Append("\x1b[37m");
        Array.Clear(_buffer);
        return _sb;
    }

    //public static StringBuilder DrawBufferChanged2(StringBuilder log, Stopwatch frameTime)
    //{
    //    log.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Start {nameof(DrawBufferChanged2)}");
    //    _sb.Clear();
    //    log.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - _sb Buffer Cleared");
    //    GetBufferChanges();
    //    log.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Get Changes");
    //    //var debugBuffer = string.Join("\n", _buffer.Select(e => string.Join("", e.Select(e => e?.Char))));
    //    //var debugLast = string.Join("\n", _oldBuffer.Select(e => string.Join("", e.Select(e => e?.Char))));
    //    //var debug = string.Join("\n", _changedBuffer.Select(e => string.Join("", e.Select(e => e ? '1' : '0'))));
    //    //var sb = new StringBuilder();
    //    var lastColor = string.Empty;
    //    for (var i = 0; i < _changedBuffer.Length; i++)
    //    {
    //        var y = (int)i / _width;
    //        var x = (int)i % _width;
    //        var lastX = -10;
    //        if (!_changedBuffer[i])
    //        {
    //            continue;
    //        }

    //        var c = _buffer[i];
    //        if (c is null)
    //        {
    //            continue;
    //        }

    //        if (lastColor != c.AnsiConsoleColor)
    //        {
    //            _sb.Append($"\x1b[{c.AnsiConsoleColor}m");
    //            lastColor = c.AnsiConsoleColor;
    //        }

    //        if (x == lastX + 1)
    //        {
    //            _sb.Append(c.Char);

    //        }
    //        else
    //        {
    //            _sb.Append($"\x1b[{y + 1};{x + 1}H{c.Char}");
    //        }

    //        _oldBuffer[i] = c;
    //        lastX = x;
    //    }

    //    log.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Write Changes to _sb");
    //    //var debugBuffer2 = string.Join("\n", _buffer.Select(e => string.Join("", e.Select(e => e?.Char))));
    //    //Clear(_buffer);
    //    //Clear(_changedBuffer);
    //    Array.Clear(_buffer);
    //    Array.Clear(_changedBuffer);
    //    log.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Clear Buffers");
    //    _sb.Append("\x1b[37m");
    //    //return Console.Out.WriteAsync(_sb);
    //    return _sb;
    //}

    //public static Task DrawBufferChanged()
    //{
    //    //var debugBuffer = string.Join("\n", _buffer.Select(e => string.Join("", e.Select(e => e?.Char))));
    //    //var debugLast = string.Join("\n", _oldBuffer.Select(e => string.Join("", e.Select(e => e?.Char))));
    //    var changed = GetBufferChanges();
    //    //var debug = string.Join("\n", _changedBuffer.Select(e => string.Join("", e.Select(e => e ? '1' : '0'))));
    //    //var sb = new StringBuilder();
    //    var lastColor = string.Empty;
    //    for (var y = 0; y < changed.Length; y++)
    //    {
    //        for (var x = 0; x < changed[y].Length; x++)
    //        {
    //            if (!changed[y][x])
    //            {
    //                continue;
    //            }

    //            var c = _buffer[y][x];
    //            if (c is null)
    //            {
    //                continue;
    //            }

    //            if (lastColor != c.AnsiConsoleColor)
    //            {
    //                _sb.Append($"\x1b[{c.AnsiConsoleColor}m");
    //                lastColor = c.AnsiConsoleColor;
    //            }

    //            _sb.Append($"\x1b[{y + 1};{x + 1}H{c.Char}");
    //        }
    //    }

    //    for (var i = 0; i < _buffer.Length; i++)
    //    {
    //        Array.Copy(_buffer[i], _oldBuffer[i], _buffer[i].Length);
    //    }

    //    Clear(_buffer);
    //    var message = $"{_sb}\x1b[37m";
    //    _sb.Clear();
    //    //Console.Write(message);
    //    return Console.Out.WriteAsync(message);
    //    //return Console.Out.WriteAsync($"{sb}\x1b[37m");
    //}

    //private static string ToLineString(IMatrixConsoleChar[] chars)
    //{
    //    var sb = new StringBuilder();
    //    var lastColor = string.Empty;
    //    foreach (var c in chars)
    //    {
    //        if (lastColor != c.AnsiConsoleColor)
    //        {
    //            sb.Append($"\x1b[{c.AnsiConsoleColor}m");
    //            lastColor = c.AnsiConsoleColor;
    //        }

    //        sb.Append(c.Char);
    //    }

    //    return sb.ToString();
    //}

    //private static void GetBufferChanges()
    //{
    //    for (var x = 0; x < _buffer.Length; x++)
    //    {
    //        var a = _buffer[x];
    //        var b = _oldBuffer[x];
    //        var hasChanged = a?.Char != b?.Char || a?.AnsiConsoleColor != b?.AnsiConsoleColor;
    //        _changedBuffer[x] = hasChanged;
    //        if (hasChanged && a is null)
    //        {
    //            _buffer[x] = _clearChar;
    //        }
    //    }
    //}
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
        var factor = Math.Max(1, (100 - percentage));
        return new((byte)(R * factor / 100), (byte)(G * factor / 100), (byte)(B * factor / 100));
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
