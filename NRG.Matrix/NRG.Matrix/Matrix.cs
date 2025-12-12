using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix;

public enum MatrixStyles
{
    ZeroOne = 0,
    ShotCount = 1,
}

public record MatrixOptions
{
    public MatrixStyles Style { get; set; }
}

public class Matrix(MatrixOptions options)
{
    public async Task Enter(CancellationToken token)
    {
        var width = 0;
        var height = 0;
        var generateNewObjectSW = Stopwatch.StartNew();
        var generateNewObjectTimeBase = TimeSpan.FromMilliseconds(30000);
        var generateNewObjectTime = generateNewObjectTimeBase;
        var shots = new List<Shot>();
        var chars = new List<MatrixChar>();

        //var shotCount = 0;
        var frameTime = Stopwatch.StartNew();
        var targetFrameTime = 1000 / (float)100;
        var shotFactory = new MatrixShotFactory();

        while (!token.IsCancellationRequested)
        {
            if (width != Console.BufferWidth || height != Console.BufferHeight)
            {
                width = Console.BufferWidth;
                height = Console.BufferHeight;
                MatrixConsole.Initialize();
                generateNewObjectTime = generateNewObjectTimeBase.Divide(width);
            }

            if (generateNewObjectSW.Elapsed > generateNewObjectTime)
            {
                var shot = shotFactory.GenerateShot(options.Style, 8);

                chars.AddRange(shot.Chars);
                shots.Add(shot);
                generateNewObjectSW.Restart();
            }

            var shotsToFall = shots.Where(e => e.IsExceeded).ToArray();
            var charsToFall = chars.OrderByDescending(e => e.Shot.Z);
            foreach (var o in charsToFall)
            {
                MatrixConsole.Set(o.Shot.X, o.Y, o);
            }

            foreach (var shot in shotsToFall)
            {
                shot.Fall();
                shot.SW.Restart();
            }

            MatrixConsole.DrawBufferChanged();
            var wait = Math.Max(0, targetFrameTime - frameTime.Elapsed.TotalMilliseconds);
            await Task.Delay((int)wait, token);
            var fps = Math.Round(1 / frameTime.Elapsed.TotalSeconds);
            Console.Title = $"FPS: {fps}";
            //Console.Title = $"{frameTime.Elapsed.TotalMilliseconds}";
            frameTime.Restart();
        }
    }
}

public record Shot(int X, byte Z, TimeSpan Time)
{
    public bool IsExceeded => SW.Elapsed >= Time;
    public Stopwatch SW { get; } = Stopwatch.StartNew();
    public MatrixChar[] Chars { get; set; } = [];

    public void Fall(int Y = 1)
    {
        foreach (var c in Chars)
        {
            c.Y += Y;
        }
    }
}

public record MatrixChar(Shot Shot, char Char, RGB Rgb) : IMatrixConsoleChar
{
    public int Y { get; set; }
    public string AnsiConsoleColor => Rgb.AnsiConsoleColor;
}
