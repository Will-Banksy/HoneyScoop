using HoneyScoop.FileHandling;

namespace HoneyScoop.FileHandling.FileTypes;

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
		} //🐍
	}
}