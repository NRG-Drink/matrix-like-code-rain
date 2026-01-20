namespace NRG.Matrix.Models;

public readonly struct NumberWithRandomness(int number, int spread)
{
    public int Number => number;
    public int Spread => spread;

    public int NumberWithSpread => Random.Shared.Next(number - spread, number + spread);
    public TimeSpan TimeWithSpread => TimeSpan.FromMilliseconds(NumberWithSpread);

    public override string ToString()
    {
        return $"{Number}/{Spread}";
    }
}
