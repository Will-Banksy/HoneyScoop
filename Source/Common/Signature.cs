namespace HoneyScoop.Common {
	public struct PatternUnit {
		private byte? _patternByte;

		public static PatternUnit ExactValue(byte value) {
			return new PatternUnit(value);
		}

		public static PatternUnit AnyValue() {
			return new PatternUnit(null);
		}

		private PatternUnit(byte? unit) {
			_patternByte = unit;
		}

		public bool Match(byte value) {
			if(!_patternByte.HasValue) {
				return true;
			} else {
				return _patternByte.Value == value;
			}
		}
	}

	public struct Signature {
		public PatternUnit[] Pattern { get; private set; }

		public Signature(PatternUnit[] pattern) {
			Pattern = pattern;
		}

		public static Signature From(byte[] bytes) {
			return new Signature(bytes.Select((val) => PatternUnit.ExactValue(val)).ToArray());
		}
	}
}