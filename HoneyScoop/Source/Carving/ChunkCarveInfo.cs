using HoneyScoop.FileHandling;

namespace HoneyScoop.Carving;

/// <summary>
/// Specifies the type of carving operation for a chunk
/// </summary>
internal enum ChunkCarveType {
	StartStopCarve,
	StartNextStopCarve,
	StartCarve,
	ContinueCarve,
	StopCarve,
	SkipCarve,
}

/// <summary>
/// What scalpel refers to as work queues - Describes a carving operation for a chunk
/// </summary>
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