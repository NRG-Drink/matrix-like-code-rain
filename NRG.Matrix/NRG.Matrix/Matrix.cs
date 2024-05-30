using NRG.Matrix.App.Models;
using System.Text;

namespace NRG.Matrix.App;

public class Matrix(Option option)
{
	private List<DisplayObject> _displayObjects = [];
	private (int Width, int Height) _windowDimension = (Console.WindowWidth, Console.WindowHeight);

	private readonly float _addRate = option.AddRate;
	private readonly int _delay = option.Delay < 0 ? 0 : option.Delay;
	private readonly int _maxObjects = option.MaxObjects;
	private float _objectBuildup = 1;

	public void Enter()
	{
		try
		{
			Console.Clear();
			while (true)
			{
				AddObjects(Console.WindowWidth);
				HandleObjects(Console.WindowHeight);
				PrintObjects();

				Task.Delay(_delay).Wait();
			}
		}
		finally
		{
			LeaveMatrix(_delay);
		}
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
			Console.CursorVisible = false;
			var validColors = _displayObjects.Where(e => e.Pos.Y >= 0).GroupBy(e => e.Color);

			var traces = validColors.FirstOrDefault(e => e.Key is ConsoleColor.DarkGreen);
			PrintTrace(traces);

			var others = validColors.Where(e => e.Key is not ConsoleColor.DarkGreen);
			PrintOthers(others);
		}
		catch (ArgumentOutOfRangeException)
		{
			// Window was resized to a smaller size.
			HandleWindowSizeChange();
		}
	}

	private void PrintTrace(IGrouping<ConsoleColor, DisplayObject>? traces)
	{
		if (traces is null)
		{
			return;
		}

		var orderedRows = traces.OrderBy(e => e.Pos.X).GroupBy(e => e.Pos.Y);
		Console.ForegroundColor = ConsoleColor.DarkGreen;
		foreach (var row in orderedRows)
		{
			var obj = row.ToList();
			var first = obj.First().Pos.X;
			var last = obj.Last().Pos.X;

			Console.SetCursorPosition(first, row.Key);
			var line = GetLineText(obj, first, last);
			Console.Write(line);
		}
	}

	private static string GetLineText(List<DisplayObject> obj, int first, int last)
	{
		var len = last - first - obj.Count + 1;
		var line = string.Join("", Enumerable.Repeat(" ", len));
		var sb = new StringBuilder(line);
		foreach (var o in obj)
		{
			sb.Insert(o.Pos.X - first, o.Symbol);
		}

		return sb.ToString();
	}

	private void PrintOthers(IEnumerable<IGrouping<ConsoleColor, DisplayObject>> others)
	{
		foreach (var group in others)
		{
			Console.ForegroundColor = group.Key;
			foreach (var obj in group)
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

	//private void HandleBenchmarkMode()
	//{
	//	Console.SetCursorPosition(5, 1);
	//	Console.Write($"Number of objects in RAM: {_displayObjects.Count} ");
	//	_frames++;
	//}

	private void LeaveMatrix(int delay, TimeSpan? time = null)
	{
		Console.CursorVisible = true;
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.SetCursorPosition(0, Console.WindowHeight + 1);
		//if (!_isEndlessMode)
		//{
		//	Console.WriteLine(
		//		$"Rendered Frames: {_frames} " +
		//		$"of possible {time!.Value.TotalMilliseconds / delay} " +
		//		$"in time {time} (hh:MM:ss)\n" +
		//		$"With an average calculation time of " +
		//		$"{((time!.Value.TotalMilliseconds - delay * _frames) / _frames):0.0} ms per frame"
		//	);
		//}
	}
}
