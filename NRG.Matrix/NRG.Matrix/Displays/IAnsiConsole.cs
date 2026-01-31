namespace NRG.Matrix.Displays;

public interface IAnsiConsole
{
    public bool HasResolutionChanged(out int width, out int height);
    public void Display(IEnumerable<IAnsiConsoleChar> chars);
}
