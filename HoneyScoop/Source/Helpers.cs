using CommandLine;

namespace HoneyScoop;

public class Helpers
{
    /// List of supported filetypes

    public List<string> supportedFormats = new List<string> { "jpg", "png", "gif", "mp4", "mp3", "wav", "xlsx", "pdf", "docx", "pptx", "zip" };

    /// The commandlineparser library does not take a list of strings in
    /// Converting it to a string makes handling them easier

    public Helpers()
    {
        FileTypes = string.Join(",", supportedFormats);
    }

    /// Accepted arguments

    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }

    [Option('q', "quiet", Required = false, HelpText = "Set whether to view output.")]
    public bool QuietMode { get; set; }

    [Option('O', "no_organise", Required = false, HelpText = "Don't organise carved files by type. By default files will be organised into subdirectories by type.")]
    public bool NoOrganise { get; set; }

    [Option('T', "timestamp", Required = false, HelpText = "Timestamp the output directories, disabled by default.")]
    public bool Timestamp { get; set; }

    [Option('t', "threads", Required = false, HelpText = "The number of threads to use for processing, by default its 40.")]
    public int NumThreads { get; set; } = 40;

    [Option('o', "output", Required = false, HelpText = "The output directory path, by default its the current directory.")]
    public string OutputDirectory { get; set; } = Environment.CurrentDirectory;

    [Option('c', "types", Required = false, HelpText = "The types of files to process")]
    public string FileTypes { get; set; }
}