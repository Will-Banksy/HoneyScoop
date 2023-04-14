namespace HoneyScoop.FileHandling;

internal class FileHandler {
	internal const int DefaultBufferSize = 1024 * 1024 * 2; // default buffer size in bytes (2 MB)

	private readonly FileStream _fStream;
	internal readonly byte[] Buffer;
	internal long CurrentPosition = 0;
	private bool _eof;
	internal bool Eof => _eof;

	/// <summary>
	/// Constructs a new FileHandler object, opening the specified file for processing
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="bufferSize"></param>
	internal FileHandler(string filePath, int bufferSize = DefaultBufferSize) {
		_fStream = File.OpenRead(filePath);
		Buffer = new byte[bufferSize];
		_eof = false;
	}

	/// <summary>
	/// Return the next range of bytes as a read only span
	/// </summary>
	/// <returns></returns>
	internal ReadOnlySpan<byte> Next() {
		if(_eof) {
			throw new InvalidOperationException();
		}
		
		_fStream.Seek(CurrentPosition, SeekOrigin.Begin); // Set the stream position to the last position
		int bytesRead = _fStream.Read(Buffer, 0, Buffer.Length); // read up to the set buffer position from the current position

		if(bytesRead == 0) {
			_eof = true;
		}
		
		CurrentPosition += bytesRead; // Update the current position
		return Buffer.AsSpan(..bytesRead); // Return the updated buffer
	}

	/// <summary>
	/// Read a specific range of bytes from within the current chunk and return in a span
	/// </summary>
	/// <param name="start"></param>
	/// <param name="stop"></param>
	/// <returns></returns>
	internal ReadOnlySpan<byte> Read(long? start = null, long? stop = null) {
		long istart = start.GetValueOrDefault(CurrentPosition);
		long istop = stop.GetValueOrDefault(CurrentPosition + Buffer.Length);

		if(istart < CurrentPosition) {
			istart = CurrentPosition;
		}
		if(istop > CurrentPosition + Buffer.Length) {
			istop = CurrentPosition + Buffer.Length;
		}

		if(istart > istop) {
			throw new ArgumentException();
		}

		int bufferIndex = (int)(istart - CurrentPosition);
		int bytesToRead = (int)(istart - istop);

		_fStream.Seek(istart, SeekOrigin.Begin);
		int _ = _fStream.Read(Buffer, bufferIndex, bytesToRead);

		return Buffer.AsSpan(bufferIndex, bytesToRead);
	}

	/// <summary>
	/// Move on to the next chunk of bytes without reading anything
	/// </summary>
	internal void Skip() {
		if(CurrentPosition + Buffer.Length >= _fStream.Length) {
			CurrentPosition = _fStream.Length;
			_eof = true;
		} else {
			CurrentPosition += Buffer.Length;
		}
	}

	internal void Reset() {
		CurrentPosition = 0;
		_eof = false;
	}

	/// <summary>
	/// Must be called once processing is done. Closes the underlying opened file
	/// </summary>
	internal void Close() {
		_fStream.Close();
	}

	// internal void HandleFile() {
	// 	const int sectionSize = 100; // specify the section size in bytes
	//
	// 	// open the file stream
	// 	using(FileStream stream = File.OpenRead(_filePath)) {
	// 		int bytesRead;
	// 		var buffer = new byte[_bufferSize];
	// 		long totalBytesRead = 0;
	// 		var fileSize = stream.Length; // get the total size of the file
	// 		Stopwatch stopwatch = new Stopwatch(); // create a stopwatch to measure elapsed time
	//
	// 		stopwatch.Start(); // start the stopwatch
	//
	// 		// read the file in chunks and process each section
	// 		while((bytesRead = stream.Read(buffer, 0, _bufferSize)) > 0) {
	// 			// split the chunk into sections and process each section
	// 			for(var i = 0; i < bytesRead; i += sectionSize) {
	// 				var sectionBytes = Math.Min(sectionSize, bytesRead - i);
	// 				var section = new byte[sectionBytes];
	// 				Array.Copy(buffer, i, section, 0, sectionBytes);
	//
	// 				// processes the current section
	// 				ProcessSection(section, sectionBytes);
	// 			}
	//
	// 			totalBytesRead += bytesRead; // increment the total bytes read
	// 			var percentage = (int)((double)totalBytesRead / fileSize * 100); // calculate the percentage of file read
	//
	// 			// print the progress to the console
	// 			Console.CursorLeft = 0;
	// 			Console.Write($"Loading: {percentage}% (estimated time remaining: {GetEstimatedTimeRemaining(stopwatch.Elapsed, totalBytesRead, fileSize)})");
	// 		}
	//
	// 		stopwatch.Stop(); // stops the stopwatch
	// 		Console.WriteLine(); // adds a newline to the console
	// 	}
	//
	// 	Console.ForegroundColor = ConsoleColor.Yellow;
	// 	Console.WriteLine("File has been processed into sections."); // prints a completion message to the console
	// 	Console.ResetColor();
	// }

	// private void ProcessSection(byte[] buffer, int bytesRead) {
	// 	// space for processing the section so probably where our next work will be
	// }

	// private static TimeSpan GetEstimatedTimeRemaining(TimeSpan elapsedTime, long totalBytesRead, long fileSize) {
	// 	var bytesPerSecond = totalBytesRead / elapsedTime.TotalSeconds; // calculate the current speed in bytes per second
	// 	double remainingBytes = fileSize - totalBytesRead; // calculate the remaining bytes to read
	// 	var secondsRemaining = remainingBytes / bytesPerSecond; // calculate the remaining time in seconds
	//
	// 	return TimeSpan.FromSeconds(secondsRemaining); // return the remaining time as a TimeSpan object
	// }
}