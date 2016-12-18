using System;

namespace Flac {
	internal static class Crc16 {

		private const ushort Polynomial = 0x8005;
		
		private static ushort[] Table = new ushort[256];

		internal static ushort ComputeHash(byte[] bytes, int offset, int count) {
			unchecked {
				ushort crc = 0;
				for (int i = offset; i < offset + count; i++) {
					crc = (ushort)((crc << 8) ^ Table[((crc >> 8) ^ bytes[i])]);
				}
				return crc;
			}
		}

		static Crc16() {
			unchecked {
				for (int i = 0; i < 256; i++) {
					ushort a = 0;
					ushort b = (ushort)(i << 8);
					for (int j = 0; j < 8; j++) {
						if (((a ^ b) & 0x8000) != 0) {
							a = (ushort)((a << 1) ^ Polynomial);
						} else {
							a <<= 1;
						}
						b <<= 1;
					}
					Table[i] = a;
				}
			}
		}
		
	}
}