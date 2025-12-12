namespace NRG.Matrix;

// TODO: Play around with bold characters
// TODO: Rethink styles: what should a style control based on which parameters?
public class MatrixShotFactory
{
    private int _shotCount = 0;

    public Shot GenerateShot(MatrixStyles style, int length)
        => style switch
        {
            MatrixStyles.GreenWhite => GreenWhite(length),
            MatrixStyles.ShotCount => ShotCount(length),
            _ => GreenWhite(length)
        };

    public Shot GreenWhite(int length)
    {
        length = length * 2;
        var x = Random.Shared.Next(0, Console.BufferWidth + 1);
        var z = (byte)Random.Shared.Next(0, 3);
        var fallDelay = TimeSpan.FromMilliseconds(Random.Shared.Next(50, 500));
        var shot = new Shot(x, z, fallDelay);

        var y = 0;

        var numberColor = new RGB(220, 220, 250);
        var zeroColor = new RGB(30, 200, 60);
        var lFactor = -50;

        var head = new MatrixChar(shot, GetRandomChar(), numberColor.Luminos(z * lFactor)) { Y = y-- };
        var tail = Enumerable.Range(0, length - 1)
            .Select(e => new MatrixChar(shot, GetRandomChar(), zeroColor.Luminos(z * lFactor)) { Y = y-- });

        shot.Chars = [head, .. tail];

        return shot;
    }

    private char GetRandomChar() => (char)Random.Shared.Next(33, 90);

    public Shot ShotCount(int length)
    {
        var x = Random.Shared.Next(0, Console.BufferWidth + 1);
        var fallDelay = TimeSpan.FromMilliseconds(Random.Shared.Next(100, 800));
        var z = (byte)Random.Shared.Next(0, 6);
        var shot = new Shot(x, z, fallDelay);
        _shotCount++;
        var numberStr = $"{_shotCount.ToString($"D{length}")}";
        var y = 0;
        var numberColor = new RGB(220, 220, 250);
        var zeroColor = new RGB(40, 40, 80);
        var zColor = new RGB(150, 30, 200);
        var lFactor = -10;
        var numbers = numberStr.SkipWhile(e => e == '0')
            .Reverse()
            .Select(e => new MatrixChar(shot, e, numberColor.Luminos(z * lFactor)) { Y = y-- });
        var zeros = numberStr.TakeWhile(e => e == '0')
            .Select(e => new MatrixChar(shot, e, zeroColor.Luminos(z * lFactor)) { Y = y-- });
        var zChar = new MatrixChar(shot, z.ToString().ToCharArray().First(), zColor.Luminos(z * lFactor)) { Y = y-- };

        shot.Chars = [..numbers, ..zeros, zChar];

        return shot;
    }

}
