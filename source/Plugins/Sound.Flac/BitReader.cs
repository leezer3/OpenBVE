using System.IO;

namespace Flac
{
	/// <summary>Represents a bit reader using a big-endian bit and byte order.</summary>
	internal class BitReader
	{

		// --- members ---

		/// <summary>The array of bytes from which is read.</summary>
		internal readonly byte[] Bytes;

		/// <summary>The byte position from which will be read next.</summary>
		internal int BytePosition;

		/// <summary>The bit position in the current byte from which will be read next. Values can range from 7 (most-significant bit) to 0 (least-significant bit).</summary>
		private int BitPosition;


		// --- constructors ---

		/// <summary>Creates a new bit reader.</summary>
		/// <param name="bytes">The array of bytes from which is read.</param>
		internal BitReader(byte[] bytes)
		{
			Bytes = bytes;
			BytePosition = 0;
			BitPosition = 7;
		}


		// --- functions ---

		/// <summary>Checks whether the end of the stream has been reached.</summary>
		/// <returns>Whether the end of the stream has been reached.</returns>
		internal bool EndOfStream()
		{
			return BytePosition == Bytes.Length;
		}

		/// <summary>Aligns the reader with the next byte boundary.</summary>
		internal void Align()
		{
			if (BitPosition != 7)
			{
				BitPosition = 7;
				BytePosition++;
			}
		}

		/// <summary>Reads a single bit.</summary>
		/// <returns>The bit.</returns>
		internal uint ReadBit()
		{
			if (BitPosition == 0)
			{
				BitPosition = 7;
				return (uint) Bytes[BytePosition++] & 1;
			}
			return ((uint) Bytes[BytePosition] >> (BitPosition--)) & 1;
		}

		/// <summary>Reads the specified number of bits.</summary>
		/// <param name="number">The number of bits. Must be between 0 and 32.</param>
		/// <returns>The bits.</returns>
		internal uint ReadBits(int number)
		{
			if (BitPosition == 7)
			{
				/* The start is on a byte boundary. */
				if ((number & 7) == 0)
				{
					/* The start and end are on a byte boundary. */
					if (number < 16)
					{
						if (number == 0)
						{
							return 0;
						}
						return Bytes[BytePosition++];
					}
					if (number == 16)
					{
						return ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
					}
					if (number == 24)
					{
						return ((uint) Bytes[BytePosition++] << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
					}
					return ((uint) Bytes[BytePosition++] << 24) | ((uint) Bytes[BytePosition++] << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
				}

				/* The start is on a byte boundary, but the end is not. */
				if (number < 16)
				{
					if (number < 8)
					{
						BitPosition = 7 - number;
						return (uint) Bytes[BytePosition] >> (8 - number);
					}
					BitPosition = 15 - number;
					return ((uint) Bytes[BytePosition++] << (number - 8)) | ((uint) Bytes[BytePosition] >> (16 - number));
				}

				if (number < 24)
				{
					BitPosition = 23 - number;
					return ((uint) Bytes[BytePosition++] << (number - 8)) | ((uint) Bytes[BytePosition++] << (number - 16)) | ((uint) Bytes[BytePosition] >> (24 - number));
				}

				BitPosition = 31 - number;
				return ((uint) Bytes[BytePosition++] << (number - 8)) | ((uint) Bytes[BytePosition++] << (number - 16)) | ((uint) Bytes[BytePosition++] << (number - 24)) | ((uint) Bytes[BytePosition] >> (32 - number));
			}

			/* The start is not on a byte boundary. */
			if (((number - BitPosition - 1) & 7) == 0)
			{
				/* The start is not on a byte boundary, but the end is. */
				if (number < 16)
				{
					if (number < 8)
					{
						BitPosition = 7;
						return Bytes[BytePosition++] & (((uint) 1 << number) - 1);
					}
					BitPosition = 7;
					return ((Bytes[BytePosition++] & (((uint) 1 << (number - 8)) - 1)) << 8) | Bytes[BytePosition++];
				}

				if (number < 24)
				{
					BitPosition = 7;
					return ((Bytes[BytePosition++] & (((uint) 1 << (number - 16)) - 1)) << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
				}
				BitPosition = 7;
				return ((Bytes[BytePosition++] & (((uint) 1 << (number - 24)) - 1)) << 24) | ((uint) Bytes[BytePosition++] << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
			}

			/* Neither start nor end are on a byte boundary. */
			int bytes = (number - BitPosition + 15) >> 3;
			if (bytes < 3)
			{
				if (bytes == 1)
				{
					BitPosition -= number;
					return ((uint) Bytes[BytePosition] >> (BitPosition + 1)) & (((uint) 1 << number) - 1);
				}
				BitPosition -= number - 8;
				return ((Bytes[BytePosition++] & (((uint) 1 << (BitPosition + number - 7)) - 1)) << (7 - BitPosition)) | ((uint) Bytes[BytePosition] >> (BitPosition + 1));
			}

			if (bytes < 5)
			{
				if (bytes == 3)
				{
					BitPosition -= number - 16;
					return ((Bytes[BytePosition++] & (((uint) 1 << (BitPosition + number - 15)) - 1)) << (15 - BitPosition)) | ((uint) Bytes[BytePosition++] << (7 - BitPosition)) | ((uint) Bytes[BytePosition] >> (BitPosition + 1));
				}
				BitPosition -= number - 24;
				return ((Bytes[BytePosition++] & (((uint) 1 << (BitPosition + number - 23)) - 1)) << (23 - BitPosition)) | ((uint) Bytes[BytePosition++] << (15 - BitPosition)) | ((uint) Bytes[BytePosition++] << (7 - BitPosition)) | ((uint) Bytes[BytePosition] >> (BitPosition + 1));
			}
			BitPosition -= number - 32;
			return ((Bytes[BytePosition++] & (((uint) 1 << (BitPosition + number - 31)) - 1)) << (31 - BitPosition)) | ((uint) Bytes[BytePosition++] << (23 - BitPosition)) | ((uint) Bytes[BytePosition++] << (15 - BitPosition)) | ((uint) Bytes[BytePosition++] << (7 - BitPosition)) | ((uint) Bytes[BytePosition] >> (BitPosition + 1));
		}

		/// <summary>Reads an unsigned 8-bit integer.</summary>
		/// <returns>The unsigned 8-bit integer.</returns>
		internal uint ReadByte()
		{
			if (BitPosition == 7)
			{
				return Bytes[BytePosition++];
			}
			return ReadBits(8);
		}

		/// <summary>Reads an unsigned 16-bit integer.</summary>
		/// <returns>The unsigned 16-bit integer.</returns>
		internal uint ReadUInt16BE()
		{
			if (BitPosition == 7)
			{
				return ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
			}
			return ReadBits(16);
		}

		/// <summary>Reads an unsigned 24-bit integer.</summary>
		/// <returns>The unsigned 24-bit integer.</returns>
		internal uint ReadUInt24BE()
		{
			if (BitPosition == 7)
			{
				return ((uint) Bytes[BytePosition++] << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
			}
			return ReadBits(24);
		}

		/// <summary>Reads an unsigned 32-bit integer.</summary>
		/// <returns>The unsigned 32-bit integer.</returns>
		internal uint ReadUInt32BE()
		{
			if (BitPosition == 7)
			{
				return ((uint) Bytes[BytePosition++] << 24) | ((uint) Bytes[BytePosition++] << 16) | ((uint) Bytes[BytePosition++] << 8) | Bytes[BytePosition++];
			}
			return ReadBits(32);
		}

		/// <summary>Reads the specified number of bytes.</summary>
		/// <param name="number">The number of bytes.</param>
		/// <returns>The bytes.</returns>
		internal byte[] ReadBytes(int number)
		{
			if (BitPosition == 7)
			{
				byte[] bytes = new byte[number];
				for (int i = 0; i < number; i++)
				{
					bytes[i] = Bytes[BytePosition++];
				}
				return bytes;
			}
			else
			{
				byte[] bytes = new byte[number];
				for (int i = 0; i < number; i++)
				{
					bytes[i] = (byte) ReadBits(8);
				}
				return bytes;
			}
		}

		/// <summary>Reads a unary-encoded integer according to the scheme 0=0, 1=11, 2=101, 3=1001, 4=10001, etc.</summary>
		/// <returns>The integer.</returns>
		internal uint ReadUnaryEncodedInteger()
		{
			if (ReadBit() == 0)
			{
				return 0;
			}

			uint value = 1;
			while (ReadBit() == 0)
			{
				value++;
			}
			return value;
		}

		/// <summary>Reads a signed rice-encoded integer.</summary>
		/// <param name="riceParameter">The rice parameter.</param>
		/// <returns>The signed integer.</returns>
		internal int ReadRiceEncodedInteger(int riceParameter)
		{
			uint quotient = 0;
			while (ReadBit() == 0)
			{
				quotient++;
			}
			uint mod = ReadBits(riceParameter);
			uint value = (quotient << riceParameter) | mod;
			if ((value & 1) == 0)
			{
				return (int) (value >> 1);
			}
			return ~(int) (value >> 1);
		}

		/// <summary>Reads an unsigned integer encoded according to the extended UTF-8 scheme with up to 36 bits.</summary>
		/// <returns>The integer.</returns>
		internal ulong ReadUTF8EncodedInteger()
		{
			ulong a = ReadByte();
			if ((a & 0x80) == 0)
			{
				return a & 0x7F;
			}
			if ((a & 0xE0) == 0xC0)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((a & 0x1F) << 6) | (b & 0x3F);
			}

			if ((a & 0xF0) == 0xE0)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong c = ReadByte();
				if ((c & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((a & 0x0F) << 12) | ((b & 0x3F) << 6) | (c & 0x3F);
			}

			if ((a & 0xF8) == 0xF0)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong c = ReadByte();
				if ((c & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong d = ReadByte();
				if ((d & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((a & 0x07) << 18) | ((b & 0x3F) << 12) | ((c & 0x3F) << 6) | (d & 0x3F);
			}

			if ((a & 0xFC) == 0xF8)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong c = ReadByte();
				if ((c & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong d = ReadByte();
				if ((d & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong e = ReadByte();
				if ((e & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((a & 0x03) << 24) | ((b & 0x3F) << 18) | ((c & 0x3F) << 12) | ((d & 0x3F) << 6) | (e & 0x3F);
			}
			if ((a & 0xFE) == 0xFC)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong c = ReadByte();
				if ((c & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong d = ReadByte();
				if ((d & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong e = ReadByte();
				if ((e & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong f = ReadByte();
				if ((f & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((a & 0x01) << 30) | ((b & 0x3F) << 24) | ((c & 0x3F) << 18) | ((d & 0x3F) << 12) | ((e & 0x3F) << 6) | (f & 0x3F);
			}
			if (a == 0xFE)
			{
				ulong b = ReadByte();
				if ((b & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong c = ReadByte();
				if ((c & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong d = ReadByte();
				if ((d & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong e = ReadByte();
				if ((e & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong f = ReadByte();
				if ((f & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				ulong g = ReadByte();
				if ((g & 0xC0) != 0x80)
				{
					throw new InvalidDataException();
				}
				return ((b & 0x3F) << 30) | ((c & 0x3F) << 24) | ((d & 0x3F) << 18) | ((e & 0x3F) << 12) | ((f & 0x3F) << 6) | (g & 0x3F);
			}
			throw new InvalidDataException();
		}
	}
}
