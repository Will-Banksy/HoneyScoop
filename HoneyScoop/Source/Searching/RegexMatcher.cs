using HoneyScoop.Searching.RegexImpl;
using HoneyScoop.Util;

using State = HoneyScoop.Searching.RegexImpl.FiniteStateMachine<byte>.State;
using StateConnection = HoneyScoop.Searching.RegexImpl.FiniteStateMachine<byte>.StateConnection;

namespace HoneyScoop.Searching;

/// <summary>
/// This class is basically the abstraction of the RegexImpl namespace - Its frontend
/// </summary>
internal class RegexMatcher {
	private readonly FiniteStateMachine<byte> _nfa;
	private readonly List<(State, int)> _states;
	private readonly uint _type;
	/// <summary>
	/// Preprocessed data to speed up matching.
	/// For each state, the flattened connection list (following transparent/ε-connections) and whether the state is the end is stored
	/// </summary>
	private Dictionary<State, (List<StateConnection>, bool)>? _preprocData = null; // TODO: Actually implement this when it gets round to optimisation time

	/// <summary>
	/// Initialises a RegexMatcher that matches the given regular expression
	/// </summary>
	/// <param name="regex">The regular expression that this matcher will use</param>
	/// <param name="type">An integer used for identification; These will be passed back in the <see cref="Match"/> structs returned in <see cref="Advance"/></param>
	internal RegexMatcher(string regex, uint type) {
		_nfa = RegexEngine.ParseRegex(regex);
		_states = new List<(State, int)>();
		_type = type;
		
// Disable this section for now, useful for debugging but otherwise just clutters stdout
#if DEBUG && false
		_nfa.Debug();
#endif
	}

	/// <summary>
	/// Advances the regex matcher through the input range of bytes, returning a list of Matches
	/// </summary>
	/// <param name="bytes"></param>
	/// <param name="currentOffset">The offset in the source data that <see cref="bytes"/> is taken from</param>
	/// <returns></returns>
	internal List<Match> Advance(ReadOnlySpan<byte> bytes, long currentOffset = 0) {
		List<Match> matches = new List<Match>();
		for(int i = 0; i < bytes.Length; i++) {
			for(int j = 0; j < _states.Count; j++) {
				var connections = Helper.Flatten(_states[j].Item1);
				bool hasAdvanced = false;
				for(int k = 0; k < connections.Count; k++) {
					if(bytes[i] == connections[k].Symbol || connections[k].Wildcard) {
						_states[j] = (connections[k].Next, _states[j].Item2);
						hasAdvanced = true;
						if(Helper.IsEndState(_states[j].Item1, _nfa.End)) {
							matches.Add(new Match(_states[j].Item2 + currentOffset, i + currentOffset, _type));
							_states.RemoveAt(j);
							j--;
						}

						break;
					}
				}

				if(!hasAdvanced) {
					_states.RemoveAt(j);
				}
			}

			var startConnections = Helper.Flatten(_nfa.Start);
			for(int j = 0; j < startConnections.Count; j++) {
				if(bytes[i] == startConnections[j].Symbol || startConnections[j].Wildcard) {
					if(Helper.IsEndState(startConnections[j].Next, _nfa.End)) {
						matches.Add(new Match(i + currentOffset, i + currentOffset, _type));
					} else {
						_states.Add((startConnections[j].Next, i));
					}
				}
			}
		}

// Disable this section for now, useful for debugging but otherwise just clutters stdout
#if DEBUG && false
		Console.Write("Ending with states: [");
		foreach(var state in _states) {
			Console.Write($"{state}, ");
		}
		Console.WriteLine("]");
#endif

		return matches;
	}
}