using System.Drawing;

namespace NRG.Matrix.App;

public record DisplayObject
{
	private static readonly Random _random = new();
	private static readonly int _traceLength = 25;
	private string? _symbol;
	private string _lastRandom = GetRandomSymbol();

	public DisplayObject(int xRange)
	{
		Pos = new Point(_random.Next(0, xRange), 0);
		IsTrace = false;
		Color = OtherColor;
	}

	public DisplayObject(Point pos, int yOffset)
	{
		Pos = pos with { Y = pos.Y - yOffset };
		IsTrace = true;
		Color = TraceColor;
	}

	public static ConsoleColor TraceColor { get; set; } = ConsoleColor.DarkGreen;
	public static ConsoleColor OtherColor { get; set; } = ConsoleColor.Gray;

    public Point Pos { get; init; } = new(999, 999);
	public bool IsTrace { get; init; } = false;
	public ConsoleColor Color { get; init; } = ConsoleColor.Gray;

	public string Symbol
	{
		get => _symbol is null ? GetNextRandomSymbol() : _symbol;
		init => _symbol = value;
	}

	public DisplayObject Fall(int x = 0, int y = 1)
		=> this with { Pos = new Point(Pos.X + x, Pos.Y + y) };

	public IEnumerable<DisplayObject> Trace()
		=> Enumerable.Range(1, _traceLength)
			.Select(GetTrace)
			.Append(GetClear(_traceLength + 1));

	private DisplayObject GetTrace(int offset)
		=> new(Pos, offset);

	private DisplayObject GetClear(int offset)
		=> new(Pos, offset) { Symbol = " " };

	private string GetNextRandomSymbol()
	{
		var getRandom = ShouldGetNewRandom();
		if (!IsTrace && !getRandom)
		{
			// Double chance for changed symbol on non trace object.
			getRandom = ShouldGetNewRandom();
		}
		if (getRandom)
		{
			_lastRandom = DisplayObject.GetRandomSymbol();
		}
		return _lastRandom;
	}

	private static string GetRandomSymbol()
		=> ((char)_random.Next(33, 123)).ToString();

	private static bool ShouldGetNewRandom()
		=> _random.Next(0, 3) == 0;
}