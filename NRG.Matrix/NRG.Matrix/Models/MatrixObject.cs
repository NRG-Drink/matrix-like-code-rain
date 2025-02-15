using System.Drawing;

namespace NRG.Matrix.Models;

public record MatrixObject
{
    public Point Pos { get; set; }
    public char Symbol { get; set; }
    public ConsoleColor Color { get; init; }
    public byte SymbolChangeChance { get; init; }

    public static MatrixObject CreateLead(Point p, char symbol)
        => new()
        {
            Pos = p,
            Symbol = symbol,
            Color = ConsoleColor.Gray,
            SymbolChangeChance = 3
        };

    public static MatrixObject CreateTrace(Point p, char symbol)
        => new()
        {
            Pos = p,
            Symbol = symbol,
            Color = ConsoleColor.DarkGreen,
            SymbolChangeChance = 6
        };

    public static MatrixObject CreateClean(Point p)
        => new()
        {
            Pos = p,
            Symbol = ' ',
            Color = 0,
            SymbolChangeChance = byte.MaxValue
        };
};
