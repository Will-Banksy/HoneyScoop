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
		internal record struct Empty() : IToken;
	}

	/// <summary>
	/// Operators in input: <c>(, ), +, ?, *, |</c>
	/// </summary>
	/// <param name="src">The regex string</param>
	/// <returns>A list of tokens that represents the regex string</returns>
	internal static List<IToken> Tokenize(string src) {
		var tokens = new List<IToken>();

		for (int i = 0; i < src.Length; i++) {
			switch (src[i])
			{
				case '|':
					tokens.Add(new IToken.BinaryOp(BinaryOpType.Alternate));
					Console.WriteLine("Alternate Works");
					break;
				case '(':
					tokens.Add(new IToken.OpenParen());
					Console.WriteLine("OpenParen Works");
					break;
				case ')':
					tokens.Add(new IToken.CloseParen());
					Console.WriteLine("CloseParen Works");
					break;
				case '+':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateLoopOnce));
					Console.WriteLine("AlternateLoopOnce Works");
					break;
				case '*':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateLoop));
					Console.WriteLine("AlternateLoop Works");
					break;
				case '?':
					tokens.Add(new IToken.UnaryOp(UnaryOpType.AlternateEmpty));
					Console.WriteLine("AlternateEmpty Works");
					break;
				case '\\':
					if (src[i+1] == 'x') {
						if (src[i + 2] >= '0' && src[i + 2] <= '9' || src[i + 2] >= 'a' && src[i + 2] <= 'f') {
							if (src[i + 3] >= '0' && src[i + 3] <= '9' || src[i + 3] >= 'a' && src[i + 3] <= 'f')
							{
								char[] chars = { src[i + 2], src[i + 3] };
								string hex = new string(chars);
								byte hextobyte = byte.Parse(hex);
								tokens.Add(new IToken.Literal(hextobyte));
								i += 3;
								Console.WriteLine("Literals Work");
							}
						}
					}
					break;
			}
		}
		
		return tokens;
	}
}