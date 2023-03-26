using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes {
	internal class FileTypeMov : IFileType {
		public string Header => @"\x00\x00\x00\x14";
		public string Footer => ""; // Does not have a footer
		
		private const int BrandSize = 4;
		
		private static readonly string[] SupportedBrands = {
			@"\x71\x74\x20\x20", // QuickTime Movie File
			@"\x6D\x70\x34\x31", // MPEG-4 file format version 1
			@"\x6D\x70\x34\x32", // MPEG-4 file format version 2
			@"\x69\x73\x6F\x6D", // ISO base media file format
			@"\x6D\x34\x61\x20", // MPEG-4 audio file format
			@"\x6D\x34\x76\x20", // MPEG-4 video file format
			@"\x61\x76\x63\x31", // Advanced Video Coding (AVC) file format
			@"\x68\x65\x69\x63", // High Efficiency Image Format (HEIF)
			@"\x68\x65\x69\x78", // High Efficiency Image Format (HEIF) (Raw)
		};

		public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
			// Check if data is longer than header and is present
			if(data.Length < Header.Length) {
				return AnalysisResult.Unrecognised;
			}

			// Check if brand is present and supported
			ReadOnlySpan<byte> brandData = data.Slice(8, BrandSize);
			string brandString = Encoding.ASCII.GetString(brandData);
			if(!Array.Exists(SupportedBrands, brand => brand == brandString)) {
				return AnalysisResult.Unrecognised;
			}

			return AnalysisResult.Correct;
		}
	}
}