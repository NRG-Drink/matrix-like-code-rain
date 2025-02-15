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
            var colorGroups = obj
                .Where(e => e.Pos.Y >= 0)
                .Where(e => e.Pos.Y < _windowDimension.Height)
                .Where(e => e.Pos.X >= 0)
                .Where(e => e.Pos.X <= _windowDimension.Width)
                .OrderBy(e => e switch
                {
                    MatrixLead => 1,
                    MatrixTrace => 2,
                    MatrixClean => 3,
                    _ => 4
                })
                .DistinctBy(e => e.Pos)
                .GroupBy(e => e.Color);

            var darkGreens = colorGroups
                .FirstOrDefault(e => e.Key is ConsoleColor.DarkGreen)?
                .DistinctBy(e => e.Pos);
            PrintToConsoleByLine(darkGreens, ConsoleColor.DarkGreen);

            var others = colorGroups.Where(e => e.Key is not ConsoleColor.DarkGreen);
            foreach (var o in others)
            {
                Console.ForegroundColor = o.Key;
                foreach (var c in o.DistinctBy(e => e.Pos))
                {
                    Console.SetCursorPosition(c.Pos.X, c.Pos.Y);
                    Console.Write(c.Symbol);
                }
            }
        }
        catch (ArgumentOutOfRangeException)
        {
            // Window was resized to a smaller size.
            HandleWindowSizeChange();
        }
    }

    private void PrintToConsoleByLine(IEnumerable<MatrixObject>? traces, ConsoleColor color)
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

    private char[] GetLineText(IEnumerable<MatrixObject> row, int first)
    {
        var inRanges = row.Where(e => e.Pos.X < _windowDimension.Width).ToArray();
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
