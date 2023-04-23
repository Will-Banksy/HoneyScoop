using System.Text.RegularExpressions;
namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeJpeg : IFileType {
    public string Header => "\xFF\xD8";
    public string Footer => "\xFF\xD9"; // End of Image (EOI) marker
    public bool HasFooter => true;
    public string FileExtension => "jpg";

    private const int HeaderSize = 4; // The JPEG header is 4 bytes long, including the SOI marker and APP0 marker

    /// <param name="data">The stream of data bytes that get checked.</param>
    public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
        // Check if data is at least the length of the header
        if(data.Length < HeaderSize) {
            return AnalysisResult.Corrupted;
        }

        // Convert data stream into string but still in hex form
        string dataString = BitConverter.ToString(data.Slice(0, HeaderSize).ToArray()).Replace("-", "\\x");

        // Pattern matching for JPEG header, valid APP0 marker, and JFIF identifier
        string pattern = @"^\xFF\xD8\xFF\xE0\x00\x10\x4A\x46\x49\x46\x00\x01\x01\x01\x00\x60\x00\x60\x00\x00"; 
        if (Regex.IsMatch(dataString, pattern))
        {
            return AnalysisResult.Correct;
        }

        return AnalysisResult.Unrecognised;
    }
}