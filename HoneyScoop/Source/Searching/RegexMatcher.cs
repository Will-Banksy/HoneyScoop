using HoneyScoop.Searching.RegexImpl;
using HoneyScoop.Util;
namespace HoneyScoop.Searching;

/// <summary>
/// This class is basically the abstraction of the RegexImpl namespace - It's frontend
/// </summary>
internal class RegexMatcher {
	private FiniteStateMachine<byte> _nfa;
	private List<FiniteStateMachine<byte>.State> _states;
	
	/// <summary>
	/// Initialises a RegexMatcher that matches the regex
	/// </summary>
	/// <param name="regex"></param>
	internal RegexMatcher(string regex) {
		_nfa = RegexEngine.ParseRegex(regex);
		_states = new List<FiniteStateMachine<byte>.State>{};
	}

	/// <summary>
	/// Initialises a RegexMatcher that matches any of the passed in regexprs
	/// </summary>
	/// <param name="regexprs"></param>
	internal RegexMatcher(params string[] regexprs) {
		if(regexprs.Length > 0) {
			_nfa = RegexEngine.ParseRegex(regexprs[0]);
			for(int i = 1; i < regexprs.Length; i++) {
				_nfa.Alternate(RegexEngine.ParseRegex(regexprs[i]));
			}
		} else {
			_nfa = new FiniteStateMachine<byte>();
		}
		
		_states = new List<FiniteStateMachine<byte>.State>{};
	}

	/// <summary>
	/// Advances the regex matcher through the input range of bytes, returning a list of matches by index
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	internal List<int> Advance(ReadOnlySpan<byte> bytes) {
		List<int> indexOfBytes = new List<int>();
		for (int i = 0; i < bytes.Length; i++)
		{
			for (int j = 0; j < _states.Count; j++)
			{
				var connections = Helper.Flatten(_states[j]);
				bool advanceExist = false;
				{
					
				}
				for (int k = 0; k < connections.Count; k++) {
					if (bytes[i] == connections[k].Symbol) {
						_states[j] = connections[k].Next;
						advanceExist = true;
						if (_states[j] == _nfa.End) {
							indexOfBytes.Add(i);
							_states.RemoveAt(j);
						}
						break;
					}
				}

				if (advanceExist == false) {_states.RemoveAt(j); }
			}

			var connectionsNfa = Helper.Flatten(_nfa.Start);
			for (int j = 0; j < connectionsNfa.Count; j++) {
				
				if (bytes[i] == connectionsNfa[j].Symbol) {
					if (connectionsNfa[j].Next == _nfa.End) {
						indexOfBytes.Add(i);
					}
					else {
						_states.Add(connectionsNfa[j].Next);
					}

					
				}
			}

		}

		return indexOfBytes;
	}
}