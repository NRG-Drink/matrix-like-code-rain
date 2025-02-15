using CommandLine;
using NRG.Matrix.Models;

namespace NRG.Matrix;

internal class Program
{
	static void Main(string[] args)
	{
		Parser.Default.ParseArguments<Option>(args)
			.WithParsed(o =>
			{
				var matrix = new Matrix(o);
				matrix.Enter();
			});
	}
}
