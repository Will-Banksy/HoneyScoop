using System.Runtime.InteropServices;
using Force.Crc32;
using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePng : IFileType {
	public string Header { get { return @"\x89\x50\x4e\x47\x0d\x0a\x1a\x0a"; } } // PNG signature
	public string Footer { get { return @"\x49\x45\x4e\x44\xae\x42\x60\x82"; } } // "IEND" + CRC32 of "IEND"

	private const int HeaderSize = 8;
	private const int FooterSize = 8;
	private const int IhdrSize = 25;

	internal readonly ref struct Chunk {
		internal const uint TypeIhdr = 1229472850; // "IHDR" as uint
		internal const uint TypeIdat = 1229209940; // "IDAT" as uint
		internal const uint TypePlte = 1347179589; // "PLTE" as uint
		internal const uint TypeIend = 1229278788; // "IEND" as uint

		private const uint TypeIendCrc = 0xAE426082; // CRC of "IEND"

		private readonly uint _length;
		internal readonly uint Type;
		private readonly ReadOnlySpan<byte> _data;
		private readonly uint _crc;
		
		private readonly bool _isValid;
		internal readonly int TotalLength;

		private Chunk(uint length, uint type, ReadOnlySpan<byte> data, uint crc, uint calculatedCrc) {
			_length = length;
			Type = type;
			_data = data;
			_crc = crc;
			_isValid = crc == calculatedCrc;
			TotalLength = (int)_length + 12;
		}

		internal AnalysisResult CheckDataValid(out bool ihdrRequiresPlte, out bool ihdrPlteForbidden) {
			ihdrRequiresPlte = false;
			ihdrPlteForbidden = false;
			switch(Type) {
				case TypeIhdr: {
					// uint width = Helper.FromBigEndian(_data);
					// uint height = Helper.FromBigEndian(_data[4..]);
					byte bitDepth = _data[8];
					byte colourType = _data[9];
					byte compressionMethod = _data[10];
					byte filterMethod = _data[11];
					byte interlaceMethod = _data[12];
					if(colourType == 3) {
						ihdrRequiresPlte = true;
					} else if(colourType == 0 || colourType == 4) {
						ihdrPlteForbidden = true;
					}
					bool isExpectedValues = (bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8 || bitDepth == 16) &&
											(colourType == 0 || colourType == 2 || colourType == 3 || colourType == 4 || colourType == 6) && 
											compressionMethod == 0 && 
											filterMethod == 0 && 
											interlaceMethod < 2;
					if(isExpectedValues) {
						if(colourType == 2 && bitDepth != 8 && bitDepth != 16) {
							isExpectedValues = false;
						} else if(colourType == 3 && bitDepth != 1 && bitDepth != 2 && bitDepth != 4 && bitDepth != 8) {
							isExpectedValues = false;
						} else if((colourType == 4 || colourType == 6) && bitDepth != 8 && bitDepth != 16) {
							isExpectedValues = false;
						}
					}

					if(isExpectedValues && _isValid) {
						return AnalysisResult.Correct;
					} else if(_isValid) {
						return AnalysisResult.FormatError;
					} else {
						return AnalysisResult.Corrupted;
					}
				}
				
				case TypePlte: {
					bool correctLength = _length % 3 == 0;
					if(correctLength && _isValid) {
						return AnalysisResult.Correct;
					} else if(_isValid) {
						return AnalysisResult.FormatError;
					} else {
						return AnalysisResult.Corrupted;
					}
				}
			}

			return _isValid ? AnalysisResult.Correct : AnalysisResult.Corrupted;
		}

		internal static Chunk Decode(ReadOnlySpan<byte> data) {
			uint chunkLen = Helper.FromBigEndian(data);
			uint chunkType = Helper.FromBigEndian(data[4..]);
			// TODO: Optimise so as to avoid making copies - Perhaps fork Crc32.NET or take it's code and modify it to use spans, or derive my own Crc32 implementation
			// BUG: Using the commented code seems to allocate a LOT of memory...
			uint calcCrc = TypeIendCrc;// chunkType == TypeIend ? TypeIendCrc : Crc32Algorithm.Compute(data.Slice(4, ((int)chunkLen + 4)).ToArray());
			Chunk c = new Chunk(
				length: chunkLen,
				type: chunkType,
				data: data.Slice(8, (int)chunkLen),
				crc: Helper.FromBigEndian(data.Slice(8 + (int)chunkLen, 4)),
				calculatedCrc: calcCrc
			);
			return c;
		}
	}

	public AnalysisResult Analyse(ReadOnlySpan<byte> data) {
		if(data.Length < (HeaderSize + IhdrSize + FooterSize)) {
			return AnalysisResult.Unrecognised;
		}

		int offset = HeaderSize;

		Chunk ihdr = Chunk.Decode(data[offset..]);
		AnalysisResult ihdrAnalysisResult = ihdr.CheckDataValid(out bool requiresPlte, out bool plteForbidden);
		if(ihdr.Type != Chunk.TypeIhdr) {
			return AnalysisResult.Unrecognised;
		} else if(ihdrAnalysisResult == AnalysisResult.FormatError) {
			return AnalysisResult.FormatError;
		} else if(ihdrAnalysisResult == AnalysisResult.Corrupted) {
			return AnalysisResult.Corrupted;
		}

		offset += ihdr.TotalLength;

		bool hasIdat = false;
		bool hasPlte = false;

		while(true) {
			uint nextChunkLength = Helper.FromBigEndian(data[offset..]);
			if(data[offset..].Length < nextChunkLength + 12) {
				break;
			}

			Chunk chunk = Chunk.Decode(data[offset..]);
			if(chunk.Type == Chunk.TypePlte) {
				hasPlte = true;
			} else if(chunk.Type == Chunk.TypeIdat) {
				hasIdat = true;
			}
		}

		if(!hasIdat || (requiresPlte && !hasPlte)) {
			return AnalysisResult.Partial;
		} else if(plteForbidden && hasPlte) {
			return AnalysisResult.FormatError;
		}

		return AnalysisResult.Correct;
	}
}