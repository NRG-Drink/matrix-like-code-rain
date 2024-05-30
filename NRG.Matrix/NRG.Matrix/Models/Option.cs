using CommandLine;

namespace NRG.Matrix.App.Models;

public class Option
{
	[Option('d', "delay", Required = false, Default = 80, HelpText = "Delay in Milliseconds between two frames (speed of falling objects).")]
	public int Delay { get; init; }
	[Option('o', "max-objects", Required = false, Default = 9999, HelpText = "Maximum number of objects on the screen (including traces).")]
	public int MaxObjects { get; init; }
	[Option('a', "add-rate", Required = false, Default = 1, HelpText = "Number of objects added each frame (float):")]
	public float AddRate { get; init; }
}
