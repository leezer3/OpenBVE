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
}
