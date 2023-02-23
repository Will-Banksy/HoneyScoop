namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePng : IFileType {
	public string Header { get { return @"\x89\x50\x4e\x47\x0d\x0a\x1a\x0a"; } } // PNG signature
	public string Footer { get { return @"\x49\x45\x4e\x44"; } } // "IEND"

	private struct PngFormat {
		// PNG fields
	}

	public float Analyse(byte[] data, ulong headerSignatureIdx, ulong? footerSignatureIdx) {
		return 1.0f;
	}
}