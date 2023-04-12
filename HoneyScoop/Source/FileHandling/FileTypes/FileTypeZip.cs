namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeZip : IFileType {
	public string Header => @"\x50\x4B\x03\x04"; // Zip signature
	public string Footer => @"\x50\x4B\x05\x06"; // Zip footer (EOCD)
	public bool HasFooter => true;
	
	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}