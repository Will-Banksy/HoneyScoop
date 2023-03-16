namespace HoneyScoop.Util; 

public static class Crc32 {
	private static readonly uint[] CrcTable = MakeCrc32Table();

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <returns>The CRC32 table of the CRC32 of each 8-bit value</returns>
	private static uint[] MakeCrc32Table() {
		var table = new uint[256];

		for(int n = 0; n < 256; n++) {
			uint c = (uint)n;
			for(int k = 0; k < 8; k++) {
				if((c & 0x1) == 1) {
					c = 0xedb88320 ^ (c >> 1);
				} else {
					c = c >> 1;
				}
			}

			table[n] = c;
		}

		return table;
	}

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <param name="crc">The current CRC32</param>
	/// <param name="buffer">The buffer to update the CRC32 with</param>
	/// <returns>The updated CRC32</returns>
	internal static uint UpdateCrc32(uint crc, ReadOnlySpan<byte> buffer) {
		uint c = crc;

		for(int n = 0; n < buffer.Length; n++) {
			c = CrcTable[(c ^ buffer[n]) & 0xff] ^ (c >> 8);
		}
		return c;
	}

	/// <summary>
	/// Adapted from <a href="https://www.w3.org/TR/png/#D-CRCAppendix">https://www.w3.org/TR/png/#D-CRCAppendix</a>
	/// </summary>
	/// <param name="buffer">The buffer to calculate the CRC32 of</param>
	/// <returns>The CRC32 of the provided buffer</returns>
	internal static uint CalculateCrc32(ReadOnlySpan<byte> buffer) {
		return UpdateCrc32(uint.MaxValue, buffer) ^ uint.MaxValue;
	}
}