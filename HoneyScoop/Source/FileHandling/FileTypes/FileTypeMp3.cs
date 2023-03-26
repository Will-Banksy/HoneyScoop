namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeMp3 : IFileType {
	public string Header => @"\x49\x44\x33";
	public string Footer => ""; // Does not have a footer

	private const int TagIDSize = 3; // Part of header
	private const int MajorVersionNumberSize = 1; // Part of header
	private const int MinorVersionNumberSize = 1; // Usually just reserved
	private const int TagUsageSize = 1; // Usually just reserved
	private const int TagSizeSize = 4; // Size of the whole Tag

	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		// Check if data is longer than header and is present
		if(data.Length < (
			   TagIDSize +
			   MajorVersionNumberSize +
			   MinorVersionNumberSize +
			   TagUsageSize +
			   TagSizeSize)) {
			return AnalysisResult.Unrecognised;
		}

		ReadOnlySpan<byte> tagData = data.Slice(0, 10);
		ReadOnlySpan<byte> tagSizeBytes = tagData.Slice(6, 4);
		int tagSize = BitConverter.ToInt32(tagSizeBytes);

		// Check if the size of the data is not zero
		if(tagSize == 0) {
			return AnalysisResult.Corrupted;
		}

		// Check if the size of the data being examined is the same as
		// the file defined in the header
		if(data.Length != tagSize) {
			return AnalysisResult.Corrupted;
		}

		return AnalysisResult.Correct;
	}
}