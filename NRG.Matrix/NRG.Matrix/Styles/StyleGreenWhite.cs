using NRG.Matrix.Displays;
using NRG.Matrix.Models;
using System.Diagnostics;
using System.Text;

namespace NRG.Matrix.Styles;

public class StyleGreenWhite : IMatrixStyle
{
    private const int MaxShotLength = 30;

    public long Frametime { get; set; }
    private readonly ObjectPool<CharDynamic> _charPool = new();
    private readonly ObjectPool<Shot> _shotPool = new();
    private readonly ObjectPool<CharStatic> _charStaticPool = new();
    private readonly RGB _colorHead = new(220, 220, 250);
    private readonly RGB _colorTail = new(30, 200, 60);
    // Pre-calculated tail colors to avoid RGB allocations in hot path
    private readonly RGB[] _tailColors;
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
    private readonly StringBuilder _controlBuilder = new();

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
        // Pre-calculate all possible tail colors to avoid allocations during GenerateShot
        _tailColors = new RGB[MaxShotLength];
        _tailColors[0] = _colorHead; // Head color
        for (var i = 1; i < MaxShotLength; i++)
        {
            _tailColors[i] = _colorTail.Luminos(i * 3);
        }

        AddKeyInputHandlers();
        SetControlChars();
        //_controls.AddRange(SetControlChars());
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

    public bool UpdateInternalObjects()
    {
        if (_display.HasResolutionChanged(out var width, out var height))
        {
            _generateNewTime = Math.Max(_generateNewTimeBase / width, 1);
            SetControlChars();
        }

        var isGenerateNew = _generateNewTime < _generateNewSW.ElapsedMilliseconds;

        // Check if any shot needs to fall (avoid LINQ allocation)
        var isFall = false;
        for (var i = 0; i < _shots.Count; i++)
        {
            if (_shots[i].IsExpired)
            {
                isFall = true;
                break;
            }
        }

        // Check if any char needs update (avoid LINQ allocation)
        var isChar = false;
        for (var i = 0; i < _chars.Count; i++)
        {
            if (_chars[i].IsExpired)
            {
                isChar = true;
                break;
            }
        }

        if (!(isGenerateNew || isFall || isChar))
        {
            return false;
        }

        // Generate new shot
        if (isGenerateNew)
        {
            var shot = GenerateShot(20);
            _shots.Add(shot);

            // Avoid OfType<T>() allocation - we know all chars are CharDynamic
            var chars = shot.Chars;
            var charsLength = shot.CharsLength;
            for (var i = 0; i < charsLength; i++)
            {
                if (chars[i] is CharDynamic cd)
                {
                    _chars.Add(cd);
                }
            }

            _generateNewSW.Restart();
        }

        // Make shots fall (avoid LINQ Where allocation)
        for (var i = 0; i < _shots.Count; i++)
        {
            var shot = _shots[i];
            if (shot.IsExpired)
            {
                shot.Fall();
                shot.SW.Restart();
            }
        }

        // Remove shots outside screen - iterate backwards to avoid collection modification issues
        for (var i = _shots.Count - 1; i >= 0; i--)
        {
            var shot = _shots[i];
            if (shot.YTop <= height)
            {
                continue;
            }

            _shots.RemoveAt(i);
            _shotPool.Return(shot);
            // Return chars to pool using CharsLength
            var chars = shot.Chars;
            var charsLength = shot.CharsLength;
            for (var j = 0; j < charsLength; j++)
            {
                if (chars[j] is CharDynamic charDynamic)
                {
                    _chars.Remove(charDynamic);
                    _charPool.Return(charDynamic);
                }
            }
        }

        // Update dynamic char (avoid LINQ Where allocation)
        for (var i = 0; i < _chars.Count; i++)
        {
            var c = _chars[i];
            if (c.IsExpired)
            {
                c.PickNewCharIfNeeded();
            }
        }

        // Update statistics
        UpdateStatistics();

        return true;
    }

    public void DisplayFrame()
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

        _display.Display(_grouped.Values);
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

    public void HandleKeyInput(ConsoleKeyInfo keyInfo)
    {
        _keyInputHandler.ExecuteFirstMatchingHandler(keyInfo);
    }

    public Shot GenerateShot(int length = 20)
    {
        // Clamp length to pre-calculated color array size
        var actualLength = Math.Min(length, MaxShotLength);
        var x = Random.Shared.Next(0, _display.Width);
        var y = 0;
        var z = (byte)Random.Shared.Next(0, 3);
        var shot = _shotPool.Rent();
        shot.Init(x, z, y, actualLength, _fallDelay.NumberWithSpread);

        var chars = _charPool.Rent(actualLength);
        // Use pre-calculated colors to avoid RGB allocations
        chars[0].Init(shot, _greenWhiteChars, _tailColors[0], 1000, y--);
        for (var i = 1; i < actualLength; i++)
        {
            chars[i].Init(shot, _greenWhiteChars, _tailColors[i], 500, y--);
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

    private void UpdateStatistics()
    {
        if (!_showStatisticsPanel)
        {
            return;
        }

        foreach (var s in _statistics)
        {
            _charStaticPool.Return((CharStatic)s);
        }

        _statistics.Clear();
        //_statisticsPoolIndex = 0;

        long avg = 0, min = 0, max = 0;
        if (_frameTimeHistory.Count > 0)
        {
            // Manual calculation to avoid ZLinq allocations
            long sum = 0;
            min = long.MaxValue;
            max = long.MinValue;
            foreach (var ft in _frameTimeHistory)
            {
                sum += ft;
                if (ft < min) min = ft;
                if (ft > max) max = ft;
            }
            avg = sum / _frameTimeHistory.Count;
        }

        // Use StringBuilder with direct append to avoid ToString allocations
        _statisticsBuilder.Clear();
        _statisticsBuilder
            .Append("Frame time (avg/min/max): ").AppendPadded(avg, 3).Append(" / ").AppendPadded(min, 3).Append(" / ").AppendPadded(max, 3).Append(" ms").Append(Environment.NewLine)
            .Append("FPS (theo): ").AppendPadded(avg > 0 ? 1000 / avg : 1000, 5).Append(" fps").Append(Environment.NewLine)
            .Append("Shot Count: ").AppendPadded(_shots.Count, 5).Append(" pcs").Append(Environment.NewLine)
            .Append("Char Count: ").AppendPadded(_chars.Count, 5).Append(" pcs").Append(Environment.NewLine)
            .Append("New Base:   ").AppendPadded(_generateNewTimeBase, 5).Append(" ms").Append(Environment.NewLine)
            .Append("Cons Width: ").AppendPadded(_display.Width, 5).Append(" crs").Append(Environment.NewLine)
            .Append("New Object: ").AppendPadded(_generateNewTime, 5).Append(" ms").Append(Environment.NewLine)
            .Append("Falltime: ").Append(_fallDelay.Number).Append('/').Append(_fallDelay.Spread).Append(" ms");

        // Convert StringBuilder to CharStatic objects without allocating new string
        AddStringBuilderAsChars(1, 1, 99, _statisticsBuilder, _statisticColor.AnsiConsoleColor, _statistics);
    }

    private void SetControlChars()
    {
        if (_controlBuilder.Length < 2)
        {
            _controlBuilder
                .Append("Controls: ").Append(Environment.NewLine)
                .Append("C: Controls (this)").Append(Environment.NewLine)
                .Append("S: Show Statistics Panel").Append(Environment.NewLine)
                .Append("SHIFT+Arrows: Shot Speed").Append(Environment.NewLine)
                .Append("CTRL+Arrows: Object Generation ");
        }

        _controls.Clear();
        AddStringBuilderAsChars(1, _display.Height - 5, 98, _controlBuilder, _controlColor.AnsiConsoleColor, _controls);
    }

    /// <summary>
    /// Converts StringBuilder content to CharStatic objects without allocating a string.
    /// Reuses pooled CharStatic objects to avoid per-frame allocations.
    /// </summary>
    private void AddStringBuilderAsChars(int x, int y, int z, StringBuilder sb, string ansiColor, List<IAnsiConsoleChar> target)
    {
        var currentX = x;
        var currentY = y;

        for (var i = 0; i < sb.Length; i++)
        {
            var c = sb[i];

            // Handle newlines
            if (c == '\r')
            {
                continue; // Skip carriage return
            }
            if (c == '\n')
            {
                currentY++;
                currentX = x;
                continue;
            }

            var charStatic = _charStaticPool.Rent();

            // Update the pooled object
            charStatic.Char = c;
            charStatic.X = currentX;
            charStatic.Y = currentY;
            charStatic.Z = z;
            charStatic.AnsiColor = ansiColor;

            target.Add(charStatic);
            currentX++;
        }
    }

    private readonly record struct XY(int X, int Y);
}
