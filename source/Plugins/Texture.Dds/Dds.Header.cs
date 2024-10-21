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
		public uint sizeorpitch;
		public int depth;
		public uint mipmapcount;
		public uint alphabitdepth;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PixelFormat
		{
			public uint flags;
			public FourCC fourcc;
			public int rgbbitcount;
			public uint rbitmask;
			public uint gbitmask;
			public uint bbitmask;
			public uint alphabitmask;
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
			if (pixelFormat.rgbbitcount != 32)
			{
				return false;
			}

			if (pixelFormat.rbitmask == 0x3FF00000 && pixelFormat.gbitmask == 0x000FFC00 && pixelFormat.bbitmask == 0x000003FF
			    && pixelFormat.alphabitmask == 0xC0000000)
			{
				// a2b10g10r10 format
				return true;
			}

			if (pixelFormat.rbitmask == 0x000003FF && pixelFormat.gbitmask == 0x000FFC00 && pixelFormat.bbitmask == 0x3FF00000
			    && pixelFormat.alphabitmask == 0xC0000000)
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
			sizeorpitch = reader.ReadUInt32();
			depth = (int)reader.ReadUInt32();
			mipmapcount = reader.ReadUInt32();
			alphabitdepth = reader.ReadUInt32();

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

			pixelFormat.rgbbitcount = rgbbitcount;
			pixelFormat.rbitmask = reader.ReadUInt32();
			pixelFormat.gbitmask = reader.ReadUInt32();
			pixelFormat.bbitmask = reader.ReadUInt32();
			pixelFormat.alphabitmask = reader.ReadUInt32();
			ddscaps.caps1 = reader.ReadUInt32();
			ddscaps.caps2 = reader.ReadUInt32();
			ddscaps.caps3 = reader.ReadUInt32();
			ddscaps.caps4 = reader.ReadUInt32();
			texturestage = reader.ReadUInt32();
		}
	}
}
