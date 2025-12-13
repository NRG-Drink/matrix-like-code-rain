namespace NRG.Matrix;

public class ObjectPool<T> where T : new()
{
    private readonly Stack<T> _pool = new();

    public T Rent()
    {
        lock (_pool)
        {
            return _pool.Count > 0 ? _pool.Pop() : new T();
        }
    }

    public T[] Rent(int n)
        => Enumerable.Range(0, n).Select(e => Rent()).ToArray();

    public void Return(T obj)
    {
        lock (_pool)
        {
            _pool.Push(obj);
        }
    }
}

// TODO: Play around with bold characters
// TODO: Rethink styles: what should a style control based on which parameters?
public class MatrixShotFactory
{
    private int _shotCount = 0;
    private char[] _greenWhiteChars = [
        .. Alphabeth.LatinUpper,
        .. Alphabeth.Numbers,
        .. Alphabeth.Katakana,
        .. Alphabeth.Symbols,
    ];

    public Shot GenerateShot(MatrixStyles style, int length)
        => style switch
        {
            MatrixStyles.GreenWhite => GreenWhite(length),
            //MatrixStyles.ShotCount => ShotCount(length),
            _ => GreenWhite(length)
        };

    public ObjectPool<MatrixCharDynamic> CharPool = new();
    public ObjectPool<Shot> ShotPool = new();

    public Shot GreenWhite(int length)
    {
        length = 20;
        var x = Random.Shared.Next(0, Console.BufferWidth);
        var y = 0;
        var z = (byte)Random.Shared.Next(0, 3);
        var fallDelay = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 400));
        //var shot = new Shot(x, z, fallDelay) { YBottom = y, YTop = y - length };
        var shot = ShotPool.Rent();
        shot.Init(x, z, y, length, fallDelay);

        var chars = CharPool.Rent(length);

        var numberColor = new RGB(220, 220, 250);
        var zeroColor = new RGB(30, 200, 60);

        chars[0].Init(shot, _greenWhiteChars, numberColor, 1000, y--);

        //var head = new MatrixCharDynamic(shot, _greenWhiteChars, numberColor, 1000) { Y = y-- };

        //var tail = new IMatrixChar[length];
        for (var i = 1; i < length; i++)
        {
            var color = zeroColor.Luminos(i * 3);
            chars[i].Init(shot, _greenWhiteChars, color, 500, y--);
            //var c = new MatrixCharDynamic(shot, _greenWhiteChars, color, 500) { Y = y - i };
            //tail[i] = c;
        }

        //shot.Chars = [head, .. tail];
        shot.Chars = chars;

        return shot;
    }

    private char GetRandomChar(char[] charset) => charset[Random.Shared.Next(0, charset.Length)];

    private char GetRandomChar(int start = 33, int len = 90) => (char)Random.Shared.Next(start, len);

    //public Shot ShotCount(int length)
    //{
    //    var x = Random.Shared.Next(0, Console.BufferWidth + 1);
    //    var fallDelay = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 800));
    //    var z = (byte)Random.Shared.Next(0, 6);
    //    var shot = new Shot(x, z, fallDelay);
    //    _shotCount++;
    //    var numberStr = $"{_shotCount.ToString($"D{length}")}";
    //    var y = 0;
    //    var numberColor = new RGB(220, 220, 250);
    //    var zeroColor = new RGB(40, 40, 80);
    //    var zColor = new RGB(150, 30, 200);
    //    var lFactor = -10;
    //    var numbers = numberStr.SkipWhile(e => e == '0')
    //        .Reverse()
    //        .Select(e => new MatrixChar(shot, e, numberColor.Luminos(z * lFactor)) { Y = y-- });
    //    var zeros = numberStr.TakeWhile(e => e == '0')
    //        .Select(e => new MatrixChar(shot, e, zeroColor.Luminos(z * lFactor)) { Y = y-- });
    //    var zChar = new MatrixChar(shot, z.ToString().ToCharArray().First(), zColor.Luminos(z * lFactor)) { Y = y-- };

    //    shot.Chars = [..numbers, ..zeros, zChar];

    //    return shot;
    //}

}
