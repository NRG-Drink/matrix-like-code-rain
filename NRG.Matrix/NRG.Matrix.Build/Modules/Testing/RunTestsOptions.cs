namespace NRG.Matrix.Build.Modules.Testing;

public record RunTestsOptions
{
    public bool Coverage { get; set; }
    public string CoverageOutputFormat { get; set; } = "cobertura";
    public string? TreenodeFilter { get; set; } = "/*/*/*/*[Category!=NotRunInPipeline]";
    public string? ResultsDirecotry { get; set; }
    public List<string> Arguments { get; set; } = [];

    public List<string> ToArguments()
    {
        var args = new List<string>();
        if (Coverage)
        {
            args.Add("--coverage");
        }

        if (CoverageOutputFormat is not null)
        {
            args.Add("--coverage-output-format");
            args.Add(CoverageOutputFormat);
        }

        if (TreenodeFilter is not null)
        {
            args.Add("--treenode-filter");
            args.Add(TreenodeFilter);
        }

        if (ResultsDirecotry is not null)
        {
            args.Add("--results-directory");
            args.Add(ResultsDirecotry);
        }

        if (Arguments.Count != 0)
        {
            args.AddRange(Arguments);
        }

        return args;
    }
}
