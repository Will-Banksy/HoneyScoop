namespace HoneyScoop.FileHandling;

internal readonly struct PatternUnit {
	private readonly byte? _patternByte;

	internal static PatternUnit ExactValue(byte value) {
		return new PatternUnit(value);
	}

	internal static PatternUnit AnyValue() {
		return new PatternUnit(null);
	}

	private PatternUnit(byte? unit) {
		_patternByte = unit;
	}

	internal bool Match(byte value) {
		if(!_patternByte.HasValue) {
			return true;
		} else {
			return _patternByte.Value == value;
		}
	}
}

internal struct Signature {
	internal PatternUnit[] Pattern { get; private set; }

	internal Signature(PatternUnit[] pattern) {
		Pattern = pattern;
	}

	public static Signature From(IEnumerable<byte> bytes) {
		return new Signature(bytes.Select((val) => PatternUnit.ExactValue(val)).ToArray());
	}
}