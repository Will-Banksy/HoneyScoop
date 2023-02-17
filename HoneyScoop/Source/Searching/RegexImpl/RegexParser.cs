// using TreeCollections;

namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexParser {
	// internal class RegexAST : SerialTreeNode<RegexAST> { // TODO: Is SerialTreeNode the correct class to derive from?
	// 	internal RegexLexer.Token Token;
	//
	// 	public RegexAST() { // public cause SerialTreeNode requires it. Actually not true C# requires it
	// 		Token = new RegexLexer.Token();
	// 	}
	// 	
	// 	internal RegexAST(RegexLexer.Token rootToken, RegexAST[] children) : base(children) {
	// 		Token = rootToken;
	// 	}
	// }

	/// <summary>
	/// Rearranges an infix stream of tokens to postfix.
	/// This method assumes all binary operators are infix, while all unary operators are postfix.
	/// Error handling is minimal.<br /><br />
	/// See <a href="https://www.web4college.com/converters/infix-to-postfix-prefix.php">https://www.web4college.com/converters/infix-to-postfix-prefix.php</a>
	/// </summary>
	/// <param name="tokens"></param>
	internal static List<RegexLexer.Token> RearrangeToPostfix(ReadOnlySpan<RegexLexer.Token> tokens) { // TODO: Complete this. With error handling?
		List<RegexLexer.Token> postfix = new List<RegexLexer.Token>();

		Stack<RegexLexer.Token> opStack = new Stack<RegexLexer.Token>();
		
		for(int i = 0; i < tokens.Length; i++) {
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

		// var tlts = new List<RegexLexer.Token>(); // Top level tokens
		// bool opWantsArg = false;
		//
		// for(int i = 0; i < tokens.Length; i++) {
		// 	if(i < tokens.Length - 1) {
		// 		switch(tokens[i + 1].Type) {
		// 			case RegexLexer.TokenType.UnaryOperator:
		// 			case RegexLexer.TokenType.BinaryOperator: {
		// 				opWantsArg = true;
		// 				break;
		// 			}
		// 		}
		// 	}
		// 	switch(tokens[i].Type) {
		// 		case RegexLexer.TokenType.Literal: {
		// 			Console.WriteLine("Literal (" + tokens[i].LiteralValue + ")");
		// 			if(!opWantsArg) {
		// 				tlts.Add(tokens[i]);
		// 			}
		// 			opWantsArg = false;
		// 			break;
		// 		}
		// 		case RegexLexer.TokenType.BinaryOperator: {
		// 			Console.WriteLine("BinaryOp");
		// 			opWantsArg = true;
		// 			break;
		// 		}
		// 		case RegexLexer.TokenType.UnaryOperator: {
		// 			Console.WriteLine("UnaryOp");
		// 			opWantsArg = false;
		// 			break;
		// 		}
		// 		case RegexLexer.TokenType.OpenParenthesis: {
		// 			Console.WriteLine("OpenParen");
		// 			var j = i;
		// 			for(; j < tokens.Length; j++) {
		// 				// TODO: Skip until matching closing parenthesis (which is not necessarily the next one)
		// 				break;
		// 			}
		// 			i = j;
		// 			break;
		// 		}
		// 		case RegexLexer.TokenType.CloseParenthesis: {
		// 			Console.WriteLine("CloseParen");
		// 			break;
		// 		}
		// 	};
		// }

		// return new RegexAST(); // TODO: Actually return something meaningful
		return postfix;
	}
}