using NRG.Matrix.Models;

namespace NRG.Matrix;

public class MatrixObjectFactory
{
    public int MinTraceLength { get; init; } = 20;
    public int VarianceTraceLength { get; init; } = 10;
    public char[] LeadCharset { get; init; } = Alphabets.Default;
    public char[] TraceCharset { get; init; } = Alphabets.Default;

    public IEnumerable<MatrixObject> CreateNewMatrixLine(int xRange)
    {
        var yPos = -1;
        var xPos = Random.Shared.Next(0, xRange);
        var traceLength = MinTraceLength + Random.Shared.Next(0, VarianceTraceLength + 1);

        var lead = new MatrixLead(new(xPos, yPos), LeadCharset);
        yield return lead;

        var traces = Enumerable
            .Range(1, traceLength)
            .Select((e, i) => new MatrixTrace(new(xPos, yPos - e), TraceCharset));
        foreach (var t in traces)
        {
            yield return t;
        }

        var clean = new MatrixClean(new(xPos, yPos - traceLength - 1));
        yield return clean;
    }
}