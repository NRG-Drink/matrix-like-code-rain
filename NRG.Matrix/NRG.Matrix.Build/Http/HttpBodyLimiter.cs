namespace NRG.Matrix.Build.Http;

public static class HttpLoggingLimiter
{
    public static int MaxLength { get; set; } = 450;

    public static string Limit(string body)
    {
        if (body.Length > MaxLength)
        {
            body = $"{body[..MaxLength]}\n... (truncated. original length: {body.Length})";
        }

        return body;
    }
}

public static class HttpBodyLimiter
{
    public static int MaxLength { get; set; } = 200;

    public static string Limit(string body)
    {
        if (body.Length > MaxLength)
        {
            body = $"{body[..MaxLength]}\n... (truncated. original length: {body.Length})";
        }

        return body;
    }
}
