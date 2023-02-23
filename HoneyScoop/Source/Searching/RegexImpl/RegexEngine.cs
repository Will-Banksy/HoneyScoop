namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexEngine {
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
		var postfix = ParseToPostfix(regex);
		Stack<FiniteStateMachine<byte>> finiteStack = new Stack<FiniteStateMachine<byte>>();

		foreach(RegexLexer.Token token in postfix) {
			switch(token.Type) {
				case RegexLexer.TokenType.Literal:
					finiteStack.Push(new FiniteStateMachine<byte>(token.LiteralValue));
					Console.WriteLine(" Lit ");
					break;
				
				case RegexLexer.TokenType.UnaryOperator:
					switch(token.OpType) {
						case RegexLexer.OperatorType.AlternateEmpty:
							var nfaAltEmp = finiteStack.Pop();
							nfaAltEmp.AlternateEmpty();
							finiteStack.Push(nfaAltEmp);
							Console.WriteLine(" AltEmp ");
							break;
						
						case RegexLexer.OperatorType.AlternateLoop:
							var nfaAltLoop = finiteStack.Pop();
							nfaAltLoop.AlternateLoop();
							finiteStack.Push(nfaAltLoop);
							Console.WriteLine(" AltLoop ");
							break;
						
						case RegexLexer.OperatorType.AlternateLoopOnce:
							var nfaAltLoopOnce = finiteStack.Pop();
							nfaAltLoopOnce.AlternateLoopOnce();
							finiteStack.Push(nfaAltLoopOnce);
							Console.WriteLine(" AltLoopOnce ");
							break;
					}
					break;
				
				case RegexLexer.TokenType.BinaryOperator:
					switch(token.OpType) {
						case RegexLexer.OperatorType.Alternate:
							var firstPopAlt = finiteStack.Pop();
							var nfaAlt = finiteStack.Pop();
							nfaAlt.Alternate(nfaAlt);
							finiteStack.Push(nfaAlt);
							finiteStack.Push(firstPopAlt);
							Console.WriteLine(" Alt ");
							break;
						
						case RegexLexer.OperatorType.Concat:
							var firstPopCon = finiteStack.Pop();
							var nfaCon = finiteStack.Pop();
							nfaCon.Concatenate(nfaCon);
							finiteStack.Push(nfaCon);
							finiteStack.Push(firstPopCon);
							Console.WriteLine(" Con ");
							break;
					}
					break;
			}
		}
		
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
	internal static List<RegexLexer.Token> ParseToPostfix(string regex) { // TODO Make private. Internal for testing
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