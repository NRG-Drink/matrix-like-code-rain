namespace NRG.Matrix;

public record Alphabets
{
    public static char[] OneZero => ['0', '1'];
    public static char[] Default => [.. Enumerable.Range(33, 90).Select(e => (char)e)];
}
