using NRG.Matrix.App.Models;
using System.Diagnostics;

namespace NRG.Matrix.App;

public class Matrix(Option option)
{
	private readonly int _delay = Math.Clamp(option.Delay, 0, 9999);
	private readonly int _maxObjects = Math.Clamp(option.MaxObjects, 1, int.MaxValue);
	private readonly ConsoleColor _formerColor = Console.ForegroundColor;
	private readonly PrintService _printService = new();
	private readonly FactorsProvider _factorProvider = new()
	{
		Cadence = 20,
		Free = 3,
		Ease = 20,
		MaxAddRate = option.AddRate,
	};

	private List<DisplayObject> _displayObjects = [];
	private float _objectBuildup = 1;
	private float _addRate = 1;

	public void Enter()
	{
		try
		{
			Console.Clear();
			while (true)
			{
				var sw = Stopwatch.StartNew();
				ProcessFrame();
				_printService.PrintObjects(_displayObjects);

				if (option.IsBench == true)
				{
					PrintBenchValues(sw.ElapsedMilliseconds);
				}

				var time = sw.ElapsedMilliseconds;
				var frameTimeOffset = option.MaxFrameTime - (int)time;
				_addRate = _factorProvider.AdjustAddRate(0, frameTimeOffset, _addRate);
				var delay = _delay - (int)time;
				if (delay > 0)
				{
					Task.Delay(delay).Wait();
				}
			}
		}
		finally
		{
			LeaveMatrix();
		}
	}

	private static bool IsPositionValid(DisplayObject e, int width, int height)
		=> e.Pos.Y < height && e.Pos.X < width;

	private void ProcessFrame()
	{
		var width = Console.WindowWidth;
		var height = Console.WindowHeight;

		_displayObjects = _displayObjects
			.Select(e => e.Fall())
			.Concat(ObjectsToAdd(width))
			.Where(e => IsPositionValid(e, width, height))
			.OrderBy(e => e.IsTrace)
			.DistinctBy(e => e.Pos)
			.ToList();
	}

	private IEnumerable<DisplayObject> ObjectsToAdd(int width)
	{
		if (_maxObjects < _displayObjects.Count)
		{
			yield break;
		}

		_objectBuildup += width * _addRate / 200;
		var objectsToAdd = (int)_objectBuildup;
		for (int i = 0; i < objectsToAdd; i++)
		{
			var obj = new DisplayObject(width);
			yield return obj;

			foreach (var e in obj.Trace())
			{
				yield return e;
			}
		}
		_objectBuildup -= objectsToAdd;
	}

	private void PrintBenchValues(long time)
	{
		if (option.IsBench == true)
		{
			Console.Title = $"" +
				$"  -o {_displayObjects.Count,6} " +
				$"| -a {_addRate:00.00} " +
				$"| -m {time:00}";
		}
	}

	private void LeaveMatrix()
	{
		Console.CursorVisible = true;
		Console.ForegroundColor = _formerColor;
		Console.SetCursorPosition(0, Console.WindowHeight + 1);
	}
}
