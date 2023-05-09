using System.Diagnostics;

namespace HoneyScoop;

internal static class MainClass {
	/// <summary>
	/// The entry point into the program. Handles program arguments, and initialises state to perform work depending on the arguments
	/// </summary>
	/// <param name="args"></param>
	public static void Main(string[] args) {
		// Taking in Command line arguments
		// Works only after running ParseArgs, which sets the CLI arguments as required
		
		CommandLineArguments argParser = new CommandLineArguments();
		List<string> specifiedFileTypes = argParser.ParseArgs(args);

		HoneyScoop controller = HoneyScoop.Instance();
		controller.Initialise(argParser, specifiedFileTypes);
		
		// If not quiet, start timer
		Stopwatch? sw = null;
		if(!controller.Quiet) {
			sw = new Stopwatch();
			sw.Start();
		}
		
		controller.StartCarving();
		
		// End timer and print elapsed time
		if(!controller.Quiet) {
			sw?.Stop();
			Console.WriteLine($"Took {sw?.Elapsed.TotalSeconds:0.00}s");
		}
	}
}
