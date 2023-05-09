using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeRar : IFileType {
	public string Header => @"\x52\x61\x72\x21\x1A\x07\x00";
	public string Footer => "";
	public bool HasFooter => false;
	public string FileExtension => "rar";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	private const int HeaderSize = 7; // The RAR header is 7 bytes long, including the signature and version information

	/// <param name="data">The stream of data bytes that get checked.</param>
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		// Check if data is at least the length of the header
		if(data.Length < HeaderSize) {
			return AnalysisResult.Corrupted.Wrap();
		}

		// Check if data stream starts with the RAR header signature
		if(data.Slice(0, HeaderSize).SequenceEqual(Encoding.ASCII.GetBytes(Header))) {
			return AnalysisResult.Correct.Wrap();
		}

		return AnalysisResult.Unrecognised.Wrap();
	}
}