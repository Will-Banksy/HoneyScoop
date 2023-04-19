using System.Text;
using HoneyScoop.FileHandling;
using HoneyScoop.Searching;
using HoneyScoop.Util;

namespace HoneyScoop.Carving; 

internal class CarveHandler {
	private int _chunkSize;
	
	/// <summary>
	/// The index of the last chunk that contains any data
	/// </summary>
	private int _numImportantChunks;
	
	/// <summary>
	/// Contains a mapping from chunk indexes to a list of <see cref="MatchedFile"/>s that are present in that chunk.
	/// Each MatchedFile may appear more than once
	/// </summary>
	private Dictionary<int, List<MatchedFile>> _chunkFiles;
	
	private Dictionary<int, List<ChunkCarveInfo>> _carveInfo;

	private const int DefaultCarveSize = 1024 * 1024 * 1024; // 1 GB

	internal CarveHandler(FileHandler fileHandler, List<(Match, Match?)> pairs) {
		_chunkSize = fileHandler.Buffer.Length;

		HashSet<string> usedFilenames = new HashSet<string>();

		_numImportantChunks = 0;

		_chunkFiles = new Dictionary<int, List<MatchedFile>>();
		for(int i = 0; i < pairs.Count; i++) {
			(int chunkRangeStart, int chunkRangeEnd) = Helper.MapToChunkRange(
				(int)pairs[i].Item1.StartOfMatch,
				(int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize),
				_chunkSize
			);
			string filename = CreateFilename(pairs[i].Item1, pairs[i].Item2, usedFilenames);
			MatchedFile file = new MatchedFile(pairs[i].Item1, pairs[i].Item2, filename);
			for(int j = chunkRangeStart; j < chunkRangeEnd; j++) {
				if(!_chunkFiles.ContainsKey(j)) {
					_chunkFiles[j] = new List<MatchedFile>();
				}
				_chunkFiles[j].Add(file);
			}
			
			_numImportantChunks = Int32.Max(chunkRangeEnd, _numImportantChunks);
		}
		
		// TODO: Create ChunkCarveInfos for each chunk

		for(int i = 0; i < _numImportantChunks; i++) {
			if(!_chunkFiles.ContainsKey(i)) {
				continue;
			}

			List<MatchedFile> filesInChunk = _chunkFiles[i];

			throw new NotImplementedException(); // TODO: Create ChunkCarveInfos based on all available chunk files etc etc.
		}
	}

	private static string CreateFilename(Match start, Match? end, HashSet<string> usedFilenames) {
		StringBuilder sb = new();
		sb.Append(start.StartOfMatch.ToString());
		sb.Append(end?.EndOfMatch.ToString() ?? (start.StartOfMatch + DefaultCarveSize).ToString());
		string filename = sb.ToString();
		int appended = 0;
		if(usedFilenames.Contains(filename)) {
			string checkFilename = $"{filename}_{appended}";
			while(usedFilenames.Contains(checkFilename)) {
				appended++;
				checkFilename = $"{filename}_{appended}";
			}

			filename = checkFilename;
		}
		usedFilenames.Add(filename);

		sb.Clear();
		sb.Append(filename);
		if(SupportedFileTypes.FileTypeHandlers.ContainsKey(start.MatchType.Type)) {
			sb.Append(SupportedFileTypes.FileTypeHandlers[start.MatchType.Type].FileExtension);
		}
		
		return sb.ToString();
	}
}