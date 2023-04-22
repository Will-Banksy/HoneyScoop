using HoneyScoop.FileHandling;

namespace HoneyScoop.Carving;

internal class CarveFileInfo {
	internal readonly string Filename;
	internal readonly FileType FType;
	internal FileStream? OutputStream;

	internal CarveFileInfo(string filename, FileType fileType) {
		Filename = filename;
		FType = fileType;
		OutputStream = null;
	}
}