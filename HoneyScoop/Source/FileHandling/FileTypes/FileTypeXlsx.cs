using System;
using System.Linq;

//repurposed Zip file but with PPTX signatures, will need reworked 

namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypeXlsx : IFileType
    {
        private static readonly byte[] XlsxHeader = { 0x50, 0x4B, 0x03, 0x04 }; // XLSX signature (same as ZIP)
        private static readonly byte[] XlsxFooter = { 0x50, 0x4B, 0x05, 0x06 }; // XLSX footer (EOCD) (same as ZIP)

		public string Header => FileTypeZip.ZipHeaderRegex;
		public string Footer => FileTypeZip.ZipFooterRegex;
        public bool HasFooter => true;
		public string FileExtension => "xlsx";

        public AnalysisResult Analyse(ReadOnlySpan<byte> data)
        {
            try
            {
                // Find the EOCD signature
                int eocdPosition = data.LastIndexOf(XlsxFooter);

                if (eocdPosition < 0)
                {
                    return AnalysisResult.Unrecognised;
                }

                // Get the central directory position from the EOCD record
                int centralDirectoryPosition = BitConverter.ToInt32(data.Slice(eocdPosition + 16, 4));

                // Check central directory file headers
                while (centralDirectoryPosition < eocdPosition)
                {
                    ReadOnlySpan<byte> centralDirectorySignatureSpan = data.Slice(centralDirectoryPosition, 4);
                    uint centralDirectorySignature = BitConverter.ToUInt32(centralDirectorySignatureSpan);

                    // Central directory file header signature: 0x02014b50
                    if (centralDirectorySignature != 0x02014b50)
                    {
                        return AnalysisResult.Unrecognised;
                    }

                    // Read additional fields if necessary
                    ushort centralDirectoryFileNameLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 28, 2));
                    ushort centralDirectoryExtraFieldLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 30, 2));
                    ushort centralDirectoryCommentLength = BitConverter.ToUInt16(data.Slice(centralDirectoryPosition + 32, 2));

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
    }
}