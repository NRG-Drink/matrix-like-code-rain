namespace NRG.Matrix;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        using var tokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            tokenSource.Cancel();
            eventArgs.Cancel = true;
        };

        var options = new MatrixOptions()
        {
            Style = MatrixStyles.GreenWhite
        };

        var matrix = new Matrix(options);
        await matrix.Enter(tokenSource.Token);
        //Parser.Default.ParseArguments<Option>(args)
        //    .WithParsed(o =>
        //    {
        //        var matrix = new Matrix(o);
        //        matrix.Enter();
        //    });
    }
}
