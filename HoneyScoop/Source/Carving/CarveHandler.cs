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

		// TODO: Optimisation - Could I merge the creation of the ChunkCarveInfos with the creation of the MatchedFiles... Do I even need MatchedFiles
		// And then I'll just loop through the chunks and do the actual carving... I think
		
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

		_carveInfo = new Dictionary<int, List<ChunkCarveInfo>>();
		for(int chunkI = 0; chunkI < _numImportantChunks; chunkI++) {
			if(!_chunkFiles.ContainsKey(chunkI)) {
				continue;
			}

			List<MatchedFile> filesInChunk = _chunkFiles[chunkI];

			_carveInfo[chunkI] = new List<ChunkCarveInfo>();

			ChunkCarveInfo continueCarveInfo = new ChunkCarveInfo(
				_chunkSize * chunkI,
				_chunkSize * (chunkI + 1) - 1,
				ChunkCarveType.ContinueCarve,
				new List<string>()
			);
			for(int i = 0; i < filesInChunk.Count; i++) {
				ChunkCarveType type = filesInChunk[i].GetCarveType(chunkI, _chunkSize);

				if(type == ChunkCarveType.ContinueCarve) {
					continueCarveInfo.Filenames.Add(filesInChunk[i].Filename);
				}
				
				ChunkCarveInfo info = new ChunkCarveInfo(
					(int)filesInChunk[i].Start.StartOfMatch,
					(int)(filesInChunk[i].End?.EndOfMatch ?? filesInChunk[i].Start.StartOfMatch + DefaultCarveSize),
					type,
					new List<string>() { filesInChunk[i].Filename }
				);
				_carveInfo[chunkI].Add(info);
			}

			if(continueCarveInfo.Filenames.Count != 0) {
				_carveInfo[chunkI].Add(continueCarveInfo);
			}

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