using System;
using System.Linq;

//repurposed Zip file but with PPTX signatures, will need reworked 

namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypePptx : IFileType
    {
        private static readonly byte[] PptxHeader = { 0x50, 0x4B, 0x03, 0x04 }; // PPTX signature (same as ZIP)
        private static readonly byte[] PptxFooter = { 0x50, 0x4B, 0x05, 0x06 }; // PPTX footer (EOCD) (same as ZIP)

        public string Header => @"\x50\x4B\x03\x04";
        public string Footer => @"\x50\x4B\x05\x06";
        public bool HasFooter => true;
		public string FileExtension => "pptx";

        public AnalysisResult Analyse(ReadOnlySpan<byte> data)
        {
            try
            {
                // Check if data is long enough to contain both header and footer
                if (data.Length < PptxHeader.Length + PptxFooter.Length)
                {
                    return AnalysisResult.Unrecognised;
                }

                // Check if header is present at the beginning of the data
                if (!data.Slice(0, PptxHeader.Length).SequenceEqual(PptxHeader))
                {
                    return AnalysisResult.Unrecognised;
                }

                // Check if footer is present at the end of the data
                if (!data.Slice(data.Length - PptxFooter.Length).SequenceEqual(PptxFooter))
                {
                    return AnalysisResult.Unrecognised;
                }

                // Find the EOCD signature
                int eocdPosition = data.LastIndexOf(PptxFooter);

                if (eocdPosition < 0)
                {
                    return AnalysisResult.Unrecognised;
                }

                // Get the central directory position from the EOCD record
                int centralDirectoryPosition = BitConverter.ToInt32(data.Slice(eocdPosition + 16, 4).ToArray());

                // Check central directory file headers
                while (centralDirectoryPosition < eocdPosition)
                {
                    ReadOnlySpan<byte> centralDirectorySignatureSpan = data.Slice(centralDirectoryPosition, 4);
                    uint centralDirectorySignature = BitConverter.ToUInt32(centralDirectorySignatureSpan.ToArray());

                    // Central directory file header signature: 0x02014b50
                    if (centralDirectorySignature != 0x02014b50)
                    {
                        return AnalysisResult.Unrecognised;
                    }

                    // Read additional fields if necessary
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
    }
}