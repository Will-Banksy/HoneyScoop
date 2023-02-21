// Conventions that should be followed throughout the codebase, based off of existing conventions. Feel free to suggest changes TODO Revisit
// PascalCase for classes/structs/records and functions, delegates, namespaces and public/internal member variables as well as properties
// camelCase for private member variables, method parameters, local variables
//     Prefix private member variables with an underscore e.g. `_privateMemberVar`
// Prefix interfaces with I, e.g. IFileType
// Explicit access modifiers for everything
//     Everything is `internal` unless it is `private`

// asdf

// For notes on writing performant C# and for C# resources: https://willbanksy-pkb.notion.site/C-edef060a627f4f2babe13346a11e5962

using System.Runtime.InteropServices;
using HoneyScoop.Searching.RegexImpl;

namespace HoneyScoop;

internal static class MainClass {
	/// <summary>
	/// Just using the Main function for testing rn
	/// </summary>
	/// <param name="args"></param>
	public static void Main(string[] args) {
		// Handle arguments, create HoneyScoop instance to perform work
		// Might be an idea to spread the argument handling across different files or use a library for it (NuGet, e.g. https://www.nuget.org/packages/CommandLineParser#readme-body-tab)
		
		var infix = @"((\x0a\x0b*)|\x0c?)+\x0d\x0e\x0f";
		Console.WriteLine($"Infix: {infix}");
		var postfix = RegexEngine.ParseToPostfix(infix);
		Console.Write("Postfix: ");
		PrintTokens(postfix); // Works
		Console.WriteLine();
		
		Console.WriteLine("Hello, The Hive");
	}

	/// <summary>
	/// Could be generic tbf but just loops through a token list and prints each item (no separators or newlines)
	/// </summary>
	/// <param name="tokens"></param>
	private static void PrintTokens(List<RegexLexer.Token> tokens) {
		foreach(var t in tokens) {
			Console.Write(t);
		}
	}
}
