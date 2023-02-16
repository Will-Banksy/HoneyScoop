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
	internal static List<RegexLexer.Token> ParseTokenStream(ReadOnlySpan<RegexLexer.Token> tokens) {
		// Aight so how we doing this
		// Step 1: Find each sub-expression, innermost to outermost
		// Step 2: TODO

		List<RegexLexer.Token> postfix = new List<RegexLexer.Token>();

		Stack<RegexLexer.Token> stack = new Stack<RegexLexer.Token>();
		
		for(int i = 0; i < tokens.Length; i++) {
			switch(tokens[i].Type) {
				case RegexLexer.TokenType.Literal:
					postfix.Add(tokens[i]);
					break;
				
				case RegexLexer.TokenType.OpenParenthesis:
					stack.Push(tokens[i]);
					break;
				
				case RegexLexer.TokenType.CloseParenthesis:
					while(stack.Peek().Type != RegexLexer.TokenType.OpenParenthesis) {
						postfix.Add(stack.Pop());
					}

					stack.Pop();
					break;
				
				case RegexLexer.TokenType.UnaryOperator:
				case RegexLexer.TokenType.BinaryOperator:
					// Push tokens on to the stack in precedence order - Highest first
					if(stack.Count != 0) {
						if(RegexLexer.OperatorPrecedence(tokens[i].OpType) > RegexLexer.OperatorPrecedence(stack.Peek().OpType)) {
						}
					} else {
						
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