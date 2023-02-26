namespace HoneyScoop.FileHandling;

internal interface IFileType {
	internal string Header { get; }
	internal string Footer { get; }

	/// <summary>
	/// This method should analyse the data in fstream starting at headerSignature
	/// </summary>
	/// <param name="data">The data to analyse, the first byte being the first byte of the header signature and the last byte the last byte of the footer signature</param>
	/// <returns>A floating-point number between 0-1 that represents the likelihood that the given range of bytes is a file of type specified by the override</returns>
	internal float Analyse(ReadOnlySpan<byte> data);
}