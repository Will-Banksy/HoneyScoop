using HoneyScoop.Util;

namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexEngine {
	/// <summary>
	/// Cache to avoid constructing the same FiniteStateMachine multiple times
	/// </summary>
	private static Dictionary<string, FiniteStateMachine<byte>> _parseCache = new();

	/// <summary>
	/// Parses a regex string into a Finite State Machine that is capable of being used to match the regex pattern.<br /><br />
	///
	/// Supported metacharacters:<br />
	///	    a*    Matches any amount of 'a' or nothing<br />
	///		a+    Matches 'a' and any more instances of 'a' directly afterwards<br />
	///     a|b   Matches 'a' or 'b'<br />
	///     a?    Matches 'a' or nothing<br /><br />
	///
	/// You can use brackets () to group expressions e.g. (abc)+
	/// </summary>
	/// <param name="regex"></param>
	/// <returns></returns>
	internal static FiniteStateMachine<byte> ParseRegex(string regex) {
		if(_parseCache.TryGetValue(regex, out var cached)) {
			return cached;
		}

		var postfix = ParseToPostfix(regex);

// Disable this section for now, useful for debugging but otherwise just clutters stdout
#if DEBUG && false
		Console.WriteLine($"Infix: {regex} --> Postfix: {Helper.ListToStringTight(postfix)}");
#endif

		Stack<FiniteStateMachine<byte>> finiteStack = new Stack<FiniteStateMachine<byte>>();
		int uuid = 0;

		foreach(RegexLexer.Token token in postfix) { //Goes through the list of tokens and does an action based on what operator it is
			switch(token.Type) {
				case RegexLexer.TokenType.Literal: //pushes the literal straight onto the stack
					finiteStack.Push(new FiniteStateMachine<byte>(ref uuid, token.LiteralValue, token.LiteralWildcard));
					break;

				case RegexLexer.TokenType.UnaryOperator: //operator that only takes one input
					switch(token.OpType) {
						case RegexLexer.OperatorType.AlternateEmpty: //pops once then does the AlternateEmpty function and pushes it onto the stack
							var nfaAltEmp = finiteStack.Pop();
							nfaAltEmp = nfaAltEmp.AlternateEmpty();
							finiteStack.Push(nfaAltEmp);
							break;

						case RegexLexer.OperatorType.AlternateLoop: //pops once then does the AlternateLoop function and pushes it onto the stack
							var nfaAltLoop = finiteStack.Pop();
							nfaAltLoop = nfaAltLoop.AlternateLoop();
							finiteStack.Push(nfaAltLoop);
							break;

						case RegexLexer.OperatorType.AlternateLoopOnce: //pops once then does the AlternateLoopOnce function and pushes it onto the stack
							var nfaAltLoopOnce = finiteStack.Pop();
							nfaAltLoopOnce = nfaAltLoopOnce.AlternateLoopOnce();
							finiteStack.Push(nfaAltLoopOnce);
							break;
					}

					break;

				case RegexLexer.TokenType.BinaryOperator: //Operator that takes two inputs
					switch(token.OpType) {
						case RegexLexer.OperatorType.Alternate: //Pops twice and does the Alternate function on the second pop and then pushes the two pops back in order of second then first pop
							var firstPopAlt = finiteStack.Pop();
							var nfaAlt = finiteStack.Pop();
							firstPopAlt = firstPopAlt.Alternate(ref uuid, nfaAlt);
							finiteStack.Push(firstPopAlt);
							break;

						case RegexLexer.OperatorType.Concat: //Pops twice and does the Concatenate function on the second pop and then pushes the two pops back in order of second then first pop
							var nfaCon = finiteStack.Pop();
							var firstPopCon = finiteStack.Pop();
							firstPopCon = firstPopCon.Concatenate(nfaCon);
							finiteStack.Push(firstPopCon);
							break;
					}

					break;
			}
		}

		_parseCache[regex] = finiteStack.Peek();

		return finiteStack.Peek();
	}

	/// <summary>
	/// Convert a regex string to postfix notation for easier parsing and NFA/Finite State Machine construction<br /><br />
	///
	/// Postfix notation:<br />
	///     a*  => a*<br />
	///     a+  => a+<br />
	///	    a?  => a?<br />
	///     a|b => ab|<br />
	///	    ab  => ab'<br />
	///     (ab)c => ab'c'<br /><br />
	///
	/// Operator precedence (Highest to lowest): () -> *, +, ? -> ' -> |
	/// </summary>
	/// <param name="regex"></param>
	/// <returns></returns>
	private static List<RegexLexer.Token> ParseToPostfix(string regex) {
		// Each postfix operator produces 1 NFA. Operators | and ' take 2 NFAs the rest take 1
		// Inputs => Outputs
		// ((ab*)|c)+def => ab*'c|+d'e'f'

		// Step 1: Use lexer to create token stream
		var tokens = RegexLexer.Tokenize(regex);

		// Rearrange that token stream to postfix
		var postfix = RegexParser.RearrangeToPostfix(tokens);

		return postfix;
	}
}