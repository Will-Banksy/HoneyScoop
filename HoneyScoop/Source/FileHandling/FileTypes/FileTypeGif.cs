namespace HoneyScoop.FileHandling.FileTypes;

// TODO: Write the header and footer signatures using Regex e.g. \x00\x00 instead of 00 and \x47\x49\x46\x08 instead of GIF8
// Also look at this: https://en.wikipedia.org/wiki/GIF#File_format
// (Can use regex with the header to match either GIF87a or GIF89a and do some more research on the footer cause 0x0000 is likely to come up a lot so if that is the only viable to use footer that's an issue)
internal class FileTypeGif : IFileType {
	public string Header => "GIF8"; // GIF signature
	public string Footer => "00"; // Trailer byte (hex value "00")

	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}

	private const int HeaderSize = 3;
	private const int FooterSize = 1;

	private readonly ref struct Block {
		internal const byte ExtensionIntroducer = 0x21;
		internal const byte GraphicControlLabel = 0xF9;
		internal const byte ImageDescriptorLabel = 0x2C;
		internal const byte Trailer = 0x3B;

		internal readonly byte BlockType;
		internal readonly ReadOnlySpan<byte> Data;

		internal Block(byte blockType, ReadOnlySpan<byte> data) {
			BlockType = blockType;
			Data = data;
		}
	}
}