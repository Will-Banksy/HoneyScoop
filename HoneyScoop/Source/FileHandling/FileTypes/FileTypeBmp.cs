namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeBmp : IFileType {
	public string Header => @"\x42\x4D"; //  BMP signature
	public string Footer => @""; // No Footer Found
	public bool HasFooter => false;
	public string FileExtension => "bmp";

	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}