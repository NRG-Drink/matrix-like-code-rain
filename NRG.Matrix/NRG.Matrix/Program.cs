using CommandLine;
using NRG.Matrix.App.Models;

namespace NRG.Matrix.App;

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
