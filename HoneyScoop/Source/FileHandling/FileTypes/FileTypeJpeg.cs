namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeJpeg : IFileType { // That is a short header and footer is that definitely as specific as you can be
	public string Header => @"\xFF\xD8"; // JPEG header signature
	public string Footer => @"\xFF\xD9"; // JPEG footer signature

	public AnalysisResult Analyse(System.ReadOnlySpan<byte> data) {
		throw new System.NotImplementedException();
	}

	private const int HeaderSize = 2;
	private const int FooterSize = 2;
}