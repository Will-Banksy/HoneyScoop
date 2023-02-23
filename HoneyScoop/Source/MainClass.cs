// Conventions that should be followed throughout the codebase, based off of existing conventions. Feel free to suggest changes TODO Revisit
// PascalCase for classes/structs/records and functions, delegates, namespaces and public/internal member variables as well as properties
// camelCase for private member variables, method parameters, local variables
//     Prefix private member variables with an underscore e.g. `_privateMemberVar`
// Prefix interfaces with I, e.g. IFileType
// Explicit access modifiers for everything
//     Everything is `internal` unless it is `private`

// For notes on writing performant C# and for C# resources: https://willbanksy-pkb.notion.site/C-edef060a627f4f2babe13346a11e5962

using System.Diagnostics;
using System.Runtime.InteropServices;
using HoneyScoop.Searching.RegexImpl;
using CommandLine;

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

		// Taking in Command line arguments
		Parser.Default.ParseArguments<Helpers>(args)
            .WithParsed<Helpers>(o =>
            {
                Console.WriteLine($"[+] The output directory is {o.OutputDirectory}.");
                Console.WriteLine($"[+] The program will use {o.NumThreads} threads for processing.");                                
                if (o.Verbose && !o.QuietMode)
                {
                    Console.WriteLine("[+] Verbose output enabled.");
                }
                if (o.QuietMode && !o.Verbose)
                {
                    Console.WriteLine("[+] Quiet mode enabled.");
                }
                if (o.NoOrganise)
                {
                    Console.WriteLine("[+] The results will not be organised into directories by filetype.");
                }
                if (o.Timestamp)
                {
                    Console.WriteLine("[+] The timestamps will be displayed.");
                } 

                /// String formatting magic because the commandLineParser does not like Lists

                var fileTypes = o.FileTypes.Split(',');
                List<string> definedFileTypes = new List<string> ();


                foreach (string fileType in fileTypes)
                {
                    if(o.supportedFormats.Contains(fileType))
                    {
                        Console.WriteLine($"[+] Reconstruction will be conducted on {fileType} files...");
                        definedFileTypes.Add(fileType);

                    }
                    else
                    {
                        Console.WriteLine($"[-] Filetype: {fileType} is not supported.");
                    }
                }

                /// If there is no supported types supplied in.

                if(!definedFileTypes.Any())
                {
                    Console.WriteLine($"[-] Please provide filetypes accepted by the tool. ({string.Join(", ", o.supportedFormats)}) ");
                    System.Environment.Exit(0);
                    
                }                           
            }
        );


		var regex = @"\x0a";
		var expected = new FiniteStateMachine<byte>(0x0a);
		var got = RegexEngine.ParseRegex(regex);
		Debug.Assert(got.Equals(expected), "Test Failed: ParseRegex doesn't work :(");

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
