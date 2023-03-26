namespace HoneyScoop.Searching;

internal readonly struct Match {
	internal readonly int StartOfMatch;
	internal readonly int EndOfMatch;
	internal readonly uint MatchType;

	internal Match(int start, int end, uint type) {
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