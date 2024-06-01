namespace NRG.Matrix.Models;

public class FactorsProvider
{
	private readonly float[] _factors;
	private int counter = -10;

	public FactorsProvider()
	{
		_factors = InitFactors();
	}

	public int Cadence { get; init; } = 20;
	public int Free { get; init; } = 3;
	public int Ease { get; init; } = 20;
	public float MaxAddRate { get; init; } = 5;

	public float AdjustAddRate(int target, int value, float addRate)
	{
		if (++counter < Cadence)
		{
			return addRate;
		}
		counter = 0;

		var offset = target - value;
		var factor = GetFactor(offset);

		return Math.Clamp(addRate * factor, 0.00001f, MaxAddRate);
	}

	private float GetFactor(int offset)
	{
		var abs = Math.Abs(offset);
		var clamp = Math.Clamp(abs, 0, _factors.Length - 1);

		var factor = _factors[clamp];

		if (offset < 0)
		{
			return 1f + factor;
		}

		return 1f - factor;
	}

	private float[] InitFactors()
	{
		var factors = new float[Free + Ease];
		Array.Fill(factors, 0);

		for (int i = 0; i < Ease; i++)
		{
			factors[Free + i] = (float)i / (float)(Ease * 1.2f);
		}

		return factors;
	}
}
