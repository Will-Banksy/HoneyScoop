using HoneyScoop.FileHandling;
using HoneyScoop.Searching;
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
		// Firstly, create an instance of FileHandler
		var fileHandler = new FileHandler(InputFile);
		// And then create the instances of RegexMatcher from the SupportedFileTypes.FileTypeHandlers
		List<RegexMatcher> matchers = CreateMatchers();

		// This list will keep track of matches found
		List<Pair<Match, Match?>> matches = new();

		do {
			ReadOnlySpan<byte> sectionBytes = fileHandler.Next();

			for(int i = 0; i < matchers.Count; i++) {
				List<Match> sectionMatches = matchers[i].Advance(sectionBytes);
				for(int j = 0; j < sectionMatches.Count; j++) {
					// Check whether the MatchType is even - if so, it is a header
					if((sectionMatches[j].MatchType & 0x1) != 0x1) {
						// Handle found header
					} else {
						// Handle found footer
					}
				}
			}
		} while(!fileHandler.Eof);
	}

	private List<RegexMatcher> CreateMatchers() {
		List<RegexMatcher> matchers = new();
		matchers.EnsureCapacity(SupportedFileTypes.FileTypeHandlers.Values.Count);

		uint i = 0;

		foreach(IFileType fileTypeInstance in SupportedFileTypes.FileTypeHandlers.Values) {
			matchers.Add(new RegexMatcher(fileTypeInstance.Header, i));
			i++;
			matchers.Add(new RegexMatcher(fileTypeInstance.Footer, i));
			i++;
		}

		return matchers;
	}

	public override string ToString() {
		return $"{nameof(FileTypes)}: {FileTypes}, {nameof(Verbose)}: {Verbose}, {nameof(Quiet)}: {Quiet}, {nameof(NoOrganise)}: {NoOrganise}, {nameof(Timestamp)}: {Timestamp}, {nameof(NumThreads)}: {NumThreads}, {nameof(OutputDirectory)}: {OutputDirectory}, {nameof(InputFile)}: {InputFile}";
	}
}
