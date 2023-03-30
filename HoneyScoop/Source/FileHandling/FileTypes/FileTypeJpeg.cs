using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes {
	internal class FileTypeJpg : IFileType {
		public string Header => @"^\xFF\xD8"; // JPG signature
		public string Footer => @"\xFF\xD9"; // End of Image (EOI) marker

		private const int HeaderSize = 2;
		private const int FooterSize = 2;

		private readonly struct Segment {
			internal const byte StartByte = 0xFF;
			internal const byte EndByte = 0xD9;

			internal readonly byte Marker;
			internal readonly int Length;
			// internal readonly ReadOnlySpan<byte> Data;
			internal readonly bool IsValid;
			internal readonly int TotalLength;

			/// <summary>
			/// Construct a Segment, giving it a reference to the segment data, length, marker, and whether it is valid
			/// </summary>
			/// <param name="data"></param>
			/// <param name="length"></param>
			/// <param name="marker"></param>
			/// <param name="isValid"></param>
			private Segment(ReadOnlySpan<byte> data, int length, byte marker, bool isValid) {
				Marker = marker;
				Length = length;
				// Data = data;
				IsValid = isValid;
				TotalLength = length + 2;
			}

			/// <summary>
			/// Checks whether the segment data matches what is expected of the segment type.
			/// </summary>
			/// <returns>An <see cref="AnalysisResult"/> indicating how the segment data matches it's expectations</returns>
			internal AnalysisResult CheckDataValid() {
				if(!IsValid) {
					return AnalysisResult.Corrupted;
				}

				switch(Marker) {
					case 0xC0:
					case 0xC1:
					case 0xC2:
					case 0xC3:
					case 0xC5:
					case 0xC6:
					case 0xC7:
					case 0xC9:
					case 0xCA:
					case 0xCB:
					case 0xCD:
					case 0xCE:
					case 0xCF:
						return AnalysisResult.Correct;

					default:
						return AnalysisResult.FormatError;
				}
			}

			/// <summary>
			/// Parse a segment from the given data, starting at the given position
			/// </summary>
			/// <param name="data"></param>
			/// <param name="pos"></param>
			/// <returns>A new Segment or null if the data is corrupted</returns>
			internal static Segment? Parse(ReadOnlySpan<byte> data, int pos) {
				if(data[pos] != StartByte) {
					return null;
				}

				int marker = data[pos + 1];
				if(marker == EndByte) {
					// End of Image (EOI) marker
					return null;
				}

				if(marker == 0) {
					// Some editors use 0 as padding before the actual marker
					return null;
				}

				int length = (data[pos + 2] << 8) + data[pos + 3];
				if(length < 2 || length > data.Length - pos - 2) {
					return null;
				}

				ReadOnlySpan<byte> segmentData = data.Slice(pos + 4, length - 2);
				bool isValid = data[pos + length - 1] == EndByte;

				return new Segment(segmentData, length, (byte)marker, isValid);
			}
		}

		// TODO: finish this part
		public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
			var segments = new List<Segment>();
			int pos = HeaderSize;

			while(pos < data.Length - FooterSize) {
				var segment = Segment.Parse(data, pos);
				if(segment == null) {
					break;
				}

				segments.Add(segment.Value);
				pos += segment.Value.TotalLength;
			}

			bool isValid = segments.Any() && segments.All(s => s.CheckDataValid() == AnalysisResult.Correct);

			throw new NotImplementedException(); // Need to implement the FileInformation struct. Or just return an AnalysisResult which is what this method should return
			// return new FileInformation(FileType.Jpg, isValid, segments.Count, data.Length, HeaderSize, FooterSize);
		}
	}
}