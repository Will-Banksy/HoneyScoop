using HoneyScoop.FileHandling.FileTypes;

namespace HoneyScoop.FileHandling; 

public enum FileType {
	Png,
	Jpg,
	Gif,
	Mp4,
	Mp3,
	Wav,
	Xlsx,
	Docx,
	Pptx,
	Pdf,
	Zip,
}

internal static class SupportedFileTypes {
	internal static Dictionary<FileType, IFileType> FileTypeHandlers = SetupSupportedFileTypes();

	private static Dictionary<FileType, IFileType> SetupSupportedFileTypes() {
		return new Dictionary<FileType, IFileType> {
			{ FileType.Png, new FileTypePng() }
		};
	}
}