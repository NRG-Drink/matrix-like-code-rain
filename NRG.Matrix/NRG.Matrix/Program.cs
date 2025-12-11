using CommandLine;
using NRG.Matrix.Models;

namespace NRG.Matrix;

internal class Program
{
    static async Task Main(string[] args)
    {
        using var tokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };

        var matrix = new Matrix();
        await matrix.Enter(tokenSource.Token);
        //Parser.Default.ParseArguments<Option>(args)
        //    .WithParsed(o =>
        //    {
        //        var matrix = new Matrix(o);
        //        matrix.Enter();
        //    });
    }
}
