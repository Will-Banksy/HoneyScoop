using HoneyScoop.FileHandling;

namespace HoneyScoop.Carving; 

internal class CarveBufferManager {
	private byte[] _buffer;
	private int _chunkSize;
	private FileHandler _fileHandler;

	internal CarveBufferManager(FileHandler fileHandler, int chunkSize) {
		_chunkSize = chunkSize;
		_fileHandler = fileHandler;
		_buffer = new byte[_chunkSize * 2];
	}
}