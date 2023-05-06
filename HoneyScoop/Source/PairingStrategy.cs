namespace HoneyScoop; 

/// <summary>
/// The strategy used to pair a header/footer match
/// </summary>
public enum PairingStrategy {
	/// <summary>
	/// A header is paired with the next occurrence of a corresponding footer
	/// </summary>
	PairNext,
	
	/// <summary>
	/// A header is paired with the last occurrence of a corresponding footer within a specific range (TODO)
	/// </summary>
	PairLast,
}