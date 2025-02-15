//using NRG.Matrix.Models;
//using System.Drawing;

//namespace NRG.Matrix;

//public record DisplayObject
//{
//	private readonly int _traceLength = 20 + Random.Shared.Next(0, 11);
//    private readonly int _newSymbolChance;
//	private char _lastRandom = GetRandomSymbol();

//	public DisplayObject(int xRange)
//	{
//		var p = new Point(Random.Shared.Next(0, xRange), 0);
//        MatrixSymbol = new(p, ' ', ConsoleColor.Gray);
//		IsTrace = false;
//		_newSymbolChance = 3;
//	}

//	public DisplayObject(Point pos, int yOffset, char? symbol = null)
//	{
//		var p = pos with { Y = pos.Y - yOffset };
//		MatrixSymbol = new(p, symbol ?? GetRandomSymbol(), ConsoleColor.DarkGreen);
//		IsTrace = true;
//		_newSymbolChance = 6;
//	}

//    public MatrixObject MatrixSymbol { get; init; }
//	public bool IsTrace { get; init; } = false;

//	public DisplayObject Fall(int x = 0, int y = 1)
//		=> this with
//		{
//			MatrixSymbol = new(
//				new(MatrixSymbol.Pos.X + x, MatrixSymbol.Pos.Y + y), 
//				GetNextRandomSymbol(),
//				MatrixSymbol.Color
//			)
//		};

//	public IEnumerable<DisplayObject> Trace()
//		=> Enumerable.Range(1, _traceLength)
//			.Select(GetTrace)
//			.Append(GetClear(_traceLength + 1));

//	private DisplayObject GetTrace(int offset, int _)
//		=> new(MatrixSymbol.Pos, offset);

//	private DisplayObject GetClear(int offset)
//		=> new(MatrixSymbol.Pos, offset, ' ');

//	private char GetNextRandomSymbol()
//	{
//		var isNewSymbol = ShouldGetNewSymbol(_newSymbolChance);
//		if (isNewSymbol)
//		{
//			_lastRandom = GetRandomSymbol();
//		}
//		return _lastRandom;
//	}

//	private static char GetRandomSymbol()
//		=> (char)Random.Shared.Next(33, 123);

//	private static bool ShouldGetNewSymbol(int chance)
//		=> Random.Shared.Next(0, chance) == 0;
//}