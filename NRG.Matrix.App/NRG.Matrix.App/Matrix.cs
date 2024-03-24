using System;
using System.Diagnostics;
using System.Text;

namespace NRG.Matrix.App;

public class Matrix(int delay = 80, TimeSpan? time = null, int maxObjects = 9999, Func<int, float>? objectAddRate = null)
{
	private readonly bool _isEndlessMode = time is null;

	private List<DisplayObject> list = [];
	private (int Width, int Height) _windowDimension = (Console.WindowWidth, Console.WindowHeight);
	private int _frames = 0;
	private float _objectBuildup = 1;


	public void Enter()
	{
		delay = delay < 0 ? 0 : delay;
		objectAddRate ??= e => e / 200.0f;
		try
		{
			Console.Clear();
			var sw = Stopwatch.StartNew();
			while (_isEndlessMode || sw.Elapsed < time)
			{
				AddObjects(Console.WindowWidth);
				HandleObjects(Console.WindowHeight);
				PrintObjects();

				if (!_isEndlessMode)
				{
					HandleBenchmarkMode();
				}

				Task.Delay(delay).Wait();
			}
		}
		finally
		{
			LeaveMatrix(delay, time);
		}
	}

	private void AddObjects(int width)
	{
		if (maxObjects < list.Count)
		{
			return;
		}

		_objectBuildup += objectAddRate!(width);
		var objectsToAdd = (int)_objectBuildup;
        for (int i = 0; i < objectsToAdd; i++)
        {
			var obj = new DisplayObject(width);
			list.Add(obj);
			list.AddRange(obj.Trace());
        }
		_objectBuildup -= objectsToAdd;
	}

	private void HandleObjects(int height)
	{
		list = list
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
			var validColors = list.Where(e => e.Pos.Y >= 0).GroupBy(e => e.Color);

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
		if (hasWindowSizeChanges)
		{
			Console.Clear();
			list = list
				.Where(e => e.Pos.X < Console.WindowWidth)
				.Where(e => e.Pos.Y < Console.WindowHeight)
				.ToList();
			_windowDimension = (Console.WindowWidth, Console.WindowHeight);
		}
	}

	private void HandleBenchmarkMode()
	{
		Console.SetCursorPosition(5, 1);
		Console.Write($"Number of objects in RAM: {list.Count} ");
		_frames++;
	}

	private void LeaveMatrix(int delay, TimeSpan? time)
	{
		Console.CursorVisible = true;
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.SetCursorPosition(0, Console.WindowHeight + 1);
		if (!_isEndlessMode)
		{
			Console.WriteLine(
				$"Rendered Frames: {_frames} " +
				$"of possible {time!.Value.TotalMilliseconds / delay} " +
				$"in time {time} (hh:MM:ss)\n" +
				$"With an average calculation time of " +
				$"{((time!.Value.TotalMilliseconds - delay * _frames) / _frames):0.0} ms per frame"
				);
		}
	}
}
