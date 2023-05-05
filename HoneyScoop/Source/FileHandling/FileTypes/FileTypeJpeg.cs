namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypeJpg : IFileType {
  
    public string Header => @"\xFF\xD8\xFF\xE0\x00\x10"; // JPG signature
    public string Footer => @"\xFF\xD9"; // End of Image (EOI) marker
    public bool HasFooter => true;
    public string FileExtension => "jpg";
    
    public AnalysisResult Analyse(ReadOnlySpan<byte> data)
    {
        var segments = new List<Segment>();
        int pos = HeaderSize;

        while (pos < data.Length - FooterSize)
        {
            var segment= Segment.Parse(data, pos);
            if (segment == null)
            {
                break;
            }

            segments.Add(segment.Value);
            pos += segment.Value.TotalLength;
        }

        bool isValid = segments.Any() && segments.All(s => s.CheckDataValid() == AnalysisResult.Correct);

        
        bool headerMatched = false;
        foreach (string header in Headers)
        {
            if (!data.Slice(0, header.Length).SequenceEqual(Encoding.ASCII.GetBytes(header))) continue;
            headerMatched = true;
            break;
        }

        if (headerMatched)
        {
            return isValid ? AnalysisResult.Correct : AnalysisResult.Corrupted;
        }
        else
        {
            return AnalysisResult.Unrecognised;
        }
    }

    private const int HeaderSize = 6;
    private const int FooterSize = 2;

    private readonly struct Segment {
        internal const byte StartByte = 0xFF;
        internal const byte EndByte = 0xD9;

        internal readonly byte Marker;

        internal readonly int Length;

        
        internal readonly bool IsValid;
        internal readonly int TotalLength;

      
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <param name="marker"></param>
        /// <param name="isValid"></param>
        private Segment(ReadOnlySpan<byte> data, int length, byte marker, bool isValid) {
            Marker = marker;
            Length = length;
            IsValid = isValid;
            TotalLength = length + 2;
        }
        

        /// <summary>
        /// Checks whether the segment data matches what is expected of the segment type.
        /// </summary>
        /// <returns>An <see cref="AnalysisResult"/> indicating how the segment data matches it's expectations</returns>
        internal AnalysisResult CheckDataValid() {
            if(!IsValid) {
                return AnalysisResult.Corrupted;
            }

            switch(Marker) {
                case 0xC0:
                case 0xC1:
                case 0xC2:
                case 0xC3:
                case 0xC5:
                case 0xC6:
                case 0xC7:
                case 0xC9:
                case 0xCA:
                case 0xCB:
                case 0xCD:
                case 0xCE:
                case 0xCF:
                    return AnalysisResult.Correct;

                default:
                    return AnalysisResult.FormatError;
            }
        }


        /// <param name="data"></param>
        /// <param name="pos"></param>

        internal static Segment? Parse(ReadOnlySpan<byte> data, int pos)
        {
            if (data[pos] != StartByte)
            {
                return null;
            }

            int marker = data[pos + 1];
            if (marker == EndByte)
            {
                // End of Image (EOI) marker
                return null;
            }

            if (marker == 0)
            {
                // Some editors use 0 as padding before the actual marker
                return null;
            }

            int length = (data[pos + 2] << 8) + data[pos + 3];
            if (length < 2 || length > data.Length - pos - 2)
            {
                return null;
            }

            ReadOnlySpan<byte> segmentData = data.Slice(pos + 4, length - 2);
            bool isValid = data[pos + length - 1] == EndByte;

            return new Segment(segmentData, length, (byte)marker, isValid);


            AnalysisResult Analyse(ReadOnlySpan<byte> data)
            {
                var segments = new List<Segment>();
                int pos = HeaderSize;

                while (pos < data.Length - FooterSize)
                {
                    var segment = Segment.Parse(data, pos);
                    if (segment == null)
                    {
                        break;
                    }

                    segments.Add(segment.Value);
                    pos += segment.Value.TotalLength;
                }

                bool isValid = segments.Any() && segments.All(s => s.CheckDataValid() == AnalysisResult.Correct);

                bool headerMatched = false;
                foreach (string header in Headers)
                {
                    if (!data.Slice(0, header.Length).SequenceEqual(Encoding.ASCII.GetBytes(header))) continue;
                    headerMatched = true;
                    break;
                }

                if (headerMatched)
                {
                    return isValid ? AnalysisResult.Correct : AnalysisResult.Corrupted;
                }
                else
                {
                    return AnalysisResult.Unrecognised;
                }
            }
        }
