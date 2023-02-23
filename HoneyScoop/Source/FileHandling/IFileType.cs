namespace HoneyScoop.FileHandling;

internal interface IFileType {
	internal string Header { get; }
	internal string Footer { get; }

	/// <summary>
	/// This method should analyse the data in fstream starting at headerSignature
	/// </summary>
	/// <param name="data">The data to analyse that contains the header signature and footer signature</param>
	/// <param name="headerSignature">The index of the start of the header signature</param>
	/// <param name="footerSignature">The index of the start of the footer signature</param>
	/// <returns></returns>
	internal float Analyse(byte[] data, ulong headerSignature, ulong? footerSignature);
}