using NRG.Matrix.Displays;
using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix.Styles;

public class StyleGreenWhite : IMatrixStyle
{
    public long Frametime { get; set; }
    private readonly ObjectPool<CharDynamic> _charPool = new();
    private readonly ObjectPool<Shot> _shotPool = new();
    private readonly RGB _colorHead = new(220, 220, 250);
    private readonly RGB _colorTail = new(30, 200, 60);
    private readonly char[] _greenWhiteChars = [
        .. Alphabeth.LatinUpper,
        .. Alphabeth.Numbers,
        .. Alphabeth.Katakana,
        .. Alphabeth.Symbols,
    ];

    private readonly AnsiConsolePrintAll _display = new();
    private readonly List<Shot> _shots = [];
    private readonly List<CharDynamic> _chars = [];
    private readonly List<IAnsiConsoleChar> _statistics = [];
    private readonly List<IAnsiConsoleChar> _controls = [];
    private readonly RGB _statisticColor = new(60, 60, 250);
    private readonly RGB _controlColor = new(120, 40, 40);

    private readonly Stopwatch _generateNewSW = Stopwatch.StartNew();
    private NumberWithRandomness _fallDelay = new(225, 175);
    private int _generateNewTimeBase = 20000;
    private int _generateNewTime = 0;
    private bool _showStatisticsPanel = true;
    private bool _showControlsPanel = true;
    private readonly Queue<long> _frameTimeHistory = [];

    public void SetFrametime(long frametime)
    {
        if (_frameTimeHistory.Count >= 100)
        {
            _frameTimeHistory.Dequeue();
        }

        _frameTimeHistory.Enqueue(frametime);
    }

    public Task HandleKeyInput(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Modifiers is ConsoleModifiers.None)
        {
            if (keyInfo.Key is ConsoleKey.S)
            {
                _showStatisticsPanel = !_showStatisticsPanel;
            }
            else if (keyInfo.Key is ConsoleKey.C)
            {
                _showControlsPanel = !_showControlsPanel;
            }
        }
        // Handle generate new speed.
        else if (keyInfo.Modifiers is ConsoleModifiers.Control)
        {
            if (keyInfo.Key is ConsoleKey.UpArrow or ConsoleKey.RightArrow)
            {
                _generateNewTimeBase = (int)(_generateNewTimeBase / 1.01);
            }
            else if (keyInfo.Key is ConsoleKey.DownArrow or ConsoleKey.LeftArrow)
            {
                _generateNewTimeBase = (int)(_generateNewTimeBase * 1.01);
            }

            _generateNewTime = _generateNewTimeBase / _display.Width;
        }
        // Handle fall-delay speed and spread change.
        else if (keyInfo.Modifiers is ConsoleModifiers.Shift)
        {
            if (keyInfo.Key is ConsoleKey.UpArrow)
            {
                _fallDelay = new(_fallDelay.Number + 5, _fallDelay.Spread);
            }
            else if (keyInfo.Key is ConsoleKey.DownArrow)
            {
                _fallDelay = new(_fallDelay.Number - 5, _fallDelay.Spread);
            }
            else if (keyInfo.Key is ConsoleKey.RightArrow)
            {
                _fallDelay = new(_fallDelay.Number, _fallDelay.Spread + 5);
            }
            else if (keyInfo.Key is ConsoleKey.LeftArrow)
            {
                _fallDelay = new(_fallDelay.Number, _fallDelay.Spread - 5);
            }
        }

        return Task.CompletedTask;
    }

    public Task<bool> UpdateInternalObjects()
    {
        if (_display.HasResolutionChanged(out var width, out var height))
        {
            _generateNewTime = _generateNewTimeBase / width;
            _controls.Clear();
            _controls.AddRange(GetControlChars());
        }

        var isGenerateNew = _generateNewTime < _generateNewSW.ElapsedMilliseconds;
        var isFall = _shots.Any(e => e.IsExpired);
        var isChar = _chars.Any(e => e.IsExpired);
        if (!isGenerateNew && !isFall && !isChar)
        {
            return Task.FromResult(false);
        }

        // Generate new shot
        if (_generateNewTime < _generateNewSW.ElapsedMilliseconds)
        {
            var shot = GenerateShot(20);
            _shots.Add(shot);
            _chars.AddRange(shot.Chars.OfType<CharDynamic>());
            _generateNewSW.Restart();
        }

        // Make shots fall
        foreach (var shot in _shots.Where(e => e.IsExpired))
        {
            shot.Fall();
            shot.SW.Restart();
        }

        // Remove shots outside screen
        var shotsToRemove = _shots.Where(e => e.YTop > height).ToArray();
        foreach (var shot in shotsToRemove)
        {
            _shots.Remove(shot);
            _shotPool.Return(shot);
            foreach (var c in shot.Chars.OfType<CharDynamic>())
            {
                _chars.Remove(c);
                _charPool.Return(c);
            }
        }

        // Update dynamic char
        foreach (var c in _chars.Where(e => e.IsExpired))
        {
            c.PickNewCharIfNeeded();
        }

        // Update statistics
        UpdateStatistics();

        return Task.FromResult(true);
    }

    public async Task DisplayFrame()
    {
        var grouped = new Dictionary<XY, List<IAnsiConsoleChar>>();

        IEnumerable<IAnsiConsoleChar> chars = _chars;
        if (_showStatisticsPanel)
        {
            chars = chars.Concat(_statistics);
        }

        if (_showControlsPanel)
        {
            chars = chars.Concat(_controls);
        }

        //foreach (var c in _chars.Concat(_statistics).Concat(_controls))
        foreach (var c in chars)
        {
            var xy = new XY(c.X, c.Y);
            if (grouped.TryGetValue(xy, out var list) && list is not null)
            {
                list.Add(c);
                continue;
            }

            grouped.Add(xy, [c]);
        }

        // The highest Z value per point will be displayed.
        var ordered = grouped.Select(e => e.Value.OrderByDescending(e => e.Z).First());

        await _display.Display(ordered);
    }

    private IAnsiConsoleChar[] ToAnsiConsoleChars(int x, int y, int z, string text, RGB color)
    {
        var chars = new List<IAnsiConsoleChar>();
        var lines = text.Split(Environment.NewLine);
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Select((e, si) => new CharStatic()
            {
                Char = e,
                X = x + si,
                Y = y + i,
                Z = z,
                AnsiColor = color.AnsiConsoleColor,
            });
            chars.AddRange(line);
        }

        return [.. chars];
    }

    public Shot GenerateShot(int length = 20)
    {
        var x = Random.Shared.Next(0, _display.Width);
        var y = 0;
        var z = (byte)Random.Shared.Next(0, 3);
        var shot = _shotPool.Rent();
        shot.Init(x, z, y, length, _fallDelay.NumberWithSpread);

        var chars = _charPool.Rent(length);
        chars[0].Init(shot, _greenWhiteChars, _colorHead, 1000, y--);
        for (var i = 1; i < length; i++)
        {
            var color = _colorTail.Luminos(i * 3);
            chars[i].Init(shot, _greenWhiteChars, color, 500, y--);
        }

        shot.Chars = chars;

        return shot;
    }

    private void UpdateStatistics()
    {
        _statistics.Clear();
        if (!_showStatisticsPanel)
        {
            return;
        }


        long avg = 0, min = 0, max = 0;
        if (_frameTimeHistory.Count > 0)
        {
            avg = (long)_frameTimeHistory.Average();
            min = _frameTimeHistory.Min();
            max = _frameTimeHistory.Max();
        }

        var n = Environment.NewLine;
        var statisticsText =
            $"Frame time (avg/min/max): {avg:000} / {min:000} / {max:000} ms" +
            $"{n}FPS (theo): {(avg > 0 ? (int)(1000 / avg) : 1000):00000} fps" +
            $"{n}Shot Count: {_shots.Count:00000} pcs" +
            $"{n}Char Count: {_chars.Count:00000} pcs" +
            $"{n}New Object: {_generateNewTime:00000} ms" +
            $"{n}Falltime: {_fallDelay} ms";
        _statistics.AddRange(ToAnsiConsoleChars(1, 1, 99, statisticsText, _statisticColor));
    }

    private IAnsiConsoleChar[] GetControlChars()
    {
        var controlText = $"Controls: " +
            $"{Environment.NewLine}C: Controls (this)" +
            $"{Environment.NewLine}S: Show Statistics Panel" +
            $"{Environment.NewLine}SHIFT+Arrows: Shot Speed" +
            $"{Environment.NewLine}CTRL+Arrows: Object Generation Time";

        return ToAnsiConsoleChars(1, _display.Height - 5, 98, controlText, _controlColor);
    }

    private readonly record struct XY(int X, int Y);
}
