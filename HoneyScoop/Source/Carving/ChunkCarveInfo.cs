namespace HoneyScoop.Carving;

internal enum ChunkCarveType {
	StartStopCarve,
	StartCarve,
	ContinueCarve,
	StopCarve,
	SkipCarve,
}

internal class ChunkCarveInfo {
	internal int Start = 0;
	internal int Stop = 0;
	internal ChunkCarveType Type = ChunkCarveType.SkipCarve;
	internal List<string> Filenames = new();
}