using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix;

public enum MatrixStyles
{
    ZeroOne = 0,
    ShotCount = 1,
    GreenWhite = 2,
}

public record MatrixOptions
{
    public MatrixStyles Style { get; set; }
}

// TODO: Implement char change
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

        var frameTime = Stopwatch.StartNew();
        var targetFrameTime = (int)(1000 / (float)60);
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
                var shot = shotFactory.GenerateShot(options.Style, 15);

                shots.Add(shot);
                generateNewObjectSW.Restart();
            }

            var shotsToFall = shots.Where(e => e.IsExceeded).ToArray();
            if (shotsToFall.Length is 0)
            {
                await Task.Delay(targetFrameTime);
                frameTime.Restart();
                continue;
            }

            var charsToFall = shots
                .SelectMany(e => e.Chars)
                .OrderByDescending(e => e.Shot.Z)
                .ToArray();
            foreach (var o in charsToFall)
            {
                MatrixConsole.Set(o.Shot.X, o.Y, o);
            }

            foreach (var shot in shotsToFall)
            {
                shot.Fall();
                shot.SW.Restart();
            }

            await MatrixConsole.DrawBufferChanged();
            shots.RemoveAll(e => e.X >= (width + 20) || e.Chars.OrderBy(e => e.Y).First().Y > (height + 20));
            var remainingFrameTime = targetFrameTime - (int)frameTime.Elapsed.TotalMilliseconds;
            if (remainingFrameTime > 5)
            {
                await Task.Delay(remainingFrameTime);
            }

            PrintFps(frameTime.Elapsed.TotalSeconds, shots.Count);
            frameTime.Restart();
        }
    }

    private void PrintFps(double elapsedSeconds, int count)
    {
        var fps = Math.Round(1d / elapsedSeconds);
        Console.Title = $"S: {count:D3} - FPS: {fps}";
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
