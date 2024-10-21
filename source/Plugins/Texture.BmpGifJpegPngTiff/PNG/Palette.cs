//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Colors;
using System;

namespace Plugin.PNG
{
	internal class Palette
	{
		/// <summary>The array of colors</summary>
		internal Color32[] Colors;

		/// <summary>Creates the color palette from a PLTE chunk</summary>
		/// <param name="chunkBytes">The PLTE chunk</param>
		internal Palette(byte[] chunkBytes)
		{
			Colors = new Color32[chunkBytes.Length / 3];
			for (int i = 0; i < chunkBytes.Length; i+= 3)
			{
				// The PLTE chunk contains colors without alpha values
				Colors[i / 3] = new Color32(chunkBytes[i], chunkBytes[i + 1], chunkBytes[i + 2], 255);
			}
		}

		/// <summary>Sets the alpha values for colors in a color palette</summary>
		/// <param name="chunkBytes">The tRNS chunk</param>
		internal void SetAlphaValues(byte[] chunkBytes)
		{
			// Alpha chunk length can be greater than the number of colors in a pallette
			// e.g. BveTs\Scenarios\t5\structures\markers\ATS0.png
			for (int i = 0; i < Math.Min(chunkBytes.Length, Colors.Length); i++)
			{
				Colors[i].A = chunkBytes[i];
			}
		}
	}
}
