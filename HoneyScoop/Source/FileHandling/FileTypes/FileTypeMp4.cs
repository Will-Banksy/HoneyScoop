using System.Text;
namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypeMp4 : IFileType
    {
        public string Header => @"\x66\x74\x79\x70"; // "ftyp" in hexadecimal format

        // Does not have a footer

        private const int HeaderSize = 12; // Size of the header
        private const int BrandSize = 4; // Size of the brand field in the header

        // Supported brands in hexadecimal format
        private static readonly string[] SupportedBrands = { @"\x6D\x70\x34\x31", @"\x6D\x70\x34\x32", @"\x69\x73\x6F\x6D", @"\x69\x73\x6F\x32" };

        public AnalysisResult Analyse(ReadOnlySpan<byte> data)
        {
            // Check if data is longer than header and is present
            if (data.Length < HeaderSize)
            {
                return AnalysisResult.Unrecognised;
            }

            // Check if brand is supported
            ReadOnlySpan<byte> brandData = data.Slice(8, BrandSize);
            string brandString = Encoding.ASCII.GetString(brandData);
            if (!Array.Exists(SupportedBrands, brand => brand == brandString))
            {
                return AnalysisResult.Unrecognised;
            }

            // Check if data is not empty
            if (data.Length <= HeaderSize)
            {
                return AnalysisResult.Corrupted;
            }

            // Check if the file size is greater than the header size
            ReadOnlySpan<byte> sizeData = data.Slice(4, 4);
            int tagSize = BitConverter.ToInt32(sizeData);
            if (tagSize <= HeaderSize)
            {
                return AnalysisResult.Corrupted;
            }

            return AnalysisResult.Correct;
        }
    }
}