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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi;
using OpenBveApi.Hosts;

namespace Plugin.BMP
{
	internal class BmpDecoder : IDisposable
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
		/// <summary>The number of planes in the bitmap</summary>
		/// <remarks>Must be set to 1 https://devblogs.microsoft.com/oldnewthing/20041201-00/?p=37163 </remarks>
		private int numPlanes;
		/// <summary>Used for building the reduced color table when hacks are enabled</summary>
		private HashSet<Color24> reducedColorTable;

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
		/// <summary>Buffer containing data read from the file</summary>
		internal byte[] buffer;
		/// <summary>Holds the bytes for the current row when decoding RLE data</summary>
		private byte[] rowBytes;
		/// <summary>The index of the current row</summary>
		private int currentRow;
		/// <summary>The index of the pixel in the current row</summary>
		private int rowPixel;

		/// <summary>Reads a BMP file using this decoder</summary>
		/// <param name="fileName">The file to read</param>
		/// <returns>Whether reading succeeded</returns>
		internal bool Read(string fileName)
		{
			using (Stream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				// HEADER
				buffer = new byte[14];
				if (fileReader.Read(buffer, 0, 14) != 14)
				{
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient Header data in Bitmap file " + fileName);
					return false;
				}
				string signature = Encoding.ASCII.GetString(buffer, 0, 2);
				if (signature != "BM")
				{
					// not a bitmap file
					return false;
				}
				fileSize = LittleEndianBinaryExtensions.ToInt32(buffer, 2);
				/*
				 * Next 4 bytes are unused
				 * Actually specified as application specific data, but in practice always set to zero
				 * unless using some ridiculously obscure DOS stuff, which we can safely ignore
				 */
				dataOffset = LittleEndianBinaryExtensions.ToInt32(buffer, 10);

				// INFO HEADER
				if (fileReader.Read(buffer, 0, 4) != 4)
				{
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient InfoHeader data in Bitmap file " + fileName);
					return false;
				}
				int headerSize = LittleEndianBinaryExtensions.ToInt32(buffer, 0);
				switch (headerSize)
				{
					case 12:
						Format = BmpFormat.OS2v1;
						break;
					case 16:
						Format = BmpFormat.OS2v2;
						break;
					case 40:
					case 64:
						Format = BmpFormat.BmpVersion2;
						break;
					case 108:
						Format = BmpFormat.BmpVersion4;
						break;
					default:
						// Unknown header size
						Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Unrecognised Header size of " + headerSize + " in Bitmap file " + fileName);
						return false;

				}
				buffer = new byte[headerSize - 4];
				if (fileReader.Read(buffer, 0, headerSize - 4) != headerSize - 4)
				{
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient Header data in Bitmap file " + fileName);
				}
				

				switch (Format)
				{
					case BmpFormat.OS2v1:
						Width = LittleEndianBinaryExtensions.ToInt16(buffer, 0);
						Height = LittleEndianBinaryExtensions.ToInt16(buffer, 2);
						numPlanes = LittleEndianBinaryExtensions.ToInt16(buffer, 4);
						BitsPerPixel = (BitsPerPixel)LittleEndianBinaryExtensions.ToInt16(buffer, 6);
						CompressionFormat = CompressionFormat.BI_RGB;
						switch (BitsPerPixel)
						{
							case BitsPerPixel.Monochrome:
								ColorsUsed = 2;
								break;
							case BitsPerPixel.TwoBitPalletized:
								ColorsUsed = 4;
								break;
							case BitsPerPixel.FourBitPalletized:
								ColorsUsed = 16;
								break;
							case BitsPerPixel.EightBitPalletized:
								// Documentation is a little sparse, but this seems to be right (not 256!)
								ColorsUsed = 252;
								break;
						}
						break;
					case BmpFormat.OS2v2:
						Width = LittleEndianBinaryExtensions.ToInt32(buffer, 0);
						Height = LittleEndianBinaryExtensions.ToInt32(buffer, 4);
						numPlanes = LittleEndianBinaryExtensions.ToInt16(buffer, 8);
						BitsPerPixel = (BitsPerPixel)LittleEndianBinaryExtensions.ToInt16(buffer, 10);
						CompressionFormat = CompressionFormat.BI_RGB;
						switch (BitsPerPixel)
						{
							case BitsPerPixel.Monochrome:
								ColorsUsed = 2;
								break;
							case BitsPerPixel.TwoBitPalletized:
								ColorsUsed = 4;
								break;
							case BitsPerPixel.FourBitPalletized:
								ColorsUsed = 16;
								break;
							case BitsPerPixel.EightBitPalletized:
								// Documentation is a little sparse, but this seems to be right (not 256!)
								ColorsUsed = 252;
								break;
						}
						break;
					default:
						Width = LittleEndianBinaryExtensions.ToInt32(buffer, 0);
						Height = LittleEndianBinaryExtensions.ToInt32(buffer, 4);
						numPlanes = LittleEndianBinaryExtensions.ToInt16(buffer, 8);
						BitsPerPixel = (BitsPerPixel)LittleEndianBinaryExtensions.ToInt16(buffer, 10);
						CompressionFormat = (CompressionFormat)LittleEndianBinaryExtensions.ToInt32(buffer, 12);
						ImageResolution.X = LittleEndianBinaryExtensions.ToInt32(buffer, 20);
						ImageResolution.Y = LittleEndianBinaryExtensions.ToInt32(buffer, 24);
						ColorsUsed = LittleEndianBinaryExtensions.ToInt32(buffer, 28);
						ImportantColors = LittleEndianBinaryExtensions.ToInt32(buffer, 32);
						ImageSize = LittleEndianBinaryExtensions.ToInt32(buffer, 16);
						break;
				}
				
				if (Math.Abs(Height) != Height)
				{
					// Image is stored in the uncommon top-down format
					TopDown = true;
					Height = Math.Abs(Height);
				}
				
				if (numPlanes != 1)
				{
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Invalid NumPlanes in Bitmap file " + fileName);
					return false;
				}
				
				if (CompressionFormat == CompressionFormat.BITFIELDS)
				{
					/* A BMP V3 file is identical to a V2 unless bitmask is used
					 * As they were only created by NT4.0, highly unlikely to be encountered in the wild
					 *
					 * For the minute, we'll assume we don't support them
					 */

					Format = BmpFormat.BmpVersion3;
					Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "Bitmap V3 is not supported by this decoder in " + fileName);
					return false;
				}
				
				if (ImageSize == 0 && Format > BmpFormat.OS2v2)
				{
					/* Compressed image size of zero should only be valid with uncompressed data, unless OS/2 bitmaps
					 * However, continue to load and see what happens
					 *
					 * Only report this in debug mode, as this seems to be a 'common' problem....
					 */
					
					#if DEBUG
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Invalid compressed image size in Bitmap file " + fileName);
					#endif
				}

				if (TopDown && CompressionFormat != CompressionFormat.BI_RGB && CompressionFormat != CompressionFormat.BITFIELDS)
				{
					// Top down bitmap cannot be in compressed format
					Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "A TopDown bitmap cannot be compressed in Bitmap file " + fileName);
					return false;
				}

				if (Plugin.EnabledHacks.ReduceTransparencyColorDepth && BitsPerPixel > BitsPerPixel.EightBitPalletized)
				{
					reducedColorTable = new HashSet<Color24>();
				}

				ColorTable = new Color24[ColorsUsed];
				
				int colorSize = Format == BmpFormat.OS2v1 ? 3 : 4;
				// COLOR TABLE
				if (ColorsUsed != 0)
				{
					buffer = new byte[ColorsUsed * colorSize];
					int readDataLength = fileReader.Read(buffer, 0, ColorsUsed * colorSize);
					if (readDataLength != ColorsUsed * colorSize)
					{
						Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient ColorTable data in Bitmap file " + fileName);
						ColorsUsed = readDataLength / colorSize; // will cause any missing colors to be mapped to black
					}
					for (int currentColor = 0; currentColor < ColorsUsed; currentColor++)
					{
						int idx = currentColor * colorSize;
						ColorTable[currentColor] = new Color24(buffer[idx + 2], buffer[idx + 1], buffer[idx]); // stored as BGR in bitmap, we want RGB
					}
				}
				else
				{
					/*
					 * NOTE:
					 * Documentation states that if the number of colors used is set to zero,
					 * the color table is set to the one of the standard Windows color pallettes
					 *
					 * However, in practice there seem to be some bitmaps in the wild
					 * which contain a color table regardless. 
					 *
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
							AttemptToFindColorTable(fileReader, colorSize);
							break;
						case BitsPerPixel.EightBitPalletized:
							ColorsUsed = 256;
							ColorTable = ColorPalettes.Windows256ColorPalette;
							AttemptToFindColorTable(fileReader, colorSize);
							break;
					}

					
					
				}

				if (fileReader.Position != dataOffset)
				{
					fileReader.Seek(dataOffset, SeekOrigin.Begin);
				}
				// PIXEL DATA
				buffer = new byte[(int)fileReader.Length - (int)fileReader.Position];
				// ReSharper disable once MustUseReturnValue
				fileReader.Read(buffer, 0, (int)fileReader.Length - (int)fileReader.Position);

				ImageData = new byte[Width * Height * 4];
				int sourceIdx = 0;
				int destIdx = 0;
				switch (CompressionFormat)
				{
					case CompressionFormat.BI_RGB:
						switch (BitsPerPixel)
						{
							case BitsPerPixel.Monochrome:
								// WARNING: Monochrome may actually be any two colors
								for (currentRow = 0; currentRow < Height; currentRow++)
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
								for (currentRow = 0; currentRow < Height; currentRow++)
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
								for (currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel+= 2)
									{
										byte leftNibble = (byte)((buffer[sourceIdx] & 0xF0) >> 4); // color of left pixel
										byte rightNibble = (byte) (buffer[sourceIdx] & 0x0F); // color of right pixel
										ImageData[destIdx] = ColorTable[leftNibble].R;
										ImageData[destIdx + 1] = ColorTable[leftNibble].G;
										ImageData[destIdx + 2] = ColorTable[leftNibble].B;
										ImageData[destIdx + 3] = byte.MaxValue;
										destIdx+= 4;
										if (currentPixel < Width - 1)
										{
											// Final nibble should be discarded if not divisible by 2
											ImageData[destIdx] = ColorTable[rightNibble].R;
											ImageData[destIdx + 1] = ColorTable[rightNibble].G;
											ImageData[destIdx + 2] = ColorTable[rightNibble].B;
											ImageData[destIdx + 3] = byte.MaxValue;
											destIdx += 4;
										}
										sourceIdx++;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							case BitsPerPixel.EightBitPalletized:
								for (currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel++)
									{
										if (buffer[sourceIdx] >= ColorTable.Length && Plugin.CurrentOptions.EnableBveTsHacks)
										{
											/*
											 * The BMP specification is unclear as to what should happen here
											 *
											 * Windows appears to interpret this case as pure black, but other decoders (Photoshop, GIMP)
											 * interpret this as the *last* color in the color table
											 *
											 * https://github.com/leezer3/OpenBVE/issues/1042
											 */
											ImageData[destIdx] = 0;
											ImageData[destIdx + 1] = 0;
											ImageData[destIdx + 2] = 0;
											ImageData[destIdx + 3] = byte.MaxValue;
										}
										else
										{
											int colorIndex = Math.Min(buffer[sourceIdx], ColorTable.Length - 1);
											ImageData[destIdx] = ColorTable[colorIndex].R;
											ImageData[destIdx + 1] = ColorTable[colorIndex].G;
											ImageData[destIdx + 2] = ColorTable[colorIndex].B;
											ImageData[destIdx + 3] = byte.MaxValue;
										}
										sourceIdx++;
										destIdx+= 4;
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							case BitsPerPixel.SixteenBitRGB:
								for (currentRow = 0; currentRow < Height; currentRow++)
								{
									if (!TopDown)
									{
										destIdx = (Height - currentRow - 1) * Width * 4;
									}
									for (int currentPixel = 0; currentPixel < Width; currentPixel++)
									{
										short px = LittleEndianBinaryExtensions.ToShort(buffer, sourceIdx);
										byte r = (byte)((px & 0xF800) >> 11);
										byte g = (byte)((px & 0x07E0) >> 5);
										byte b = (byte)(px & 0x1F);
										ImageData[destIdx] = r;
										ImageData[destIdx + 1] =  g;
										ImageData[destIdx + 2] =  b;
										ImageData[destIdx + 3] = byte.MaxValue;
										if (Plugin.EnabledHacks.ReduceTransparencyColorDepth)
										{
											Color24 c = new Color24(r, g, b);
											reducedColorTable.Add(c);
										}
										sourceIdx++;
										destIdx+= 4;
										
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
							default:
								for (currentRow = 0; currentRow < Height; currentRow++)
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
										if (Plugin.EnabledHacks.ReduceTransparencyColorDepth)
										{
											Color24 c = new Color24(buffer[sourceIdx + 2], buffer[sourceIdx + 1], buffer[sourceIdx]);
											reducedColorTable.Add(c);
										}
										sourceIdx+= 3;
										if (BitsPerPixel == BitsPerPixel.ThirtyTwoBitRGB)
										{
											// Alpha in bitmaps is not currently supported by this decoder
											sourceIdx++;
										}
										destIdx+= 4;
										
									}
									// BMP scan lines are zero-padded to the nearest 4-byte boundary
									sourceIdx = sourceIdx % 4 == 0 ? sourceIdx : sourceIdx + 4 - sourceIdx % 4;
								}
								break;
						}
						break;
					case CompressionFormat.BI_RLE4:
					case CompressionFormat.BI_RLE8:
					case CompressionFormat.BI_RLE24:
						rowBytes = new byte[Width * 4];
						currentRow = TopDown ? 0 : Height - 1;
						
						rowPixel = 0;
						while (true)
						{
							int numPix = buffer[sourceIdx];
							sourceIdx++;
							if (sourceIdx > buffer.Length - 1)
							{
								break;
							}
							byte currentOp = buffer[sourceIdx];
							if (rowPixel > Width)
							{
								// Run off the end of the row with pixel data. Shouldn't happen, but let's handle anyway
								StartNextRow();
							}
							if (numPix == 0)
							{
								switch (currentOp)
								{
									case 0:
										//EOL
										StartNextRow();
										sourceIdx++;
										break;
									case 1:
										//EOF
										if (Plugin.EnabledHacks.ReduceTransparencyColorDepth && BitsPerPixel > BitsPerPixel.EightBitPalletized)
										{
											ColorTable = reducedColorTable.ToArray();
										}
										return true;
									case 2:
										/*
										 * Delta
										 * NOTE: Implementation specific 'quirk'
										 * pixels between the current position and the delta are undefined
										 * Let's set them to transparent for the moment
										 */
										int xPos = buffer[sourceIdx + 1];
										int yPos = buffer[sourceIdx + 2] - 1;

										if (!TopDown)
										{
											yPos = Height - yPos;
										}

										if (yPos != currentRow)
										{
											StartNextRow();
											rowPixel = xPos;
											currentRow = yPos;
										}

										sourceIdx+= 3;
										break;
									default:
										int runLength = currentOp;
										sourceIdx++;
										switch (CompressionFormat)
										{
											case CompressionFormat.BI_RLE4:
											case CompressionFormat.BI_RLE8:
												for (int i = 0; i < runLength; i++)
												{
													int pix = buffer[sourceIdx];
													if (CompressionFormat == CompressionFormat.BI_RLE4)
													{
														// Two color indicies
														pix = (byte)((pix >> 4) & 0xF); // upper 4 bytes
														if (i % 2 != 0)
														{
															pix = (byte)(buffer[sourceIdx] & 0xF); // lower 4 bytes
															sourceIdx++;
														}
														if (rowPixel > Width - 1)
														{
															StartNextRow();
														}
													}
													else
													{
														// Single color index
														sourceIdx++;
													}
													
													int startByte = rowPixel * 4;
													if (pix > ColorTable.Length - 1)
													{
														rowBytes[startByte] = byte.MinValue;
														rowBytes[startByte + 1] = byte.MinValue;
														rowBytes[startByte + 2] = byte.MinValue;
														rowBytes[startByte + 3] = byte.MaxValue;
													}
													else
													{
														rowBytes[startByte] = ColorTable[pix].R;
														rowBytes[startByte + 1] = ColorTable[pix].G;
														rowBytes[startByte + 2] = ColorTable[pix].B;
														rowBytes[startByte + 3] = byte.MaxValue;
													}
													rowPixel++;
												}
												if (runLength % 2 != 0)
												{
													// Run length is odd, so we need to skip next byte to get back to data
													sourceIdx++;
												}
												// Padding
												sourceIdx = sourceIdx % 2 == 0 ? sourceIdx : sourceIdx + 2 - sourceIdx % 2;
												break;
											case CompressionFormat.BI_RLE24:
												for (int i = 0; i < runLength; i++)
												{
													if (rowPixel > Width - 1)
													{
														StartNextRow();
														sourceIdx++;
														break;
													}
													int startByte = rowPixel * 4;
													rowBytes[startByte] = buffer[sourceIdx + 2];
													rowBytes[startByte + 1] = buffer[sourceIdx + 1];
													rowBytes[startByte + 2] = buffer[sourceIdx];
													rowBytes[startByte + 3] = byte.MaxValue;
													if (Plugin.EnabledHacks.ReduceTransparencyColorDepth && BitsPerPixel > BitsPerPixel.EightBitPalletized)
													{
														Color24 c = new Color24(buffer[sourceIdx + 2], buffer[sourceIdx + 1], buffer[sourceIdx]);
														reducedColorTable.Add(c);
													}
													rowPixel++;
													sourceIdx += 3;
												}
												// padding byte
												if (runLength % 2 != 0)
												{
													sourceIdx++;
												}
												break;
										}
										
										break;
								}
							}
							else
							{
								switch (CompressionFormat)
								{
									case CompressionFormat.BI_RLE4:
									case CompressionFormat.BI_RLE8:
										int leftPixel = buffer[sourceIdx];
										int rightPixel = buffer[sourceIdx];
										if (CompressionFormat == CompressionFormat.BI_RLE4)
										{
											leftPixel &= 0x0F; // upper 4 bytes
											rightPixel = (rightPixel & 0xF0) >> 4; // lower 4 bytes
										}
										
										for (int i = 0; i < numPix; i++)
										{
											if (rowPixel > Width - 1)
											{
												sourceIdx++;
												StartNextRow();
												break;
											}

											int startByte = rowPixel * 4;
											int pix = i % 2 == 0 ? rightPixel : leftPixel;
											if (pix > ColorTable.Length - 1)
											{
												rowBytes[startByte] = byte.MinValue;
												rowBytes[startByte + 1] = byte.MinValue;
												rowBytes[startByte + 2] = byte.MinValue;
												rowBytes[startByte + 3] = byte.MaxValue;
											}
											else
											{
												rowBytes[startByte] = ColorTable[pix].R;
												rowBytes[startByte + 1] = ColorTable[pix].G;
												rowBytes[startByte + 2] = ColorTable[pix].B;
												rowBytes[startByte + 3] = byte.MaxValue;
											}
											rowPixel++;
										}
										sourceIdx++;
										// Padding
										sourceIdx = sourceIdx % 2 == 0 ? sourceIdx : sourceIdx + 2 - sourceIdx % 2;
										break;
									case CompressionFormat.BI_RLE24:
										for (int i = 0; i < numPix; i++)
										{
											if (rowPixel > Width - 1)
											{
												sourceIdx++;
												StartNextRow();
                                                break;
                                            }

											int startByte = rowPixel * 4;
											rowBytes[startByte] = buffer[sourceIdx + 2];
											rowBytes[startByte + 1] = buffer[sourceIdx + 1];
											rowBytes[startByte + 2] = buffer[sourceIdx];
											rowBytes[startByte + 3] = byte.MaxValue;
											if (Plugin.EnabledHacks.ReduceTransparencyColorDepth && BitsPerPixel > BitsPerPixel.EightBitPalletized)
											{
												Color24 c = new Color24(buffer[sourceIdx + 2], buffer[sourceIdx + 1], buffer[sourceIdx]);
												reducedColorTable.Add(c);
											}
											rowPixel++;
										}
										sourceIdx += 3;
										break;
								}
							}
						}
						break;
				}
			}
			if (Plugin.EnabledHacks.ReduceTransparencyColorDepth && BitsPerPixel > BitsPerPixel.EightBitPalletized)
			{
				ColorTable = reducedColorTable.ToArray();
			}
			return true;
		}

		/// <summary>Attempts to find an undeclared color table</summary>
		private void AttemptToFindColorTable(Stream fileReader, int colorSize)
		{
			int possibleColorTableSize = dataOffset - (int)fileReader.Position;
			int possibleColors = possibleColorTableSize / colorSize;
			if (possibleColors > 3) // color table appears to be present, even though length was declared as zero
			{
				buffer = new byte[possibleColorTableSize];
				// ReSharper disable once MustUseReturnValue
				fileReader.Read(buffer, 0, possibleColorTableSize);
				bool colorTableFound = false;
				for (int i = 0; i < buffer.Length; i++)
				{
					// sense check- there should be some colors here, not just pure black / white!
					if (buffer[i] != byte.MinValue && buffer[i] != byte.MaxValue)
					{
						colorTableFound = true;
						break;
					}
				}

				if (colorTableFound)
				{
					// Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Undeclared ColorTable found in Bitmap file " + fileName);
					ColorsUsed = possibleColors;
					ColorTable = new Color24[ColorsUsed];
					for (int currentColor = 0; currentColor < ColorsUsed; currentColor++)
					{
						int idx = currentColor * colorSize;
						ColorTable[currentColor] = new Color24(buffer[idx + 2], buffer[idx + 1], buffer[idx]); // stored as BGR in bitmap, we want RGB
					}
				}
			}

		}

		/// <summary>Places the current row onto the pixel stack and starts a new row</summary>
		private void StartNextRow()
		{
			if (currentRow < 0 || currentRow > Height -1)
			{
				rowPixel = 0;
				return;
			}
			Array.Copy(rowBytes, 0, ImageData, currentRow * Width * 4, rowBytes.Length);
			Array.Clear(rowBytes,0, rowBytes.Length);
			if (TopDown)
			{
				currentRow++;
			}
			else
			{
				currentRow--;
			}
			rowPixel = 0;
		}

		

		public void Dispose()
		{
			
		}
	}
}
#pragma warning restore IDE0052, CS0414
