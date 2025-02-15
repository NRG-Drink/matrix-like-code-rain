using System.Drawing;

namespace NRG.Matrix.Models;

public abstract record MatrixObject
{
    private readonly char[] _charset;
    private readonly int _change;

    protected MatrixObject(Point pos, ConsoleColor color, char[] charset, int change)
    {
        _change = change;
        _charset = charset;
        Pos = pos;
        Color = color;
        Symbol = charset.Length is 0 ? ' ' : GetRandomChar(charset);
    }

    public Point Pos { get; private set; }
    public char Symbol { get; protected set; }
    public ConsoleColor Color { get; private set; }

    public void Fall(int x, int y)
        => Pos = Pos with { X = Pos.X + x, Y = Pos.Y + y };

    public virtual void ChangeSymbol()
    {
        if (ShouldGetNewSymbol(_change))
        {
            Symbol = GetRandomChar(_charset);
        }
    }

    protected static bool ShouldGetNewSymbol(int change)
        => change < 0
            ? false
            : Random.Shared.Next(0, change) is 0;

    protected static char GetRandomChar(char[] charset)
        => charset[Random.Shared.Next(0, charset.Length)];
};

public record MatrixLead(Point Pos, char[] Charset) : MatrixObject(Pos, ConsoleColor.Gray, Charset, 3);

public record MatrixTrace(Point Pos, char[] Charset) : MatrixObject(Pos, ConsoleColor.DarkGreen, Charset, 6);

public record MatrixClean(Point Pos) : MatrixObject(Pos, 0, [' '], -1)
{
    public override void ChangeSymbol() { }
}
