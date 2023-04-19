using System.Collections.ObjectModel;
using HoneyScoop.FileHandling.FileTypes;

namespace HoneyScoop.FileHandling; 

/// <summary>
/// An enum that describes the type of a file
/// </summary>
internal enum FileType : short {
	None,
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

/// <summary>
/// An enum that describes either the <see cref="Header"/> or <see cref="Footer"/> of a file
/// </summary>
internal enum FilePart : short {
	None,
	Header,
	Footer
}

/// <summary>
/// A struct containing a <see cref="FileType"/> and <see cref="FilePart"/>
/// </summary>
internal readonly struct FileTypePart {
	internal readonly FileType Type = FileType.None;
	internal readonly FilePart Part = FilePart.None;

	internal FileTypePart(FileType type, FilePart part) {
		Type = type;
		Part = part;
	}

	internal bool Equals(FileTypePart other) {
		return Type == other.Type && Part == other.Part;
	}

	public override bool Equals(object? obj) {
		return obj is FileTypePart other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine((int)Type, (int)Part);
	}

	public static bool operator ==(FileTypePart a, FileTypePart b) {
		return a.Equals(b);
	}

	public static bool operator !=(FileTypePart a, FileTypePart b) {
		return !a.Equals(b);
	}
}

internal static class SupportedFileTypes {
	/// <summary>
	/// Contains a mapping from supported <see cref="FileType"/>s to their implementation
	/// </summary>
	internal static readonly ReadOnlyDictionary<FileType, IFileType> FileTypeHandlers = SetupSupportedFileTypes();

	private static ReadOnlyDictionary<FileType, IFileType> SetupSupportedFileTypes() {
		return new ReadOnlyDictionary<FileType, IFileType>(new Dictionary<FileType, IFileType>() {
			{ FileType.Png, new FileTypePng() }
		});
	}
}