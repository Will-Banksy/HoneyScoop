using HoneyScoop.FileHandling;
using HoneyScoop.Searching;
using HoneyScoop.Util;

namespace HoneyScoop.Carving; 

internal class MatchedFile {
	internal readonly Match Start;
	internal readonly Match? End;
	internal FileType Type;
	internal readonly string Filename;

	internal MatchedFile(Match start, Match? end, string filename) {
		Start = start;
		End = end;
		Type = start.MatchType.Type;
		Filename = filename;
	}

	internal ChunkCarveType GetCarveType(int chunkI, int chunkSize) {
		int startChI = Helper.MapToChunk(chunkI, chunkSize);
		int endChI = Helper.MapToChunk(chunkI, chunkSize);
	
		if(startChI == chunkI && startChI == endChI) {
			return ChunkCarveType.StartStopCarve;
		} else if(startChI < chunkI && endChI > chunkI) {
			return ChunkCarveType.ContinueCarve;
		} else if(startChI == chunkI) {
			return ChunkCarveType.StartCarve;
		} else if(endChI == chunkI) {
			return ChunkCarveType.StopCarve;
		} else {
			return ChunkCarveType.SkipCarve;
		}
	}
}