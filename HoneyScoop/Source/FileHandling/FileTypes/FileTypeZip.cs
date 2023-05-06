using System;
using System.Linq;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeZip : IFileType {
	private static readonly byte[] ZipHeader = { 0x50, 0x4B, 0x03, 0x04 }; // Zip signature
	private static readonly byte[] ZipFooter = { 0x50, 0x4B, 0x05, 0x06 }; // Zip footer (EOCD)

	internal static readonly string ZipHeaderRegex = @"\x50\x4B\x03\x04";
	internal static readonly string ZipFooterRegex = @"\x50\x4B\x05\x06.................."; // The 18 '.'s are for making sure that the whole EOCD record is matched

	public string Header => ZipHeaderRegex;
	public string Footer => ZipFooterRegex;
	public bool HasFooter => true;
	public string FileExtension => "zip";

	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		try {
			// Find the EOCD signature(finds last occurance of Zipfooter)
			int eocdPosition = data.LastIndexOf(ZipFooter);

			//if the sequence <0 the Eocd doesn't exist
			if(eocdPosition < 0) {
				return AnalysisResult.Unrecognised;
			}

			// Get the central directory position from the EOCD record(gets the 4 bytes after the first 16 and stores it)
			int centralDirectoryPosition = BitConverter.ToInt32(data.Slice(eocdPosition + 16, 4));

			// Check central directory file headers(while the central directory position is less than the end (EOCD))
			while(centralDirectoryPosition < eocdPosition) {
				//reads the next 4 bytes and stores 
				ReadOnlySpan<byte> centralDirectorySignatureSpan = data.Slice(centralDirectoryPosition, 4);
				uint centralDirectorySignature = BitConverter.ToUInt32(centralDirectorySignatureSpan);

				// Central directory file header signature: 0x02014b50(checks if the signature matches)
				if(centralDirectorySignature != 0x02014b50) {
					return AnalysisResult.Unrecognised;
				}

				// Read additional fields if necessary(probably isn't)
				ushort centralDirectoryFileNameLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 28, 2));
				ushort centralDirectoryExtraFieldLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 30, 2));
				ushort centralDirectoryCommentLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 32, 2));

				// Move to the next central directory file header
				centralDirectoryPosition += (46 + centralDirectoryFileNameLength + centralDirectoryExtraFieldLength + centralDirectoryCommentLength);
			}

			return AnalysisResult.Correct;
		} catch(Exception) {
			return AnalysisResult.Unrecognised;
		}
	}

	//TODO ADD IN CRC32 validation
	//TODO test for potential errors etc(unexpected zip formats maybe?)
}


/*	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
	throw new NotImplementedException();
}*/