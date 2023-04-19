using HoneyScoop.FileHandling;
using HoneyScoop.Searching;

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
}