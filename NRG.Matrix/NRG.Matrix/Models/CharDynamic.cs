using NRG.Matrix.Displays;
using System.Diagnostics;

namespace NRG.Matrix.Models;

public class CharDynamic : IAnsiConsoleChar, ICanFall
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    // Needed for ObjectPool
    public CharDynamic() { }

    public void Init(Shot shot, char[] abc, RGB color, int time, int y)
    {
        Shot = shot;
        Alphabeth = abc;
        Rgb = color;
        ChangeTimeMilliseconds = time;
        Char = Alphabeth[Random.Shared.Next(0, Alphabeth.Length)];
        Y = y;
        _sw.Restart();
        AnsiConsoleColor = Rgb.AnsiConsoleColor;
        AnsiColor = Rgb.AnsiConsoleColor;
    }

    public Shot Shot { get; set; }
    public char[] Alphabeth { get; set; }
    public RGB Rgb { get; set; }
    public int ChangeTimeMilliseconds { get; set; }
    public int Y { get; set; }
    public string AnsiConsoleColor { get; private set; }
    public bool IsExpired => _sw.Elapsed.TotalMilliseconds >= ChangeTimeMilliseconds;
    public char Char { get; set; }
    public int X => Shot.X;
    public int Z => Shot.Z;
    public string AnsiColor { get; private set; }

    public void PickNewCharIfNeeded()
    {
        if (IsExpired)
        {
            Char = Alphabeth[Random.Shared.Next(0, Alphabeth.Length)];
            _sw.Restart();
        }
    }
}
