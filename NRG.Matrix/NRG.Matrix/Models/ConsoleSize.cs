namespace NRG.Matrix.Models;

public struct ConsoleSize()
{
	public int Width { get; set; } = Console.WindowWidth;
	public int Height { get; set; } = Console.WindowHeight;

	public static bool operator ==(ConsoleSize e1, ConsoleSize e2)
		=> e1.Width == e2.Width && e1.Height == e2.Height;

	public static bool operator !=(ConsoleSize e1, ConsoleSize e2)
		=> !(e1 == e2);

	public static bool operator >(ConsoleSize e1, ConsoleSize e2)
		=> e1.Width > e2.Width || e1.Height > e2.Height;

	public static bool operator <(ConsoleSize e1, ConsoleSize e2)
		=> e1.Width < e2.Width || e1.Height < e2.Height;

	public override readonly bool Equals(object? obj)
		=> obj is ConsoleSize cs && this == cs;

	public override readonly int GetHashCode()
		=> HashCode.Combine(Width, Height);
}