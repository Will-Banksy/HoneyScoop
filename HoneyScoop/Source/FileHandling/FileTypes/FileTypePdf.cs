namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePdf : IFileType {
	public string Header => @"\x25\x50\x44\x46\x2D"; // PDF signature
	public string Footer => @"(\x0A\x25\x25\x45\x4f\x46)|(\x0A\x25\x25\x45\x4F\x46\x0A)|(\x0D\x0A\x25\x25\x45\x4F\x46\x0D\x0A)|(\x0A\x25\x25\x45\x4F\x46\x0A)";
	public bool HasFooter => true;
	public string FileExtension => "pdf";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairLast;


	/// <summary>
	/// When implemented, compare header 
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>

	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}