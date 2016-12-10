using System;
using System.IO;

namespace Flac {
	/// <summary>Represents a bit reader using a big-endian bit and byte order.</summary>
	internal class BitReader {
		
		// --- members ---
		
		/// <summary>The array of bytes from which is read.</summary>
		internal byte[] Bytes;
		
		/// <summary>The byte position from which will be read next.</summary>
		internal int BytePosition;
		
		/// <summary>The bit position in the current byte from which will be read next. Values can range from 7 (most-significant bit) to 0 (least-significant bit).</summary>
		internal int BitPosition;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new bit reader.</summary>
		/// <param name="bytes">The array of bytes from which is read.</param>
		internal BitReader(byte[] bytes) {
			this.Bytes = bytes;
			this.BytePosition = 0;
			this.BitPosition = 7;
		}
		
		
		// --- functions ---

		/// <summary>Checks whether the end of the stream has been reached.</summary>
		/// <returns>Whether the end of the stream has been reached.</returns>
		internal bool EndOfStream() {
			return this.BytePosition == this.Bytes.Length;
		}
		
		/// <summary>Aligns the reader with the next byte boundary.</summary>
		internal void Align() {
			if (this.BitPosition != 7) {
				this.BitPosition = 7;
				this.BytePosition++;
			}
		}

		/// <summary>Reads a single bit.</summary>
		/// <returns>The bit.</returns>
		internal uint ReadBit() {
			if (this.BitPosition == 0) {
				this.BitPosition = 7;
				return (uint)this.Bytes[this.BytePosition++] & 1;
			} else {
				return ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition--)) & 1;
			}
		}

		/// <summary>Reads the specified number of bits.</summary>
		/// <param name="number">The number of bits. Must be between 0 and 32.</param>
		/// <returns>The bits.</returns>
		internal uint ReadBits(int number) {
			if (this.BitPosition == 7) {
				/* The start is on a byte boundary. */
				if ((number & 7) == 0) {
					/* The start and end are on a byte boundary. */
					if (number < 16) {
						if (number == 0) {
							return 0;
						} else {
							return (uint)this.Bytes[this.BytePosition++];
						}
					} else {
						if (number == 16) {
							return ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
						} else if (number == 24) {
							return ((uint)this.Bytes[this.BytePosition++] << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
						} else {
							return ((uint)this.Bytes[this.BytePosition++] << 24) | ((uint)this.Bytes[this.BytePosition++] << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
						}
					}
				} else {
					/* The start is on a byte boundary, but the end is not. */
					if (number < 16) {
						if (number < 8) {
							this.BitPosition = 7 - number;
							return (uint)this.Bytes[this.BytePosition] >> (8 - number);
						} else {
							this.BitPosition = 15 - number;
							return ((uint)this.Bytes[this.BytePosition++] << (number - 8)) | ((uint)this.Bytes[this.BytePosition] >> (16 - number));
						}
					} else {
						if (number < 24) {
							this.BitPosition = 23 - number;
							return ((uint)this.Bytes[this.BytePosition++] << (number - 8)) | ((uint)this.Bytes[this.BytePosition++] << (number - 16)) | ((uint)this.Bytes[this.BytePosition] >> (24 - number));
						} else {
							this.BitPosition = 31 - number;
							return ((uint)this.Bytes[this.BytePosition++] << (number - 8)) | ((uint)this.Bytes[this.BytePosition++] << (number - 16)) | ((uint)this.Bytes[this.BytePosition++] << (number - 24)) | ((uint)this.Bytes[this.BytePosition] >> (32 - number));
						}
					}
				}
			} else {
				/* The start is not on a byte boundary. */
				if (((number - this.BitPosition - 1) & 7) == 0) {
					/* The start is not on a byte boundary, but the end is. */
					if (number < 16) {
						if (number < 8) {
							this.BitPosition = 7;
							return (uint)this.Bytes[this.BytePosition++] & (((uint)1 << number) - 1);
						} else {
							this.BitPosition = 7;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (number - 8)) - 1)) << 8) | (uint)this.Bytes[this.BytePosition++];
						}
					} else {
						if (number < 24) {
							this.BitPosition = 7;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (number - 16)) - 1)) << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
						} else {
							this.BitPosition = 7;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (number - 24)) - 1)) << 24) | ((uint)this.Bytes[this.BytePosition++] << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
						}
					}
				} else {
					/* Neither start nor end are on a byte boundary. */
					int bytes = (number - this.BitPosition + 15) >> 3;
					if (bytes < 3) {
						if (bytes == 1) {
							this.BitPosition -= number;
							return ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition + 1)) & (((uint)1 << number) - 1);
						} else {
							this.BitPosition -= number - 8;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (this.BitPosition + number - 7)) - 1)) << (7 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition + 1));
						}
					} else if (bytes < 5) {
						if (bytes == 3) {
							this.BitPosition -= number - 16;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (this.BitPosition + number - 15)) - 1)) << (15 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (7 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition + 1));
						} else {
							this.BitPosition -= number - 24;
							return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (this.BitPosition + number - 23)) - 1)) << (23 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (15 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (7 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition + 1));
						}
					} else {
						this.BitPosition -= number - 32;
						return (((uint)this.Bytes[this.BytePosition++] & (((uint)1 << (this.BitPosition + number - 31)) - 1)) << (31 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (23 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (15 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition++] << (7 - this.BitPosition)) | ((uint)this.Bytes[this.BytePosition] >> (this.BitPosition + 1));
					}
				}
			}
		}

		/// <summary>Reads an unsigned 8-bit integer.</summary>
		/// <returns>The unsigned 8-bit integer.</returns>
		internal uint ReadByte() {
			if (this.BitPosition == 7) {
				return this.Bytes[this.BytePosition++];
			} else {
				return this.ReadBits(8);
			}
		}

		/// <summary>Reads an unsigned 16-bit integer.</summary>
		/// <returns>The unsigned 16-bit integer.</returns>
		internal uint ReadUInt16BE() {
			if (this.BitPosition == 7) {
				return ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
			} else {
				return this.ReadBits(16);
			}
		}
		
		/// <summary>Reads an unsigned 24-bit integer.</summary>
		/// <returns>The unsigned 24-bit integer.</returns>
		internal uint ReadUInt24BE() {
			if (this.BitPosition == 7) {
				return ((uint)this.Bytes[this.BytePosition++] << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
			} else {
				return this.ReadBits(24);
			}
		}

		/// <summary>Reads an unsigned 32-bit integer.</summary>
		/// <returns>The unsigned 32-bit integer.</returns>
		internal uint ReadUInt32BE() {
			if (this.BitPosition == 7) {
				return ((uint)this.Bytes[this.BytePosition++] << 24) | ((uint)this.Bytes[this.BytePosition++] << 16) | ((uint)this.Bytes[this.BytePosition++] << 8) | (uint)this.Bytes[this.BytePosition++];
			} else {
				return this.ReadBits(32);
			}
		}
		
		/// <summary>Reads the specified number of bytes.</summary>
		/// <param name="number">The number of bytes.</param>
		/// <returns>The bytes.</returns>
		internal byte[] ReadBytes(int number) {
			if (this.BitPosition == 7) {
				byte[] bytes = new byte[number];
				for (int i = 0; i < number; i++) {
					bytes[i] = this.Bytes[this.BytePosition++];
				}
				return bytes;
			} else {
				byte[] bytes = new byte[number];
				for (int i = 0; i < number; i++) {
					bytes[i] = (byte)this.ReadBits(8);
				}
				return bytes;
			}
		}
		
		/// <summary>Reads a unary-encoded integer according to the scheme 0=0, 1=11, 2=101, 3=1001, 4=10001, etc.</summary>
		/// <returns>The integer.</returns>
		internal uint ReadUnaryEncodedInteger() {
			if (this.ReadBit() == 0) {
				return 0;
			} else {
				uint value = 1;
				while (this.ReadBit() == 0) {
					value++;
				}
				return value;
			}
		}
		/// <summary>Reads a signed rice-encoded integer.</summary>
		/// <param name="riceParameter">The rice parameter.</param>
		/// <returns>The signed integer.</returns>
		internal int ReadRiceEncodedInteger(int riceParameter) {
			uint quotient = 0;
			while (this.ReadBit() == 0) {
				quotient++;
			}
			uint mod = this.ReadBits(riceParameter);
			uint value = (quotient << (int)riceParameter) | mod;
			if ((value & 1) == 0) {
				return (int)(value >> 1);
			} else {
				return ~(int)(value >> 1);
			}
		}
		
		/// <summary>Reads an unsigned integer encoded according to the extended UTF-8 scheme with up to 36 bits.</summary>
		/// <returns>The integer.</returns>
		internal ulong ReadUTF8EncodedInteger() {
			ulong a = (ulong)this.ReadByte();
			if ((a & 0x80) == 0) {
				return a & 0x7F;
			} else if ((a & 0xE0) == 0xC0) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((a & 0x1F) << 6) | (b & 0x3F);
			} else if ((a & 0xF0) == 0xE0) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong c = (ulong)this.ReadByte();
				if ((c & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((a & 0x0F) << 12) | ((b & 0x3F) << 6) | (c & 0x3F);
			} else if ((a & 0xF8) == 0xF0) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong c = (ulong)this.ReadByte();
				if ((c & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong d = (ulong)this.ReadByte();
				if ((d & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((a & 0x07) << 18) | ((b & 0x3F) << 12) | ((c & 0x3F) << 6) | (d & 0x3F);
			} else if ((a & 0xFC) == 0xF8) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong c = (ulong)this.ReadByte();
				if ((c & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong d = (ulong)this.ReadByte();
				if ((d & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong e = (ulong)this.ReadByte();
				if ((e & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((a & 0x03) << 24) | ((b & 0x3F) << 18) | ((c & 0x3F) << 12) | ((d & 0x3F) << 6) | (e & 0x3F);
			} else if ((a & 0xFE) == 0xFC) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong c = (ulong)this.ReadByte();
				if ((c & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong d = (ulong)this.ReadByte();
				if ((d & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong e = (ulong)this.ReadByte();
				if ((e & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong f = (ulong)this.ReadByte();
				if ((f & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((a & 0x01) << 30) | ((b & 0x3F) << 24) | ((c & 0x3F) << 18) | ((d & 0x3F) << 12) | ((e & 0x3F) << 6) | (f & 0x3F);
			} else if (a == 0xFE) {
				ulong b = (ulong)this.ReadByte();
				if ((b & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong c = (ulong)this.ReadByte();
				if ((c & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong d = (ulong)this.ReadByte();
				if ((d & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong e = (ulong)this.ReadByte();
				if ((e & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong f = (ulong)this.ReadByte();
				if ((f & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				ulong g = (ulong)this.ReadByte();
				if ((g & 0xC0) != 0x80) {
					throw new InvalidDataException();
				}
				return ((b & 0x3F) << 30) | ((c & 0x3F) << 24) | ((d & 0x3F) << 18) | ((e & 0x3F) << 12) | ((f & 0x3F) << 6) | (g & 0x3F);
			} else {
				throw new InvalidDataException();
			}
		}
		
	}
}