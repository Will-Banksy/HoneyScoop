using TreeCollections;

namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexParser {
	internal class RegexAST : SerialTreeNode<RegexAST> { // TODO: Is SerialTreeNode the correct class to derive from?
		internal RegexLexer.Token Token;

		public RegexAST() { // public cause SerialTreeNode requires it. Actually not true C# requires it
			Token = new RegexLexer.Token();
		}
		
		internal RegexAST(RegexLexer.Token rootToken, RegexAST[] children) : base(children) {
			Token = rootToken;
		}
	}

	/// <summary>
	/// Produces an Abstract Syntax Tree for a regex from a token stream. This method assumes all binary operators are infix, while all unary operators are postfix
	/// </summary>
	/// <param name="tokens"></param>
	internal static RegexAST ParseTokenStream(ReadOnlySpan<RegexLexer.Token> tokens) {
		// Aight so how we doing this
		// Step 1: Scan the tokens stream, collecting top level tokens
		//     (literals and expressions, where an expression is basically an operator. Basically, a TLT is anything that should be concatenated together at the highest level)
		// Step 2: TODO

		var tlts = new List<RegexLexer.Token>(); // Top level tokens
		bool opWantsArg = false;
		
		for(int i = 0; i < tokens.Length; i++) {
			if(i < tokens.Length - 1) {
				switch(tokens[i + 1].Type) {
					case RegexLexer.TokenType.UnaryOperator:
					case RegexLexer.TokenType.BinaryOperator: {
						opWantsArg = true;
						break;
					}
				}
			}
			switch(tokens[i].Type) {
				case RegexLexer.TokenType.Literal: {
					Console.WriteLine("Literal (" + tokens[i].LiteralValue + ")");
					if(!opWantsArg) {
						tlts.Add(tokens[i]);
					}
					opWantsArg = false;
					break;
				}
				case RegexLexer.TokenType.BinaryOperator: {
					Console.WriteLine("BinaryOp");
					opWantsArg = true;
					break;
				}
				case RegexLexer.TokenType.UnaryOperator: {
					Console.WriteLine("UnaryOp");
					opWantsArg = false;
					break;
				}
				case RegexLexer.TokenType.OpenParenthesis: {
					Console.WriteLine("OpenParen");
					var j = i;
					for(; j < tokens.Length; j++) {
						// TODO: Skip until matching closing parenthesis (which is not necessarily the next one)
						break;
					}
					i = j;
					break;
				}
				case RegexLexer.TokenType.CloseParenthesis: {
					Console.WriteLine("CloseParen");
					break;
				}
			};
		}

		return new RegexAST(); // TODO: Actually return something meaningful
	}
}