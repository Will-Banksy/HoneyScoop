using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes {
	internal class FileTypeMov : IFileType {
		public string Header => @"\x00\x00\x00\x14\x66\x74\x79\x70";
		public string Footer => ""; // Does not have a footer
		
		private const int BrandSize = 4;
		
		private static readonly byte[][] SupportedBrands = {
			new byte[] {0x71, 0x74, 0x20, 0x20}, // QuickTime Movie File
			new byte[] {0x6D, 0x70, 0x34, 0x31}, // MPEG-4 file format version 1
			new byte[] {0x6D, 0x70, 0x34, 0x32}, // MPEG-4 file format version 2
			new byte[] {0x69, 0x73, 0x6F, 0x6D}, // ISO base media file format
			new byte[] {0x6D, 0x34, 0x61, 0x20}, // MPEG-4 audio file format
			new byte[] {0x6D, 0x34, 0x76, 0x20}, // MPEG-4 video file format
			new byte[] {0x61, 0x76, 0x63, 0x31}, // Advanced Video Coding (AVC) file format
			new byte[] {0x68, 0x65, 0x69, 0x63}, // High Efficiency Image Format (HEIF)
			new byte[] {0x68, 0x65, 0x69, 0x78}, // High Efficiency Image Format (HEIF) (Raw)
		};


		public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
			// Check if data is longer than header and is present
			if(data.Length < Header.Length) {
				return AnalysisResult.Unrecognised;
			}

			// Check if brand is present and supported
			ReadOnlySpan<byte> brandData = data.Slice(8, BrandSize);
			for (int i = 0; i < SupportedBrands.Length; i++)
			{
				if (!brandData.SequenceEqual(SupportedBrands[i]))
				{
					return AnalysisResult.Unrecognised;
				}
			}

			return AnalysisResult.Correct;
		}
	}
}