using System.Diagnostics;

namespace NRG.Matrix.Models;

public class Shot
{
    // Needed for ObjectPool
    public Shot() { }

    public void Init(int x, byte z, int y, int len, int time)
    {
        X = x;
        YBottom = y;
        YTop = y - len;
        Z = z;
        ChangeTimeMilliseconds = time;
        CharsLength = len;
        SW.Restart();
    }

    public int X { get; set; }
    public int Z { get; set; }
    public int ChangeTimeMilliseconds { get; set; }

    public int YTop { get; set; }
    public int YBottom { get; set; }
    public bool IsExpired => SW.ElapsedMilliseconds >= ChangeTimeMilliseconds;
    public Stopwatch SW { get; } = Stopwatch.StartNew();
    public ICanFall[] Chars { get; set; } = [];
    /// <summary>
    /// The actual number of characters used in the Chars array (may be less than Chars.Length due to ArrayPool).
    /// </summary>
    public int CharsLength { get; set; }

    public void Fall(int yFall = 1)
    {
        YTop += yFall;
        YBottom += yFall;
        for (var i = 0; i < CharsLength; i++)
        {
            Chars[i].Y += yFall;
        }
    }
}
