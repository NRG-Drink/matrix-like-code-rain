using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace NRG.Matrix.App;

internal class Program
{
	static void Main(string[] args)
	{
		var exString = args[1];
		var expression = CSharpScript.EvaluateAsync<Func<int, float>>(exString).Result;

		var one = expression(200);
		var two = expression(400);
		var twoHalf = expression(500);

		// Benchmark Mode.
		//var matrix = new Matrix(delay: 80, time: new TimeSpan(0, 0, 20));
		var matrix = new Matrix(objectAddRate: e => 5);
		matrix.Enter();
	}
}
