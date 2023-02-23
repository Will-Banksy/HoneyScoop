using System.Diagnostics;

namespace HoneyScoop;

internal class _FileHandling // TODO: Relocate the code within this class to the class in FileHandling/FileHandler.cs. Keep in mind the coding conventions outlined in MainClass
{
	internal static void _Main(string[] args) // TODO: There must only be one Main function in the program and that is in MainClass - put this code within a different function in FileHandler.cs
	{
		var filePath = @"D:\Uni work\Beemovei.txt"; // specify the file path
		const int sectionSize = 100; // specify the section size in bytes
		var bufferSize = 1024 * 1024; // specify the buffer size in bytes (1 MB)

		// open the file stream
		using(FileStream stream = File.OpenRead(filePath)) {
			int bytesRead;
			var buffer = new byte[bufferSize];
			long totalBytesRead = 0;
			var fileSize = stream.Length; // get the total size of the file
			Stopwatch stopwatch = new Stopwatch(); // create a stopwatch to measure elapsed time

			stopwatch.Start(); // start the stopwatch

			// read the file in chunks and process each section
			while((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0) {
				// split the chunk into sections and process each section
				for(var i = 0; i < bytesRead; i += sectionSize) {
					var sectionBytes = Math.Min(sectionSize, bytesRead - i);
					var section = new byte[sectionBytes];
					Array.Copy(buffer, i, section, 0, sectionBytes);

					// processes the current section
					ProcessSection(section, sectionBytes);
				}

				totalBytesRead += bytesRead; // increment the total bytes read
				var percentage = (int)((double)totalBytesRead / fileSize * 100); // calculate the percentage of file read

				// print the progress to the console
				Console.CursorLeft = 0;
				Console.Write("Loading: {0}% (estimated time remaining: {1})", percentage, GetEstimatedTimeRemaining(stopwatch.Elapsed, totalBytesRead, fileSize));
			}

			stopwatch.Stop(); // stops the stopwatch
			Console.WriteLine(); // adds a newline to the console
		}

		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine("File has been processed into sections."); // prints a completion message to the console
		Console.ResetColor();
	}

	static void ProcessSection(byte[] buffer, int bytesRead) {
		// space for processing the section so probably where our next work will be
	}

	static TimeSpan GetEstimatedTimeRemaining(TimeSpan elapsedTime, long totalBytesRead, long fileSize) {
		var bytesPerSecond = totalBytesRead / elapsedTime.TotalSeconds; // calculate the current speed in bytes per second
		double remainingBytes = fileSize - totalBytesRead; // calculate the remaining bytes to read
		var secondsRemaining = remainingBytes / bytesPerSecond; // calculate the remaining time in seconds

		return TimeSpan.FromSeconds(secondsRemaining); // return the remaining time as a TimeSpan object
	}
}