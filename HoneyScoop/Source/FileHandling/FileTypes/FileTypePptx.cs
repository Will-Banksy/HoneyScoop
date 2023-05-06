using System;
using System.Linq;

//repurposed Zip file but with PPTX signatures, will need reworked 

namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypePptx : IFileType
    {
        private static readonly byte[] PptxHeader = { 0x50, 0x4B, 0x03, 0x04 }; // PPTX signature (same as ZIP)
        private static readonly byte[] PptxFooter = { 0x50, 0x4B, 0x05, 0x06 }; // PPTX footer (EOCD) (same as ZIP)

		public string Header => FileTypeZip.ZipHeaderRegex;
		public string Footer => FileTypeZip.ZipFooterRegex;
        public bool HasFooter => true;
		public string FileExtension => "pptx";

        public AnalysisResult Analyse(ReadOnlySpan<byte> data)
        {
            try
            {
                // Find the EOCD signature
                int eocdPosition = data.LastIndexOf(PptxFooter);
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

				    // Read additional fields if necessary
				    ushort centralDirectoryFileNameLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 28, 2));
				    ushort centralDirectoryExtraFieldLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 30, 2));
				    ushort centralDirectoryCommentLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 32, 2));

                    // Read the compressed and uncompressed sizes from the local file header
            	    int compressedSize = BitConverter.ToInt32(data.Slice(centralDirectoryFileNameLength + 18, 4));
           		    int uncompressedSize = BitConverter.ToInt32(data.Slice(centralDirectoryFileNameLength + 22, 4));

				
				    // Read the stored CRC value from the local file header
           		    uint storedCRC = BitConverter.ToUInt32(data.Slice(centralDirectoryFileNameLength + 14, 4));
            	    // Calculate the start and length of the file data within the data span
            	    ReadOnlySpan<byte> fileData = data.Slice(centralDirectoryCommentLength + 30 + centralDirectoryFileNameLength + centralDirectoryExtraFieldLength, compressedSize);

                    // Initialize the CRC calculation
				    uint crc = 0xFFFFFFFF;

				    // Iterate over each byte in the file data and update the CRC value
				    foreach (byte b in fileData)
				    {
    				    // XOR the current byte with the CRC value
    				    crc ^= b;

    				    // Update the CRC value based on the current byte's bits
    				    for (int i = 0; i < 8; i++)
    				        {
        				    // Shift the CRC value right by one bit and XOR it with a polynomial value
        				    // if the least significant bit is 1 (0xEDB88320 is the polynomial representation)
        				    crc = (crc >> 1) ^ (0xEDB88320 & ~((crc & 1) - 1));
    				    }
				    }
                    // Invert the CRC value to obtain the final calculated CRC
				    uint calculatedCRC = ~crc;

				    /* Compare the stored CRC value with the calculated CRC value
		 		    If they don't match, the file is considered corrupted*/

				    if (storedCRC != calculatedCRC)
				    {
   		 			    return AnalysisResult.Corrupted;
				    }


				    // Move to the next central directory file header
				    centralDirectoryPosition += (46 + centralDirectoryFileNameLength + centralDirectoryExtraFieldLength + centralDirectoryCommentLength);

                }

			    return AnalysisResult.Correct;
		    } catch(Exception) {
			    return AnalysisResult.Unrecognised;
		    }
	    }
    }
}