using System.Diagnostics.CodeAnalysis;

namespace NRG.Matrix.Displays;

public interface IAnsiConsole
{
    public bool HasResolutionChanged(out int width, out int  height);
    public Task Display(IEnumerable<IAnsiConsoleChar> chars);
}
