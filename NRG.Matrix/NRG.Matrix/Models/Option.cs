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
	[Option('m', "max-frame-time", Required = false, Default = 20, HelpText = "Maximum calculation time of a frame (prevent lagging).")]
	public int MaxFrameTime { get; init; }
	[Option('b', "bench-mode", Required = false, Default = false, HelpText = "Enter benchmark-mode to see computing stats.")]
	public bool? IsBench { get; init; }
}
