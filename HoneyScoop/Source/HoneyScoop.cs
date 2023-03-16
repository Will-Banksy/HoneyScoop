using HoneyScoop.FileHandling;
using HoneyScoop.Util;

namespace HoneyScoop;

/// <summary>
/// This class is the main driver of the CLI application - It takes various options (although not parsing the CLI args itself)
/// and informed by them, uses various parts of the codebase to produce the appropriate output
/// </summary>
internal class HoneyScoop {
	private static HoneyScoop? _instance = null;

	internal List<FileType> FileTypes;
	internal bool Verbose;
	internal bool Quiet;
	internal bool NoOrganise;
	internal bool Timestamp;
	internal int NumThreads;
	internal string OutputDirectory;
	internal string InputFile;

	private HoneyScoop() {
		FileTypes = new List<FileType>();
		Verbose = false;
		Quiet = false;
		NoOrganise = false;
		Timestamp = false;
		NumThreads = 0;
		OutputDirectory = "";
		InputFile = "";
	}

	/// <summary>
	/// Get the instance of the HoneyScoop class, creating one if it doesn't exist.
	/// Only one instance will ever exist (Singleton pattern).
	/// </summary>
	/// <returns></returns>
	internal static HoneyScoop Instance() {
		if(_instance == null) {
			_instance = new HoneyScoop();
		}

		return _instance;
	}

	internal void Initialise(CommandLineArguments parsedArgs, List<string> fileTypes) {
		for(int i = 0; i < fileTypes.Count; i++) {
			FileType type = Helper.FromString(fileTypes[i]);
			if(type != FileType.None) {
				FileTypes.Add(type);
			}
		}

		Verbose = parsedArgs.Verbose;
		Quiet = parsedArgs.QuietMode;
		NoOrganise = parsedArgs.NoOrganise;
		Timestamp = parsedArgs.Timestamp;
		NumThreads = parsedArgs.NumThreads;
		OutputDirectory = parsedArgs.OutputDirectory;
		InputFile = parsedArgs.InputFile;
	}

	/// <summary>
	/// Start the carving process
	/// </summary>
	internal void StartCarving() {
		// TODO
	}

	public override string ToString() {
		return $"{nameof(FileTypes)}: {FileTypes}, {nameof(Verbose)}: {Verbose}, {nameof(Quiet)}: {Quiet}, {nameof(NoOrganise)}: {NoOrganise}, {nameof(Timestamp)}: {Timestamp}, {nameof(NumThreads)}: {NumThreads}, {nameof(OutputDirectory)}: {OutputDirectory}, {nameof(InputFile)}: {InputFile}";
	}
}
