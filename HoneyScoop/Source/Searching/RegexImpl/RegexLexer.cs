namespace HoneyScoop.Searching.RegexImpl;

internal class RegexLexer {
	internal enum BinaryOpType {
		Concat,
		Union
	}

	internal interface IToken {
		internal record struct Literal(byte val) : IToken;
		internal record struct BinaryOp(BinaryOpType type) : IToken;
		internal record struct UnaryOp();
		internal record struct OpenParen();
		internal record struct CloseParen();
	}

	internal List<IToken> Tokenize(string src) {
		var tokens = new List<IToken>();

		for (int i = 0; i < src.Length; i++) {
			switch (src[i]) {
				case '|':
				case '\'':
					// tokens.Add(IToken.BinaryOp(BinaryOpType.Concat));
					break;
			}
		}
		
		return tokens;
	}
}