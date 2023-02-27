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
}