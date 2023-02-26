// Conventions that should be followed throughout the codebase, based off of existing conventions. Feel free to suggest changes
// Most of these conventions from MS docs should be followed: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions
//     Exceptions and highlights are outlined below
//
// PascalCase for classes/structs/records and functions, delegates, namespaces and public/internal member variables as well as properties
// camelCase for private member variables, method parameters, local variables
//     Prefix private member variables with an underscore e.g. `_privateMemberVar`
// Prefix interfaces with I, e.g. IFileType
// Explicit access modifiers for everything
//     Everything is `internal` unless it is `private` (although for overridden methods they may have to be public)
// Use `var` for declaring variables only when the type is obvious
// Space after the `//` and `///` of comments (before the actual comment text)
// Indent using TABS, align using SPACES (Indent in comments with spaces too)
// Braces on the same line, e.g. // TODO Revisit, cause this is not usual C# style and therefore not what editors default to but I vastly prefer it
//
//     if(flag) {
//         // Code
//     } else {
//         // Code
//     }
//
// Instead of
//
//     if(flag)
//     {
//         // Code
//     }
//
// Also no space between keywords and any arguments in brackets after them e.e. `if(flag)` not `if (flag)` and `switch(thing)` not `switch (thing)`
//
// For notes on writing performant C# and for C# resources: https://willbanksy-pkb.notion.site/C-edef060a627f4f2babe13346a11e5962

using System.Diagnostics;
using HoneyScoop.FileHandling.FileTypes;
using HoneyScoop.Searching.RegexImpl;

namespace HoneyScoop;

internal static class MainClass {
	/// <summary>
	/// The entry point into the program. Handles program arguments, and initialises state to perform work depending on the arguments
	///
	/// Also using the Main function for testing rn (probably need a better solution... Unit Test project?)
	/// </summary>
	/// <param name="args"></param>
	public static void Main(string[] args) {
		#region Testing
		
		byte[] testPngData = {
			0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x0E, 0x00, 0x00, 0x00, 0x16, 
			0x04, 0x03, 0x00, 0x00, 0x00, 0x02, 0xE3, 0xB0, 0x14
		};
		var ihdr = FileTypePng.Ihdr.DeserializeFrom(testPngData);
		Debug.Assert(
			ihdr.ChunkSize == 13 &&
			ihdr.ChunkType == 1229472850 &&
			ihdr.Width == 14 &&
			ihdr.Height == 22 &&
			ihdr.BitDepth == 4 &&
			ihdr.ColourType == 3 &&
			ihdr.CompressionMethod == 0 &&
			ihdr.FilterMethod == 0 &&
			ihdr.InterlaceMethod == 0 &&
			ihdr.Crc == 48476180,
			"IHDR chunk was deserialized incorrectly"
		);

		var infix = @"((\x0a\x0b*)|\x0c?)+\x0d\x0e\x0f";
		var expectedPostfix = @"\x0a\x0b*'\x0c?|+\x0d'\x0e'\x0f'";
		Console.WriteLine($"Infix: {infix}");
		string postfix = RegexLexer.Token.TokensToString(RegexEngine.ParseToPostfix(infix));
		Console.WriteLine($"Postfix: {postfix}");
		Debug.Assert(postfix.Equals(expectedPostfix), "Test Failed: Infix regex was not converted to correct postfix expression");
		
		var regex = @"\x0a\x0b";

		StateTransitionTable stt = StateTransitionTable.Build(RegexEngine.ParseToPostfix(regex));
		Console.WriteLine($"stt({stt.ToString()})");

		var expected = new FiniteStateMachine<byte>(0x0a);
		FiniteStateMachine<byte> got = RegexEngine.ParseRegex(regex);
		Debug.Assert(got.Equals(expected), "Test Failed: Postfix regex was not converted into a Finite State Machine/NFA correctly (or in this case the NFA comparison is broken while I work on a solution)");

		#endregion
		
		Console.WriteLine("Hello, The Hive");
		
		// Taking in Command line arguments
		// Works only after running ParseArgs, which sets the CLI arguments as required
		
		Helpers takenArguments = new Helpers();
		List<string> definedArguments = takenArguments.ParseArgs(args);
		
		// Accessible arguments:
		// Pattern: TakenArguments.COMMAND_LINE_ARGUMENT
		// TakenArguments.OutputDirectory String path, which is the place the directories, files should be made, current directory path by default
		// TakenArguments.NumThreads Integer number of threads to be used 40 by default
		// TakenArguments.Verbose Boolean if everything should be in CL, false by default
		// TakenArguments.QuietMode Boolean if no CL output wanted, false by default
		// TakenArguments.Timestamp Boolean if the output directories are to be timestamped, false by default
		// TakenArguments.NoOrganise Boolean if organising by filetype is not needed, false by default(or organised by default)
		// DefinedArguments a List of the filetypes needed to search for.
	}
}
