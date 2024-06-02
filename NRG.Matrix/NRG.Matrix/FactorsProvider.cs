namespace NRG.Matrix;

public class FactorsProvider
{
	private readonly float[] _factors;
	private int _counter = -10;

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
		if (++_counter < Cadence)
		{
			return addRate;
		}
		_counter = 0;

		var offset = target - value;

		if (offset > -Free && offset < Free)
		{
			return addRate;
		}

		var factor = GetFactor(offset);
		var res = offset < 0
			? addRate * factor
			: addRate / factor;

		return Math.Clamp(res, 0.00001f, MaxAddRate);
	}

	private float GetFactor(int offset)
	{
		var abs = Math.Abs(offset) - Free;

		if (abs >= _factors.Length)
		{
			return _factors[^1];
		}

		return _factors[abs];
	}

	private float[] InitFactors()
		=> Enumerable.Range(1, Ease)
			.Select(e => 1f + e / (float)(Ease * 1.2f))
			.ToArray();
}
