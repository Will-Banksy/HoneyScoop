namespace HoneyScoop.FileHandling;

internal interface IFileType { // TODO: For some reason, it is an error to make members Header, Footer and Analyse anything but public
	internal Signature Header { get; }
	internal Signature Footer { get; }

	/// <summary>
	/// This method should analyse the data in fstream starting at headerSignature
	/// </summary>
	/// <param name="data">The data to analyse that contains the header signature and footer signature</param>
	/// <param name="headerSignature">The index of the start of the header signature</param>
	/// <param name="footerSignature">The index of the start of the footer signature</param>
	/// <returns></returns>
	internal float Analyse(byte[] data, ulong headerSignature, ulong? footerSignature);
}