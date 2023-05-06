using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes; 

internal class FileTypeMp4 : IFileType {
	public string Header => @"\x00\x00\x00\x20\x66\x74\x79\x70"; // "ftyp" in hexadecimal format
	public string Footer => ""; // Does not have a footer
	public bool HasFooter => false;
	public string FileExtension => "mp4";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	private const int HeaderSize = 12; // Size of the header
	private const int BrandSize = 4; // Size of the brand field in the header

	// Supported brands in hexadecimal format
	private static readonly byte[][] SupportedBrands = {
		new byte[] { 0x69, 0x73, 0x6F, 0x6D },
		new byte[] { 0x4D, 0x34, 0x41, 0x20 },
		new byte[] { 0x69, 0x73, 0x6F, 0x32 },
		new byte[] { 0x6D, 0x70, 0x34, 0x32 },
		new byte[] { 0x4D, 0x34, 0x56, 0x20 }
	};

	/// <summary>
	/// Checks the header sizes and the whether the proper 4 following bytes are (some) of
	/// the ones usually located in an mp4.
	/// </summary>
	/// <param name="data">The stream of data bytes that get checked.</param>
	/// <returns> Returns whether the conditions of an mp4 file are present or not.</returns>
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		// Check if data is longer than header and is present
		if(data.Length < HeaderSize) {
			return AnalysisResult.Unrecognised.Wrap();
		}

		// Check if brand is supported
		ReadOnlySpan<byte> brandData = data.Slice(8, BrandSize);
		for (int i = 0; i < SupportedBrands.Length; i++)
		{
			if (brandData.SequenceEqual(SupportedBrands[i]))
			{
				// Check if data is not empty
				if(data.Length <= HeaderSize) {
					return AnalysisResult.Corrupted.Wrap();
				}

				// Check if the file size is greater than the header size
				ReadOnlySpan<byte> sizeData = data.Slice(4, 4);
				int tagSize = BitConverter.ToInt32(sizeData);
				if(tagSize <= HeaderSize) {
					return AnalysisResult.Corrupted.Wrap();
				}

				return AnalysisResult.Correct.Wrap();
			}
		}

		return AnalysisResult.Unrecognised.Wrap();
	}
}