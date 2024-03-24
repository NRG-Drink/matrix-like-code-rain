using CommandLine;

namespace NRG.Matrix.App.Models;

public class Option
{
    [Option('d', "delay",Required = false, Default = 80, HelpText = "Delay in Milliseconds between two frames.")]
    public int Delay { get; set; }
    [Option('t', "time", Required = false, Default = null, HelpText = "How long the benchmark should run.")]
	public TimeSpan? Time { get; set; }
	[Option('m', "maxobjects", Required = false, Default = 9999, HelpText = "Maximum number of objects on the screen.")]
	public int MaxObjects { get; set; }
	[Option('a', "addrate", Required = false, HelpText = "Rate on how fast per frame the objects should spawn. (float)")]
	public string AddRate { get; set; } = "e => 1";
}
