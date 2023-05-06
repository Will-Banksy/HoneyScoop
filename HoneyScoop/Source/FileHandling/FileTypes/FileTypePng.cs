using HoneyScoop.Util;

namespace HoneyScoop.FileHandling.FileTypes;

internal class FileTypePng : IFileType {
	public string Header => @"\x89\x50\x4e\x47\x0d\x0a\x1a\x0a"; // PNG signature
	public string Footer => @"\x49\x45\x4e\x44\xae\x42\x60\x82"; // "IEND" + CRC32 of "IEND"
	public bool HasFooter => true;
	public string FileExtension => "png";
	public bool RequiresFooter => false;
	public PairingStrategy PairingMethod => PairingStrategy.PairNext;

	private const int HeaderSize = 8;
	private const int FooterSize = 8;
	private const int IhdrSizeTotal = 25;

	private readonly ref struct Chunk {
		internal const uint TypeIhdr = 0x49484452; // "IHDR" as uint
		internal const uint TypeIdat = 0x49444154; // "IDAT" as uint
		internal const uint TypePlte = 0x504C5445; // "PLTE" as uint
		internal const uint TypeIend = 0x49454E44; // "IEND" as uint

		internal const uint TypeIhdrLength = 13;
		private const uint TypeIendCrc = 0xAE426082; // CRC of "IEND"

		internal readonly uint Length;
		internal readonly uint Type;
		private readonly ReadOnlySpan<byte> _data;
		private readonly uint _crc;
		
		private readonly bool _isValid;
		internal readonly int TotalLength;
		internal readonly uint AdjustedLength;

		/// <summary>
		/// Construct a Chunk, giving it a reference to the chunk data + all the other decoded info & calculated CRC
		/// </summary>
		/// <param name="length"></param>
		/// <param name="type"></param>
		/// <param name="data"></param>
		/// <param name="crc"></param>
		/// <param name="calculatedCrc"></param>
		/// <param name="adjustedLength">If the decoding process needed to shrink the chunk data, this will be set to the new value</param>
		private Chunk(uint length, uint type, ReadOnlySpan<byte> data, uint crc, uint calculatedCrc, uint? adjustedLength = null) {
			Length = length;
			Type = type;
			_data = data;
			_crc = crc;
			_isValid = crc == calculatedCrc;
			TotalLength = (int)Length + 12;
			AdjustedLength = adjustedLength ?? length;
		}

		/// <summary>
		/// Checks whether the chunk data matches what is expected of the chunk type.
		/// The chunk type is taken from the <see cref="Type"/> field or assumedType argument (if not null)
		/// </summary>
		/// <param name="ihdrRequiresPlte">Returns whether this IHDR chunk data indicates that there should be a PLTE chunk</param>
		/// <param name="ihdrPlteForbidden">Returns whether this IHDR chunk data indicates that there should <i>not</i> be a PLTE chunk</param>
		/// <param name="assumedType">If not null, is used instead of the chunk type</param>
		/// <returns>An <see cref="AnalysisResult"/> indicating how the chunk data matches it's expectations</returns>
		internal AnalysisResult CheckDataValid(out bool ihdrRequiresPlte, out bool ihdrPlteForbidden, uint? assumedType = null) {
			ihdrRequiresPlte = false;
			ihdrPlteForbidden = false;
			if(Length != AdjustedLength) {
				return AnalysisResult.Corrupted;
			}
			switch(assumedType ?? Type) {
				case TypeIhdr: {
					// uint width = Helper.FromBigEndian(_data);
					// uint height = Helper.FromBigEndian(_data[4..]);
					if(_data.Length < TypeIhdrLength) {
						return AnalysisResult.Unrecognised;
					}
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
					bool correctLength = Length % 3 == 0;
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

		/// <summary>
		/// Decodes/deserialises a <see cref="Chunk"/> from a raw byte stream, calculating a CRC of the chunk type and data as it does so
		/// </summary>
		/// <param name="data">The raw byte stream</param>
		/// <returns>The decoded <see cref="Chunk"/></returns>
		internal static Chunk Decode(ReadOnlySpan<byte> data) {
			uint chunkLen = Helper.FromBigEndian(data);
			uint chunkType = Helper.FromBigEndian(data[4..]);
			uint? adjustedLen = chunkLen > data.Length - 12 ? (uint)data.Length - 12 : null;
			uint calcCrc = chunkType == TypeIend ? TypeIendCrc : Crc32.CalculateCrc32(data.Slice(4, ((int)chunkLen + 4)));
			Chunk c = new Chunk(
				length: chunkLen,
				type: chunkType,
				data: data.Slice(8, (int)chunkLen),
				crc: Helper.FromBigEndian(data.Slice(8 + (int)chunkLen, 4)),
				calculatedCrc: calcCrc,
				adjustedLength: adjustedLen
			);
			return c;
		}
	}

	public (AnalysisResult, AnalysisFileInfo) Analyse(ReadOnlySpan<byte> data) {
		if(data.Length < (HeaderSize + IhdrSizeTotal + FooterSize)) {
			return AnalysisResult.Unrecognised.Wrap(); // If the data is too small, return Unrecognised
		}

		int offset = HeaderSize; // Initialise the offset to the header size

		var ret = AnalysisResult.Correct;

		// Decode the first chunk. This should be IHDR
		Chunk ihdr = Chunk.Decode(data[offset..]);
		bool isIhdr = ihdr.Type == Chunk.TypeIhdr;
		bool correctLength = ihdr.Length == Chunk.TypeIhdrLength;
		
		// Perform the chunk data analysis, forcing the chunk to be analysed as an IHDR chunk
		AnalysisResult ihdrAnalysisResult = ihdr.CheckDataValid(out bool requiresPlte, out bool plteForbidden, assumedType: Chunk.TypeIhdr);
		ret = ret.UpdateResultWith(ihdrAnalysisResult); // Update the analysis result
		
		// If the chunk is not an IHDR chunk, does not have the correct length, and the chunk data is evaluated to be either errored or corrupted,
		// then it is most likely that this data is not a PNG image
		if(!isIhdr && !correctLength && (ihdrAnalysisResult == AnalysisResult.FormatError || ihdrAnalysisResult == AnalysisResult.Corrupted)) {
			return AnalysisResult.Unrecognised.Wrap(); // Return as no further updates can happen to ret
		}

		if(ret == AnalysisResult.Corrupted) {
			return ret.Wrap(); // Return as no further updates can happen to ret
		}

		offset += ihdr.TotalLength;

		bool hasIdat = false;
		bool hasPlte = false;
		uint prevChunkType = ihdr.Type;

		while(true) {
			if(data[offset..].Length <= 4) {
				break;
			}
			uint nextChunkLength = Helper.FromBigEndian(data[offset..]);
			if(data[offset..].Length < nextChunkLength + 12) {
				break;
			}

			Chunk chunk = Chunk.Decode(data[offset..]);
			if(chunk.Type == Chunk.TypePlte) {
				hasPlte = true;
			} else if(chunk.Type == Chunk.TypeIdat) {
				if(hasIdat && prevChunkType != Chunk.TypeIdat) {
					ret = ret.UpdateResultWith(AnalysisResult.FormatError);
				}
				hasIdat = true;
			}

			var chunkRes = chunk.CheckDataValid(out bool _, out bool _);
			switch(chunkRes) {
				case AnalysisResult.FormatError:
					ret = ret.UpdateResultWith(AnalysisResult.FormatError);
					break;
				case AnalysisResult.Corrupted:
					ret = ret.UpdateResultWith(AnalysisResult.Corrupted);
					break;
			}

			prevChunkType = chunk.Type;
			offset += chunk.TotalLength;

			// If ret is corrupted, then return, as no further updates are possible
			if(ret == AnalysisResult.Corrupted) {
				return ret.Wrap();
			}
		}

		if(!hasIdat || (requiresPlte && !hasPlte)) {
			ret = ret.UpdateResultWith(AnalysisResult.Partial);
		}
		if(plteForbidden && hasPlte) {
			ret = ret.UpdateResultWith(AnalysisResult.FormatError);
		}

		return ret.Wrap();
	}
}