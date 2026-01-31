using System.Text;

namespace NRG.Matrix.Tests;

[Category("#Unit")]
[Category("Misc")]
public class StringBuilderExtensionsTests
{
    public static IEnumerable<Func<(Action<StringBuilder> Setup, string Expected)>> GetAppendPaddedTestCases()
    {
        // Basic zero cases
        yield return () => (sb => sb.AppendPadded(0, 0), "0");
        yield return () => (sb => sb.AppendPadded(0, 5), "00000");

        // Positive numbers
        yield return () => (sb => sb.AppendPadded(123, 3), "123");
        yield return () => (sb => sb.AppendPadded(42, 5), "00042");
        yield return () => (sb => sb.AppendPadded(12345, 3), "12345");
        yield return () => (sb => sb.AppendPadded(7, 4), "0007");
        yield return () => (sb => sb.AppendPadded(9876543210, 5), "9876543210");

        // Negative numbers
        yield return () => (sb => sb.AppendPadded(-42, 5), "-0042");
        yield return () => (sb => sb.AppendPadded(-0, 3), "000");
        yield return () => (sb => sb.AppendPadded(-123, 4), "-123");
        yield return () => (sb => sb.AppendPadded(-5, 3), "-05");
        yield return () => (sb => sb.AppendPadded(-99, 3), "-99");
        yield return () => (sb => sb.AppendPadded(-999, 2), "-999");
        yield return () => (sb => sb.AppendPadded(-1, 2), "-1");
        yield return () => (sb => sb.AppendPadded(-1, 5), "-0001");
        yield return () => (sb => sb.AppendPadded(-999, 5), "-0999");

        // Extreme values
        yield return () => (sb => sb.AppendPadded(long.MaxValue, 20), "09223372036854775807");
        yield return () => (sb => sb.AppendPadded(long.MinValue + 1, 20), "-9223372036854775807");

        // Width of one
        yield return () => (sb => sb.AppendPadded(5, 1), "5");
        yield return () => (sb => sb.AppendPadded(99, 1), "99");
        yield return () => (sb => sb.AppendPadded(-5, 1), "-5");

        // Zero width
        yield return () => (sb => sb.AppendPadded(42, 0), "42");
        yield return () => (sb => sb.AppendPadded(-42, 0), "-42");

        // With existing content
        yield return () => (sb => { sb.Append("prefix:"); sb.AppendPadded(42, 5); }, "prefix:00042");

        // Multiple calls chained
        yield return () => (sb => sb.AppendPadded(1, 2).AppendPadded(23, 3).AppendPadded(456, 4), "010230456");

        // Powers of ten
        yield return () => (sb =>
        {
            sb.AppendPadded(1, 0);
            sb.Append(',');
            sb.AppendPadded(10, 0);
            sb.Append(',');
            sb.AppendPadded(100, 0);
            sb.Append(',');
            sb.AppendPadded(1000, 0);
        }, "1,10,100,1000");

        // Negative powers of ten
        yield return () => (sb =>
        {
            sb.AppendPadded(-1, 0);
            sb.Append(',');
            sb.AppendPadded(-10, 0);
            sb.Append(',');
            sb.AppendPadded(-100, 0);
            sb.Append(',');
            sb.AppendPadded(-1000, 0);
        }, "-1,-10,-100,-1000");

        // Alternating zero and non-zero
        yield return () => (sb =>
        {
            sb.AppendPadded(0, 2);
            sb.Append(',');
            sb.AppendPadded(1, 2);
            sb.Append(',');
            sb.AppendPadded(0, 2);
        }, "00,01,00");

        // Zero with different widths
        yield return () => (sb =>
        {
            sb.AppendPadded(0, 1);
            sb.Append('|');
            sb.AppendPadded(0, 2);
            sb.Append('|');
            sb.AppendPadded(0, 3);
            sb.Append('|');
            sb.AppendPadded(0, 4);
        }, "0|00|000|0000");

        // Boundary values
        yield return () => (sb => sb.AppendPadded(999, 5), "00999");

        // Long chain
        yield return () => (sb =>
        {
            for (var i = 0; i < 10; i++)
            {
                sb.AppendPadded(i, 2);
                if (i < 9) sb.Append(' ');
            }
        }, "00 01 02 03 04 05 06 07 08 09");

        // Sequence with formatting
        yield return () => (sb =>
        {
            sb.Append("[");
            sb.AppendPadded(1, 3);
            sb.Append(", ");
            sb.AppendPadded(-2, 4);
            sb.Append(", ");
            sb.AppendPadded(999, 2);
            sb.Append("]");
        }, "[001, -002, 999]");

        // All single digits with padding
        yield return () => (sb =>
        {
            for (var i = 0; i <= 9; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.AppendPadded(i, 3);
            }
        }, "000 001 002 003 004 005 006 007 008 009");
    }

    [Test]
    [MethodDataSource(nameof(GetAppendPaddedTestCases))]
    public async Task AppendPadded_VariousCases_ProducesExpectedOutput(Action<StringBuilder> setup, string expected)
    {
        var sb = new StringBuilder();
        setup(sb);
        await Assert.That(sb.ToString()).IsEqualTo(expected);
    }

    [Test]
    public async Task AppendPadded_ReusedStringBuilder_NoSideEffects()
    {
        var sb = new StringBuilder();
        sb.AppendPadded(10, 3);
        var first = sb.ToString();
        
        sb.Clear();
        sb.AppendPadded(20, 3);
        var second = sb.ToString();

        await Assert.That(first).IsEqualTo("010");
        await Assert.That(second).IsEqualTo("020");
    }
}
