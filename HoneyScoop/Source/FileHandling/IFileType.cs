namespace HoneyScoop.FileHandling;

/// <summary>
/// The interface that any file-type-specific file analysis classes/structs should implement.
/// Defines fields <see cref="Header"/>, <see cref="Footer"/> and function <see cref="Analyse"/>.
/// </summary>
internal interface IFileType {
	/// <summary>
	/// A regex string that matches the header(s) for a specific file type
	/// </summary>
	internal string Header { get; }
	
	/// <summary>
	/// A regex string that matches the footer(s) for a specific file type
	/// </summary>
	internal string Footer { get; }
	
	/// <summary>
	/// A boolean that returns whether the file type defines a footer.
	/// This should be false if <see cref="Footer"/> returns an empty string
	/// </summary>
	internal bool HasFooter { get; }
	
	/// <summary>
	/// Returns the (most common) file extension for this file type
	/// </summary>
	internal string FileExtension { get; }
	
	/// <summary>
	/// Returns whether this file should be carved if it was not matched with a footer
	/// </summary>
	internal bool RequiresFooter { get; }

	/// <summary>
	/// Implementations of this method should interpret the data as that of a specific file type, and check that the data does conform to the expectations of that file type,
	/// e.g. certain bytes being set to certain values, CRCs correct, lengths of data correct.
	/// </summary>
	/// <param name="data">The data to analyse, the first byte being the first byte of the header signature and the last byte the last byte of the footer signature</param>
	/// <returns>An <see cref="AnalysisResult"/> enum variant that describes broadly how the data matches the expected format</returns>
	internal (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data);
}

/// <summary>
/// The result of analysis of a block of data to attempt to determine whether it contains the contents of a specific file type.<br /><br />
/// It is important to note that each variant has a precedence, ascending in the order that they appear in the enum:
/// <list type="number">
///     <item><see cref="Correct"/> - Should be returned when <i>all</i> the data is correct</item>
///     <item><see cref="Partial"/> - Should be returned when all present data is Correct, but there is some data missing</item>
///     <item><see cref="FormatError"/> - Should be returned when the data is either Correct or Partial, but also has some errors that can't be put down to corruption</item>
///     <item><see cref="Corrupted"/> - Should be returned when the data is either Correct, Partial or FormatError but there are some errors within it that may be corruption</item>
///     <item><see cref="Unrecognised"/> - Should be returned when the data does not resemble the expected format. If other variants are found to apply to some parts of the data, then use a different, more appropriate variant</item>
/// </list>
/// See variant documentation for more details
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
	/// An example could be that field A is set to a value that is only valid when field B is set to a certain value and B is not set to that value, and this is indicated to be intended via a correct checksum.<br /><br />
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
	Unrecognised,
	/// <summary>
	/// The file has not yet been analysed
	/// </summary>
	Unanalysed
}

/// <summary>
/// A struct that contains file metadata obtained during analysis that is useful to the carving process.
/// As of writing, this may not be fully implemented 
/// </summary>
internal struct AnalysisFileInfo { // TODO: Any other relevant data? Maybe actual file offset? And then refine and use this
	// TODO: ooh idea what if for ZIP we actually just match the EOCD and use this to match the entire file? Would probably require another field in IFileType - MaxSize or SetSize or summin
	/// <summary>
	/// The actual size of the file, according to the internal metadata - Only set this if you are *sure*
	/// that it will be correct, but the carving process might ignore it if the analysis result is not <see cref="AnalysisResult.Correct"/> anyway.
	/// Set to -1 for this to be ignored
	/// </summary>
	internal long ActualFileSize;

	/// <summary>
	/// The offset that the file actually occurs at, according to the internal metadata - Only set this if you are *sure*
	/// that it will be correct, but the carving process might ignore it if the analysis result is not <see cref="AnalysisResult.Correct"/> anyway.
	/// Set to -1 for this to be ignored
	/// </summary>
	internal long ActualFileOffset;

	/// <summary>
	/// The actual type of the file, according to the internal metadata - Only set this if you are *sure*
	/// that it will be correct, but the carving process might ignore it if the analysis result is not <see cref="AnalysisResult.Correct"/> anyway.
	/// Set to <see cref="FileType.None"/> for this to be ignored
	/// </summary>
	internal FileType ActualFileType;

	internal AnalysisFileInfo(long actualFileSize = -1, long actualFileOffset = -1, FileType actualFileType = FileType.None) {
		ActualFileSize = actualFileSize;
		ActualFileOffset = actualFileOffset;
		ActualFileType = actualFileType;
	}
}