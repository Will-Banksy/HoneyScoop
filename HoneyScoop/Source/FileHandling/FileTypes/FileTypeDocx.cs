namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeDocx : IFileType {
	public string Header => @"\x50\x4B\x03\x04\x14\x00\x06\x00"; // Docx signature
	public string Footer => @"\x50\x4B\x05\x06"; // Docx footer, same as zip (EOCD)
	public bool HasFooter => true;
	
	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}