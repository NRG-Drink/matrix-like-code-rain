namespace NRG.Matrix.Models;

public readonly struct NumberWithRandomness(int number, int spread)
{
    public int Number => Math.Max(number, 0);
    public int Spread => Math.Max(spread, 0);

    public int NumberWithSpread => Random.Shared.Next(Number - Spread, Number + Spread + 1);
    public TimeSpan TimeWithSpread => TimeSpan.FromMilliseconds(NumberWithSpread);

    public override string ToString() => $"{Number}/{Spread}";
}
