using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeGif : IFileType {
	public string Header => "GIF8(7|9)a";
	public string Footer => @"(\x00\x00\x3b)|(\xff\xd9)";
	public bool HasFooter => true;
	public string FileExtension => "gif";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	private const int HeaderSize = 13; // The GIF header is 13 bytes long, including the version number and logical screen width/height

	/// <summary>
	/// </summary>
	/// <param name="data">The stream of data bytes that get checked.</param>
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		// Check if data is at least the length of the header
		if(data.Length >= HeaderSize) {
			return AnalysisResult.Correct.Wrap();
		}

		return AnalysisResult.Unrecognised.Wrap();
	}
}