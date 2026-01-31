using NRG.Matrix.Styles;
using System.Diagnostics;

namespace NRG.Matrix;

public class Matrix
{
    public async Task Enter(CancellationToken token)
    {
        Console.Title = $"Matrix";
        var style = new StyleGreenWhite();
        var frameTime = Stopwatch.StartNew();
        var frameTimeTarget = 1000 / 60;
        while (!token.IsCancellationRequested)
        {
            var hasChanged = style.UpdateInternalObjects();
            if (!hasChanged)
            {
                await WaitFrameTime(frameTimeTarget, frameTime.ElapsedMilliseconds, token);
                frameTime.Restart();
                continue;
            }

            style.DisplayFrame();
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                style.HandleKeyInput(key);
            }

            style.SetFrametime(frameTime.ElapsedMilliseconds);
            await WaitFrameTime(frameTimeTarget, frameTime.ElapsedMilliseconds, token);

            Console.Title = $"Matrix";
            frameTime.Restart();
        }
    }

    private async Task<int> WaitFrameTime(
        int frameTimeTarget,
        long frameTime,
        CancellationToken token)
    {
        var remainingFrameTime = (int)(frameTimeTarget - frameTime);
        if (remainingFrameTime > 5)
        {
            await Task.Delay(remainingFrameTime);
        }

        return remainingFrameTime;
    }
}
