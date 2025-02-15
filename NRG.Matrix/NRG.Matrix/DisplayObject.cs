using System.Drawing;

namespace NRG.Matrix;

public record DisplayObject
{
	private static readonly Random _random = new();
	private readonly int _traceLength = 20 + _random.Next(0, 11);
	private char? _symbol;
	private char _lastRandom = GetRandomSymbol();

	public DisplayObject(int xRange)
	{
		Pos = new Point(_random.Next(0, xRange), 0);
		IsTrace = false;
		Color = ConsoleColor.Gray;
		NewSymbolChance = 3;
	}

	public DisplayObject(Point pos, int yOffset)
	{
		Pos = pos with { Y = pos.Y - yOffset };
		IsTrace = true;
		Color = ConsoleColor.DarkGreen;
		NewSymbolChance = 6;
	}


	public Point Pos { get; init; } = new(999, 999);
	public ConsoleColor Color { get; init; } = ConsoleColor.Gray;
	public bool IsTrace { get; init; } = false;

	public int NewSymbolChance { get; init; } = 3;
	public char Symbol
	{
		get => _symbol is null ? GetNextRandomSymbol() : _symbol.Value;
		init => _symbol = value;
	}

	public DisplayObject Fall(int x = 0, int y = 1)
		=> this with { Pos = new Point(Pos.X + x, Pos.Y + y) };

	public IEnumerable<DisplayObject> Trace()
		=> Enumerable.Range(1, _traceLength)
			.Select(GetTrace)
			.Append(GetClear(_traceLength + 1));

	private DisplayObject GetTrace(int offset, int _)
		=> new(Pos, offset);

	private DisplayObject GetClear(int offset)
		=> new(Pos, offset) { Symbol = ' ' };

	private char GetNextRandomSymbol()
	{
		var isNewSymbol = ShouldGetNewSymbol(NewSymbolChance);
		if (isNewSymbol)
		{
			_lastRandom = GetRandomSymbol();
		}
		return _lastRandom;
	}

	private static char GetRandomSymbol()
		=> (char)_random.Next(33, 123);

	private static bool ShouldGetNewSymbol(int chance)
		=> _random.Next(0, chance) == 0;
}