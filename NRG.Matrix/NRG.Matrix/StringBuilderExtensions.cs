using System.Text;

namespace NRG.Matrix;

public static class StringBuilderExtensions
{
    /// <summary>
    /// Appends a long value left-padded with zeros to the specified width.
    /// Avoids string allocations from ToString with format strings.
    /// </summary>
    public static StringBuilder AppendPadded(this StringBuilder sb, long value, int width)
    {
        // Handle negative numbers
        if (value < 0)
        {
            sb.Append('-');
            value = -value;
            width--;
        }

        // Count digits
        var digits = CountDigits(value);

        // Add leading zeros
        for (var i = digits; i < width; i++)
        {
            sb.Append('0');
        }

        // Append the number itself
        if (value == 0)
        {
            // Already handled by padding if width > 0, but if width is 0, append 0
            if (digits == 1 && width <= 1)
            {
                sb.Append('0');
            }
        }
        else
        {
            AppendDigits(sb, value, digits);
        }

        return sb;
    }

    private static int CountDigits(long value)
    {
        if (value == 0) return 1;
        var count = 0;
        while (value > 0)
        {
            count++;
            value /= 10;
        }
        return count;
    }

    private static void AppendDigits(StringBuilder sb, long value, int digits)
    {
        // Use stack-allocated buffer for digits
        Span<char> buffer = stackalloc char[20]; // Max long is ~19 digits
        var pos = digits - 1;
        while (value > 0)
        {
            buffer[pos--] = (char)('0' + (value % 10));
            value /= 10;
        }
        for (var i = 0; i < digits; i++)
        {
            sb.Append(buffer[i]);
        }
    }
}
