﻿using NRG.Matrix.Models;
using System.Drawing;

namespace NRG.Matrix;

public class MatrixObjectHandler(
    int minTraceLength = 20,
    int varianceTraceLength = 10,
    int yFall = 1
    )
{
    public IEnumerable<MatrixObject> CreateNewMatrixLine(int xRange)
    {
        var yPos = -1;
        var xPos = Random.Shared.Next(0, xRange);
        var traceLength = minTraceLength + Random.Shared.Next(0, varianceTraceLength + 1);

        var lead = MatrixObject.CreateLead(new(xPos, yPos), 'L');
        yield return lead;

        var traces = Enumerable
            .Range(1, traceLength)
            .Select((e, i) => MatrixObject.CreateTrace(new(xPos, yPos - e), 't'));
        foreach (var t in traces)
        {
            yield return t;
        }

        var clean = MatrixObject.CreateClean(new(xPos, yPos - traceLength - 1));
        yield return clean;
    }

    public MatrixObject Fall(MatrixObject obj)
        => obj with { Pos = FallPoint(obj.Pos) };

    public MatrixObject Symbol(MatrixObject obj)
        => ShouldGetNewSymbol(obj.SymbolChangeChance)
            ? obj with { Symbol = GetRandomSymbol() }
            : obj;

    private Point FallPoint(Point p)
        => p with { Y = p.Y + yFall };

    private static char GetRandomSymbol()
        => (char)Random.Shared.Next(33, 123);

    private static bool ShouldGetNewSymbol(int chance)
        => Random.Shared.Next(0, chance) is 0;
}