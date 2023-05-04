using System.Text;
using System.Text.RegularExpressions;
namespace HoneyScoop.FileHandling.FileTypes;


internal class FileTypeMp3 : IFileType {
	public string Header => @"\x49\x44\x33";
	public string Footer => ""; // Does not have a footer
	public bool HasFooter => false;
	public string FileExtension => "mp3";

	private const int TagIdSize = 3; // Part of header
	private const int MajorVersionNumberSize = 1; // Part of header
	private const int MinorVersionNumberSize = 1; // Usually just reserved
	private const int TagUsageSize = 1; // Usually just reserved
	private const int TagSizeSize = 4; // Size of the whole Tag
	
	/// <summary>
	/// Checks if proper header length is present and converts the datastream into stream,
	/// this then is checked with regex for an incomplete hex stream, which is 0000FFF or 0000FFE
	/// representing the Synchronisation Frame of an MP3 file.
	/// </summary>
	/// <param name="data">The stream of data bytes that get checked.</param>
	/// <returns>Returns whether the conditions of an mp3 file are present or not.</returns>
	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		// Check if data is longer than header and is present
		if(data.Length < (
			   TagIdSize +
			   MajorVersionNumberSize +
			   MinorVersionNumberSize +
			   TagUsageSize +
			   TagSizeSize)) {
			return AnalysisResult.Corrupted;
		}

		// Converts data stream into string but still in hex form
		// string dataString = BitConverter.ToString(data.ToArray()).Replace("-", "\\x");
		for (int i = 0; i < data.Length - 1; i++)
		{
			if (data[i] == 0xFF && (data[i+1] >> 4) == 0x0F || (data[i+1] >> 4) == 0x0E)
			{
				return AnalysisResult.Correct;
			}
		}


		return AnalysisResult.Unrecognised;
	}
}