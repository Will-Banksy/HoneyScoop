namespace HoneyScoop.FileHandling.FileTypes; 

internal class FileTypeWav : IFileType {
	public string Header => @"\x25\x50\x44\x46\x2D"; // PDF signature
	public string Footer => @"\x52\x49\x46\x46\x00\x00\x00\x00\x57\x41\x56\x45\x66\x6D\x74\x20"; // The \x00\x00\x00\x00 is a place holder as file size should be located there
	public bool HasFooter => true;
	public string FileExtension => "wav";
	public bool RequiresFooter => false;
	
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}