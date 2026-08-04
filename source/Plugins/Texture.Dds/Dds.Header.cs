/*
 * Copyright (C) 1999, 2000 NVIDIA Corporation
 * This file is provided without support, instruction, or implied warranty of any
 * kind.  NVIDIA makes no guarantee of its fitness for a particular purpose and is
 * not liable under any circumstances for any damages or loss whatsoever arising
 * from the use or inability to use this file or items derived from it.
 *
 * Converted to C#, assorted changes to make compatible with openBVE texture loading
 * Also some minor enum conversion & cleanup
 */

using System.IO;
using System.Runtime.InteropServices;

namespace Texture.Dds
{
	/// <summary>Represents the header for a DDS texture</summary>
	internal class DdsHeader
	{
		public uint flags;
		public int height;
		public int width;
		public uint sizeOrPitch;
		public int depth;
		public uint mipmapCount;
		public uint alphaBitDepth;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PixelFormat
		{
			public uint flags;
			public FourCC fourcc;
			public int rgbBitCount;
			public uint rBitmask;
			public uint gBitmask;
			public uint bBitmask;
			public uint alphaBitmask;
		}

		public PixelFormat pixelFormat;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct DdsCaps
		{
			public uint caps1;
			public uint caps2;
			public uint caps3;
			public uint caps4;
		}

		public DdsCaps ddscaps;
		public uint texturestage;

		internal bool Check16BitComponents()
		{
			if (pixelFormat.rgbBitCount != 32)
			{
				return false;
			}

			if (pixelFormat.rBitmask == 0x3FF00000 && pixelFormat.gBitmask == 0x000FFC00 && pixelFormat.bBitmask == 0x000003FF
			    && pixelFormat.alphaBitmask == 0xC0000000)
			{
				// a2b10g10r10 format
				return true;
			}

			if (pixelFormat.rBitmask == 0x000003FF && pixelFormat.gBitmask == 0x000FFC00 && pixelFormat.bBitmask == 0x3FF00000
			    && pixelFormat.alphaBitmask == 0xC0000000)
			{
				// a2r10g10b10 format
				return true;
			}

			return false;
		}

		internal DdsHeader(BinaryReader reader)
		{
			byte[] signature = reader.ReadBytes(4);
			if (!(signature[0] == 'D' && signature[1] == 'D' && signature[2] == 'S' && signature[3] == ' '))
			{
				throw new InvalidDataException("DDS Header invalid.");
			}

			if (reader.ReadUInt32() != 124)
			{
				throw new InvalidDataException("DDS Header size invalid.");
			}

			flags = reader.ReadUInt32();
			height = (int)reader.ReadUInt32();
			width = (int)reader.ReadUInt32();
			sizeOrPitch = reader.ReadUInt32();
			depth = (int)reader.ReadUInt32();
			mipmapCount = reader.ReadUInt32();
			alphaBitDepth = reader.ReadUInt32();

			for (int i = 0; i < 10; i++)
			{
				reader.ReadUInt32(); //Reserved 10 DWORD values : Microsoft documentation states unused
			}

			if (reader.ReadUInt32() != 32)
			{
				throw new InvalidDataException("Pixel Format size invalid.");
			}

			pixelFormat.flags = reader.ReadUInt32();
			pixelFormat.fourcc = (FourCC)reader.ReadUInt32();
			int rgbbitcount = (int)reader.ReadUInt32();
			if (rgbbitcount == 0)
			{
				/* Textures supplied with https://bowlroll.net/file/250304
				 * Possibly has something to do with the FourCC, but documentation is sparse
				 *
				 * Use 16 as a best guess sensible value for the minute
				*/
				rgbbitcount = 16;
			}

			pixelFormat.rgbBitCount = rgbbitcount;
			pixelFormat.rBitmask = reader.ReadUInt32();
			pixelFormat.gBitmask = reader.ReadUInt32();
			pixelFormat.bBitmask = reader.ReadUInt32();
			pixelFormat.alphaBitmask = reader.ReadUInt32();
			ddscaps.caps1 = reader.ReadUInt32();
			ddscaps.caps2 = reader.ReadUInt32();
			ddscaps.caps3 = reader.ReadUInt32();
			ddscaps.caps4 = reader.ReadUInt32();
			texturestage = reader.ReadUInt32();
		}
	}
}
