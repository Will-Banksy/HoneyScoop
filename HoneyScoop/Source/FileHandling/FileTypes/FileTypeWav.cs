using System;
using System.Text;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes
{
    internal class FileTypeWav : IFileType
    {
        public string Header => "RIFF....WAVEfmt ";
        public string Footer => null;
        public bool HasFooter => false;
        public string FileExtension => "wav";
        public bool RequiresFooter => false;
		public PairingStrategy PairingMethod => PairingStrategy.PairNext;

        public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data)
        {
            // Read the first 4 bytes and convert them to an ASCII string to check the RIFF Header
            string riffHeader = Encoding.ASCII.GetString(data.Slice(0, 4));
            if (riffHeader != "RIFF")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            // Read bytes 8 to 11 and convert them to an ASCII string to check the Wave Chunk
            string waveChunk = Encoding.ASCII.GetString(data.Slice(8, 4));
            if (waveChunk != "WAVE")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            // Read bytes 12 to 15 and convert them to an ASCII string to check the Format Chunk
            string formatChunk = Encoding.ASCII.GetString(data.Slice(12, 4));
            if (formatChunk != "fmt ")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            // Read bytes 16 to 19 and convert them to an integer to get the Format Chunk Size
            int formatChunkSize = BitConverter.ToInt32(data.Slice(16, 4));

            // Read bytes 20 to 21 and convert them to an unsigned short to get the Audio Format
            ushort audioFormat = BitConverter.ToUInt16(data.Slice(20, 2));

            // Read bytes 22 to 23 and convert them to an unsigned short to get the Number of Channels
            ushort numberOfChannels = BitConverter.ToUInt16(data.Slice(22, 2));

            // Read bytes 24 to 27 and convert them to an integer to get the Sample Rate
            int sampleRate = BitConverter.ToInt32(data.Slice(24, 4));

            // Read bytes 34 to 35 and convert them to an unsigned short to get the Bits per Sample
            ushort bitsPerSample = BitConverter.ToUInt16(data.Slice(34, 2));

            // Read bytes after the Format Chunk (36 + formatChunkSize) and convert them to an ASCII string to check the Data Chunk
            string dataChunk = Encoding.ASCII.GetString(data.Slice(36 + formatChunkSize, 4));
            if (dataChunk != "data")
            {
                return AnalysisResult.Unrecognised.Wrap();
            }

            return AnalysisResult.Correct.Wrap();
        }
    }
}