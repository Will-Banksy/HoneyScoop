namespace HoneyScoop.Util; 

/// <summary>
/// Super simple hash function
/// </summary>
public struct CyclicPolynomialHash {
	private ulong _hash;
	private readonly int _length;
	
	/// <summary>
	/// Creates the hash from param bytes, initialising the hash to that value
	/// </summary>
	/// <param name="bytes"></param>
	internal CyclicPolynomialHash(ReadOnlySpan<byte> bytes) {
		_hash = 0;
		_length = bytes.Length;

		for(int i = 0; i < bytes.Length; i++) {
			_hash ^= Rotl(_length - 1 - i, bytes[i]);
		}
	}

	internal void Update(byte entering, byte leaving) {
		_hash = Rotl(1, _hash ^ Rotl(_length - 1, leaving)) ^ entering;
	}

	private ulong Rotl(int n, ulong k) {
		n %= sizeof(ulong);
		return ulong.RotateLeft(k, n);
	}

	internal bool Equals(CyclicPolynomialHash other) {
		return _hash == other._hash && _length == other._length;
	}

	public override bool Equals(object? obj) {
		return obj is CyclicPolynomialHash other && Equals(other);
	}

	public override int GetHashCode() {
		return HashCode.Combine(_hash, _length);
	}
}