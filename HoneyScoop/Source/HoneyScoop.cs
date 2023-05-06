using System.Diagnostics;
using HoneyScoop.Carving;
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
	internal bool UnrecognisedOutput;
	internal string OutputDirectory;
	internal string InputFile;

	private HoneyScoop() {
		FileTypes = new List<FileType>();
		Verbose = false;
		Quiet = false;
		NoOrganise = false;
		Timestamp = false;
		UnrecognisedOutput = false;
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

	/// <summary>
	/// Sets up the singleton HoneyScoop instance with parsed arguments
	/// </summary>
	/// <param name="parsedArgs"></param>
	/// <param name="fileTypes"></param>
	internal void Initialise(CommandLineArguments parsedArgs, List<string> fileTypes) {
		for(int i = 0; i < fileTypes.Count; i++) {
			FileType type = Helper.FromString(fileTypes[i]);
			if(type != FileType.None && !FileTypes.Contains(type)) {
				FileTypes.Add(type);
			}
		}

		Verbose = parsedArgs.Verbose;
		Quiet = parsedArgs.QuietMode;
		NoOrganise = parsedArgs.NoOrganise;
		Timestamp = parsedArgs.Timestamp;
		UnrecognisedOutput = parsedArgs.UnrecognisedOutput;
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
		//     3. 2nd pass: Extract file data where footers matching headers within the current chunk or previous chunks (as long as the amount of data that would be carved does not exceed a maximum) have been found,
		//        or reading a defined max number of bytes from header if a footer has not been found or defined for that file type
		// This is not actually fully correct or might not be just read this: https://dfrws.org/sites/default/files/session-files/2005_USA_paper-scalpel_-_a_frugal_high_performance_file_carver.pdf

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

		const int chunkSize = 1024 * 1024 * 10; // 10 MB

		// Firstly, create an instance of FileHandler
		var fileHandler = new FileHandler(InputFile);

		// Perform the first phase/pass - Find all headers and footers in the file for each specified file type
		List<Match> headerFooterMatches = SearchPhase(fileHandler, chunkSize, FileTypes);

		// Reset the file handler for further operations
		fileHandler.Reset();

		// Process the search results to get the match pairs to feed to the next phase
		List<(Match, Match?)> matchPairs = ProcessSearchResults(headerFooterMatches);

		// Perform the second phase/pass - Analyse and write out the found files
		CarvePhase(fileHandler, chunkSize, matchPairs);

		// Close the file handler; Free the resource
		fileHandler.Close();
	}

	/// <summary>
	/// This function searches the file for headers and footers, returning a list of <see cref="Match"/> values describing headers and footers
	/// </summary>
	/// <param name="fileHandler"></param>
	/// <param name="chunkSize"></param>
	/// <param name="fileTypes"></param>
	private List<Match> SearchPhase(FileHandler fileHandler, int chunkSize, List<FileType> fileTypes) {
		// Create the instances of RegexMatcher, passing in the fileTypes list
		List<RegexMatcher> matchers = CreateMatchers(fileTypes);

		if(Verbose) {
			Console.WriteLine($"Starting searching... (Chunk size: {chunkSize} bytes)");
		}

		// List to keep track of found matches throughout the whole file
		List<Match> foundMatches = new();

		// Allocate buffer for storing chunk data
		byte[] buffer = new byte[chunkSize];

		Stopwatch timer = Stopwatch.StartNew();

		// Read the file chunk by chunk and search each chunk of the file for headers and footers
		do {
			long currentOffset = fileHandler.CurrentPosition;
			fileHandler.Next(buffer);
			ReadOnlySpan<byte> chunkBytes = buffer;

			for(int i = 0; i < matchers.Count; i++) {
				List<Match> chunkMatches = matchers[i].Advance(chunkBytes, currentOffset);
				foundMatches.AddRange(chunkMatches);
			}

			if(!Quiet) {
				string elapsed = timer.Elapsed.TotalSeconds.ToString("0.00");
				string progress = (((float)currentOffset / (float)fileHandler.FileSize) * 100).ToString("0.00");
				Console.Write($"\rSearching... {progress}% complete ({elapsed}s elapsed)");
			}
		} while(!fileHandler.Eof);

		foundMatches.Sort((m1, m2) => (int)(m1.StartOfMatch - m2.StartOfMatch));

		Console.WriteLine();

		if(Verbose) {
			Console.WriteLine($"Done searching ({foundMatches.Count} total header/footer matches)");
		}

		return foundMatches;
	}

	private List<(Match, Match?)> ProcessSearchResults(List<Match> matches) { // TODO: Complete/correct this
		if(Verbose) {
			Console.WriteLine("Processing search results...");
		}

		List<(Match, Match?)> completeMatches = new();
		Dictionary<FileType, Stack<Match>> matchTracker = new();

		for(int i = 0; i < matches.Count; i++) {
			FileType matchType = matches[i].MatchType.Type;
			if(Helper.HasFooter(matchType) && matches[i].MatchType.Part == FilePart.Header) {
				if(!matchTracker.ContainsKey(matchType)) {
					matchTracker[matchType] = new Stack<Match>();
				}

				matchTracker[matchType].Push(matches[i]);
			} else if(matches[i].MatchType.Part == FilePart.Header) {
				completeMatches.Add((matches[i], null));
			} else {
				if(!matchTracker.ContainsKey(matchType)) {
					matchTracker[matchType] = new Stack<Match>();
				}
				
				Stack<Match> matchStack = matchTracker[matchType];
				while(matchStack.Count != 0 && matchStack.Peek().MatchType.Part != FilePart.Header) {
					matchStack.Pop();
				}

				if(matchStack.Count != 0) {
					completeMatches.Add((matchStack.Pop(), matches[i]));
				} else if(matches[i].MatchType.Part == FilePart.Header && !SupportedFileTypes.FileTypeHandlers[matchType].RequiresFooter) {
					completeMatches.Add((matches[i], null));
				}
			}
		}

		foreach(Stack<Match> matchStack in matchTracker.Values) {
			while(matchStack.Count != 0) {
				Match match = matchStack.Pop();
				if(match.MatchType.Part == FilePart.Header && !SupportedFileTypes.FileTypeHandlers[match.MatchType.Type].RequiresFooter) {
					completeMatches.Add((match, null));
				}
			}
		}

		// // Pair every header with a footer if a suitable one exists, if not then pair it with null
		// //     A suitable one would be probably the next footer that has the same file type as the header (multiple headers might be paired with the same footer)
		// // Return those header-footer/null pairs
		// Stack<Match> matchStack = new Stack<Match>();
		// List<(Match, Match?)> completeMatches = new List<(Match, Match?)>();
		//
		// for(var i = 0; i < matches.Count; i++) {
		// 	//Removes any footers that precede the first header
		// 	if(matches[i].MatchType.Part == FilePart.Footer && matchStack.Count == 0) {
		// 		continue;
		// 	}
		//
		// 	//Once it finds a header it will add it to the stack to be matched with a footer
		// 	if(matches[i].MatchType.Part == FilePart.Header && matchStack.Count == 0) {
		// 		matchStack.Push(matches[i]);
		// 	}
		// 	//if a new header is found before a footer pair is found for the previous header then it will pair it with a null footer (Main concern is false positive header being found and wiping away the old one, hopefully shouldn't be the case)
		// 	else if(matches[i].MatchType.Part == FilePart.Header && matchStack.Count != 0) {
		// 		completeMatches.Add((matchStack.Pop(), null));
		//
		// 		matchStack.Push(matches[i]);
		// 	} else {
		// 		if(matches[i].MatchType.Part == FilePart.Footer && matches[i].MatchType.Type.Equals(matchStack.Peek().MatchType.Type)) {
		// 			completeMatches.Add((matchStack.Pop(), matches[i]));
		// 		}
		// 		//When a footer is found after a header but doesn't match the header it is skipped because there shouldn't be overlapping headers and footers from different filetypes
		// 		else if(matches[i].MatchType.Part == FilePart.Footer && matches[i].MatchType != matchStack.Peek().MatchType) {
		// 			continue;
		// 		}
		// 	}
		// }
		//
		// while(matchStack.Count != 0) {
		// 	if(matchStack.Peek().MatchType.Part.Equals(FilePart.Footer)) {
		// 		matchStack.Pop();
		// 	} else if(matchStack.Peek().MatchType.Part.Equals(FilePart.Header)) {
		// 		completeMatches.Add((matchStack.Pop(), null));
		// 	}
		// }

		if(Verbose) {
			Console.WriteLine($"Done processing search results ({completeMatches.Count} matches)");
		}

		return completeMatches;
	}

	private void CarvePhase(FileHandler fileHandler, int chunkSize, List<(Match, Match?)> pairs) {
		// First, preprocess the match pairs to figure out which ranges need to be read into which files
		// Probably need to map match pairs to file chunks and then assign "work" to be done in each chunk - basically roughly replicating what scalpel does
		// Then read out the data from the file into the carved files - try be efficient here and only read what is necessary
		// Remember to work with the various command-line options (e.g. Timestamp, Quiet, Verbose)

		if(Verbose) {
			Console.WriteLine("Building carving information...");
		}

		if(Timestamp) {
			Helper.SetTimestampedOutputDir();
		}

		CarveHandler carveHandler = new CarveHandler(chunkSize, pairs);

		if(Verbose) {
			Console.WriteLine("Done building carving information");
			Console.WriteLine("Performing carving...");
		}

		carveHandler.PerformCarving(fileHandler);

		if(Verbose) {
			Console.WriteLine("Done carving");
		}
	}

	/// <summary>
	/// This function creates and returns RegexMatchers for each supplied <see cref="FileType"/>.
	/// Each FileType is paired with a uint which is the UID for that file type - Usual process is to assign the UID of the FileType
	/// to be the index of it in the FileType enum.<br />
	/// Specifically, the UID supplied with each file type is first multiplied by 2 and then assigned to the matcher that matches the header
	/// for that file type - the footer is the UID * 2 + 1 (1 more than the header), so that UIDs assigned to headers are always even and those
	/// assigned to footers are always odd, and doing integer division of the matcher UID by 2 returns the UID of the FileType.
	/// </summary>
	/// <param name="fileTypes"></param>
	/// <returns></returns>
	private List<RegexMatcher> CreateMatchers(List<FileType> fileTypes) {
		if(Verbose) {
			Console.WriteLine("Creating matchers...");
		}

		List<RegexMatcher> matchers = new();
		matchers.EnsureCapacity(fileTypes.Count);

		foreach(var fileType in fileTypes.Distinct()) {
			bool supported = SupportedFileTypes.FileTypeHandlers.TryGetValue(fileType, out IFileType? iFileType);
			if(!supported || iFileType == null) {
				if(!Quiet) {
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine($"Skipping unsupported/unimplemented file type {fileType}");
					Console.ResetColor();
				}
			} else {
				var headerMatcher = new RegexMatcher(iFileType.Header, new FileTypePart(fileType, FilePart.Header));
				matchers.Add(headerMatcher);
				if(iFileType.HasFooter) {
					var footerMatcher = new RegexMatcher(iFileType.Footer, new FileTypePart(fileType, FilePart.Footer));
					matchers.Add(footerMatcher);
				}
			}
		}

		if(Verbose) {
			Console.WriteLine($"Done creating matchers ({matchers.Count} matchers)");
		}

		return matchers;
	}

	public override string ToString() {
		return $"{nameof(FileTypes)}: {FileTypes}, {nameof(Verbose)}: {Verbose}, {nameof(Quiet)}: {Quiet}, {nameof(NoOrganise)}: {NoOrganise}, {nameof(Timestamp)}: {Timestamp}, {nameof(OutputDirectory)}: {OutputDirectory}, {nameof(InputFile)}: {InputFile}";
	}
}