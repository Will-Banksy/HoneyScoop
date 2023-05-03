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

	/// <summary>
	/// A dictionary mapping FIDs (File IDs) to CarveFileInfos that contain information about a found file
	/// </summary>
	private readonly Dictionary<int, CarveFileInfo> _fileInfo;

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
		_fileInfo = new Dictionary<int, CarveFileInfo>();

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

			// The fid can be equal to i - As i is already unique per found file
			int fid = i;

			// Create and store a CarveFileInfo struct to hold the file data
			CarveFileInfo fInfo = new CarveFileInfo(filename, pairs[i].Item1.MatchType.Type);
			_fileInfo.Add(fid, fInfo);

			// For each pair of matches, create ChunkCarveInfos and add them to the _carveInfo dictionary for each chunk index the Match pair intersects
			for(int j = chunkRangeStart; j <= chunkRangeEnd; j++) {
				if(!_carveInfo.ContainsKey(j)) {
					_carveInfo.Add(j, new List<ChunkCarveInfo>());
				}

				ChunkCarveType type = GetCarveType(j, chunkRangeStart, chunkRangeEnd);

				// By default start at chunk start and stop at chunk end
				int startCarve = _chunkSize * j;
				int stopCarve = _chunkSize * (j + 1) - 1;

				switch(type) {
					// If the type is ContinueCarve (which carves the entire chunk) then put that first cause then can optimise to reduce unnecessary reads
					case ChunkCarveType.ContinueCarve:
						ChunkCarveInfo continueCarveInfo = new ChunkCarveInfo(
							startCarve,
							stopCarve,
							ChunkCarveType.ContinueCarve,
							fid
						);
						_carveInfo[j].Insert(0, continueCarveInfo);
						continue;

					case ChunkCarveType.StartCarve:
					case ChunkCarveType.StartNextStopCarve:
						// Start at the defined start point
						startCarve = (int)pairs[i].Item1.StartOfMatch;
						break;

					case ChunkCarveType.StopCarve:
						// Stop at the defined stop point (which, if footer match is null, will be the start + the default carve size)
						stopCarve = (int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize);
						break;
					
					case ChunkCarveType.StartStopCarve:
						startCarve = (int)pairs[i].Item1.StartOfMatch;
						stopCarve = (int)(pairs[i].Item2?.EndOfMatch ?? pairs[i].Item1.StartOfMatch + DefaultCarveSize);
						break;
				}

				// Collect the information into an object and store that
				ChunkCarveInfo info = new ChunkCarveInfo(
					startCarve,
					stopCarve,
					type,
					fid
				);
				_carveInfo[j].Add(info);
			}

			_numImportantChunks = Int32.Max(chunkRangeEnd, _numImportantChunks);
		}
	}

	internal void PerformCarving(FileHandler fileHandler) {
		// The buffer manager that will be used for reading and keeping data loaded
		CarveBufferManager buffer = new CarveBufferManager(fileHandler, _chunkSize);

		// Maps fids to where the carving started (file index)
		// Fids that were started this chunk and will be completed next chunk
		Dictionary<int, int> analyseFidsNext = new Dictionary<int, int>();
		// Fids that are going to be completed this chunk
		Dictionary<int, int> analyseFidsNow = new Dictionary<int, int>();

		for(int chunkI = 0; chunkI <= _numImportantChunks; chunkI++) {
			bool keepChunkData = false;

			List<ChunkCarveInfo> carveInfos = _carveInfo[chunkI];

			for(int i = 0; i < carveInfos.Count; i++) {
				ChunkCarveInfo info = carveInfos[i];

				switch(info.Type) {
					case ChunkCarveType.StartStopCarve: {
						// Fetch the data and file info of that data and perform analysis
						CarveFileInfo fileInfo = _fileInfo[info.Fid];
						ReadOnlySpan<byte> fileData = buffer.Fetch(info.Start, info.Stop);
						AnalysisResult analysisResult = SupportedFileTypes.FileTypeHandlers[fileInfo.FType].Analyse(fileData);
						string filepath = Helper.OutputPath(analysisResult, fileInfo.FType, fileInfo.Filename);
						if(!Helper.EnsureExists(filepath)) {
							return;
						}
						FileStream oStream = new FileStream(filepath, FileMode.Create);
						oStream.Write(fileData);
						oStream.Close();
						break;
					}

					case ChunkCarveType.StartNextStopCarve: {
						// All there is to do here is make sure the data is loaded and will be loaded next chunk, and also to track that this fid has unwritten data
						// that needs to be analysed
						keepChunkData = true;
						analyseFidsNext.Add(info.Fid, info.Start);
						buffer.Fetch(info.Start, info.Stop);
						break;
					}

					case ChunkCarveType.ContinueCarve: {
						CarveFileInfo fileInfo = _fileInfo[info.Fid];
						ReadOnlySpan<byte> fileData = buffer.Fetch(info.Start, info.Stop);
						fileInfo.OutputStream?.Write(fileData);
						break;
					}

					case ChunkCarveType.StartCarve: {
						CarveFileInfo fileInfo = _fileInfo[info.Fid];
						ReadOnlySpan<byte> fileData = buffer.Fetch(info.Start, info.Stop);
						AnalysisResult analysisResult = AnalysisResult.Unanalysed;
						string filepath = Helper.OutputPath(analysisResult, fileInfo.FType, fileInfo.Filename);
						if(!Helper.EnsureExists(filepath)) {
							return;
						}
						fileInfo.OutputStream = new FileStream(filepath, FileMode.Create);
						fileInfo.OutputStream?.Write(fileData);
						break;
					}

					case ChunkCarveType.StopCarve: {
						CarveFileInfo fileInfo = _fileInfo[info.Fid];
						if(analyseFidsNow.TryGetValue(info.Fid, out int startLoadFrom)) {
							buffer.Fetch(info.Start, info.Stop);
							ReadOnlySpan<byte> fileData = buffer.GetWithLast(startLoadFrom, info.Stop);
							AnalysisResult analysisResult = SupportedFileTypes.FileTypeHandlers[fileInfo.FType].Analyse(fileData);
							string filepath = Helper.OutputPath(analysisResult, fileInfo.FType, fileInfo.Filename);
							if(!Helper.EnsureExists(filepath)) {
								return;
							}
							FileStream oStream = new FileStream(filepath, FileMode.Create);
							oStream.Write(fileData);
							oStream.Close();
						} else {
							ReadOnlySpan<byte> fileData = buffer.Fetch(info.Start, info.Stop);
							fileInfo.OutputStream?.Write(fileData);
							fileInfo.OutputStream?.Close();
						}
						break;
					}
				}
			}

			// Shift everything from analyseFidsNext to analyseFidsNow, and create a new dictionary for analyseFidsNext
			analyseFidsNow = analyseFidsNext;
			analyseFidsNext = new Dictionary<int, int>();
			
			buffer.MoveNext(keepChunkData);
		}
	}

	private static string CreateFilename(Match start, Match? end, HashSet<string> usedFilenames) {
		StringBuilder sb = new();
		sb.Append(start.StartOfMatch.ToString());
		sb.Append("-");
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
			sb.Append($".{handler.FileExtension}");
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
		} else if(chunkStart == chunkI && chunkStart == chunkStop - 1) {
			return ChunkCarveType.StartNextStopCarve;
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