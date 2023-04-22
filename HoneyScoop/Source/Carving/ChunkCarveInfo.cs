using HoneyScoop.FileHandling;

namespace HoneyScoop.Carving;

internal enum ChunkCarveType {
	StartStopCarve,
	StartNextStopCarve,
	StartCarve,
	ContinueCarve,
	StopCarve,
	SkipCarve,
}

internal class ChunkCarveInfo {
	internal int Start;
	internal int Stop;
	internal ChunkCarveType Type;
	internal int Fid;

	internal ChunkCarveInfo(int start, int stop, ChunkCarveType type, int fid) {
		Start = start;
		Stop = stop;
		Type = type;
		Fid = fid;
	}
}