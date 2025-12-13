using System.Diagnostics;
using System.Text;

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

/* Settings
 * FPS
 * Style
 * Shot generation rate
 * Falling speed
 * Char generation rate
 * Char change speed?
 */
public class Matrix(MatrixOptions options)
{
    public async Task Enter(CancellationToken token)
    {
        var width = 0;
        var height = 0;
        var generateNewObjectSW = Stopwatch.StartNew();
        var generateNewObjectTimeBase = TimeSpan.FromMilliseconds(20000);
        var generateNewObjectTime = generateNewObjectTimeBase;
        var shots = new List<Shot>();
        var chars = new List<IMatrixChar>();

        var frameTime = Stopwatch.StartNew();
        var targetFrameTime = (int)(1000 / (float)30);
        var shotFactory = new MatrixShotFactory();
        var writeTask = Task.CompletedTask;
        double maxFT = int.MinValue;
        double minFT = int.MaxValue;
        var avgFT = new List<double>();
        var loopCount = 0;

        var sb = new StringBuilder();
        while (!token.IsCancellationRequested)
        {
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Start");
            if (width != Console.BufferWidth || height != Console.BufferHeight)
            {
                width = Console.BufferWidth;
                height = Console.BufferHeight;
                MatrixConsole.Initialize();
                generateNewObjectTime = generateNewObjectTimeBase.Divide(width);
                sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Resize Window");
            }

            if (generateNewObjectSW.Elapsed > generateNewObjectTime)
            {
                var shot = shotFactory.GenerateShot(options.Style, 15);

                chars.AddRange(shot.Chars);
                shots.Add(shot);
                generateNewObjectSW.Restart();
                sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Object Generated");
            }

            if (!shots.Any(e => e.IsExceeded))
            {
                sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - No shots to fall -> continue");
                await Task.Delay(targetFrameTime);
                frameTime.Restart();
                continue;
            }

            foreach (var shot in shots.Where(e => e.IsExceeded))
            {
                shot.Fall();
                shot.SW.Restart();
            }
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Fall Shots");

            var shotsToRemove = shots.Where(e => e.YTop > height).ToArray();
            foreach (var shot in shotsToRemove)
            {
                shots.Remove(shot);
                shotFactory.ShotPool.Return(shot);
                foreach (var c in shot.Chars.OfType<MatrixCharDynamic>())
                {
                    chars.Remove(c);
                    shotFactory.CharPool.Return(c);
                }
            }
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Remove Shots and Chars");

            foreach (var o in chars)
            {
                if (o is MatrixCharDynamic d)
                {
                    d.PickNewCharIfNeeded();
                }

                MatrixConsole.Set(o.Shot.X, o.Y, o);
            }
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Set Chars to Buffer (Objs: {chars.Count})");

            //var buffers = MatrixConsole.DrawBufferChanged3();
            //sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Calculate Buffer (Buffers: {buffers.Length})");
            //var tasks = buffers.Select(e => Console.Out.WriteAsync(e));
            //await Task.WhenAll(tasks);
            //foreach (var b in buffers)
            //{
            //    Console.Write(b);
            //    sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Draw Buffer Async (Len: {b.Length})");
            //}
            //sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Finish Draw Buffer Async");

            //var buffer = MatrixConsole.DrawBufferChanged2(sb, frameTime);
            var buffer = MatrixConsole.DrawBuffer();
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Calculate Buffer (BufferLen: {buffer.Length})");
            //var text = buffer.ToString();
            //var text = new char[buffer.Length];
            //buffer.CopyTo(0, text, text.Length);
            //sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Perform buffer.ToString()");
            //await writeTask;
            //sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Await Draw Buffer Async");
            //writeTask = Console.Out.WriteAsync(buffer);
            //writeTask = new Task(() => Console.Write(buffer));
            //writeTask.Start();
            Console.Write(buffer);
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Start Draw Buffer Async");

            //MatrixConsole.DrawBufferChanged();
            //await writeTask;
            //writeTask = MatrixConsole.DrawBufferChanged2();
            //Console.Write(buffer);
            //var span = new Span<char>();
            //var span = new char[buffer.Length];
            //buffer.CopyTo(0, span, buffer.Length);
            //Console.Write(span);
            //var stdout = Console.OpenStandardOutput();
            //stdout.Write()
            //var charBuffer = new Span<char>();
            //buffer.CopyTo(charBuffer,)
            //sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Draw Buffer");
            //shots.RemoveAll(e => e.X >= width || e.YTop > height);

            //var remainingFrameTime = targetFrameTime - (int)frameTime.Elapsed.TotalMilliseconds;
            //if (remainingFrameTime > 5)
            //{
            //    //await Task.Delay(remainingFrameTime);
            //}

            if (++loopCount > 1000)
            {
                loopCount = 0;
                maxFT = int.MinValue;
                minFT = int.MaxValue;
            }

            var curFT = frameTime.Elapsed.TotalMilliseconds;
            avgFT.Add(curFT);
            if (avgFT.Count > 100)
            {
                avgFT.RemoveAt(0);
            }

            if (maxFT < curFT)
            {
                maxFT = curFT;
            }

            if (minFT > curFT)
            {
                minFT = curFT;
            }

            var avg = avgFT.Average();
            Console.Title = $"{chars.Count} {curFT:00.0} | {minFT:00.0} | {maxFT:00.0} | {avgFT.Average():00.0}";
            sb.AppendLine($"{frameTime.Elapsed.TotalMilliseconds:000.0000} - Loop End");
            //Console.WriteLine(sb);
            //if (frameTime.ElapsedMilliseconds > 200 || (curFT == maxFT && curFT > 30))
            if (avg > 20)
            {
                frameTime.Stop();
                var mesage = sb.ToString();
            }
            //PrintFps(frameTime.Elapsed.TotalSeconds, shots.Count);
            sb.Clear();
            frameTime.Restart();
        }
    }

    private void PrintFps(double elapsedSeconds, int count)
    {
        var fps = Math.Round(1d / elapsedSeconds);
        Console.Title = $"S: {count:D3} - FPS: {fps}";
    }
}

public class Shot
{
    public Shot() { }

    public void Init(int x, byte z, int y, int len, TimeSpan time)
    {
        X = x;
        YBottom = y;
        YTop = y - len;
        Z = z;
        Time = time;
        SW.Restart();
    }

    public int X { get; set; }
    public int Z { get; set; }
    public TimeSpan Time { get; set; }

    public int YTop { get; set; }
    public int YBottom { get; set; }
    public bool IsExceeded => SW.Elapsed >= Time;
    public Stopwatch SW { get; } = Stopwatch.StartNew();
    public IMatrixChar[] Chars { get; set; } = [];

    public void Fall(int yFall = 1)
    {
        YTop += yFall;
        YBottom += yFall;
        foreach (var c in Chars)
        {
            c.Y += yFall;
        }
    }
}

public interface IMatrixChar : IMatrixConsoleChar
{
    Shot Shot { get; }
    int Y { get; set; }
}

public record MatrixChar(Shot Shot, char Char, RGB Rgb) : IMatrixChar
{
    public int Y { get; set; }
    public string AnsiConsoleColor => Rgb.AnsiConsoleColor;
}

public record MatrixCharDynamic : IMatrixChar
{
    private readonly Stopwatch _sw = Stopwatch.StartNew();

    public MatrixCharDynamic() { }

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
    }

    public Shot Shot { get; set; }
    public char[] Alphabeth { get; set; }
    public RGB Rgb { get; set; }
    public int ChangeTimeMilliseconds { get; set; }
    public int Y { get; set; }
    public string AnsiConsoleColor { get; private set; }
    public bool IsExpired => _sw.Elapsed.TotalMilliseconds >= ChangeTimeMilliseconds;
    public char Char { get; set; }
    public void PickNewCharIfNeeded()
    {
        if (IsExpired)
        {
            Char = Alphabeth[Random.Shared.Next(0, Alphabeth.Length)];
            _sw.Restart();
        }
    }
}

//public record MatrixCharDynamic(Shot Shot, char[] Alphabeth, RGB Rgb, int ChangeTimeMilliseconds) : IMatrixChar
//{
//    private readonly Stopwatch _sw = Stopwatch.StartNew();

//    public int Y { get; set; }
//    public string AnsiConsoleColor => Rgb.AnsiConsoleColor;
//    public bool IsExpired => _sw.Elapsed.TotalMilliseconds >= ChangeTimeMilliseconds;
//    public char Char
//    {
//        get
//        {
//            if (IsExpired)
//            {
//                field = Alphabeth[Random.Shared.Next(0, Alphabeth.Length)];
//                _sw.Restart();
//            }

//            return field;
//        }
//    } = Alphabeth[Random.Shared.Next(0, Alphabeth.Length)];
//}