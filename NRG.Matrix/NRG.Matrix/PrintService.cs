using NRG.Matrix.Models;

namespace NRG.Matrix;

public class PrintService
{
	private IEnumerable<DisplayObject> _displayObjects = [];
	private ConsoleSize _windowDimension = new();

	public void PrintObjects(List<DisplayObject> obj)
	{
		_displayObjects = obj;
		try
		{
			HandleWindowSizeChange();
			Console.CursorVisible = false;
			var validColorGroups = _displayObjects
				.Where(e => e.Pos.Y >= 0)
				.GroupBy(e => e.Color);

			var traces = validColorGroups.FirstOrDefault(e => e.Key is ConsoleColor.DarkGreen);
			PrintToConsoleByLine(traces);
			var others = validColorGroups.Where(e => e.Key is not ConsoleColor.DarkGreen);
			PrintToConsoleByChar(others);
		}
		catch (ArgumentOutOfRangeException)
		{
			// Window was resized to a smaller size.
			HandleWindowSizeChange();
		}
	}

	private static void PrintToConsoleByLine(IGrouping<ConsoleColor, DisplayObject>? traces)
	{
		if (traces is null)
		{
			return;
		}

		Console.ForegroundColor = traces.Key;
		var groupedRows = traces.GroupBy(e => e.Pos.Y);
		foreach (var row in groupedRows)
		{
			var first = row.Min(e => e.Pos.X);
			var line = GetLineText(row, first);

			Console.SetCursorPosition(first, row.Key);
			Console.Write(line);
		}
	}

	private static char[] GetLineText(IEnumerable<DisplayObject> row, int first)
	{
		var width = Console.WindowWidth;
		var inRanges = row.Where(e => e.Pos.X < width).ToList();
		var len = inRanges.Max(e => e.Pos.X) - first + 1;

		var line = new char[len];
		Array.Fill(line, ' ');

		foreach (var o in inRanges)
		{
			line[o.Pos.X - first] = o.Symbol;
		}

		return line;
	}

	private static void PrintToConsoleByChar(IEnumerable<IGrouping<ConsoleColor, DisplayObject>> others)
	{
		foreach (var group in others)
		{
			Console.ForegroundColor = group.Key;
			var filteredGroup = group.Where(e => e.Pos.X < Console.WindowWidth);
			foreach (var obj in filteredGroup)
			{
				Console.SetCursorPosition(obj.Pos.X, obj.Pos.Y);
				Console.Write(obj.Symbol);
			}
		}
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
