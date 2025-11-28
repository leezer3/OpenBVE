using System;
using System.IO;
using System.Linq;
using OpenBveApi.Math;

namespace OpenBveApi
{
	/// <summary>Provides extension methods for working with binary data</summary>
	public static class BinaryExtensions
	{
		private static byte[] Reverse(this byte[] bytes, Endianness endianness)
		{
			if (BitConverter.IsLittleEndian ^ endianness == Endianness.Little)
			{
				return bytes.Reverse().ToArray();
			}

			return bytes;
		}

		/// <summary>Reads a UInt16 with the specified endianness</summary>
		/// <param name="reader">The binary reader</param>
		/// <param name="endianness">The endianness of the stored number</param>
		public static ushort ReadUInt16(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToUInt16(reader.ReadBytes(sizeof(ushort)).Reverse(endianness), 0);
		}

		/// <summary>Reads a Int16 with the specified endianness</summary>
		/// <param name="reader">The binary reader</param>
		/// <param name="endianness">The endianness of the stored number</param>
		public static short ReadInt16(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToInt16(reader.ReadBytes(sizeof(short)).Reverse(endianness), 0);
		}

		/// <summary>Reads a UInt32 with the specified endianness</summary>
		/// <param name="reader">The binary reader</param>
		/// <param name="endianness">The endianness of the stored number</param>
		public static uint ReadUInt32(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToUInt32(reader.ReadBytes(sizeof(uint)).Reverse(endianness), 0);
		}

		/// <summary>Reads a Int32 with the specified endianness</summary>
		/// <param name="reader">The binary reader</param>
		/// <param name="endianness">The endianness of the stored number</param>
		public static int ReadInt32(this BinaryReader reader, Endianness endianness)
		{
			return BitConverter.ToInt32(reader.ReadBytes(sizeof(int)).Reverse(endianness), 0);
		}

		/// <summary>Gets a UInt32 from the supplied byte array with the specified endianness</summary>
		/// <param name="bytes">The byte array</param>
		/// <param name="startIndex">The start index</param>
		/// <param name="endianness">The endinaness of the UInt32</param>
		public static uint GetUInt32(byte[] bytes, int startIndex, Endianness endianness)
		{
			byte[] newBytes = bytes.Skip(startIndex).Take(4).ToArray();
			newBytes = newBytes.Reverse(endianness);
			return BitConverter.ToUInt32(newBytes, 0);
		}
	}

	/// <summary>Provides extension methods for working with little-endian binary data</summary>
	public static class LittleEndianBinaryExtensions
	{
		/*
		 * Couple of little helper methods
		 * This is faster than using the BitConvertor as we don't have to init a new class every time we call it
		 */

		/// <summary>Gets a short from the specified offset in a byte array</summary>
		/// <remarks>Little-endian</remarks>
		/// <param name="byteArray">The byte array</param>
		/// <param name="offset">The starting offset of the short</param>
		public static short ToShort(byte[] byteArray, int offset)
		{
			short number = byteArray[offset + 1];
			number <<= 4;
			number += byteArray[offset];
			return number;
		}

		/// <summary>Gets an Int16 from the specified offset in a byte array</summary>
		/// <remarks>Little-endian</remarks>
		/// <param name="byteArray">The byte array</param>
		/// <param name="offset">The starting offset of the Int16</param>
		public static int ToInt16(byte[] byteArray, int offset)
		{
			return byteArray[offset] | (byteArray[offset + 1] << 8);
		}

		/// <summary>Gets an Int32 from the specified offset in a byte array</summary>
		/// <remarks>Little-endian</remarks>
		/// <param name="byteArray">The byte array</param>
		/// <param name="offset">The starting offset of the Int32</param>
		public static int ToInt32(byte[] byteArray, int offset)
		{
			return (byteArray[offset] & 0xFF) | ((byteArray[offset + 1] & 0xFF) << 8) | ((byteArray[offset + 2] & 0xFF) << 16) | ((byteArray[offset + 3] & 0xFF) << 24);
		}

		/// <summary>Gets a little-endian UInt64 from the specified offset in a byte array</summary>
		/// <param name="buffer">The byte array</param>
		/// <param name="startIndex">The starting offset of the UInt6</param>
		/// <returns></returns>
		public static ulong ToUInt64(byte[] buffer, int startIndex)
		{
			ulong ret = 0;
			for (int i = 0; i < 8; i++)
			{
				ret = unchecked((ret << 8) | buffer[startIndex + 8 - 1 - i]);
			}

			return ret;
		}
	}
}
