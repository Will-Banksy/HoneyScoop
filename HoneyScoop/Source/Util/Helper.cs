using System.Text;
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

	/// <summary>
	/// Builds a list of non-ε connections starting from <see cref="startState"/>, following transparent connections.<br />
	/// E.g. starting from state 0 in this NFA:
	/// <code>
	/// 0 -ε-> 1 ---> 3
	/// 0 -ε-> 4 -ε-> 3 ---> 5
	/// 0 ---> 2
	/// 0 ---> 0
	/// </code>
	/// This function would return a list of connections:
	/// <code>
	/// [
	///	    ---> 2,
	///     ---> 0,
	///     ---> 3,
	///     ---> 5
	/// ]
	/// </code>
	/// Note that connections are returned in the list in order of how deep the iteration goes, the deeper the further down the list, meaning that direct non-ε
	/// connections from <see cref="startState"/> are first in the returned list
	/// </summary>
	/// <param name="startState"></param>
	/// <returns></returns>
	internal static List<FiniteStateMachine<byte>.StateConnection> Flatten(FiniteStateMachine<byte>.State startState) {
		var connections = new List<FiniteStateMachine<byte>.StateConnection>();

		Queue<FiniteStateMachine<byte>.State> stateQueue = new();
		stateQueue.Enqueue(startState);
		HashSet<int> visitedStatesUids = new();

		while(stateQueue.Count > 0) {
			FiniteStateMachine<byte>.State state = stateQueue.Dequeue();
			if(visitedStatesUids.Contains(state.Uid)) {
				continue;
			}
			visitedStatesUids.Add(state.Uid);
			for(int i = 0; i < state.Connections.Count; i++) {
				if(state.Connections[i].Transparent) {
					stateQueue.Enqueue(state.Connections[i].Next);
				} else {
					connections.Add(state.Connections[i]);
				}
			}
		}

		return connections;
	}

	/// <summary>
	/// Checks whether the passed-in state is the end state, or is ε-connected to it
	/// </summary>
	/// <param name="state"></param>
	/// <param name="endState"></param>
	/// <returns></returns>
	internal static bool IsEndState(FiniteStateMachine<byte>.State state, FiniteStateMachine<byte>.State endState) {
		if(state.Equals(endState)) {
			return true;
		}
		
		HashSet<FiniteStateMachine<byte>.State> visited = new();

		return IsEndStateRecur(state, endState, visited);
	}

	/// <summary>
	/// Implementation detail of <see cref="IsEndState"/> - Recursive function that keeps track of visited nodes (states)
	/// </summary>
	/// <param name="state"></param>
	/// <param name="endState"></param>
	/// <param name="visited"></param>
	/// <returns></returns>
	private static bool IsEndStateRecur(FiniteStateMachine<byte>.State state, FiniteStateMachine<byte>.State endState, HashSet<FiniteStateMachine<byte>.State> visited) {
		if(state.Equals(endState)) {
			return true;
		}
		
		for(int i = 0; i < state.Connections.Count; i++) {
			if(state.Connections[i].Transparent && !visited.Contains(state.Connections[i].Next)) {
				visited.Add(state.Connections[i].Next);
				if(IsEndStateRecur(state.Connections[i].Next, endState, visited)) {
					return true;
				}
			}
		}
		
		return false;
	}

	internal static void Walk(FiniteStateMachine<byte> nfa, Action<FiniteStateMachine<byte>.State> action) {
		Stack<FiniteStateMachine<byte>.State> stateQueue = new();
		stateQueue.Push(nfa.Start);
		HashSet<int> visitedStatesUids = new();

		while(stateQueue.Count > 0) {
			FiniteStateMachine<byte>.State state = stateQueue.Pop();
			if(visitedStatesUids.Contains(state.Uid)) {
				continue;
			}
			visitedStatesUids.Add(state.Uid);
			action(state);
			for(int i = 0; i < state.Connections.Count; i++) {
				stateQueue.Push(state.Connections[i].Next);
			}
		}
	}
	
	internal static string ListToString<T>(IList<T> list) {
		StringBuilder sb = new();
		sb.Append('[');
		for(int i = 0; i < list.Count(); i++) {
			sb.Append($"{list[i]}, ");
		}
		sb.Append(']');

		return sb.ToString();
	}

	internal static string ListToStringTight<T>(IList<T> list) {
		StringBuilder sb = new();
		for(int i = 0; i < list.Count(); i++) {
			sb.Append($"{list[i]}");
		}

		return sb.ToString();
	}

	internal static int MapToChunk(int i, int chunkSize) {
		if(i < 0) {
			throw new ArgumentException($"Index {i} is not valid - Must be greater than zero");
		}
		return i / chunkSize;
	}

	internal static (int, int) MapToChunkRange(int startI, int endI, int chunkSize) {
		int startChunk = MapToChunk(startI, chunkSize);
		int endChunk = MapToChunk(endI, chunkSize);
		return (startChunk, endChunk);
	}

	private static string? _timestampedOutDir = null;

	internal static string SetTimestampedOutputDir() {
		_timestampedOutDir = DateTime.Now.ToString("s");
		return _timestampedOutDir;
	}
	
	/// <summary>
	/// Returns the path to <see cref="filename"/> within the output directory for <see cref="analysisResult"/> and <see cref="fileType"/> (using
	/// the output directory defined by the <see cref="HoneyScoop"/> instance - usually set through CLI args)
	/// </summary>
	/// <param name="analysisResult"></param>
	/// <param name="fileType"></param>
	/// <param name="filename"></param>
	/// <returns></returns>
	internal static string OutputPath(AnalysisResult analysisResult, FileType fileType, string filename) {
		string aRStr = analysisResult.ToString();
		string fTStr = fileType.ToString();

		if(_timestampedOutDir != null) {
			if(HoneyScoop.Instance().NoOrganise) {
				return Path.Join(HoneyScoop.Instance().OutputDirectory, _timestampedOutDir, aRStr, filename);
			}
			return Path.Join(HoneyScoop.Instance().OutputDirectory, _timestampedOutDir, aRStr, fTStr, filename);
		}

		if(HoneyScoop.Instance().NoOrganise) {
			return Path.Join(HoneyScoop.Instance().OutputDirectory, aRStr, filename);
		}
		return Path.Join(HoneyScoop.Instance().OutputDirectory, aRStr, fTStr, filename);
	}

	/// <summary>
	/// Creates directories for a path if they do not already exist
	/// </summary>
	/// <param name="filepath"></param>
	/// <returns>Returns true if the path exists or was able to be created, false otherwise</returns>
	internal static bool EnsureExists(string filepath) {
		string? dirName = Path.GetDirectoryName(filepath);
		if(dirName != null) {
			Exception? except = null;
			try {
				DirectoryInfo dirInfo = Directory.CreateDirectory(dirName);
				if(dirInfo.Exists) {
					return true;
				}
			} catch(Exception e) {
				except = e;
			}
		
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("Error creating/accessing output directory");
			if(except != null) {
				Console.WriteLine($": {except}");
			}
		}

		return false;
	}
}