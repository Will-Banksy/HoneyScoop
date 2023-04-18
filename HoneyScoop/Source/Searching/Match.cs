using HoneyScoop.FileHandling;

namespace HoneyScoop.Searching;

internal readonly struct Match {
	internal readonly long StartOfMatch;
	internal readonly long EndOfMatch;
	internal readonly FileTypePart MatchType;

	internal Match(long start, long end, FileTypePart type) {
		StartOfMatch = start;
		EndOfMatch = end;
		MatchType = type;
	}

	internal bool Equals(Match other) {
		return StartOfMatch == other.StartOfMatch && EndOfMatch == other.EndOfMatch && MatchType == other.MatchType;
	}

	public override bool Equals(object? obj) {
		return obj is Match other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(StartOfMatch, EndOfMatch, MatchType);
	}
}