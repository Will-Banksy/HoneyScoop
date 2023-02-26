using System.Runtime.InteropServices;

namespace HoneyScoop.Util;

internal static class Helper {
	/// <summary>
	/// Efficiently converts a byte span into a uint
	/// </summary>
	/// <param name="bytes"></param>
	/// <returns></returns>
	internal static uint FromBigEndian(ReadOnlySpan<byte> bytes) {
		uint res = bytes[0];
		res <<= 8;
		res |= bytes[1];
		res <<= 8;
		res |= bytes[2];
		res <<= 8;
		res |= bytes[3];

		return res;
	}

	internal static uint Crc32(ReadOnlySpan<byte> data) {
		const uint divisor = 0x04c11db7;
		List<byte> dividend = new List<byte>(data.Length + 32);
		Span<byte> divSpan = CollectionsMarshal.AsSpan(dividend);
		return (uint)divSpan.Length; // TODO: Continue. Maybe not by allocating a new array for data + 32 0s, maybe just an if or something idk. Maybe google C# CRC impls
	}
}