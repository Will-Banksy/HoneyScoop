using HoneyScoop.FileHandling;

namespace HoneyScoop.Carving; 

internal class CarveBufferManager {
	private readonly byte[] _buffer;
	private readonly int _chunkSize;
	private readonly FileHandler _fileHandler;
	private int _chunkIndex = 0;

	internal CarveBufferManager(FileHandler fileHandler, int chunkSize) {
		_chunkSize = chunkSize;
		_fileHandler = fileHandler;
		_buffer = new byte[_chunkSize * 2];
	}

	internal void JumpTo(int chunkIndex) {
		_chunkIndex = chunkIndex;
	}

	/// <summary>
	/// Loads the specified range of bytes from the file 
	/// </summary>
	/// <param name="start">The position from within the file to start reading from</param>
	/// <param name="stop">The position from within the file to stop reading at</param>
	/// <returns></returns>
	internal ReadOnlySpan<byte> Load(int start, int stop) {
		// Ensure arguments are valid - I.e. within the current chunk and start <= stop
		if(start < _chunkIndex) {
			start = _chunkIndex;
		}
		if(stop > _chunkIndex + _chunkSize) {
			stop = _chunkIndex + _chunkSize - 1;
		}
		if(start > stop) {
			throw new ArgumentException();
		}

		int bufferStart = start - (_chunkIndex * _chunkSize);
		
		_fileHandler.Read(_buffer, _chunkSize, bufferStart, start, stop);

		throw new NotImplementedException(); // TODO: Verify that this is complete
	}
}