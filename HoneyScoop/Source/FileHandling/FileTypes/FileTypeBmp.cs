namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeBmp : IFileType {
	public string Header => @"\x42\x4D"; //  BMP signature
	public string Footer => @""; // No Footer Found
	public bool HasFooter => false;
	public string FileExtension => "bmp";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}