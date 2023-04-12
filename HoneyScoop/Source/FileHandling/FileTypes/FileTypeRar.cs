namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeRar : IFileType {
	public string Header { get; }
	public string Footer { get; }
	public bool HasFooter { get; }
	
	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}
}