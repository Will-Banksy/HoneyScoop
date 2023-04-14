using System;
using System.Linq;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeZip : IFileType {
		private static readonly byte[] ZipHeader = { 0x50, 0x4B, 0x03, 0x04 }; // Zip signature
        private static readonly byte[] ZipFooter = { 0x50, 0x4B, 0x05, 0x06 }; // Zip footer (EOCD)

        public string Header => @"\x50\x4B\x03\x04";
        public string Footer => @"\x50\x4B\x05\x06";
        public bool HasFooter => true;

        public AnalysisResult Analyse(ReadOnlySpan<byte> data)
        {
		
		try
    	 {
            // Check if data is long enough to contain both header and footer
            if (data.Length < ZipHeader.Length + ZipFooter.Length)
            {
                return AnalysisResult.Unrecognised;
            }

            // Check if header is present at the beginning of the data(starts at the first byte, reads the first sequence to see if it matches ZipHeader)
            if (!data.Slice(0, ZipHeader.Length).SequenceEqual(ZipHeader))
            {
                return AnalysisResult.Unrecognised;
            }

            // Check if footer is present at the end of the data(starts at the last part, reads the last sequence to see if it matches ZipFooter)
            if (!data.Slice(data.Length - ZipFooter.Length).SequenceEqual(ZipFooter))
            {
                return AnalysisResult.Unrecognised;
            }

			

			// Find the EOCD signature(finds last occurance of Zipfooter)
    		int eocdPosition = data.LastIndexOf(ZipFooter);

			//if the sequence <0 the Eocd doesn't exist
    		if (eocdPosition < 0)
    		{
        		return AnalysisResult.Unrecognised;
    		}

    		// Get the central directory position from the EOCD record(gets the 4 bytes after the first 16 and stores it)
    		int centralDirectoryPosition = BitConverter.ToInt32(data.Slice(eocdPosition + 16, 4).ToArray());

			 // Check central directory file headers(while the central directory position is less than the end (EOCD))
    		while (centralDirectoryPosition < eocdPosition)
    		{
				//reads the next 4 bytes and stores 
        		ReadOnlySpan<byte> centralDirectorySignatureSpan = data.Slice(centralDirectoryPosition, 4);
       			uint centralDirectorySignature = BitConverter.ToUInt32(centralDirectorySignatureSpan.ToArray());

        		// Central directory file header signature: 0x02014b50(checks if the signature matches)
        		if (centralDirectorySignature != 0x02014b50)
        		{
            		return AnalysisResult.Unrecognised;
        		}

       		 	// Read additional fields if necessary(probably isn't)
        		ushort centralDirectoryFileNameLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 28, 2).ToArray());
        		ushort centralDirectoryExtraFieldLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 30, 2).ToArray());
        		ushort centralDirectoryCommentLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 32, 2).ToArray());

        		// Move to the next central directory file header
        		centralDirectoryPosition += (46 + centralDirectoryFileNameLength + centralDirectoryExtraFieldLength + centralDirectoryCommentLength);
   			}

            return AnalysisResult.Correct;
         }
		 catch (Exception)
		 {
			return AnalysisResult.Unrecognised;
		 }
	
        }

		//TODO ADD IN CRC32 validation
		//TODO test for potential errors etc(unexpected zip formats maybe?)
}



	/*	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		throw new NotImplementedException();
	}*/
