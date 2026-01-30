namespace NRG.Matrix;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Handle cancellation with CTRL+C
        using var tokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            tokenSource.Cancel();
            eventArgs.Cancel = true;
            Console.CursorVisible = true;
        };

        var matrix = new Matrix();
        await matrix.Enter(tokenSource.Token);
    }
}
