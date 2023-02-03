namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexLexer {
	[Flags]
	internal enum OperatorType {
		/// <summary>
		/// Concatenation operator <c>a'b</c> (infix) matches <c>a</c> followed by <c>b</c>
		/// </summary>
		Concat,
		/// <summary>
		/// Alternation operator <c>a|b</c> (infix) matches <c>a</c> or <c>b</c>
		/// </summary>
		Alternate,
		/// <summary>
		/// Empty alternation operator <c>a?</c> matches <c>a</c> or nothing
		/// </summary>
		AlternateEmpty,
		/// <summary>
		/// Loop alternation operator <c>a*</c> matches <c>a</c> none or more times
		/// </summary>
		AlternateLoop,
		/// <summary>
		/// Match-once loop alternation operator <c>+</c> matches <c>a</c> one or more times
		/// </summary>
		AlternateLoopOnce,
		None
	}

	[Flags]
	internal enum TokenType {
		UnaryOperator,
		BinaryOperator,
		OpenParenthesis,
		CloseParenthesis,
		Literal,
		None
	}

	internal readonly struct Token {
		internal readonly TokenType Type = TokenType.None;
		
		/// <summary>
		/// Only really has meaning if the Type is TokenType.UnaryOperator or TokenType.BinaryOperator
		/// </summary>
		internal readonly OperatorType OpType = OperatorType.None;
		
		/// <summary>
		/// Only really has meaning if the Type is TokenType.Literal
		/// </summary>
		internal readonly byte LiteralValue = 0;

		/// <summary>
		/// Construct a new, empty token
		/// </summary>
		public Token() {
		}

		/// <summary>
		/// Construct a new token of specified type
		/// </summary>
		/// <param name="type"></param>
		internal Token(TokenType type) {
			Type = type;
		}

		/// <summary>
		/// Construct token with explicit token type and operator type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="opType"></param>
		internal Token(TokenType type, OperatorType opType) {
			Type = type;
			OpType = opType;
		}

		/// <summary>
		/// Construct token with explicit operator type and inferred token type
		/// </summary>
		/// <param name="opType"></param>
		internal Token(OperatorType opType) {
			Type = opType switch {
				OperatorType.Concat | OperatorType.Alternate => TokenType.BinaryOperator,
				OperatorType.None => TokenType.None,
				_ => TokenType.UnaryOperator
			};
			OpType = opType;
		}

		/// <summary>
		/// Construct a token of type literal, with the specified value
		/// </summary>
		/// <param name="literalValue"></param>
		internal Token(byte literalValue) {
			Type = TokenType.Literal;
			LiteralValue = literalValue;
		}
	}

	/// <summary>
	/// Operators in input: <c>(, ), +, ?, *, |</c>
	/// </summary>
	/// <param name="src">The regex string</param>
	/// <returns>A list of tokens that represents the regex string</returns>
	internal static List<Token> Tokenize(string src) {
		var tokens = new List<Token>();

		for(int i = 0; i < src.Length; i++) {
			switch(src[i]) {
				case '|':
					tokens.Add(new Token(OperatorType.Alternate));
					break;
				case '(':
					tokens.Add(new Token(TokenType.OpenParenthesis));
					break;
				case ')':
					tokens.Add(new Token(TokenType.CloseParenthesis));
					break;
				case '+':
					tokens.Add(new Token(OperatorType.AlternateLoopOnce));
					break;
				case '*':
					tokens.Add(new Token(OperatorType.AlternateLoop));
					break;
				case '?':
					tokens.Add(new Token(OperatorType.AlternateEmpty));
					break;
				case '\\':
					if(src[i + 1] == 'x') {
						if(src[i + 2] >= '0' && src[i + 2] <= '9' || src[i + 2] >= 'a' && src[i + 2] <= 'f') {
							if(src[i + 3] >= '0' && src[i + 3] <= '9' || src[i + 3] >= 'a' && src[i + 3] <= 'f') {
								ReadOnlySpan<char> hexChars = src.AsSpan(i + 2, 2); // Using a span avoids unnecessarily allocating memory (which is slow)
								string hex = new string(hexChars);
								byte hexToByte = Convert.ToByte(hex, 16);
								tokens.Add(new Token(hexToByte));
								i += 3;
							}
						}
					}

					break;
			}
		}

		return tokens;
	}
}