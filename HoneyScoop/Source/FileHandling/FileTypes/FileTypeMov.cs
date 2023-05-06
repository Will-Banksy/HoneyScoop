using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes; 

internal class FileTypeMov : IFileType {
	public string Header => @"\x00\x00\x00\x20\x66\x74\x79\x70";
	public string Footer => ""; // Does not have a footer
	public bool HasFooter => false;
	public string FileExtension => "mov";
	public bool RequiresFooter => false;

	private const int BrandSize = 4;
		
	private static readonly byte[][] SupportedBrands = {
		new byte[] {0x71, 0x74, 0x20, 0x20}, // QuickTime Movie File
		new byte[] {0x61, 0x76, 0x63, 0x31}, // Advanced Video Coding (AVC) file format
		new byte[] {0x68, 0x65, 0x69, 0x63}, // High Efficiency Image Format (HEIF)
		new byte[] {0x68, 0x65, 0x69, 0x78}, // High Efficiency Image Format (HEIF) (Raw)
	};

	/// <summary>
	/// Checks if the header is of proper length and the first bytes are followed by bytes
	/// recognised as ones located in .movs. 
	/// </summary>
	/// <param name="data">The stream of data bytes that get checked.</param>
	/// <returns>Returns whether the conditions of a mov file are present or not.</returns>
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		// Check if data is longer than header and is present
		if(data.Length < Header.Length) {
			return AnalysisResult.Unrecognised.Wrap();
		}

		// Check if brand is present and supported
		ReadOnlySpan<byte> brandData = data.Slice(8, BrandSize);
		for (int i = 0; i < SupportedBrands.Length; i++)
		{
			if (brandData.SequenceEqual(SupportedBrands[i]))
			{
				return AnalysisResult.Correct.Wrap();
			}
		}

		return AnalysisResult.Unrecognised.Wrap();
	}
}