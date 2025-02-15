using Microsoft.VisualBasic;
using NRG.Matrix.Models;
using System.Diagnostics;

namespace NRG.Matrix;

public class Matrix(Option option)
{
	private readonly int _delay = Math.Clamp(option.Delay, 0, 9999);
	private readonly int _maxObjects = Math.Clamp(option.MaxObjects, 1, int.MaxValue);
	private readonly ConsoleColor _formerColor = Console.ForegroundColor;
	private readonly MatrixObjectHandler _objectHandler = new();
	private readonly PrintService _printService = new();
	private readonly FactorsProvider _factorProvider = new()
	{
		Cadence = 20,
		Free = 3,
		Ease = 20,
		MaxAddRate = option.AddRate,
	};

	private MatrixObject[] _displayObjects = [];
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

				var time = (int)sw.ElapsedMilliseconds;
				var frameTimeOffset = option.MaxFrameTime - time;
				_addRate = _factorProvider.AdjustAddRate(0, frameTimeOffset, _addRate);
				var delay = _delay - time;
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

	private void ProcessFrame()
	{
		var width = Console.WindowWidth;
		var height = Console.WindowHeight;

		_displayObjects = _displayObjects
			.Concat(ObjectsToAdd(width))
			.Select(_objectHandler.Fall)
			.Select(_objectHandler.Symbol)
            .Where(e => IsPositionValid(e, width, height))
			.OrderBy(e => e.Color is ConsoleColor.Gray)
			.DistinctBy(e => e.Pos)
            .ToArray();
	}

    private static bool IsPositionValid(MatrixObject e, int width, int height)
        => e.Pos.Y < height && e.Pos.X < width;

    private IEnumerable<MatrixObject> ObjectsToAdd(int width)
	{
		if (_maxObjects < _displayObjects.Length)
		{
			//yield break;
			return [];
		}

		_objectBuildup += width * _addRate / 200;
		var objectsToAdd = (int)_objectBuildup;
		_objectBuildup -= objectsToAdd;

		return Enumerable
			.Range(0, objectsToAdd)
			.SelectMany(e => _objectHandler.CreateNewMatrixLine(width));
	}

	private void PrintBenchValues(long time)
	{
		if (option.IsBench == true)
		{
			Console.Title = $"" +
				$"| -d {_delay:00} " +
				$"| -a {_addRate:00.00}/{option.AddRate:00.00} " +
				$"| -m {time:00}/{option.MaxFrameTime:00} " +
				$"| -o {_displayObjects.Length,6}/{option.MaxObjects} " +
				"";
		}
	}

	private void LeaveMatrix()
	{
		Console.CursorVisible = true;
		Console.ForegroundColor = _formerColor;
		Console.SetCursorPosition(0, Console.WindowHeight + 1);
	}
}
