//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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

// ReSharper disable NotAccessedField.Local
#pragma warning disable IDE0052, CS0414
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System;
using System.IO;
using System.Text;

namespace Plugin
{
	internal class BmpDecoder
	{
		// PRIVATE MEMBERS

		/// <summary>Size of the bitmap file in bytes</summary>
		private int fileSize;
		/// <summary>Offset from the beginning of the file to the bitmap data</summary>
		private int dataOffset;
		/// <summary>The compression format</summary>
		private CompressionFormat CompressionFormat;
		/// <summary>The compressed size of the image data</summary>
		private int ImageSize;
		/// <summary>The image resolution in px / meter</summary>
		private Vector2 ImageResolution;
		/// <summary>Number of colors used in the color pallette</summary>
		private int ColorsUsed;
		/// <summary>Number of important colors</summary>
		private int ImportantColors;
		/// <summary>Whether the image is stored in top-down format</summary>
		private bool TopDown;

		// INTERNAL MEMBERS

		/// <summary>The width of the bitmap</summary>
		internal int Width;
		/// <summary>The height of the bitmap</summary>
		internal int Height;
		/// <summary>The number of bits per pixel</summary>
		internal BitsPerPixel BitsPerPixel;
		/// <summary>The image data</summary>
		internal byte[] ImageData;
		/// <summary>Stores the color table when using a restricted pallette</summary>
		internal Color24[] ColorTable;
		/// <summary>The bitmap format</summary>
		internal BmpFormat Format;

		/// <summary>Reads a BMP file using this decoder</summary>
		/// <param name="fileName">The file to read</param>
		/// <returns>Whether reading succeeded</returns>

		internal bool Read(string fileName)
		{
			using (Stream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				// HEADER
				byte[] buffer = new byte[14];
				fileReader.Read(buffer, 0, 14); // header
				string signature = Encoding.ASCII.GetString(buffer, 0, 2);
				if (signature != "BM")
				{
					// not a bitmap file
					return false;
				}
				fileSize = BitConverter.ToInt32(buffer, 2);
				/*
				 * Next 4 bytes are unused
				 * Actually specified as application specific data, but in practice always set to zero
				 * unless using some ridiculously obscure DOS stuff, which we can safely ignore
				 */
				dataOffset = BitConverter.ToInt32(buffer, 10);

				// INFO HEADER
				buffer = new byte[4];
				fileReader.Read(buffer,0 , 4);
				int headerSize = BitConverter.ToInt32(buffer, 0);
				switch (headerSize)
				{
					case 40:
						Format = BmpFormat.BmpVersion2;
						break;
					case 108:
						Format = BmpFormat.BmpVersion4;
						break;
					default:
						// Unknown header size
						return false;

				}
				buffer = new byte[headerSize - 4];
				fileReader.Read(buffer, 0, headerSize - 4);
				
				Width = BitConverter.ToInt32(buffer, 0);
				Height = BitConverter.ToInt32(buffer, 4);

				if (Math.Abs(Height) != Height)
				{
					// Image is stored in the uncommon top-down format
					TopDown = true;
					Height = Math.Abs(Height);
				}

				int numPlanes = BitConverter.ToInt16(buffer, 8);
				if (numPlanes != 1)
				{
					// must be set to 1 https://devblogs.microsoft.com/oldnewthing/20041201-00/?p=37163
					return false;
				}

				BitsPerPixel = (BitsPerPixel)BitConverter.ToInt16(buffer, 10);
				CompressionFormat = (CompressionFormat)BitConverter.ToInt32(buffer, 12);
				if (CompressionFormat == CompressionFormat.BITFIELDS)
				{
					/* A BMP V3 file is identical to a V2 unless bitmask is used
					 * As they were only created by NT4.0, highly unlikely to be encountered in the wild
					 *
					 * For the minute, we'll assume we don't support them
					 */

					Format = BmpFormat.BmpVersion3;
					return false;
				}
				ImageSize = BitConverter.ToInt32(buffer, 16);
				if (ImageSize == 0 && CompressionFormat != CompressionFormat.BI_RGB)
				{
					// Compressed image size of zero should only be valid with uncompressed data
					return false;
				}
				ImageResolution.X = BitConverter.ToInt32(buffer, 20);
				ImageResolution.Y = BitConverter.ToInt32(buffer, 24);
				ColorsUsed = BitConverter.ToInt32(buffer, 28);
				ColorTable = new Color24[ColorsUsed];
				ImportantColors = BitConverter.ToInt32(buffer, 32);

				// COLOR TABLE
				if (ColorsUsed != 0)
				{
					buffer = new byte[ColorsUsed * 4];
					fileReader.Read(buffer, 0, ColorsUsed * 4);
					for (int currentColor = 0; currentColor < ColorsUsed; currentColor++)
					{
						int idx = currentColor * 4;
						ColorTable[currentColor] = new Color24(buffer[idx + 2], buffer[idx + 1], buffer[idx]); // stored as BGR in bitmap, we want RGB
					}
				}
				else
				{
					/*
					 * NOTE:
					 * If the number of colors used is set to zero,
					 * the color table is set to the one of the standard Windows color pallettes
					 */
					switch (BitsPerPixel)
					{
						case BitsPerPixel.Monochrome:
							ColorsUsed = 2;
							ColorTable = ColorPalettes.MonochromePalette;
							break;
						case BitsPerPixel.FourBitPalletized:
							ColorsUsed = 16;
							ColorTable = ColorPalettes.Windows16ColorPalette;
							break;
						case BitsPerPixel.EightBitPalletized:
							ColorsUsed = 256;
							ColorTable = ColorPalettes.Windows256ColorPalette;
							break;
					}

					
					
				}

				if (fileReader.Position != dataOffset)
				{
					fileReader.Seek(dataOffset, SeekOrigin.Begin);
				}
				// PIXEL DATA
				buffer = new byte[(int)fileReader.Length - (int)fileReader.Position];
				fileReader.Read(buffer, 0, (int)fileReader.Length - (int)fileReader.Position);

				ImageData = new byte[Width * Height * 4];
				int sourceIdx = 0;
				int destIdx = 0;
				bool availableData = true;
				int currentLine = 0;
				switch (CompressionFormat)
				{
					case CompressionFormat.BI_RGB:
						switch (BitsPerPixel)
						{
							case BitsPerPixel.Monochrome:
								// WARNING: Monochrome may actually be any two colors
								for (int currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel++)
									{
										for (int currentBit = 0; currentBit < 8; currentBit++)
										{
											if ((buffer[sourceIdx] & 1 << (7 - currentBit % 8)) != 0)
											{
												ImageData[destIdx] = ColorTable[1].R;
												ImageData[destIdx + 1] = ColorTable[1].G;
												ImageData[destIdx + 2] = ColorTable[1].B;
												ImageData[destIdx + 3] = byte.MaxValue;
											}
											else
											{
												ImageData[destIdx] = ColorTable[0].R;
												ImageData[destIdx + 1] = ColorTable[0].G;
												ImageData[destIdx + 2] = ColorTable[0].B;
												ImageData[destIdx + 3] = byte.MaxValue;
											}
											destIdx += 4;
											currentPixel++;
											if (currentPixel >= Width)
											{
												// A single byte contains 8px, but the image may not be of a multiple of this
												break;
											}
										}
										sourceIdx++;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							case BitsPerPixel.TwoBitPalletized:
								for (int currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									int currentPixel = 0;
									while(currentPixel < Width)
									{
										byte firstNibblet = (byte)((buffer[sourceIdx] >> 6) & 0x03);
										byte secondNibblet = (byte)((buffer[sourceIdx] >> 4) & 0x03);
										byte thirdNibblet = (byte)((buffer[sourceIdx] >> 2) & 0x03);
										byte fourthNibblet = (byte)(buffer[sourceIdx] & 0x03);
										ImageData[destIdx] = ColorTable[firstNibblet].R;
										ImageData[destIdx + 1] = ColorTable[firstNibblet].G;
										ImageData[destIdx + 2] = ColorTable[firstNibblet].B;
										ImageData[destIdx + 3] = byte.MaxValue;
										currentPixel++;
										destIdx+= 4;
										if (currentPixel < Width)
										{
											ImageData[destIdx] = ColorTable[secondNibblet].R;
											ImageData[destIdx + 1] = ColorTable[secondNibblet].G;
											ImageData[destIdx + 2] = ColorTable[secondNibblet].B;
											ImageData[destIdx + 3] = byte.MaxValue;
											currentPixel++;
											destIdx+= 4;
										}
										if (currentPixel < Width)
										{
											ImageData[destIdx] = ColorTable[thirdNibblet].R;
											ImageData[destIdx + 1] = ColorTable[thirdNibblet].G;
											ImageData[destIdx + 2] = ColorTable[thirdNibblet].B;
											ImageData[destIdx + 3] = byte.MaxValue;
											currentPixel++;
											destIdx+= 4;
										}
										if (currentPixel < Width)
										{
											ImageData[destIdx] = ColorTable[fourthNibblet].R;
											ImageData[destIdx + 1] = ColorTable[fourthNibblet].G;
											ImageData[destIdx + 2] = ColorTable[fourthNibblet].B;
											ImageData[destIdx + 3] = byte.MaxValue;
											currentPixel++;
											destIdx+= 4;
										}
										sourceIdx++;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
								//break;
							case BitsPerPixel.FourBitPalletized:
								for (int currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel+= 2)
									{
										byte leftNibble = (byte) (buffer[sourceIdx] & 0x0F); // color of left pixel
										byte rightNibble = (byte)((buffer[sourceIdx] & 0xF0) >> 4); // color of right pixel
										ImageData[destIdx] = ColorTable[leftNibble].R;
										ImageData[destIdx + 1] = ColorTable[leftNibble].G;
										ImageData[destIdx + 2] = ColorTable[leftNibble].B;
										ImageData[destIdx + 3] = byte.MaxValue;
										destIdx+= 4;
										if (Width % 2 == 0 || currentPixel > Width - 1)
										{
											// Final nibble should be discarded if not divisible by 2
											ImageData[destIdx] = ColorTable[rightNibble].R;
											ImageData[destIdx + 1] = ColorTable[rightNibble].G;
											ImageData[destIdx + 2] = ColorTable[rightNibble].B;
											ImageData[destIdx + 3] = byte.MaxValue;
											destIdx+= 4;
										}
										sourceIdx++;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							case BitsPerPixel.EightBitPalletized:
								for (int currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel++)
									{
										int colorIndex = buffer[sourceIdx];
										ImageData[destIdx] = ColorTable[colorIndex].R;
										ImageData[destIdx + 1] = ColorTable[colorIndex].G;
										ImageData[destIdx + 2] = ColorTable[colorIndex].B;
										ImageData[destIdx + 3] = byte.MaxValue;
										sourceIdx++;
										destIdx+= 4;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							default:
								for (int currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel++)
									{
										ImageData[destIdx] = buffer[sourceIdx + 2];
										ImageData[destIdx + 1] =  buffer[sourceIdx + 1];
										ImageData[destIdx + 2] =  buffer[sourceIdx];
										ImageData[destIdx + 3] = byte.MaxValue;
										sourceIdx+= 3;
										destIdx+= 4;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
						}
						break;
					case CompressionFormat.BI_RLE8:
					case CompressionFormat.BI_RLE4:
						/*
						 * FIXME: This will currently be in scanline reversed order once working....
						 */
						while (availableData)
						{
							int numPix = buffer[sourceIdx];
							if (numPix > 128)
							{
								// Invalid run length
								return false;
							}

							if (numPix == 0)
							{
								// Escape
								int newPos;
								byte currentOp = buffer[sourceIdx + 1];
								switch (currentOp)
								{
									case 0:
										//EOL
										newPos = (currentLine + 1) * Width;
										/*
										 * This may not actually be necessary
										 * However, assume that an EOL may be issued with pixels still remaining in the line
										 */
										while (destIdx < newPos)
										{
											ImageData[destIdx] = 0;
											destIdx++;
										}
										currentLine++;
										break;
									case 1:
										//EOF
										while (destIdx < ImageData.Length)
										{
											ImageData[destIdx] = 0;
											destIdx++;
											availableData = false;
										}
										break;
									case 2:
										/*
										 * Delta
										 * NOTE: Implementation specific 'quirk'
										 * pixels between the current position and the delta are undefined
										 * Let's set them to transparent for the moment
										 */
										int xPos = buffer[sourceIdx + 2] & 0x0F;
										int yPos = (buffer[sourceIdx + 2] & 0xF0) >> 4;
										newPos = yPos * Width + xPos;
										while (destIdx < newPos)
										{
											ImageData[destIdx] = 0;
											destIdx++;
										}
										sourceIdx++;
										break;
									default:
										for (int i = 0; i < currentOp; i++)
										{
											ImageData[destIdx] = currentOp;
											destIdx++;
										}
										break;
								}
							}
							else
							{
								// Defines a repeating 2 pixel block for nPix
								int leftNibble = buffer[sourceIdx] & 0x0F; // color of left pixel
								int rightNibble = (buffer[sourceIdx] & 0xF0) >> 4; // color of right pixel
								for (int i = 0; i < numPix; i++)
								{
									if (i % 2 == 0)
									{
										ImageData[destIdx] = ColorTable[leftNibble].R;
										ImageData[destIdx + 1] = ColorTable[leftNibble].G;
										ImageData[destIdx + 2] = ColorTable[leftNibble].B;
										ImageData[destIdx + 3] = byte.MaxValue;
									}
									else
									{
										ImageData[destIdx] = ColorTable[rightNibble].R;
										ImageData[destIdx + 1] = ColorTable[rightNibble].G;
										ImageData[destIdx + 2] = ColorTable[rightNibble].B;
									}
									destIdx+= 4;
								}
							}
							sourceIdx += 2;
						}
						break;
				}
			}

			return true;
		}
	}
}
#pragma warning restore IDE0052, CS0414
