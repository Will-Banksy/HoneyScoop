using CommandLine;

namespace HoneyScoop;

internal class CommandLineArguments {
	/// <summary>
	/// List of supported filetypes
	/// </summary>
	internal readonly List<string> SupportedFormats = new List<string> { "jpg", "png", "gif", "mp4", "mp3", "wav", "xlsx", "pdf", "docx", "pptx", "zip" };

	// The commandlineparser library does not take a list of strings in
	// Converting it to a string makes handling them easier
	public CommandLineArguments() {
		FileTypes = string.Join(",", SupportedFormats);
	}

	// Accepted arguments

	[Option('i', "input_file", Required = true, HelpText = "The input file to conduct file reconstruction on.")]
	public string InputFile { get; set; } = "";

	[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
	public bool Verbose { get; set; }

	[Option('q', "quiet", Required = false, HelpText = "Set whether to view output.")]
	public bool QuietMode { get; set; }

	[Option('O', "no_organise", Required = false, HelpText = "Don't organise carved files by type. By default files will be organised into subdirectories by type.")]
	public bool NoOrganise { get; set; }

	[Option('T', "timestamp", Required = false, HelpText = "Timestamp the output directories, disabled by default.")]
	public bool Timestamp { get; set; }

	[Option('o', "output", Required = false, HelpText = "The output directory path, by default its the current directory.")]
	public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

	[Option('c', "types", Required = false, HelpText = "The types of files to process")]
	public string FileTypes { get; set; }


	internal List<string> ParseArgs(string[] arguments) {
		List<string> definedFileTypes = new List<string>();
		Parser.Default.ParseArguments<CommandLineArguments>(arguments)
			.WithParsed<CommandLineArguments>(o => {
					if(!string.IsNullOrEmpty(o.OutputDirectory)) {
						OutputDirectory = o.OutputDirectory;
					}

					Console.WriteLine($"[+] The output directory is {o.OutputDirectory}.");

					if(o.Verbose && !o.QuietMode) {
						Verbose = true;
						Console.WriteLine("[+] Verbose output enabled.");
					}

					if(o.QuietMode && !o.Verbose) {
						QuietMode = true;
						Console.WriteLine("[+] Quiet mode enabled.");
					}

					if(o.NoOrganise) {
						NoOrganise = true;
						Console.WriteLine("[+] The results will not be organised into directories by filetype.");
					}

					if(o.Timestamp) {
						Timestamp = true;
						Console.WriteLine("[+] The timestamps will be displayed.");
					}

					if(!File.Exists(o.InputFile)) {
						Console.WriteLine("[-] The file supplied in does not exist. Please supply the path to the file in.");
						System.Environment.Exit(0);
					}

					InputFile = o.InputFile;
					Console.WriteLine($"[+] The reconstruction will take place on the following file: {o.InputFile}");

					// String formatting magic because the commandLineParser does not like Lists

					var fileTypes = o.FileTypes.Split(',');

					foreach(string fileType in fileTypes) {
						if(o.SupportedFormats.Contains(fileType)) {
							Console.WriteLine($"[+] Reconstruction will be conducted on {fileType} files...");
							definedFileTypes.Add(fileType);
						} else {
							Console.WriteLine($"[-] Filetype: {fileType} is not supported.");
						}
					}

					// If there is no supported types supplied in.

					if(!definedFileTypes.Any()) {
						Console.WriteLine($"[-] Please provide filetypes accepted by the tool. ({string.Join(", ", o.SupportedFormats)}) ");
						System.Environment.Exit(0);
					}
				}
			);
		return definedFileTypes;
	}
}