using HoneyScoop.Searching.RegexImpl;

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
		for (int i = 0; i < bytes.Length; i++)
		{
			for (int j = 0; j < _states.Count; j++)
			{
				for (int k = 0; k < _states[j].Connections.Count; k++)
				{
					
				}
			}
			
			{
				
			}
			
		} // TODO
	}
}