using System;

namespace Flac {
	internal static class Crc8 {

		private const byte Polynomial = 0x07;
		
		private static byte[] Table = new byte[256];

		internal static byte ComputeHash(byte[] bytes, int offset, int count) {
			unchecked {
				byte crc = 0;
				for (int i = offset; i < offset + count; i++) {
					crc = Table[crc ^ bytes[i]];
				}
				return crc;
			}
		}

		static Crc8() {
			unchecked {
				for (int i = 0; i < 256; i++) {
					int a = i;
					for (int j = 0; j < 8; j++) {
						if ((a & 0x80) != 0) {
							a = (a << 1) ^ Polynomial;
						} else {
							a <<= 1;
						}
					}
					Table[i] = (byte)a;
				}
			}
		}
		
	}
}