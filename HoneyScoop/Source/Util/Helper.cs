using HoneyScoop.FileHandling;
using HoneyScoop.Searching.RegexImpl;

namespace HoneyScoop.Util;

internal static class Helper {
	/// <summary>
	/// Efficiently converts a byte span into a uint. If the span contains less than 4 bytes it will fail
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	internal static uint FromBigEndian(ReadOnlySpan<byte> bytes) {
		uint res = bytes[0];
		res <<= 8;
		res |= bytes[1];
		res <<= 8;
		res |= bytes[2];
		res <<= 8;
		res |= bytes[3];

		return res;
	}

	/// <summary>
	/// Compares the two arguments and returns an <see cref="AnalysisResult"/> based on whether argument update is a valid update to argument res.<br/><br/>
	/// Valid updates to res are:
	/// <list type="bullet">
	///     <item><see cref="AnalysisResult.Partial"/> (applied if res is <see cref="AnalysisResult.Correct"/>),</item>
	///     <item><see cref="AnalysisResult.FormatError"/> (applied if res is Correct or Partial),</item>
	///     <item><see cref="AnalysisResult.Corrupted"/> (applied if res is Correct, Partial or FormatError),</item>
	/// </list>
	/// </summary>
	/// <param name="res">The base AnalysisResult</param>
	/// <param name="update">The update to the base AnalysisResult</param>
	/// <returns>Argument res, updated by argument update, if valid</returns>
	internal static AnalysisResult UpdateResultWith(this AnalysisResult res, AnalysisResult update) {
		switch(update) {
			case AnalysisResult.Partial:
				if(res == AnalysisResult.Correct) {
					res = AnalysisResult.Partial;
				}
				break;
			case AnalysisResult.FormatError:
				if(res == AnalysisResult.Correct || res == AnalysisResult.Partial) {
					res = AnalysisResult.FormatError;
				}
				break;
			case AnalysisResult.Corrupted:
				if(res == AnalysisResult.Correct || res == AnalysisResult.Partial || res == AnalysisResult.FormatError) {
					res = AnalysisResult.Corrupted;
				}
				break;
		}

		return res;
	}

	private static readonly Dictionary<string, FileType> FileTypeStrs = InitFileTypeStrs();

	private static Dictionary<string, FileType> InitFileTypeStrs() {
		return new Dictionary<string, FileType> {
			{ "png", FileType.Png },
			{ "jpg", FileType.Jpg },
			{ "gif", FileType.Gif },
			{ "mp4", FileType.Mp4 },
			{ "mp3", FileType.Mp3 },
			{ "wav", FileType.Wav },
			{ "xlsx", FileType.Xlsx },
			{ "docx", FileType.Docx },
			{ "pptx", FileType.Pptx },
			{ "pdf", FileType.Pdf },
			{ "zip", FileType.Zip }
		};
	}

	internal static FileType FromString(string fileType) {
		string lower = fileType.ToLower();
		return FileTypeStrs.GetValueOrDefault(lower, FileType.None);
	}

	internal static List<FiniteStateMachine<byte>.StateConnection> Flatten(FiniteStateMachine<byte>.State startState) {
		var connections = new List<FiniteStateMachine<byte>.StateConnection>();

		Stack<FiniteStateMachine<byte>.State> stateStack = new();
		stateStack.Push(startState);
		HashSet<FiniteStateMachine<byte>.State> visitedStates = new();

		while(stateStack.Count > 0) {
			FiniteStateMachine<byte>.State state = stateStack.Pop();
			if(visitedStates.Contains(state)) {
				continue;
			}
			visitedStates.Add(state);
			for(int i = 0; i < state.Connections.Count; i++) {
				if(state.Connections[i].Transparent) {
					stateStack.Push(state.Connections[i].Next);
				} else {
					connections.Add(state.Connections[i]);
				}
			}
		}

		return connections;
	}
}