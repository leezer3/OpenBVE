//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, Kevin Weiner, The OpenBVE Project
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
//
// LZW decoder adapted from John Cristy's ImageMagick
// https://imagemagick.org/script/license.php

using System;
using System.Collections.Generic;
using System.IO;
using OpenBveApi.Math;

namespace Plugin.GIF
{ 
	internal partial class GifDecoder : IDisposable
	{
		internal static byte[] GIF87Header = { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };     // "GIF87a"
		internal static byte[] GIF89Header = { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };     // "GIF89a"

		protected Stream inStream;
		protected DecoderStatus status;
		/// <summary>The width of the image canvas</summary>
		protected int width;
		/// <summary>The height of the image canvas</summary>
		protected int height;
		/// <summary>Flag set to true when the global color table is in use</summary>
		protected bool golbalColorTableFlag;
		/// <summary>The number of colors in the global color table</summary>
		protected int golbalColorTableSize;
		/// <summary>The number of itinerations the animation loops for</summary>
		/// <remarks>0 repeats forever</remarks>
		protected int loopCount = 1;
		/// <summary>Stores the global indexed color table</summary>
		protected int[] globalColorTable;
		/// <summary>Stores the current local indexed color table</summary>
		protected int[] localColorTable;
		/// <summary>Gets the active color table</summary>
		protected int[] activeColorTable => lctFlag ? localColorTable : globalColorTable;
		/// <summary>The index of the background color within the global color table</summary>
		protected int bgIndex;
		protected int bgColor; // background color
		protected int lastBgColor; // previous bg color
		/// <summary>The pixel aspect ratio</summary>
		protected int pixelAspect;
		/// <summary>Whether the local color table is in use</summary>
		protected bool lctFlag;
		/// <summary>Whether the current image is endcoded as interlaced</summary>
		protected bool interlace;
		/// <summary>Position of the current sub-image</summary>
		protected Vector2 imagePosition;
		/// <summary>Size of the current sub-image</summary>
		protected Vector2 imageSize;
		protected int[] image; // current frame
		protected int[] bitmap;
		protected int[] lastImage; // previous frame

		protected byte[] block = new byte[256]; // current data block
		protected int blockSize; // block size

		// last graphic control extension info
		protected DisposeMode dispose = DisposeMode.NoAction;
		protected DisposeMode lastDispose = DisposeMode.NoAction;
		protected bool transparency = false; // use transparent color
		protected int delay; // delay in milliseconds
		protected int transIndex; // transparent color index

		protected static readonly int MaxStackSize = 4096;
		// max decoder pixel stack size

		// LZW decoder working arrays
		protected short[] prefix;
		protected byte[] suffix;
		protected byte[] pixelStack;
		protected byte[] pixels;

		protected List<int[]> frames;
		protected List<int> delays;

		protected int frameCount;

		/// <summary>Gets the duration of the specified frame</summary>
		/// <param name="n">The frame index</param>
		/// <returns>The frame duration in milliseconds</returns>
		public int GetDuration(int n) 
		{
			delay = -1;
			if (n >= 0 && n < frameCount) 
			{
				delay = delays[n];
			}
			return delay;
		}

		/// <summary>Gets the total frame count</summary>
		public int GetFrameCount() 
		{
			return frameCount;
		}
		
		/// <summary>Sets the pixels for a GIF frame from the current bitmap</summary>
		protected void SetPixels() 
		{
			int[] dest = new int[width * height];
			// fill in starting image contents based on last image's dispose code
			if (lastDispose > DisposeMode.NoAction)
			{
				Array.Copy(bitmap, dest, bitmap.Length);
				
				if (lastDispose == DisposeMode.RestoreToPrevious)
				{
					// use image before last
					int n = frameCount - 2;
					lastImage = n > 0 ? GetFrame(n - 1) : null;
				}

				if (lastImage != null) 
				{
					// copy pixels
					if (lastDispose == DisposeMode.RestoreToBackground) 
					{
						// fill last image rect area with background color
						for (int i = 0; i < image.Length; i++)
						{
							if (transparency)
							{
								image[i] = BitConverter.ToInt32(new byte[] {0,0,0,byte.MaxValue}, 0); 	// assume background is transparent
							} 
							else
							{
								image[i] = lastBgColor; // use given background color
							}
						}
					}
					else
					{
						Array.Copy(lastImage, 0, dest, 0, lastImage.Length);
						Array.Copy(lastImage, 0, bitmap, 0, lastImage.Length);
					}
				}
			}


			// copy each source line to the appropriate place in the destination
			int pass = 1;
			int inc = 8;
			int iline = 0;
			for (int i = 0; i < imageSize.Y; i++) 
			{
				int line = i;
				if (interlace) 
				{
					if (iline >= imageSize.Y) 
					{
						pass++;
						switch (pass) 
						{
							case 2 :
								iline = 4;
								break;
							case 3 :
								iline = 2;
								inc = 4;
								break;
							case 4 :
								iline = 1;
								inc = 2;
								break;
						}
					}
					line = iline;
					iline += inc;
				}
				line += (int)imagePosition.Y;
				if (line < height) 
				{
					int k = line * width;
					int dx = k + (int)imagePosition.X; // start of line in dest
					int dlim = dx + (int)imageSize.X; // end of dest line
					if (k + width < dlim) 
					{
						dlim = k + width; // past dest edge
					}
					int sx = i * (int)imageSize.X; // start of line in source
					while (dx < dlim) 
					{
						// map color and insert in destination
						int index = pixels[sx++] & 0xff;
						int c = activeColorTable[index];
						if (c != 0) 
						{
							dest[dx] = c;
						}
						dx++;
					}
				}
			}
			Array.Copy(dest, bitmap, dest.Length);
		}

		/// <summary>Gets an image containing the contents of the specified frame</summary>
		/// <param name="n">The frame number</param>
		/// <returns>The image</returns>
		public int[] GetFrame(int n) 
		{
			int[] im = null;
			if (n >= 0 && n < frameCount) 
			{
				im = frames[n];
			}
			return im;
		}

		/// <summary>Gets the frame size</summary>
		/// <returns>A vector describing the size of each frame</returns>
		public Vector2 GetFrameSize() 
		{
			return new Vector2(width, height);
		}

		/// <summary>Reads a GIF file from the specified stream</summary>
		/// <param name="inputStream">The source stream</param>
		/// <returns>The status code</returns>
		public DecoderStatus Read(Stream inputStream) 
		{
			Init();
			if (inputStream != null) 
			{
				this.inStream = inputStream;
				ReadHeader();
				if (!Error) 
				{
					ReadContents();
					if (frameCount < 0) 
					{
						status = DecoderStatus.FormatError;
					}
				}
				inputStream.Close();
			} 
			else 
			{
				status = DecoderStatus.OpenError;
			}
			return status;
		}

		/// <summary>Reads a GIF file from the specified source</summary>
		/// <param name="name">The source file / URL</param>
		/// <returns>The status code</returns>
		public DecoderStatus Read(string name) 
		{
			status = DecoderStatus.OK;
			try 
			{
				status = Read(new FileInfo(name).OpenRead());
			} 
			catch (IOException) 
			{
				status = DecoderStatus.OpenError;
			}
			return status;
		}

		/// <summary>Decodes LZW image data into a pixel array</summary>
		/// <remarks>Adapted from ImageMagick code by John Cristy</remarks>
		protected void DecodeImageData() 
		{
			int NullCode = -1;
			int npix = (int)(imageSize.X * imageSize.Y);
			int bits,
				code,
				count,
				i,
				first,
				top,
				bi,
				pi;

			if (pixels == null || pixels.Length < npix) 
			{
				pixels = new byte[npix]; // allocate new pixel array
			}
			if (prefix == null) prefix = new short[MaxStackSize];
			if (suffix == null) suffix = new byte[MaxStackSize];
			if (pixelStack == null) pixelStack = new byte[MaxStackSize + 1];

			//  Initialize GIF data stream decoder.

			int data_size = Read();
			int clear = 1 << data_size;
			int end_of_information = clear + 1;
			int available = clear + 2;
			int old_code = NullCode;
			int code_size = data_size + 1;
			int code_mask = (1 << code_size) - 1;
			for (code = 0; code < clear; code++) 
			{
				prefix[code] = 0;
				suffix[code] = (byte) code;
			}

			//  Decode GIF pixel stream.

			int datum = bits = count = first = top = pi = bi = 0;

			for (i = 0; i < npix;) 
			{
				if (top == 0) 
				{
					if (bits < code_size) 
					{
						//  Load bytes until there are enough bits for a code.
						if (count == 0) 
						{
							// Read a new data block.
							count = ReadBlock();
							if (count <= 0)
								break;
							bi = 0;
						}
						datum += (block[bi] & 0xff) << bits;
						bits += 8;
						bi++;
						count--;
						continue;
					}

					//  Get the next code.

					code = datum & code_mask;
					datum >>= code_size;
					bits -= code_size;

					//  Interpret the code

					if (code > available || code == end_of_information)
						break;
					if (code == clear) 
					{
						//  Reset decoder.
						code_size = data_size + 1;
						code_mask = (1 << code_size) - 1;
						available = clear + 2;
						old_code = NullCode;
						continue;
					}
					if (old_code == NullCode) 
					{
						pixelStack[top++] = suffix[code];
						old_code = code;
						first = code;
						continue;
					}
					int in_code = code;
					if (code == available) 
					{
						pixelStack[top++] = (byte) first;
						code = old_code;
					}
					while (code > clear) 
					{
						pixelStack[top++] = suffix[code];
						code = prefix[code];
					}
					first = suffix[code] & 0xff;

					//  Add a new string to the string table,

					if (available >= MaxStackSize)
						break;
					pixelStack[top++] = (byte) first;
					prefix[available] = (short) old_code;
					suffix[available] = (byte) first;
					available++;
					if ((available & code_mask) == 0
						&& available < MaxStackSize) 
					{
						code_size++;
						code_mask += available;
					}
					old_code = in_code;
				}

				//  Pop a pixel off the pixel stack.

				top--;
				pixels[pi++] = pixelStack[top];
				i++;
			}

			for (i = pi; i < npix; i++) 
			{
				pixels[i] = 0; // clear missing pixels
			}

		}

		/// <summary>Returns whether an error was encountered whilst reading the GIF</summary>
		protected bool Error => status != DecoderStatus.OK;

		/// <summary>Initializes or reinitalizes the reader</summary>
		protected void Init() 
		{
			status = DecoderStatus.OK;
			frameCount = 0;
			frames = new List<int[]>();
			delays = new List<int>();
			globalColorTable = null;
			localColorTable = null;
		}

		/// <summary>Reads a single byte from the input stream</summary>
		protected int Read() 
		{
			int curByte = 0;
			try 
			{
				curByte = inStream.ReadByte();
			} 
			catch (IOException) 
			{
				status = DecoderStatus.FormatError;
			}
			return curByte;
		}

		/// <summary>Reads the next decoder control code from the input stream</summary>
		protected ControlCode ReadControlCode()
		{
			int curByte = Read();
			return (ControlCode)curByte;
		}

		/// <summary>Reads the next variable length input block</summary>
		/// <returns>The remaining input bytes stored in the buffer</returns>
		protected int ReadBlock() 
		{
			blockSize = Read();
			int n = 0;
			if (blockSize > 0) 
			{
				try 
				{
					while (n < blockSize) 
					{
						int count = inStream.Read(block, n, blockSize - n);
						if (count == -1) 
							break;
						n += count;
					}
				} 
				catch (IOException) 
				{
				}

				if (n < blockSize) 
				{
					status = DecoderStatus.FormatError;
				}
			}
			return n;
		}

		/// <summary>Reads the GIF Color Table as 256 integer values</summary>
		/// <param name="numberOfColors">The number of colors to read</param>
		/// <returns>The GIF color table</returns>
		protected int[] ReadColorTable(int numberOfColors) 
		{
			int nbytes = 3 * numberOfColors;
			int[] tab = null;
			byte[] c = new byte[nbytes];
			int n = 0;
			try 
			{
				n = inStream.Read(c, 0, c.Length);
			} 
			catch (IOException) 
			{
			}
			if (n < nbytes) 
			{
				status = DecoderStatus.FormatError;
			} 
			else 
			{
				tab = new int[256]; // max size to avoid bounds checks
				int i = 0;
				int j = 0;
				while (i < numberOfColors) 
				{
					byte r = (byte) (c[j++] & 0xff);
					byte g = (byte) (c[j++] & 0xff);
					byte b = (byte) (c[j++] & 0xff);
					tab[i++] = BitConverter.ToInt32(new[] {r,g,b,byte.MaxValue}, 0);
				}
			}
			return tab;
		}

		/// <summary>Reads the contents of the file</summary>
		protected void ReadContents() 
		{
			// read GIF file content blocks
			bool done = false;
			while (!(done || Error)) 
			{
				ControlCode code = ReadControlCode();
				switch (code) 
				{
					case ControlCode.ImageSeparator:
						ReadNextFrame();
						break;
					case ControlCode.ExtensionBlock:
						code = ReadControlCode();
						switch (code)
						{
							case ControlCode.GraphicsExtension:
								ReadGraphicControlExt();
								break;
							case ControlCode.ApplicationExtension:
								ReadBlock();
								string app = "";
								for (int i = 0; i < 11; i++)
								{
									app += (char) block[i];
								}

								if (app.Equals("NETSCAPE2.0") || app.Equals("ANIMEXTS1.0"))
								{
									ReadNetscapeExt();
								}
								else
								{
									Skip(); // don't care
								}
								break;
							case ControlCode.TextOverlay:
								Skip();
								break;
							default:
								Skip();
								break;
						}
						break;
					case ControlCode.Terminator:
						done = true;
						break;
					case ControlCode.BadByte:
						break;
					default :
						status = DecoderStatus.FormatError;
						break;
				}
			}
		}

		/// <summary>Reads the Graphic Control Extension values</summary>
		protected void ReadGraphicControlExt() 
		{
			Read(); // block size
			int packed = Read(); // packed fields
			dispose = (DisposeMode)((packed & 0x1c) >> 2); // disposal method
			if (dispose == DisposeMode.NoAction) 
			{
				dispose = DisposeMode.LeaveInPlace; // elect to keep old image if discretionary
			}
			transparency = (packed & 1) != 0;
			delay = ReadShort() * 10; // delay in milliseconds
			transIndex = Read(); // transparent color index
			Read(); // block terminator
		}

		/// <summary>Reads the GIF header</summary>
		protected void ReadHeader() 
		{
			string id = "";
			for (int i = 0; i < 6; i++) 
			{
				id += (char) Read();
			}
			if (!id.StartsWith("GIF")) 
			{
				status = DecoderStatus.FormatError;
				return;
			}

			ReadLSD();
			if (golbalColorTableFlag && !Error) 
			{
				globalColorTable = ReadColorTable(golbalColorTableSize);
				bgColor = globalColorTable[bgIndex];
			}
		}

		/// <summary>Reads the next frame image</summary>
		protected void ReadNextFrame()
		{
			imagePosition = ReadVector2();
			imageSize = ReadVector2();
			int packed = Read();
			lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
			interlace = (packed & 0x40) != 0; // 2 - interlace flag
			// 3 - sort flag
			// 4-5 - reserved
			int localColorTableSize = 2 << (packed & 7); // 6-8 - local color table size

			if (lctFlag) 
			{
				localColorTable = ReadColorTable(localColorTableSize); // read table
			} 
			else 
			{
				if (bgIndex == transIndex)
					bgColor = 0;
			}
			int save = 0;
			
			if (transparency) 
			{
				save = activeColorTable[transIndex];
				activeColorTable[transIndex] = 0; // set transparent color if specified
			}

			if (activeColorTable == null) 
			{
				status = DecoderStatus.FormatError; // no color table defined
			}

			if (Error) return;

			DecodeImageData(); // decode pixel data
			Skip();

			if (Error) return;

			frameCount++;
			// create new image to receive frame data
			bitmap = new int[width * height];
			image = bitmap;
			SetPixels(); // transfer pixel data to image

			frames.Add(bitmap); // add image to frame list
			delays.Add(delay);

			if (transparency && activeColorTable != null) 
			{
				activeColorTable[transIndex] = save;
			}
			ResetFrame();

		}

		/// <summary>Reads the logical screen descriptor</summary>
		protected void ReadLSD() 
		{

			// logical screen size
			width = ReadShort();
			height = ReadShort();

			// packed fields
			int packed = Read();
			golbalColorTableFlag = (packed & 0x80) != 0; // 1   : global color table flag
			// 2-4 : color resolution
			// 5   : gct sort flag
			golbalColorTableSize = 2 << (packed & 7); // 6-8 : gct size

			bgIndex = Read(); // background color index
			pixelAspect = Read(); // pixel aspect ratio
		}

		/// <summary>Reads the Netscape extensions header</summary>
		/// <remarks>This sets the itineration count</remarks>
		protected void ReadNetscapeExt() 
		{
			do 
			{
				ReadBlock();
				if (block[0] == 1) 
				{
					// loop count sub-block
					int b1 = block[1] & 0xff;
					int b2 = block[2] & 0xff;
					loopCount = (b2 << 8) | b1;
				}
			} while (blockSize > 0 && !Error);
		}

		/// <summary>Reads the next Vector2, LSB first</summary>
		protected Vector2 ReadVector2()
		{
			return new Vector2(ReadShort(), ReadShort());
		}

		/// <summary>Reads the next 16-bit value, LSB first</summary>
		protected int ReadShort() 
		{
			// read 16-bit value, LSB first
			return Read() | (Read() << 8);
		}
		
		/// <summary>Resets the frame state before reading the next image</summary>
		protected void ResetFrame() 
		{
			lastDispose = dispose;
			lastImage = image;
			lastBgColor = bgColor;
			transparency = false;
			delay = 0;
			localColorTable = null;
			dispose = DisposeMode.NoAction;
		}

		/// <summary>Skips variable length blocks upto and including the next zero length block</summary>
		protected void Skip() 
		{
			do 
			{
				ReadBlock();
			} while (blockSize > 0 && !Error);
		}

		public void Dispose()
		{
			inStream?.Dispose();
		}
	}
}

