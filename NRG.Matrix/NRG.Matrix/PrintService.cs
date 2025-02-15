using NRG.Matrix.Models;

namespace NRG.Matrix;

public class PrintService
{
	private ConsoleSize _windowDimension = new();

	public void PrintObjects(IEnumerable<MatrixObject> obj)
	{
		try
		{
			HandleWindowSizeChange();
			Console.CursorVisible = false;
			var validCoordinates = obj
				.Where(e => e.Pos.Y >= 0)
				.Where(e => e.Pos.Y < _windowDimension.Height)
				//.Where(e => e.Pos.X >= 0)
				.Where(e => e.Pos.X < _windowDimension.Width)
				;

			var darkGreens = validCoordinates.Where(e => e.Color is ConsoleColor.DarkGreen);
			PrintToConsoleByLine(darkGreens, ConsoleColor.DarkGreen);

			var others = validCoordinates.Where(e => e.Color is not ConsoleColor.DarkGreen);
			foreach (var o in others)
			{
                Console.ForegroundColor = o.Color;
                Console.SetCursorPosition(o.Pos.X, o.Pos.Y);
                Console.Write(o.Symbol);
            }
		}
		catch (ArgumentOutOfRangeException)
		{
			// Window was resized to a smaller size.
			HandleWindowSizeChange();
		}
	}

	private static void PrintToConsoleByLine(IEnumerable<MatrixObject> traces, ConsoleColor color)
	{
		if (traces is null)
		{
			return;
		}

		Console.ForegroundColor = color;
		var groupedRows = traces.GroupBy(e => e.Pos.Y);
		foreach (var row in groupedRows)
		{
			var first = row.Min(e => e.Pos.X);
			var line = GetLineText(row, first);

			Console.SetCursorPosition(first, row.Key);
			Console.Write(line);
		}
	}

	private static char[] GetLineText(IEnumerable<MatrixObject> row, int first)
	{
		var width = Console.WindowWidth;
		var inRanges = row.Where(e => e.Pos.X < width).ToArray();
		var len = inRanges.Max(e => e.Pos.X) - first + 1;

		var line = new char[len];
		Array.Fill(line, ' ');

		foreach (var o in inRanges)
		{
			line[o.Pos.X - first] = o.Symbol;
		}

		return line;
	}

	private void HandleWindowSizeChange()
	{
		var currentSize = new ConsoleSize();
		var isSame = currentSize == _windowDimension;
		if (isSame)
		{
			return;
		}

		Console.Clear();
		_windowDimension = currentSize;
	}
}
