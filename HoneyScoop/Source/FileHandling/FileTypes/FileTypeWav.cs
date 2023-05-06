using System;
using System.Text;
using System.Linq;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes; 
internal class FileTypeWav : IFileType {
	public string Header => "RIFF.{4}WAVEfmt "; 
	public string Footer => null; 
	public bool HasFooter => false;
	public string FileExtension => "wav";
	public bool RequiresFooter => false;
	
	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data)
        {

			//RIFF HEADER VALIDATIONS 

            // Check the RIFF Header (first 4 bytes)
            string riffHeader = Encoding.ASCII.GetString(data.Slice(0, 4));
            if (riffHeader != "RIFF")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            // Check Wave Chunk 
            string waveChunk = Encoding.ASCII.GetString(data.Slice(8, 4));
            if (waveChunk != "WAVE")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            // Check Format Chunk
            string formatChunk = Encoding.ASCII.GetString(data.Slice(12, 4));
            if (formatChunk != "fmt ")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

			//FORMAT VALIDATIONS TODO




			//DATA VALIDATIONS TODO


            return AnalysisResult.Correct.Wrap();
        }
}
