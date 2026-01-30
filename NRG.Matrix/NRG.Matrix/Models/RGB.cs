namespace NRG.Matrix.Models;

public record RGB(byte R, byte G, byte B)
{
    public string AnsiConsoleColor { get; } = $"38;2;{R};{G};{B}";

    public RGB Luminos(int percentage)
    {
        var factor = Math.Max(1, (100 - percentage));
        return new((byte)(R * factor / 100), (byte)(G * factor / 100), (byte)(B * factor / 100));
    }
}
