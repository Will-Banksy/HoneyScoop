using HoneyScoop.FileHandling;
using HoneyScoop.Util;

namespace HoneyScoop.Carving;

internal class CarveBufferManager {
	private readonly byte[] _buffer;
	private readonly int _chunkSize;
	private readonly FileHandler _fileHandler;
	private int _chunkIndex = 0;

	/// <summary>
	/// Keeps track of the portions of the buffer that contain fresh data
	/// </summary>
	private List<IntRange> _loadedRanges; // TODO: Use this for optimisation on reads

	internal CarveBufferManager(FileHandler fileHandler, int chunkSize) {
		_chunkSize = chunkSize;
		_fileHandler = fileHandler;
		_buffer = new byte[_chunkSize * 2];
		_loadedRanges = new List<IntRange>();
	}

	/// <summary>
	/// Advances to the next chunk. If data loaded from the last chunk needs to be preserved, <see cref="shiftBuffer"/> should be true, otherwise false
	/// </summary>
	/// <param name="shiftBuffer"></param>
	internal void MoveNext(bool shiftBuffer) {
		_chunkIndex++;
		_loadedRanges.Clear();
		if(shiftBuffer) {
			Buffer.BlockCopy(_buffer, _chunkSize, _buffer, 0, _chunkSize);
		}
	}

	/// <summary>
	/// Loads the specified range of bytes from the file, or from memory if that data is already loaded
	/// </summary>
	/// <param name="start">The position from within the file to start reading from</param>
	/// <param name="stop">The position from within the file to stop reading at</param>
	/// <returns></returns>
	internal ReadOnlySpan<byte> Fetch(int start, int stop) {
		int chunkStart = _chunkIndex * _chunkSize;
		int chunkEnd = (_chunkIndex + 1) * _chunkSize - 1;

		// Ensure arguments are valid - I.e. within the current chunk and start <= stop
		if(start < chunkStart) {
			start = chunkStart;
		}

		if(stop > chunkEnd) {
			stop = chunkEnd;
		}

		if(start > stop) {
			throw new ArgumentException($"Attempting to fetch starting at a position greater than the end position (start: {start}, stop: {stop})");
		}

		// Get the offset from the start of the current chunk to start + _chunkSize (i.e. the position within the second half of the buffer to read into)
		int bufferStart = start - _chunkIndex * _chunkSize + _chunkSize;

		// Calculate the amount of bytes to read and create a span of the portion of _buffer to read into
		int readSize = stop - start;
		Span<byte> readTarget = _buffer.AsSpan(bufferStart, readSize);

		_fileHandler.Read(readTarget, start);

		return readTarget;
	}

	/// <summary>
	/// Returns the specified range of bytes (which must lie within the current chunk or the last chunk).
	/// Does not check that they are properly loaded
	/// </summary>
	/// <param name="start"></param>
	/// <param name="stop"></param>
	/// <returns></returns>
	internal ReadOnlySpan<byte> GetWithLast(int start, int stop) {
		int bufferStart = start - (_chunkIndex - 1) * _chunkSize;
		int length = stop - start;

		IntRange reqRange = new IntRange(bufferStart, bufferStart + length);
		IntRange bufferRange = new IntRange(0, _buffer.Length);
		if(!bufferRange.Contains(reqRange)) {
			throw new ArgumentOutOfRangeException();
		}

		return _buffer.AsSpan(bufferStart, length);
	}
}