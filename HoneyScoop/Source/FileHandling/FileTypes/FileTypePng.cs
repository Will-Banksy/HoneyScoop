namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePng : IFileType {
	public Signature Header { get { return Signature.From(new byte[]{ 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }); } } // PNG signature
	public Signature Footer { get { return Signature.From(new byte[]{ 0x49, 0x45, 0x4e, 0x44 }); } } // "IEND"

	private struct PngFormat {
		// PNG fields
	}

	public float Analyse(byte[] data, ulong headerSignatureIdx, ulong? footerSignatureIdx) {
		return 1.0f;
	}
}