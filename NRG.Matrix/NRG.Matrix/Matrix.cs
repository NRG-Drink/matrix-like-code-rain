using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix;

public class Matrix
{
    public async Task Enter(CancellationToken token)
    {
        var width = 0;
        var height = 0;
        var generateNewObjectSW = Stopwatch.StartNew();
        var generateNewObjectTime = TimeSpan.FromMilliseconds(500);
        var shots = new List<Shot>();
        var objs = new List<MatrixChar>();

        var shotCount = 0;
        var frameTime = Stopwatch.StartNew();
        var targetFrameTime = 1000 / (float)60;

        while (!token.IsCancellationRequested)
        {
            if (width!= Console.BufferWidth || height != Console.BufferHeight)
            {
                width = Console.BufferWidth;
                height = Console.BufferHeight;
                MatrixConsole.Initialize();
            }

            if (generateNewObjectSW.Elapsed > generateNewObjectTime)
            {
                var shot = new Shot(Random.Shared.Next(0, Console.BufferWidth), 0, TimeSpan.FromMilliseconds(Random.Shared.Next(200, 1000)));
                shotCount++;
                var numberStr = $"{shotCount:D12}";
                var chars = numberStr
                    .Reverse()
                    .Select((e, i) => new MatrixChar(shot, e, new(200, 200, 200)) { Y = 0 - i})
                    .ToArray();
                //var chars = numberStr.Select((e, i) => new MatrixChar(shot, (char)Random.Shared.Next(33, 90), new(200, 200, 200), Random.Shared.Next(0, Console.BufferWidth)) { Y = 0 - i}).ToArray();
                shot.Chars = chars;

                objs.AddRange(chars);
                shots.Add(shot);
                generateNewObjectSW.Restart();
            }

            var shotsToFall = shots.Where(e => e.IsExceeded).ToArray();
            var charsToFall = objs;
            foreach (var o in charsToFall)
            {
                MatrixConsole.Set(o.Shot.X, o.Y, o);
            }

            foreach (var shot in shotsToFall)
            {
                shot.Fall();
                shot.SW.Restart();
            }

            MatrixConsole.DrawBuffer();
            MatrixConsole.Clear();
            //var wait = Math.Max(0, targetFrameTime - frameTime.Elapsed.TotalMilliseconds) / 2;
            //await Task.Delay((int)wait, token);
            var fps = Math.Round(1 / frameTime.Elapsed.TotalSeconds);
            //Console.Title = $"FPS: {fps}";
            Console.Title = $"{frameTime.Elapsed.TotalMilliseconds}";
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
