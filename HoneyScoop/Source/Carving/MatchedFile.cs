using HoneyScoop.FileHandling;
using HoneyScoop.Searching;

namespace HoneyScoop.Carving; 

internal class MatchedFile {
	internal readonly Match Start;
	internal readonly Match End;
	internal FileType Type;
	internal readonly string Filename;

	internal MatchedFile(Match start, Match end, FileType type, string filename) {
		Start = start;
		End = end;
		Type = type;
		Filename = filename;
	}
}