namespace HoneyScoop.FileHandling;

internal interface IFileType {
	internal string Header { get; }
	internal string Footer { get; }

	/// <summary>
	/// Implementations of this method should interpret the data as that of a specific file type, and check that the data does conform to the expectations of that file type, e.g. certain bytes being set to certain values, CRCs correct, lengths of data correct
	/// </summary>
	/// <param name="data">The data to analyse, the first byte being the first byte of the header signature and the last byte the last byte of the footer signature</param>
	/// <returns>A floating-point number between 0-1 that represents the likelihood that the given range of bytes is a file of type specified by the override</returns>
	internal AnalysisResult Analyse(ReadOnlySpan<byte> data);
}

/// <summary>
/// The result of analysis of a block of data to attempt to determine whether it contains the contents of a specific file type
/// </summary>
internal enum AnalysisResult {
	/// <summary>
	/// The data matches that of the expectations of the specific file type, and all data required by the file type is present
	/// </summary>
	Correct,
	/// <summary>
	/// The data matches that of the expectations of the specific file type, but some required data is missing.<br /><br />
	/// An example of this is a PNG file where the image metadata is present, but the colour type requires a palette that is not present in the data, or the actual image data is missing.
	/// </summary>
	Partial,
	/// <summary>
	/// The data mostly matches that of the expectations of the specific file type, but there are some errors within the format that are verified by a checksum.<br /><br />
	/// An example could be that field A is set to a value that is only valid when field B is set to a certain value and B is not set to that value, and this is intended via a correct checksum.<br /><br />
	/// It is important to note that this should only be returned when the format error can not be put down to data corruption.
	/// </summary>
	FormatError,
	/// <summary>
	/// Parts of the data are recognised as conforming to the specific file type, but there are integrity problems or errors within the file such that the data cannot be verified to be readable.<br /><br />
	/// An example of this could be, a section of data has a CRC checksum to validate integrity, and upon computing a checksum of the data it is found to not match the checksum that is recorded within the data.
	/// </summary>
	Corrupted,
	/// <summary>
	/// The data is not recognised to conform to the expectations of the specific file type - It most likely does not contain the contents of a file of that type.
	/// When saying some data is "Unrecognised" as belonging to a specific file type, this does not take headers/footers into account as file type indicators.
	/// </summary>
	Unrecognised
}