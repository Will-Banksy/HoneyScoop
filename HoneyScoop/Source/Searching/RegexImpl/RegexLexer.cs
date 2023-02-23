namespace HoneyScoop.Searching.RegexImpl;

internal static class RegexLexer {
	internal enum OperatorType {
		/// <summary>
		/// Concatenation operator <c>a'b</c> (infix)/<c>ab'</c> (postfix) matches <c>a</c> followed by <c>b</c>
		/// </summary>
		Concat,

		/// <summary>
		/// Alternation operator <c>a|b</c> (infix)/<c>ab|</c> (postfix) matches <c>a</c> or <c>b</c>
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

		/// <summary>
		/// For where anything else doesn't make sense
		/// </summary>
		None
	}

	/// <summary>
	/// Returns the precedence of the operator, where 0 is lowest and 3 is highest
	/// </summary>
	internal static int Precedence(this OperatorType type) {
		return type switch {
			OperatorType.Concat => 1,
			OperatorType.Alternate => 2,
			OperatorType.AlternateLoop or OperatorType.AlternateLoopOnce or OperatorType.AlternateEmpty => 3,
			_ => 0
		};
	}

	internal enum TokenType {
		UnaryOperator,
		BinaryOperator,
		OpenParenthesis,
		CloseParenthesis,
		Literal,

		/// <summary>
		/// For where anything else doesn't make sense
		/// </summary>
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
				OperatorType.Concat or OperatorType.Alternate => TokenType.BinaryOperator,
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

		public string ToDebugString() { // TODO: This might be the sort of thing that should be in ToString so maybe need to rethink this not that it particularly matters
			return $"Token(Type={Type},OpType={OpType},LiteralValue={LiteralValue})";
		}

		/// <summary>
		/// Very much does not return the fully qualified type name of this class/instance but returns a string representation of the Token
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return Type switch {
				TokenType.Literal => $"\\x{Convert.ToString(LiteralValue, 16).PadLeft(2, '0')}",
				TokenType.OpenParenthesis => "(",
				TokenType.CloseParenthesis => ")",
				TokenType.UnaryOperator or TokenType.BinaryOperator => OpType switch {
					OperatorType.AlternateEmpty => "?",
					OperatorType.AlternateLoop => "*",
					OperatorType.AlternateLoopOnce => "+",
					OperatorType.Alternate => "|",
					OperatorType.Concat => "'",
					_ => " "
				},
				_ => " "
			};
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
			
			switch(src[i]) { //adds the tokens into the list once the matching regex operator has been found
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
				
				case '\'': // Strictly speaking this isn't necessary but it might be like an idea to have just in case
					tokens.Add(new Token(OperatorType.Concat));
					break;
				//Different process for literals, literals are stored in hex so it needs to check the next 4 characters for the hex format
				//and then convert to bytes for adding it into the list of tokens
				case '\\':
					if(src[i + 1] == 'x') {
						if(src[i + 2] >= '0' && src[i + 2] <= '9' || src[i + 2] >= 'a' && src[i + 2] <= 'f') {
							if(src[i + 3] >= '0' && src[i + 3] <= '9' || src[i + 3] >= 'a' && src[i + 3] <= 'f') {
								ReadOnlySpan<char> hexChars = src.AsSpan(i + 2, 2); // Using a span avoids unnecessarily allocating memory (which is slow)
								string hex = new string(hexChars);
								byte hexToByte = Convert.ToByte(hex, 16); // Corrected conversion of string to byte
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