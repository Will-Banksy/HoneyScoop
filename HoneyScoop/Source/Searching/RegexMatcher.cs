using HoneyScoop.Searching.RegexImpl;
using HoneyScoop.Util;

namespace HoneyScoop.Searching;

/// <summary>
/// This class is basically the abstraction of the RegexImpl namespace - It's frontend
/// </summary>
internal class RegexMatcher {
	private FiniteStateMachine<byte> _nfa;
	private List<Pair<FiniteStateMachine<byte>.State, int>> _states;
	private uint _type;
	/// <summary>
	/// Initialises a RegexMatcher that matches the regex
	/// </summary>
	/// <param name="regex"></param>
	internal RegexMatcher(string regex, uint type) {
		_nfa = RegexEngine.ParseRegex(regex);
		_states = new List<Pair<FiniteStateMachine<byte>.State, int>> { };
		_type = type;
	}

	/// <summary>
	/// Advances the regex matcher through the input range of bytes, returning a list of matches by index
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	internal List<Match> Advance(ReadOnlySpan<byte> bytes) {
		List<Match> indexOfBytes = new List<Match>();
		for(int i = 0; i < bytes.Length; i++) {
			for(int j = 0; j < _states.Count; j++) {
				var connections = Helper.Flatten(_states[j].Item1);
				bool advanceExist = false;
				for(int k = 0; k < connections.Count; k++) {
					if(bytes[i] == connections[k].Symbol) {
						_states[j].Item1 = connections[k].Next;
						advanceExist = true;
						if(_states[j].Item1 == _nfa.End) {
							indexOfBytes.Add(new Match(_states[j].Item2, i, _type));
							_states.RemoveAt(j);
						}

						break;
					}
				}

				if(advanceExist == false) {
					_states.RemoveAt(j);
				}
			}

			var connectionsNfa = Helper.Flatten(_nfa.Start);
			for(int j = 0; j < connectionsNfa.Count; j++) {
				if(bytes[i] == connectionsNfa[j].Symbol) {
					if(connectionsNfa[j].Next == _nfa.End) {
						indexOfBytes.Add(new Match(i, i, _type));
					} else {
						_states.Add(new Pair<FiniteStateMachine<byte>.State, int>(connectionsNfa[j].Next, i));
					}
				}
			}
		}

		return indexOfBytes;
	}
}