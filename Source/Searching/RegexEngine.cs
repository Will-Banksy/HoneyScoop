namespace HoneyScoop.Searching; 

public static class RegexEngine {
	/// <summary>
	/// Parses a regex string into a Finite State Machine that is capable of being used to match the regex pattern.<br /><br />
	///
	/// Supported metacharacters:<br />
	///	    a*    Matches any amount of 'a' or nothing<br />
	///		a+    Matches 'a' and any more instances of 'a' directly afterwards<br />
	///     a|b   Matches 'a' or 'b'<br />
	///     a?    Matches 'a' or nothing<br /><br />
	///
	/// You can use brackets () to group expressions e.g. (abc)+
	/// </summary>
	/// <param name="regex"></param>
	/// <returns></returns>
	public static FiniteStateMachine ParseRegex(string regex) {
		var postfix = ConvertToPostfix(regex);
		return new FiniteStateMachine();
	}

	/// <summary>
	/// Convert a regex string to postfix notation for easier parsing and NFA/Finite State Machine construction
	/// </summary>
	/// <param name="regex"></param>
	/// <returns></returns>
	private static string ConvertToPostfix(string regex) {
		return regex;
	}
}