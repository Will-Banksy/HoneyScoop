namespace HoneyScoop.Searching.RegexImpl;

internal class RegexLexer {
	internal enum BinaryOpType {
		Concat, // ' 
		Alternate // | 
	}

	internal enum UnaryOpType {
		AlternateEmpty, // ?
		AlternateLoop, // *
		AlternateLoopOnce // +
	}
	
	internal interface IToken {
		internal record struct Literal(byte val) : IToken;
		internal record struct BinaryOp(BinaryOpType type) : IToken;
		internal record struct UnaryOp(UnaryOpType type) : IToken;
		internal record struct OpenParen() : IToken;
		internal record struct CloseParen() : IToken;
	}

	/// <summary>
	/// Operators in input: <c>(,), +, ?, *, |</c>
	/// </summary>
	/// <param name="src">The regex string</param>
	/// <returns>A list of tokens that represents the regex string</returns>
	internal static List<IToken> Tokenize(string src) {
		var tokens = new List<IToken>();

		for (int i = 0; i < src.Length; i++) {
			switch (src[i]) {
				case '|':
					tokens.Add(new IToken.BinaryOp(BinaryOpType.Alternate));
					break;
				case '(':
					tokens.Add(new IToken.OpenParen());
					break;
				case ')':
					tokens.Add(new IToken.CloseParen());
					break;
				case '+':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateLoopOnce));
					break;
				case '*':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateLoop));
					break;
				case '?':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateEmpty));
					break;
			}
		}
		
		return tokens;
	}
}