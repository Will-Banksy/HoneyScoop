using HoneyScoop.FileHandling;
using HoneyScoop.Searching;
using HoneyScoop.Util;

namespace HoneyScoop.Carving; 

internal class CarveHandler {
	private int _chunkSize;
	/// <summary>
	/// Contains a mapping from chunk indexes to a list of <see cref="MatchedFile"/>s that are present in that chunk.
	/// Each MatchedFile may appear more than once
	/// </summary>
	private Dictionary<int, List<MatchedFile>> _chunkFiles;

	private const int DefaultCarveSize = 1024 * 1024 * 1024; // 1 GB

	internal CarveHandler(FileHandler fileHandler, List<(Match, Match?)> pairs) {
		_chunkSize = fileHandler.Buffer.Length;

		_chunkFiles = new Dictionary<int, List<MatchedFile>>();
		for(int i = 0; i < pairs.Count; i++) {
			(int chunkRangeStart, int chunkRangeEnd) = Helper.MapToChunkRange(
				(int)pairs[i].Item1.StartOfMatch,
				(int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize),
				_chunkSize
			);
			// TODO: Create MatchedFile
			for(int j = chunkRangeStart; j < chunkRangeEnd; j++) {
				throw new NotImplementedException(); // TODO: Add a MatchedFile copy to dictionary mapped from j
			}
		}
	}
}