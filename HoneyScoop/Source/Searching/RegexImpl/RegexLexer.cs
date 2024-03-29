using System.Text;

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
		/// <summary>
		/// The type of the token
		/// </summary>
		internal readonly TokenType Type = TokenType.None;

		/// <summary>
		/// Only really has meaning if the Type is TokenType.UnaryOperator or TokenType.BinaryOperator. Contains the operator type the token represents
		/// </summary>
		internal readonly OperatorType OpType = OperatorType.None;

		/// <summary>
		/// Only really has meaning if the Type is TokenType.Literal. Contains the literal value the token represents
		/// </summary>
		internal readonly byte LiteralValue = 0;

		/// <summary>
		/// If true, this token represents a match to any literal value (a wildcard)
		/// </summary>
		internal readonly bool LiteralWildcard = false;

		/// <summary>
		/// Construct a new token with default values
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

		/// <summary>
		/// Construct a literal wildcard token.
		/// Note that the bool is there just to differentiate this constructor from the default constructor and the passed-in value is ignored
		/// </summary>
		/// <param name="literalWildcard">Ignored</param>
		internal Token(bool literalWildcard) {
			Type = TokenType.Literal;
			LiteralWildcard = true;
		}

		internal string ToDebugString() {
			return $"Token(Type={Type},OpType={OpType},LiteralValue={LiteralValue},LiteralWildcard={LiteralWildcard})";
		}

		/// <summary>
		/// Very much does not return the fully qualified type name of this class/instance but returns a string representation of the Token
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return Type switch {
				TokenType.Literal => LiteralWildcard ? "." : $"\\x{Convert.ToString(LiteralValue, 16).PadLeft(2, '0')}",
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

		/// <summary>
		/// Returns the string representation of the list of tokens
		/// </summary>
		/// <param name="tokens"></param>
		/// <returns></returns>
		internal static string TokensToString(IEnumerable<Token> tokens) {
			var sb = new StringBuilder();
			foreach(var t in tokens) {
				sb.Append(t.ToString());
			}

			return sb.ToString();
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
						if((src[i + 2] >= '0' && src[i + 2] <= '9') || (src[i + 2] >= 'a' && src[i + 2] <= 'f') || (src[i + 2] >= 'A' && src[i + 2] <= 'F')) {
							if((src[i + 3] >= '0' && src[i + 3] <= '9') || (src[i + 3] >= 'a' && src[i + 3] <= 'f') || (src[i + 3] >= 'A' && src[i + 3] <= 'F')) {
								ReadOnlySpan<char> hexChars = src.AsSpan(i + 2, 2); // Using a span avoids unnecessarily allocating memory (which is slow)
								string hex = new string(hexChars);
								byte hexToByte = Convert.ToByte(hex, 16); // Corrected conversion of string to byte
								tokens.Add(new Token(hexToByte));
								i += 3;
							}
						}
					}

					break;

				case '.':
					tokens.Add(new Token(true));
					break;

				default:
					byte asciiValue = (byte)src[i];
					tokens.Add(new Token(asciiValue));
					break;
			}
		}

		return tokens;
	}
}