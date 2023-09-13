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

namespace Plugin.PNG
{
	internal class Adam7
	{
		/// <summary>The grid overlayed onto the image</summary>
		private static readonly int[][] Grid =
		{
            new []{ 0 }, 
            new []{ 0 },
            new []{ 4 },
            new []{ 0, 4 },
            new []{ 2, 6 },
            new[] { 0, 2, 4, 6 },
            new[] { 1, 3, 5, 7 }
        };

		/// <summary>The column indicies</summary>
        private static readonly int[][] Columns =
        {
            new []{ 0 },
            new []{ 4 },
            new []{ 0, 4 },
            new []{ 2, 6 },
            new []{ 0, 2, 4, 6 },
            new []{ 1, 3, 5, 7 },
            new []{ 0, 1, 2, 3, 4, 5, 6, 7 }
        };

		/// <summary>Gets the number of scanlines contained in a pass</summary>
		/// <param name="height">The height of the image</param>
		/// <param name="pass">The pass number</param>
		/// <returns></returns>
        public static int GetNumberOfScanlinesForPass(int height, int pass)
        {
	        if (height % 8 == 0)
            {
                return Grid[pass].Length * (height / 8);
            }

            int additionalLines = 0;
            for (int i = 0; i < Grid[pass].Length; i++)
            {
                if (Grid[pass][i] < height % 8)
                {
                    additionalLines++;
                }
            }

            return (Grid[pass].Length * (height / 8)) + additionalLines;
        }

		/// <summary>Gets the number of pixels in the scanline for a pass</summary>
		/// <param name="width">The image width</param>
		/// <param name="pass">The pass number</param>
		/// <returns></returns>
        public static int GetNumberOfPixelsInScanline(int width, int pass)
        {
	        if (width % 8 == 0)
            {
                return Columns[pass].Length * (width / 8);
            }

            int additionalColumns = 0;
            for (int i = 0; i < Columns[pass].Length; i++)
            {
                if (Columns[pass][i] < width % 8)
                {
                    additionalColumns++;
                }
            }

            return (Columns[pass].Length * (width / 8)) + additionalColumns;
        }

		/// <summary>Gets the true index for a pixel within the image</summary>
		/// <param name="pass">The pass number</param>
		/// <param name="scanlineIndex">The index of the scanline within the pass</param>
		/// <param name="indexInScanline">The index of the pixel within the scanline</param>
		/// <param name="X">The X-coordinate of the pixel</param>
		/// <param name="Y">The Y-coordinate of the pixel</param>
		public static void GetPixelIndexForScanlineInPass(int pass, int scanlineIndex, int indexInScanline, out int X, out int Y)
        {
	        int actualRow = scanlineIndex % Grid[pass].Length;
	        int actualCol = indexInScanline % Columns[pass].Length;
	        int precedingRows = 8 * (scanlineIndex / Grid[pass].Length);
	        int precedingCols = 8 * (indexInScanline / Columns[pass].Length);
			X = precedingCols + Columns[pass][actualCol];
	        Y = precedingRows + Grid[pass][actualRow];
        }
	}
}
