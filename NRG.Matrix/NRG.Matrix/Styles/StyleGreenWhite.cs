using NRG.Matrix.Displays;
using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix.Styles;

public class StyleGreenWhite : IMatrixStyle
{
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

    private readonly Stopwatch _generateNewSW = Stopwatch.StartNew();
    private readonly int _generateNewTimeBase = 20000;
    private int _generateNewTime = 0;

    public Task HandleKeyInput(ConsoleKeyInfo keyInfo)
    {
        return Task.CompletedTask;
    }

    public Task<bool> UpdateInternalObjects()
    {
        if (_display.HasResolutionChanged(out var width, out var height))
        {
            _generateNewTime = _generateNewTimeBase / width;
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

        return Task.FromResult(true);
    }

    public async Task DisplayFrame()
    {
        await _display.Display(_chars);
    }

    public Shot GenerateShot(int length = 20)
    {
        var x = Random.Shared.Next(0, Console.BufferWidth);
        var y = 0;
        var z = (byte)Random.Shared.Next(0, 3);
        var fallDelay = (Random.Shared.Next(50, 400));
        var shot = _shotPool.Rent();
        shot.Init(x, z, y, length, fallDelay);

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
}
