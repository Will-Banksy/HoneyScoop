using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;

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
		var postfix = ConvertToPostfix(regex);
		Stack<FiniteStateMachine<byte>> finiteStack;

		foreach (RegexLexer.Token token in postfix)
		{
			switch (token.Type)
			{
				case RegexLexer.TokenType.Literal:

//TODO Push onto stack
					break;
				case RegexLexer.TokenType.UnaryOperator:
//TODO pop once operate on finite state machine with operator with  another switch ()
					break;
				case RegexLexer.TokenType.BinaryOperator:
					//TODO pop twice "" another switch
					
					break;
			}
			
			
		}

		// TODO: Use stack to iterate through postfix expression and incrementally create the NFA
		
		return new FiniteStateMachine<byte>();
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
	private static List<RegexLexer.Token> ConvertToPostfix(string regex) {
		// Each postfix operator produces 1 NFA. Operators | and ' take 2 NFAs the rest take 1
		// Inputs => Outputs
		// ((ab*)|c)+def => ab*'c|+d'e'f'
		
		// Step 1: Use lexer to create token stream
		var tokens = RegexLexer.Tokenize(regex);

		// Use a stack to turn turn the infix token stream into a postfix one
		var postfix = RegexParser.ParseTokenStream(CollectionsMarshal.AsSpan(tokens));

		return tokens;
	}
}

