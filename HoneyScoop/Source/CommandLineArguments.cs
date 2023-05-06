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
	
	[Option('u', "unrecognised_output", Required = false, HelpText = "Set if the output of unrecognised carvings should be made.")]
	public bool UnrecognisedOutput { get; set; }


	internal List<string> ParseArgs(string[] arguments) {
		List<string> definedFileTypes = new List<string>();
		Parser.Default.ParseArguments<CommandLineArguments>(arguments)
			.WithParsed<CommandLineArguments>(o => {
					if(!string.IsNullOrEmpty(o.OutputDirectory)) {
						OutputDirectory = Path.GetFullPath(o.OutputDirectory.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
					}

					if(o.Verbose && !o.QuietMode) {
						Verbose = true;
						Console.WriteLine($"[+] The output directory is {OutputDirectory}.");
						Console.WriteLine("[+] Verbose output enabled.");
					}

					if (o.UnrecognisedOutput)
					{
						UnrecognisedOutput = true; // if true, unrecognised files will also be outputted.
						if(Verbose) {
							Console.WriteLine("[+] The unrecognised files will be outputted.");
						}
					}

					if(o.QuietMode && !o.Verbose) {
						QuietMode = true;
						// Console.WriteLine("[+] Quiet mode enabled.");
					}

					if(o.NoOrganise) {
						NoOrganise = true;
						if(Verbose) {
							Console.WriteLine("[+] The results will not be organised into directories by filetype.");
						}
					}

					if(o.Timestamp) {
						Timestamp = true;
						if(Verbose) {
							Console.WriteLine("[+] The timestamps will be displayed.");
						}
					}

					if(!File.Exists(o.InputFile)) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("[-] The disk image file supplied does not exist. Please supply the path to the disk image file.");
						Console.ResetColor();
						System.Environment.Exit(0);
					}

					InputFile = o.InputFile;
					if(Verbose) {
						Console.WriteLine($"[+] The reconstruction will take place on the following file: {o.InputFile}");
					}

					// String formatting magic because the commandLineParser does not like Lists

					var fileTypes = o.FileTypes.Split(',');

					foreach(string fileType in fileTypes) {
						if(o.SupportedFormats.Contains(fileType)) {
							if(Verbose) {
								Console.WriteLine($"[+] Reconstruction will be conducted on {fileType} files...");
							}

							definedFileTypes.Add(fileType);
						} else if(Verbose) {
							Console.ForegroundColor = ConsoleColor.Yellow;
							Console.WriteLine($"[-] Filetype: {fileType} is not supported.");
							Console.ResetColor();
						}
					}
				}
			);
		return definedFileTypes;
	}
}