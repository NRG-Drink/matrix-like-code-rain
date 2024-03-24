namespace NRG.Matrix.App;

internal class Program
{
	static void Main(string[] args)
	{
		// Benchmark Mode.
		//var matrix = new Matrix(delay: 80, time: new TimeSpan(0, 0, 20));
		var matrix = new Matrix();
		matrix.Enter();
	}
}
