using NRG.Matrix.App.Models;
using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix.App;

public class Matrix(Option option)
{
	private readonly int _delay = Math.Clamp(option.Delay, 0, 9999);
	private readonly int _maxObjects = Math.Clamp(option.MaxObjects, 1, int.MaxValue);
	private readonly FactorsProvider _factorProvider = new()
	{
		Cadence = 20,
		Free = 3,
		Ease = 20,
		MaxAddRate = option.AddRate,
	};

	private List<DisplayObject> _displayObjects = [];
	private (int Width, int Height) _windowDimension = (Console.WindowWidth, Console.WindowHeight);
	private float _objectBuildup = 1;
	private float _addRate = 1;
	public float AddRate
	{
		get => _addRate;
		set => _addRate = Math.Clamp(value, 0.0001f, 999);
	}

	public void Enter()
	{
		try
		{
			Console.Clear();
			while (true)
			{
				var sw = Stopwatch.StartNew();
				AddObjects(Console.WindowWidth);
				HandleObjects(Console.WindowHeight);
				PrintObjects();

				if (option.IsBench == true)
				{
					PrintBenchValues(sw.ElapsedMilliseconds);
				}

				var time = sw.ElapsedMilliseconds;
				var frameTimeOffset = option.MaxFrameTime - (int)time;
				AddRate = _factorProvider.AdjustAddRate(0, frameTimeOffset, _addRate);
				Task.Delay(Math.Max(0, _delay - (int)time)).Wait();
			}
		}
		finally
		{
			LeaveMatrix(_delay);
		}
	}

	private void PrintBenchValues(long time)
	{
		Console.SetCursorPosition(05, 01);
		Console.Write("╔═════════════════════════╗");
		Console.SetCursorPosition(05, 02);
		Console.Write($"║ Object add rate:  {_addRate:00.00} ║");
		Console.SetCursorPosition(05, 03);
		Console.Write($"║ Frame calc. time: {time:00} ms ║");
		Console.SetCursorPosition(05, 04);
		Console.Write($"║ Max objects: {_displayObjects.Count,10} ║");
		Console.SetCursorPosition(05, 05);
		Console.Write("╚═════════════════════════╝");
	}

	private void AddObjects(int width)
	{
		if (_maxObjects < _displayObjects.Count)
		{
			return;
		}

		_objectBuildup += width * _addRate / 200;
		var objectsToAdd = (int)_objectBuildup;
		for (int i = 0; i < objectsToAdd; i++)
		{
			var obj = new DisplayObject(width);
			_displayObjects.Add(obj);
			_displayObjects.AddRange(obj.Trace());
		}
		_objectBuildup -= objectsToAdd;
	}

	private void HandleObjects(int height)
	{
		_displayObjects = _displayObjects
			.Select(e => e.Fall())
			.Where(e => e.Pos.Y < height)
			.OrderBy(e => e.IsTrace)
			.DistinctBy(e => e.Pos)
			.ToList();
	}

	private void PrintObjects()
	{
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
		catch (ArgumentOutOfRangeException ex)
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

		var orderedRows = traces.OrderBy(e => e.Pos.X).GroupBy(e => e.Pos.Y);
		Console.ForegroundColor = traces.Key;
		foreach (var row in orderedRows)
		{
			var first = row.First().Pos.X;
			var last = row.Last().Pos.X;

			var line = GetLineText([.. row], first, last);
			var width = Console.WindowWidth;
			while (last > width && first > width)
			{
				line = line[..(last - width)];
				width = Console.WindowWidth;
			}
			if (first > Console.WindowWidth || last > Console.WindowWidth)
			{
				continue;
			}

			Console.SetCursorPosition(first, row.Key);
			Console.Write(line);
		}
	}

	private static string GetLineText(List<DisplayObject> obj, int first, int last)
	{
		var line = new char[last - first + 1];
		Array.Fill(line, ' ');

		foreach (var o in obj)
		{
			line[o.Pos.X - first] = o.Symbol;
		}

		return new string(line);
	}

	private static void PrintToConsoleByChar(IEnumerable<IGrouping<ConsoleColor, DisplayObject>> others)
	{
		foreach (var group in others)
		{
			Console.ForegroundColor = group.Key;
			foreach (var obj in group.Where(e => e.Pos.X < Console.WindowWidth))
			{
				Console.SetCursorPosition(obj.Pos.X, obj.Pos.Y);
				Console.Write(obj.Symbol);
			}
		}
	}

	private void HandleWindowSizeChange()
	{
		var hasWindowSizeChanges = _windowDimension.Width != Console.WindowWidth || _windowDimension.Height != Console.WindowHeight;
		if (!hasWindowSizeChanges)
		{
			return;
		}

		Console.Clear();
		_displayObjects = _displayObjects
			.Where(e => e.Pos.X < Console.WindowWidth)
			.Where(e => e.Pos.Y < Console.WindowHeight)
			.ToList();
		_windowDimension = (Console.WindowWidth, Console.WindowHeight);
	}

	private static void LeaveMatrix(int delay, TimeSpan? time = null)
	{
		Console.CursorVisible = true;
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.SetCursorPosition(0, Console.WindowHeight + 1);
	}
}
