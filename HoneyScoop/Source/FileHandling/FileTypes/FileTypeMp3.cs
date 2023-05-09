using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeMp3 : IFileType {
	public string Header => @"(\x49\x44\x33)|(\xFF\xFB\x44\x00\x00)|(\x57\x41\x56\x45)"; // Other headers but require testing due to unique nature of MP3
	public string Footer => "";
	public bool HasFooter => false;
	public string FileExtension => "mp3";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	private const int TagIdSize = 3; // Part of header
	private const int MajorVersionNumberSize = 1; // Part of header
	private const int MinorVersionNumberSize = 1; // Usually just reserved
	private const int TagUsageSize = 1; // Usually just reserved
	private const int TagSizeSize = 4; // Size of the whole Tag

	/// <summary>
	/// Checks if proper header length is present and converts the datastream into stream,
	/// this then is checked with bitwise operations for an incomplete hex stream, which is FFF or FFE (or FF0F, FF0E using >> 4)
	/// representing the Synchronisation Frame of an MP3 file.
	/// </summary>
	/// <param name="data">The stream of data bytes that get checked.</param>
	/// <returns>Returns whether the conditions of an mp3 file are present or not.</returns>
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		// Check if data is longer than header and is present
		if(data.Length < (
			   TagIdSize +
			   MajorVersionNumberSize +
			   MinorVersionNumberSize +
			   TagUsageSize +
			   TagSizeSize)) {
			return AnalysisResult.Corrupted.Wrap();
		}

		// Bitwise checking for FFF or FFE Synchronisation frame.
		for(int i = 0; i < data.Length - 1; i++) {
			if(data[i] == 0xFF && (data[i + 1] >> 4) == 0x0F || (data[i + 1] >> 4) == 0x0E) {
				return AnalysisResult.Correct.Wrap();
			}
		}


		return AnalysisResult.Unrecognised.Wrap();
	}
}