namespace NRG.Matrix.Displays;

public interface IAnsiConsoleChar
{
    public int X { get; }
    public int Y { get; set; }
    public int Z { get; }
    public char Char { get; }
    public string AnsiColor { get; }
}
