using TreeCollections;

namespace HoneyScoop.Searching.RegexImpl; 

internal static class RegexParser {
	internal class Ast : SerialTreeNode<Ast> {
		internal RegexLexer.IToken Token;

		public Ast() {
			// Token = RegexLexer.IToken.Empty;
		}
		
		internal Ast(RegexLexer.IToken token, Ast[] children) : base(children) {
			Token = token;
		}
	}
	
	private static void ParseTokenStream(List<RegexLexer.IToken> tokens) { // TODO: Need some sort of tree structure to return - Maybe use a library that implements trees
	}
}