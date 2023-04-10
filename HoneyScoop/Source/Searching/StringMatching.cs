using HoneyScoop.Util;

namespace HoneyScoop.Searching;

public class StringMatching
{
    internal static List<Pair<Match, Match>> Interpret(List<Match> matches){
        //TODO output list of pairs of matches
        //TODO every output will be the start of a match and the end of the match (TLDR from first index value of header, to first value of corresponding footer)
        List<Pair<Match, Match>> completeMatch = new List<Pair<Match, Match>>();
        var matchStack = new Stack<Match>();
        for (var i = 0; i < matches.Count; i++)
        {
            if (matches[i].MatchType % 2 == 0)
            {
                matchStack.Push(matches[i]);

            }
            else {
                while (matchStack.Peek().MatchType != matches[i].MatchType - 1) {matchStack.Pop();}

                completeMatch.Add(new Pair<Match, Match>(matchStack.Pop(), matches[i]));
            }
        }

		throw new NotImplementedException();
	}
}