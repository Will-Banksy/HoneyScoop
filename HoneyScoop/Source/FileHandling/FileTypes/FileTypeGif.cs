using System.Text.RegularExpressions;
using HoneyScoop.FileHandling;internal class FileTypeGif : IFileType {
	public string Header => "GIF";
	public string Footer => "";
	public bool HasFooter => false;
	public string FileExtension => "gif";

	private const int HeaderSize = 13; // The GIF header is 13 bytes long, including the version number and logical screen width/height
	
	
	/// <param name="data">The stream of data bytes that get checked.</param>
	
	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		// Check if data is at least the length of the header
		if(data.Length < HeaderSize) {
			return AnalysisResult.Corrupted;
		}

		// Convert data stream into string but still in hex form
		string dataString = BitConverter.ToString(data.Slice(0, HeaderSize).ToArray()).Replace("-", "\\x");

		// Pattern matching for GIF header, valid version number, and logical screen width/height
		string pattern = @"^GIF(87a|89a).{7}\x([0-9A-Fa-f]{2}){3}"; 
		if (Regex.IsMatch(dataString, pattern))
		{
			return AnalysisResult.Correct;
		}

		return AnalysisResult.Unrecognised;
	}
}