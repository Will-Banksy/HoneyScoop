using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePng : IFileType {
	public string Header { get { return @"\x89\x50\x4e\x47\x0d\x0a\x1a\x0a"; } } // PNG signature
	public string Footer { get { return @"\x49\x45\x4e\x44"; } } // "IEND"

	private const int HeaderSize = 8;
	private const int FooterSize = 4;

	internal struct Ihdr { // 25 bytes total
		internal const int ExpectedSize = 25;
		
		internal uint ChunkSize = 0; // Should be 13
		internal uint ChunkType = 0;
		internal uint Width = 0;
		internal uint Height = 0;
		internal byte BitDepth = 0;
		internal byte ColourType = 0;
		internal byte CompressionMethod = 0;
		internal byte FilterMethod = 0;
		internal byte InterlaceMethod = 0;
		internal uint Crc = 0;

		internal uint ExpectedCrc = 0;
		
		public Ihdr() {
		}

		/// <summary>
		/// Creates an Ihdr instance from a stream of bytes starting from the start of the chunk (the chunk length) and extending at least the expected chunk length
		/// </summary>
		/// <param name="data"></param>
		internal static Ihdr DeserializeFrom(ReadOnlySpan<byte> data) {
			var ihdr = new Ihdr();
			ihdr.ChunkSize = Helper.FromBigEndian(data);
			ihdr.ChunkType = Helper.FromBigEndian(data[4..]);
			ihdr.Width = Helper.FromBigEndian(data[8..]);
			ihdr.Height = Helper.FromBigEndian(data[12..]);
			ihdr.BitDepth = data[16];
			ihdr.ColourType = data[17];
			ihdr.CompressionMethod = data[18];
			ihdr.FilterMethod = data[19];
			ihdr.InterlaceMethod = data[20];
			ihdr.Crc = Helper.FromBigEndian(data[21..]);
			// TODO: Calculate expected CRC
			return ihdr;
		}

		internal readonly bool IsAsExpected() {
			return ChunkSize == 13 &&
				   ChunkType == 1229472850 && // IHDR as uint
				   (BitDepth == 1 || BitDepth == 2 || BitDepth == 4 || BitDepth == 8 || BitDepth == 16) &&
				   (ColourType == 0 || ColourType == 2 || ColourType == 3 || ColourType == 4 || ColourType == 6) &&
				   CompressionMethod == 0 &&
				   FilterMethod == 0 &&
				   InterlaceMethod < 2;
		}
	}

	private struct Idat {
	}

	public float Analyse(ReadOnlySpan<byte> data) {
		if(data.Length < Ihdr.ExpectedSize) {
			return 0f;
		}

		Ihdr ihdr = Ihdr.DeserializeFrom(data[HeaderSize..]);
		if(!ihdr.IsAsExpected()) {
			return 0f;
		}

		return 1f;
	}
}