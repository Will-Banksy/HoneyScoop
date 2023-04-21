using System.Text;
using HoneyScoop.FileHandling;
using HoneyScoop.Searching;
using HoneyScoop.Util;

namespace HoneyScoop.Carving;

internal class CarveHandler {
	private readonly int _chunkSize;

	/// <summary>
	/// The index of the last chunk that contains any data
	/// </summary>
	private readonly int _numImportantChunks;

	/// <summary>
	/// A dictionary mapping chunk indexes to a list of carving operations to carry out - Adjacent to Scalpel's work queues
	/// </summary>
	private readonly Dictionary<int, List<ChunkCarveInfo>> _carveInfo;

	private const int DefaultCarveSize = 1024 * 1024 * 10; // 10 MB
	private const int MaximumAnalysisSize = 1024 * 1024 * 10; // 10 MB

	/// <summary>
	/// Constructs the <see cref="CarveHandler"/> instance and performs some preprocessing
	/// </summary>
	/// <param name="chunkSize">The size of the chunks to allocate for loading file data into</param>
	/// <param name="pairs"></param>
	internal CarveHandler(int chunkSize, List<(Match, Match?)> pairs) {
		_chunkSize = chunkSize;
		_numImportantChunks = 0;
		_carveInfo = new Dictionary<int, List<ChunkCarveInfo>>();

		// A hashset to ensure all used filenames are unique (which they likely will be but this avoids edge cases with little performance penalty)
		HashSet<string> usedFilenames = new HashSet<string>();

		for(int i = 0; i < pairs.Count; i++) {
			(int chunkRangeStart, int chunkRangeEnd) = Helper.MapToChunkRange(
				(int)pairs[i].Item1.StartOfMatch,
				(int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize),
				_chunkSize
			);

			// Get a filename for this pair of matches
			string filename = CreateFilename(pairs[i].Item1, pairs[i].Item2, usedFilenames);

			// For each pair of matches, create ChunkCarveInfos and add them to the _carveInfo dictionary for each chunk index the Match pair intersects
			for(int j = chunkRangeStart; j < chunkRangeEnd; j++) {
				if(!_carveInfo.ContainsKey(j)) {
					_carveInfo.Add(j, new List<ChunkCarveInfo>());
				}
				
				ChunkCarveType type = GetCarveType(j, chunkRangeStart, chunkRangeEnd);
				if(type == ChunkCarveType.ContinueCarve) {
					if(_carveInfo[j][0].Type != ChunkCarveType.ContinueCarve) {
						ChunkCarveInfo continueCarveInfo = new ChunkCarveInfo(
							_chunkSize * j,
							_chunkSize * (j + 1) - 1,
							ChunkCarveType.ContinueCarve,
							new List<string>() { filename }
						);
						_carveInfo[j].Insert(0, continueCarveInfo);
					} else {
						_carveInfo[j][0].Filenames.Add(filename);
					}
				}

				ChunkCarveInfo info = new ChunkCarveInfo(
					(int)pairs[i].Item1.StartOfMatch,
					(int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize),
					type,
					new List<string>() { filename }
				);
				_carveInfo[j].Add(info);
			}

			_numImportantChunks = Int32.Max(chunkRangeEnd, _numImportantChunks);
		}
	}

	internal void PerformCarving(FileHandler fileHandler) {
		CarveBufferManager buffer = new CarveBufferManager(fileHandler, _chunkSize);
		
		for(int chunkI = 0; chunkI < _numImportantChunks; chunkI++) {
			List<ChunkCarveInfo> carveInfos = _carveInfo[chunkI];

			for(int i = 0; i < carveInfos.Count; i++) {
				ChunkCarveInfo info = carveInfos[i];

				switch(info.Type) {
					case ChunkCarveType.StartCarve:
						break;
				}
			}
		}
		
		throw new NotImplementedException();
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
		if(SupportedFileTypes.FileTypeHandlers.TryGetValue(start.MatchType.Type, out IFileType? handler)) {
			sb.Append(handler.FileExtension);
		}

		return sb.ToString();
	}

	/// <summary>
	/// Calculates, for a file starting in <see cref="chunkStart"/> and stopping in <see cref="chunkStop"/>, the <see cref="ChunkCarveType"/> for that file for chunk index <see cref="chunkI"/>
	/// </summary>
	/// <param name="chunkI"></param>
	/// <param name="chunkStart"></param>
	/// <param name="chunkStop"></param>
	/// <returns></returns>
	private static ChunkCarveType GetCarveType(int chunkI, int chunkStart, int chunkStop) {
		if(chunkStart == chunkI && chunkStart == chunkStop) {
			return ChunkCarveType.StartStopCarve;
		} else if(chunkStart < chunkI && chunkStop > chunkI) {
			return ChunkCarveType.ContinueCarve;
		} else if(chunkStart == chunkI) {
			return ChunkCarveType.StartCarve;
		} else if(chunkStop == chunkI) {
			return ChunkCarveType.StopCarve;
		} else {
			return ChunkCarveType.SkipCarve;
		}
	}
}