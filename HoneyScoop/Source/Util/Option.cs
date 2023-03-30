namespace HoneyScoop.Util; 

/// <summary>
/// Essentially the same as <see cref="Nullable{T}"/>... But maybe slightly faster due to zero allocations...
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Option<T> where T: struct {
	private readonly T _value;
	private readonly bool _some;
	
	private Option(T value) {
		_value = value;
		_some = true;
	}

	internal static Option<T> Some(T value) {
		return new Option<T>(value);
	}

	internal static Option<T> None() {
		return new Option<T>(new T());
	}

	internal T Unwrap() {
		if(!_some) {
			throw new ArgumentNullException();
		}

		return _value;
	}

	internal T UnwrapOrDefault() {
		return _value;
	}

	internal bool IsSome() {
		return _some;
	}
}