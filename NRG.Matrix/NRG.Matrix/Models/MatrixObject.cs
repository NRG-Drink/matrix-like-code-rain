using System.Drawing;

namespace NRG.Matrix.Models;

public readonly record struct MatrixObject(
    Point Pos,
    char Symbol,
    ConsoleColor Color,
    byte SymbolChangeChance
    )
{
    public static MatrixObject CreateLead(Point p, char symbol)
        => new(p, symbol, ConsoleColor.Gray, 3);

    public static MatrixObject CreateTrace(Point p, char symbol)
        => new(p, symbol, ConsoleColor.DarkGreen, 6);

    public static MatrixObject CreateClean(Point p)
        => new(p, ' ', 0, byte.MaxValue);
};
