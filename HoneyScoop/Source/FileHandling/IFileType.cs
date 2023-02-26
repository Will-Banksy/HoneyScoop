namespace HoneyScoop.FileHandling;

internal interface IFileType {
	internal string Header { get; }
	internal string Footer { get; }

	/// <summary>
	/// Implementations of this method should interpret the data as that of a specific file type, and check that the data does conform to the expectations of that file type, e.g. certain bytes being set to certain values, CRCs correct, lengths of data correct
	/// </summary>
	/// <param name="data">The data to analyse, the first byte being the first byte of the header signature and the last byte the last byte of the footer signature</param>
	/// <returns>A floating-point number between 0-1 that represents the likelihood that the given range of bytes is a file of type specified by the override</returns>
	internal float Analyse(ReadOnlySpan<byte> data);
}