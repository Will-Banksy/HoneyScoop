namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexParser {
	/// <summary>
	/// Rearranges an infix stream of tokens to postfix using the Shunting-Yard algorithm
	/// This method assumes all binary operators are infix, while all unary operators are postfix.
	/// Error handling is minimal.<br /><br />
	/// See <a href="https://www.web4college.com/converters/infix-to-postfix-prefix.php">https://www.web4college.com/converters/infix-to-postfix-prefix.php</a> and <a href="https://blog.cernera.me/converting-regular-expressions-to-postfix-notation-with-the-shunting-yard-algorithm/">https://blog.cernera.me/converting-regular-expressions-to-postfix-notation-with-the-shunting-yard-algorithm/</a>
	/// </summary>
	/// <param name="tokens"></param>
	internal static List<RegexLexer.Token> RearrangeToPostfix(List<RegexLexer.Token> tokens) {
		// Preprocess the list to insert explicit concatenation operators
		InsertExplicitConcats(tokens);

		// The following code will correctly turn infix into postfix using the Shunting-Yard algorithm

		List<RegexLexer.Token> postfix = new List<RegexLexer.Token>();
		Stack<RegexLexer.Token> opStack = new Stack<RegexLexer.Token>();

		for(int i = 0; i < tokens.Count; i++) {
			switch(tokens[i].Type) {
				case RegexLexer.TokenType.Literal:
					postfix.Add(tokens[i]);
					break;

				case RegexLexer.TokenType.OpenParenthesis:
					opStack.Push(tokens[i]);
					break;

				case RegexLexer.TokenType.CloseParenthesis:
					if(opStack.Count == 0) {
						// If no matching '(' preceded this ')', throw exception
						throw new ArgumentException("Unexpected ')'");
					}

					while(opStack.Peek().Type != RegexLexer.TokenType.OpenParenthesis) {
						postfix.Add(opStack.Pop());
					}

					opStack.Pop();
					break;

				case RegexLexer.TokenType.UnaryOperator:
				case RegexLexer.TokenType.BinaryOperator:
					if(opStack.Count == 0) {
						opStack.Push(tokens[i]);
					} else if(tokens[i].OpType.Precedence() > opStack.Peek().OpType.Precedence()) {
						opStack.Push(tokens[i]);
					} else {
						while(opStack.Count != 0 && tokens[i].OpType.Precedence() <= opStack.Peek().OpType.Precedence()) {
							postfix.Add(opStack.Pop());
						}

						opStack.Push(tokens[i]);
					}

					break;
			}
		}

		// Pop any remaining operators off the operator stack and add them to the postfix expression
		while(opStack.Count > 0) {
			postfix.Add(opStack.Pop());
		}

		return postfix;
	}

	/// <summary>
	/// This function inserts explicit concatenation operators into an infix token stream where needed. The token stream is modified in-place<br />
	/// Concatenation operators (') are inserted between adjacent subexpressions, e.g. ab => a'b, a(b|c) => a'(b|c), (a+b)(c|de) => (a+'b)'(c|d'e)
	/// </summary>
	/// <param name="tokens"></param>
	private static void InsertExplicitConcats(List<RegexLexer.Token> tokens) {
		for(int i = 1; i < tokens.Count; i++) {
			if((tokens[i - 1].Type == RegexLexer.TokenType.Literal || tokens[i - 1].Type == RegexLexer.TokenType.CloseParenthesis) && (tokens[i].Type == RegexLexer.TokenType.Literal || tokens[i].Type == RegexLexer.TokenType.OpenParenthesis)) { // Handle case ab => a'b, )( => )'(, )a => )'a, a( => a'(
				tokens.Insert(i, new RegexLexer.Token(RegexLexer.OperatorType.Concat));
			} else if(tokens[i - 1].Type == RegexLexer.TokenType.UnaryOperator && (tokens[i].Type == RegexLexer.TokenType.Literal || tokens[i].Type == RegexLexer.TokenType.OpenParenthesis)) { // Handle case +a => +'a, +( => +'(
				tokens.Insert(i, new RegexLexer.Token(RegexLexer.OperatorType.Concat));
			}
		}
	}
}