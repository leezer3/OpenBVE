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

// ReSharper disable InconsistentNaming
namespace Texture.Dds
{
	/// <summary>The known DDS FourCC values</summary>
	enum FourCC : uint
	{
		DXT1 = 0x31545844,
		DXT2 = 0x32545844,
		DXT3 = 0x33545844,
		DXT4 = 0x34545844,
		DXT5 = 0x35545844,
		ATI1 = 0x31495441,
		ATI2 = 0x32495441,
		RXGB = 0x42475852,
		DOLLARNULL = 0x24,
		oNULL = 0x6f,
		pNULL = 0x70,
		qNULL = 0x71,
		rNULL = 0x72,
		sNULL = 0x73,
		tNULL = 0x74
	}
}
