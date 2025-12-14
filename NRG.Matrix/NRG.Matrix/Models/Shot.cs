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
        ChangeTimeMilliseconds= time;
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

    public void Fall(int yFall = 1)
    {
        YTop += yFall;
        YBottom += yFall;
        foreach (var c in Chars)
        {
            c.Y += yFall;
        }
    }
}
