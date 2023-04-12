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
		// The way scalpel does it:
		//     1. Split the file up into large chunks and read a chunk at a time, keeping the last chunk in memory
		//	   2. 1st pass: Find all headers and footers (only searching for footers if there is a header for which a footer could match in the current or last chunk),
		//        keeping track of found headers and footers
		//     3. 2nd pass: Extract file data where footers matching headers within the current chunk or last (as long as the amount of data that would be carved does not exceed a maximum) have been found,
		//        or reading a defined max number of bytes from header if a footer has not been found or defined for that file type
		
		// The way we're gonna do it (probably):
		//     1. Split the file up into large chunks and read a chunk at a time
		//     2. 1st pass: Find all headers and footers in the current chunk (including allowing for headers/footers to be across chunk boundaries),
		//        keeping track of where in the file found headers and footers are and their size
		//     3. Pair up headers and footers, discarding lone footers and providing an adequately large amount of data after a header with no matching footer or no defined footer for that type
		//     4. 2nd pass: Go through the file, accumulating data into some sort of data structure which intelligently only keeps necessary data loaded (may not be necessary),
		//        and once all data for a particular header is accumulated, call Analyse on that data and then if the result is not "unrecognised" then write that data into a file
		//        (maybe cmdline switch to enable writing unrecognised data?)
		//         - The way we will handle large found files is... do we want to do like scalpel and have a maximum size we deal with or we could simply forgo analysis on files that
		//           exceed a maximum but still write them out
		//             - The first way will miss larger files but the second could introduce false positives (although we can label them as "not analysed" ig) and could introduce some major performance issues
		//               and take up lots more disk space. The scalpel team clearly thought that the first was the optimal approach which is worth considering, as far more experienced and knowledgeable
		//               individuals I'm sure they are

		// Firstly, create an instance of FileHandler
		var fileHandler = new FileHandler(InputFile);
		// And then create the instances of RegexMatcher from the SupportedFileTypes.FileTypeHandlers
		List<RegexMatcher> matchers = CreateMatchers();
	
		// This list will keep track of matches found
		List<Pair<Match, Match?>> matches = new();
		// This stack will hold headers until a matching footer is found
		var matchStack = new Stack<Match>();

		do {
			ReadOnlySpan<byte> sectionBytes = fileHandler.Next();

			for(int i = 0; i < matchers.Count; i++) {
				List<Match> sectionMatches = matchers[i].Advance(sectionBytes);
				for(int j = 0; j < sectionMatches.Count; j++) {
					// Check whether the MatchType is even - if so, it is a header
					if((sectionMatches[j].MatchType & 0x1) != 0x1) {
						// Handle found header
						matchStack.Push(sectionMatches[j]);
					} else {
						while (matchStack.Peek().MatchType != (sectionMatches[j].MatchType - 1)) {matchStack.Pop();}
						//Adds the matched header and footer pair to the list
						matches.Add(new Pair<Match, Match?>(matchStack.Pop(), sectionMatches[j]));
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
