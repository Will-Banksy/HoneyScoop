namespace HoneyScoop.Util; 

public class ValuePair<T1, T2> {
	internal T1 Item1;
	internal T2 Item2;

	internal ValuePair(T1 item1, T2 item2) {
		Item1 = item1;
		Item2 = item2;
	}

	public override string ToString() {
		return $"({Item1}, {Item2})";
	}
}