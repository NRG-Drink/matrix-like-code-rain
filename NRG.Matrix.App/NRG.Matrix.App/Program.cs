using CommandLine;
using NRG.Matrix.App.Models;

namespace NRG.Matrix.App;

internal class Program
{
	static void Main(string[] args)
	{
		Parser.Default.ParseArguments<Option>(args)
			.WithParsed(arg =>
			{
				var addRate = FunctionParser.ParseFuncOrNull<Func<int, float>>(arg.AddRate);
				var matrix = new Matrix(
					delay: arg.Delay,
					time: arg.Time,
					maxObjects: arg.MaxObjects,
					objectAddRate: addRate
					);
				matrix.Enter();
			})
			.WithNotParsed(arg =>
			{
				Console.WriteLine("Arguments could not be parsed. Start with default values");
				Task.Delay(3000).Wait();
				Console.Clear();
				var matrix = new Matrix();
				matrix.Enter();
			});
	}
}
