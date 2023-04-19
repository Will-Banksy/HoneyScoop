namespace HoneyScoop.Carving;

internal enum ChunkCarveType {
	StartStopCarve,
	StartCarve,
	ContinueCarve,
	StopCarve,
	SkipCarve,
}

internal class ChunkCarveInfo {
	internal int Start;
	internal int Stop;
	internal ChunkCarveType Type;
	internal List<string> Filenames;

	internal ChunkCarveInfo(int start, int stop, ChunkCarveType type, List<string> filenames) {
		Start = start;
		Stop = stop;
		Type = type;
		Filenames = filenames;
	}
}