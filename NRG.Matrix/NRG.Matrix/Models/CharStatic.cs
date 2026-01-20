using NRG.Matrix.Displays;

namespace NRG.Matrix.Models;

public class CharStatic : IAnsiConsoleChar, ICanFall
{
    public int Y { get; set; }
    public int X { get; set; }
    public int Z { get; set; }
    public char Char { get; set; }
    public string AnsiColor { get; set; } = "38;2;255;255;255m";
}
