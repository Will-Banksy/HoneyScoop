namespace HoneyScoop.Searching;

internal struct Match
{
    internal int StartOfMatch;
    internal int EndOfMatch;
    internal uint MatchType;

    internal Match(int start, int end, uint type)
    {

        StartOfMatch = start;
        EndOfMatch = end;
        MatchType = type;
    }
}