using NRG.Matrix.Displays;
using NRG.Matrix.Models;
using System.Diagnostics;
using System.Text;
using ZLinq;

namespace NRG.Matrix.Styles;

public class StyleGreenWhite : IMatrixStyle
{
    public long Frametime { get; set; }
    private readonly ObjectPool<CharDynamic> _charPool = new();
    private readonly ObjectPool<Shot> _shotPool = new();
    private readonly RGB _colorHead = new(220, 220, 250);
    private readonly RGB _colorTail = new(30, 200, 60);
    private readonly char[] _greenWhiteChars = [
        .. Alpabeth.LatinUpper,
        .. Alpabeth.Numbers,
        .. Alpabeth.Katakana,
        .. Alpabeth.Symbols,
    ];

    private readonly AnsiConsolePrintAll _display = new();
    private readonly List<Shot> _shots = [];
    private readonly List<CharDynamic> _chars = [];
    private readonly Dictionary<XY, IAnsiConsoleChar> _grouped = [];
    private readonly List<IAnsiConsoleChar> _statistics = [];
    private readonly List<IAnsiConsoleChar> _controls = [];
    private readonly RGB _statisticColor = new(80, 80, 255);
    private readonly RGB _controlColor = new(180, 0, 0);
    private readonly StringBuilder _statisticsBuilder = new();
    private string? _controlText;

    private readonly Stopwatch _generateNewSW = Stopwatch.StartNew();
    private NumberWithRandomness _fallDelay = new(225, 175);
    private int _generateNewTimeBase = 20000;
    private int _generateNewTime = 1;
    private bool _showStatisticsPanel = false;
    private bool _showControlsPanel = false;
    private readonly Queue<long> _frameTimeHistory = [];
    private readonly KeyInputHandler _keyInputHandler = new();

    public StyleGreenWhite()
    {
        AddKeyInputHandlers();
        _controls.AddRange(GetControlChars());
        _generateNewTime = _display.Width <= 0
            ? _generateNewTimeBase
            : Math.Max(_generateNewTimeBase / _display.Width, 1);
    }

    public void SetFrametime(long frametime)
    {
        if (_frameTimeHistory.Count >= 100)
        {
            _frameTimeHistory.Dequeue();
        }

        _frameTimeHistory.Enqueue(frametime);
    }

    public Task<bool> UpdateInternalObjects()
    {
        if (_display.HasResolutionChanged(out var width, out var height))
        {
            _generateNewTime = Math.Max(_generateNewTimeBase / width, 1);
            _controls.Clear();
            _controls.AddRange(GetControlChars());
        }

        var isGenerateNew = _generateNewTime < _generateNewSW.ElapsedMilliseconds;
        var isFall = _shots.AsValueEnumerable().Any(e => e.IsExpired);
        var isChar = _chars.AsValueEnumerable().Any(e => e.IsExpired);
        if (!(isGenerateNew || isFall || isChar))
        {
            return Task.FromResult(false);
        }

        // Generate new shot
        if (isGenerateNew)
        {
            var shot = GenerateShot(20);
            _shots.Add(shot);

            foreach (var c in shot.Chars.AsValueEnumerable().OfType<CharDynamic>())
            {
                _chars.Add(c);
            }

            _generateNewSW.Restart();
        }

        // Make shots fall
        foreach (var shot in _shots.AsValueEnumerable().Where(e => e.IsExpired))
        {
            shot.Fall();
            shot.SW.Restart();
        }

        foreach (var shot in _shots.AsValueEnumerable().Where(e => e.YTop > height))
        {
            _shots.Remove(shot);
            _shotPool.Return(shot);
            foreach (var c in shot.Chars)
            {
                if (c is CharDynamic charDynamic)
                {
                    _chars.Remove(charDynamic);
                    _charPool.Return(charDynamic);
                }
            }
        }

        // Update dynamic char
        foreach (var c in _chars.AsValueEnumerable().Where(e => e.IsExpired))
        {
            c.PickNewCharIfNeeded();
        }

        // Update statistics
        UpdateStatistics();

        return Task.FromResult(true);
    }

    public async Task DisplayFrame()
    {
        // Reuse collections instead of allocating new ones
        _grouped.Clear();

        AddToGroupedByMax(_chars);

        if (_showStatisticsPanel)
        {
            AddToGroupedByMax(_statistics);
        }

        if (_showControlsPanel)
        {
            AddToGroupedByMax(_controls);
        }

        await _display.Display(_grouped.Select(e => e.Value));
    }

    private void AddToGroupedByMax(IEnumerable<IAnsiConsoleChar> chars)
    {
        foreach (var c in chars)
        {
            var xy = new XY(c.X, c.Y);
            if (_grouped.TryGetValue(xy, out var val))
            {
                if (c.Z <= val.Z)
                {
                    continue;
                }
            }

            _grouped[xy] = c;
        }
    }

    public Task HandleKeyInput(ConsoleKeyInfo keyInfo)
    {
        _keyInputHandler.ExecuteFirstMatchingHandler(keyInfo);
        return Task.CompletedTask;
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

    private void AddKeyInputHandlers()
    {
        _keyInputHandler
            .AddGroup(e => e.Modifiers is ConsoleModifiers.None)
                .AddHandler(e => e.Key is ConsoleKey.S, e => _showStatisticsPanel = !_showStatisticsPanel)
                .AddHandler(e => e.Key is ConsoleKey.C, e => _showControlsPanel = !_showControlsPanel)
                .CloseGroup()
            .AddGroup(e => e.Modifiers is ConsoleModifiers.Control)
                .AddHandler(e => e.Key is ConsoleKey.UpArrow, SpeedUp)
                .AddHandler(e => e.Key is ConsoleKey.DownArrow, SlowDown)
                .CloseGroup(e => _generateNewTime = _display.Width <= 0 ? _generateNewTimeBase : Math.Max(_generateNewTimeBase / _display.Width, 1))
            .AddGroup(e => e.Modifiers is ConsoleModifiers.Shift)
                .AddHandler(e => e.Key is ConsoleKey.UpArrow, e => _fallDelay = new(_fallDelay.Number + 5, _fallDelay.Spread))
                .AddHandler(e => e.Key is ConsoleKey.DownArrow, e => _fallDelay = new(_fallDelay.Number - 5, _fallDelay.Spread))
                .AddHandler(e => e.Key is ConsoleKey.RightArrow, e => _fallDelay = new(_fallDelay.Number, _fallDelay.Spread + 5))
                .AddHandler(e => e.Key is ConsoleKey.LeftArrow, e => _fallDelay = new(_fallDelay.Number, _fallDelay.Spread - 5))
                .CloseGroup();
    }

    private void SpeedUp(ConsoleKeyInfo k)
    {
        var n = _generateNewTimeBase / 1.01;
        var x = (int)n;
        var max = Math.Max(x, 1);

        _generateNewTimeBase = max;
    }

    private void SlowDown(ConsoleKeyInfo k)
    {
        var p = _generateNewTimeBase;
        var n = _generateNewTimeBase * 1.01;
        var x = (int)n;
        var max = Math.Max(x, 1);
        if (max <= p)
        {
            max++;
        }

        _generateNewTimeBase = max;
    }

    private IAnsiConsoleChar[] ToAnsiConsoleChars(int x, int y, int z, string text, RGB color)
    {
        var lines = text.Split(Environment.NewLine);
        var ansiColor = color.AnsiConsoleColor;


        // Calculate total character count to pre-allocate array
        var totalChars = 0;
        for (var i = 0; i < lines.Length; i++)
        {
            totalChars += lines[i].Length;
        }

        var chars = new CharStatic[totalChars];
        var index = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var lineY = y + i;
            var line = lines[i];
            for (var si = 0; si < line.Length; si++)
            {
                chars[index++] = new CharStatic()
                {
                    Char = line[si],
                    X = x + si,
                    Y = lineY,
                    Z = z,
                    AnsiColor = ansiColor,
                };
            }
        }

        return chars;
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
            avg = (long)_frameTimeHistory.AsValueEnumerable().Average();
            min = _frameTimeHistory.AsValueEnumerable().Min();
            max = _frameTimeHistory.AsValueEnumerable().Max();
        }

        // Use StringBuilder to avoid string allocation
        _statisticsBuilder.Clear();
        _statisticsBuilder
            .Append("Frame time (avg/min/max): ").Append(avg.ToString("000")).Append(" / ").Append(min.ToString("000")).Append(" / ").Append(max.ToString("000")).Append(" ms").Append(Environment.NewLine)
            .Append("FPS (theo): ").Append((avg > 0 ? (int)(1000 / avg) : 1000).ToString("00000")).Append(" fps").Append(Environment.NewLine)
            .Append("Shot Count: ").Append(_shots.Count.ToString("00000")).Append(" pcs").Append(Environment.NewLine)
            .Append("Char Count: ").Append(_chars.Count.ToString("00000")).Append(" pcs").Append(Environment.NewLine)
            .Append("New Base:   ").Append(_generateNewTimeBase.ToString("00000")).Append(" ms").Append(Environment.NewLine)
            .Append("Cons Width: ").Append(_display.Width.ToString("00000")).Append(" crs").Append(Environment.NewLine)
            .Append("New Object: ").Append(_generateNewTime.ToString("00000")).Append(" ms").Append(Environment.NewLine)
            .Append("Falltime: ").Append(_fallDelay.ToString()).Append(" ms");

        _statistics.AddRange(ToAnsiConsoleChars(1, 1, 99, _statisticsBuilder.ToString(), _statisticColor));
    }


    private IAnsiConsoleChar[] GetControlChars()
    {
        _controlText ??= "Controls: " +
            $"{Environment.NewLine}C: Controls (this)" +
            $"{Environment.NewLine}S: Show Statistics Panel" +
            $"{Environment.NewLine}SHIFT+Arrows: Shot Speed" +
            $"{Environment.NewLine}CTRL+Arrows: Object Generation ";

        return ToAnsiConsoleChars(1, _display.Height - 5, 98, _controlText, _controlColor);
    }

    private readonly record struct XY(int X, int Y);
}
