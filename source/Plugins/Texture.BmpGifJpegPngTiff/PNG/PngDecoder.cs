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

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Math;

namespace Plugin.PNG
{
	class PngDecoder : IDisposable
	{
		// INTERNAL MEMBERS
		/// <summary>The width of the image</summary>
		internal int Width;
		/// <summary>The height of the image</summary>
		internal int Height;
		/// <summary>The bit depth of the image</summary>
		internal int BitDepth;
		/// <summary>The color type of the image</summary>
		internal ColorType ColorType;
		/// <summary>The compression method</summary>
		internal int CompressionMethod;
		/// <summary>Buffer containing data read from the file</summary>
		internal byte[] buffer;
		/// <summary>Buffer for the pixel bytes of the current chunk</summary>
		internal byte[] pixelBuffer;
		/// <summary>Buffer for the current idat</summary>
		internal byte[] idatBuffer;
		/// <summary>Buffer for the chunk bytes</summary>
		internal byte[] chunkBuffer;
		/// <summary>The color palette in use</summary>
		internal Palette colorPalette;
		/// <summary>The number of expected bytes in a scanline</summary>
		internal int ScanlineLength;

		internal byte filterMethod;
		/// <summary>The interlacing method</summary>
		internal InterlaceMethod interlaceMethod;
		/// <summary>The number of bytes per pixel</summary>
		internal int BytesPerPixel;

		/// <summary>Reads a PNG file using this decoder</summary>
		/// <param name="fileName">The file to read</param>
		/// <returns>Whether reading succeeded</returns>
		internal bool Read(string fileName)
		{
			using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				using (BinaryReader fileReader = new BinaryReader(stream))
				{
					// HEADER
					buffer = new byte[8];
					if (fileReader.Read(buffer, 0, 8) != 8)
					{
						Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient Header data in PNG file " + fileName);
						return false;
					}

					string signature = Encoding.ASCII.GetString(buffer, 1, 3);
					if (signature != "PNG")
					{
						// not a png file
						return false;
					}

					int currentChunk = 0;
					while (true)
					{
						byte[] chunkHeader = new byte[8];
						if (fileReader.Read(chunkHeader, 0, chunkHeader.Length) != 8)
						{
							Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient Header data for Chunk " + currentChunk + " in PNG file " + fileName);
							return false;
						}

						int chunkSize = (int)BinaryExtensions.GetUInt32(chunkHeader, 0, Endianness.Big);
						ChunkType chunkType = ReadChunkType(chunkHeader);

						if (chunkType == ChunkType.IEND)
						{
							// Last image data chunk- We should now have the whole image
							break;
						}

						// Allocate chunk buffer and read the chunk data
						chunkBuffer = new byte[chunkSize];
						if (fileReader.Read(chunkBuffer, 0, chunkSize) != chunkSize)
						{
							Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient data for Chunk " + currentChunk + " in PNG file " + fileName);
							return false;
						}

						switch (chunkType)
						{
							case ChunkType.IHDR:
								Width = (int)BinaryExtensions.GetUInt32(chunkBuffer, 0, Endianness.Big);
								Height = (int)BinaryExtensions.GetUInt32(chunkBuffer, 4, Endianness.Big);
								BitDepth = chunkBuffer[8];
								switch (BitDepth)
								{
									case 1:
										ScanlineLength = 8;
										break;
									case 2:
										ScanlineLength = 4;
										break;
									case 4:
										ScanlineLength = 2;
										break;
									case 8:
										ScanlineLength = 1;
										break;
									default:
										Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Invalid or unsupported BitDepth in PNG file " + fileName);
										return false;
								}
								ColorType = (ColorType)chunkBuffer[9];
								CompressionMethod = chunkBuffer[10];
								// Appears to be ignored in practice as each scanline must have a filter byte set
								filterMethod = chunkBuffer[11];
								interlaceMethod = (InterlaceMethod)chunkBuffer[12];

								switch (ColorType)
								{
									case ColorType.Palleted: // each filtered byte becomes a pallete index
									case ColorType.Grayscale:
										BytesPerPixel = 1;
										break;
									case ColorType.Rgb:
										BytesPerPixel = 3;
										break;
									case ColorType.Rgba:
										BytesPerPixel = 4;
										break;
									default:
										Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "This decoder does not currently support " + ColorType + " in PNG file " + fileName);
										return false;
								}

								ScanlineLength = (Width * BytesPerPixel) / ScanlineLength;

								pixelBuffer = ColorType != ColorType.Palleted ? new byte[Width * Height * BytesPerPixel] : new byte[Width * Height * 4];
								
								break;
							case ChunkType.PLTE:
								colorPalette = new Palette(chunkBuffer);
								break;
							case ChunkType.tRNS:
								if (colorPalette == null)
								{
									Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "The tRNS chunk must be preceeded by a PLTE chunk in PNG file " + fileName);
									return false;
								}
								colorPalette.SetAlphaValues(chunkBuffer);
								return false;
							case ChunkType.IDAT:
								// IDAT chunks contain image data
								// Data may be split over one or more chunks
								if (idatBuffer == null)
								{
									// First chunk
									idatBuffer = chunkBuffer;
								}
								else
								{
									// Combine the new chunk with the old
									int idatLength = idatBuffer.Length;
									Array.Resize(ref idatBuffer, idatLength + chunkBuffer.Length);
									Buffer.BlockCopy(chunkBuffer, 0, idatBuffer, idatLength, chunkBuffer.Length);
								}
								break;
							case ChunkType.CgBI:
								Plugin.CurrentHost.ReportProblem(ProblemType.InvalidOperation, "CgBI encoded PNGs are not supported by this decoder in PNG file " + fileName);
								return false;
						}
						// Chunk is followed by it's CRC
						byte[] crcBytes = new byte[4];
						if (fileReader.Read(crcBytes, 0, 4) != 4)
						{
							Plugin.CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Chunk should have been followed by it's CRC- Decoder error or truncated data in PNG file " + fileName);
							return false;
						}
						//FIXME: Actually calculate the CRC
						currentChunk++;
					}

					if (idatBuffer == null)
					{
						Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "No IDAT chunk found in PNG file " + fileName);
						return false;
					}

					using (MemoryStream chunkDataStream = new MemoryStream(idatBuffer))
					{
						// Skip zlib header bytes
						chunkDataStream.Seek(2, SeekOrigin.Begin);

						int pixelsOffset = 0;

						using (DeflateStream deflate = new DeflateStream(chunkDataStream, CompressionMode.Decompress))
						{
							switch (interlaceMethod)
							{
								case InterlaceMethod.Disabled:
									byte[] scanline = new byte[ScanlineLength];
									byte[] previousScanline = new byte[ScanlineLength];
									for (int i = 0; i < Height; i++)
									{
										ScanlineFilterAlgorithm scanlineFilterAlgorithm = (ScanlineFilterAlgorithm)deflate.ReadByte();
										if (deflate.Read(scanline, 0, ScanlineLength) != ScanlineLength)
										{
											Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "Insufficient data decompressed from IDAT chunk in PNG file " + fileName);
											return false;
										}
										
										for (int x = 0; x < scanline.Length; x++)
										{
											// ReSharper disable once TooWideLocalVariableScope
											byte leftByte, upByte, upLeftByte;
											switch (scanlineFilterAlgorithm)
											{
												case ScanlineFilterAlgorithm.Sub:
													leftByte = x >= BytesPerPixel ? scanline[x - BytesPerPixel] : (byte)0;
													scanline[x] = (byte)((scanline[x] + leftByte) % 256);
													break;
												case ScanlineFilterAlgorithm.Up:
													upByte = previousScanline[x];
													scanline[x] = (byte)((scanline[x] + upByte) % 256);
													break;
												case ScanlineFilterAlgorithm.Average:
													leftByte = x >= BytesPerPixel ? scanline[x - BytesPerPixel] : (byte)0;
													upByte = previousScanline[x];
													scanline[x] = (byte)((scanline[x] + ((leftByte + upByte) >> 1)) % 256);
													break;
												case ScanlineFilterAlgorithm.Paeth:
													leftByte = x >= BytesPerPixel ? scanline[x - BytesPerPixel] : (byte)0;
													upByte = previousScanline[x];
													upLeftByte = x >= BytesPerPixel ? previousScanline[x - BytesPerPixel] : (byte)0;
													scanline[x] = (byte)((scanline[x] + PaethPredictor(leftByte, upByte, upLeftByte)) % 256);
													break;
											}
										}

										if (ColorType == ColorType.Palleted)
										{
											switch (BitDepth)
											{
												case 1:
													for (int px = 0; px < scanline.Length; px++)
													{
														for (int currentBit = 0; currentBit < 8; currentBit++)
														{
															if ((scanline[px] & 1 << (7 - currentBit % 8)) != 0)
															{
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[1].R;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[1].G;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[1].B;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[1].A;
															}
															else
															{
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[0].R;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[0].G;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[0].B;
																pixelBuffer[pixelsOffset++] = colorPalette.Colors[0].A;
															}
															if (px >= Width)
															{
																// A single byte contains 8px, but the image may not be of a multiple of this
																break;
															}
														}
													}
													break;
												case 2:
													for (int px = 0; px < scanline.Length; px++)
													{
														byte firstNibblet = (byte)((scanline[px] >> 6) & 0x03); // color of first pix
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[firstNibblet].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[firstNibblet].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[firstNibblet].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[firstNibblet].A;
														byte secondNibblet = (byte)((scanline[px] >> 4) & 0x03); // color of second pix
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].A;
														byte thirdNibblet = (byte)((scanline[px] >> 2) & 0x03); // color of third pix
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].A;
														byte fourthNibblet = (byte)(scanline[px] & 0x03); // color of fourth pix
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].A;
													}
													break;
												case 4:
													for (int px = 0; px < scanline.Length; px++)
													{
														byte leftNibble = (byte)((scanline[px] & 0xF0) >> 4); // color of left pixel
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[leftNibble].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[leftNibble].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[leftNibble].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[leftNibble].A;
														byte rightNibble = (byte) (scanline[px] & 0x0F); // color of right pixel
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].A;
													}
													break;
												case 8:
													for (int px = 0; px < scanline.Length; px++)
													{
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].R;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].G;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].B;
														pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].A;
													}
													break;
											}
											
										}
										else
										{
											// just copy raw data into the pixel buffer
											Buffer.BlockCopy(scanline, 0, pixelBuffer, pixelsOffset, scanline.Length);
											pixelsOffset += scanline.Length;
										}

										Buffer.BlockCopy(scanline, 0, previousScanline, 0, scanline.Length);
									}
									break;
									case InterlaceMethod.Adam7:
										byte[] data;
										using (MemoryStream ms = new MemoryStream())
										{
											/*
											 * Entire datastream needs to be decoded for filters to work correctly,
											 * as some use data from previous passes
											 */
											deflate.CopyTo(ms);
											data = ms.ToArray();
										}

										int currentByte = 0; // current absolute byte decoded to
										var previousRowStartByte = -1;
										for (int currentPass = 0; currentPass < 7; currentPass++)
										{
											int numberOfScanlines = Adam7.GetNumberOfScanlinesForPass(Height, currentPass);
											int pixelsPerScanline = Adam7.GetNumberOfPixelsInScanline(Width, currentPass);
											if (numberOfScanlines <= 0 || pixelsPerScanline <= 0)
											{
												// nothing to be decoded this pass [valid as per spec]
												continue;
											}

											for (int currentScanline = 0; currentScanline < numberOfScanlines; currentScanline++)
											{
												ScanlineFilterAlgorithm scanlineFilterAlgorithm = (ScanlineFilterAlgorithm)data[currentByte++];
												int rowStartByte = currentByte;
												for (int j = 0; j < pixelsPerScanline; j++)
												{
													int pixelX, pixelY; // using two ints as opposed to Vector2 is c. 10% faster
													Adam7.GetPixelIndexForScanlineInPass(currentPass, currentScanline, j, out pixelX, out pixelY);
													for (int k = 0; k < BytesPerPixel; k++)
													{
														int relativeRowByte = j * BytesPerPixel + k; // relative index of byte within line
														// ReSharper disable once TooWideLocalVariableScope
														byte leftByte, upByte, upLeftByte;
														switch (scanlineFilterAlgorithm)
														{
															case ScanlineFilterAlgorithm.Sub:
																leftByte = relativeRowByte >= BytesPerPixel ? data[rowStartByte + relativeRowByte - BytesPerPixel] : (byte)0;
																data[rowStartByte + relativeRowByte] = (byte)((data[rowStartByte + relativeRowByte] + leftByte) % 256);
																break;
															case ScanlineFilterAlgorithm.Up:
																upByte = data[previousRowStartByte + relativeRowByte];
																data[rowStartByte + relativeRowByte] = (byte)((data[rowStartByte + relativeRowByte] + upByte) % 256);
																break;
															case ScanlineFilterAlgorithm.Average:
																leftByte = relativeRowByte >= BytesPerPixel ? data[rowStartByte + relativeRowByte - BytesPerPixel] : (byte)0;
																upByte = data[previousRowStartByte + relativeRowByte];
																data[rowStartByte + relativeRowByte] = (byte)((data[rowStartByte + relativeRowByte] + ((leftByte + upByte) >> 1)) % 256);
																break;
															case ScanlineFilterAlgorithm.Paeth:
																// NOTE: For Paeth filtered Row0 in any given pass, the previous scanline must be all zeroes
																// e.g. NWM_Open\Common\buildm0.png
																leftByte = relativeRowByte - BytesPerPixel >= 0 ? data[rowStartByte + relativeRowByte - BytesPerPixel] : (byte)0;
																upByte = currentScanline == 0 ? (byte)0 : data[previousRowStartByte + relativeRowByte];
																upLeftByte = currentScanline == 0 ? (byte)0 : relativeRowByte >= BytesPerPixel ? data[previousRowStartByte + relativeRowByte - BytesPerPixel] : (byte)0;	
																data[rowStartByte + relativeRowByte] = (byte)((data[rowStartByte + relativeRowByte] + PaethPredictor(leftByte, upByte, upLeftByte)) % 256);
																break;
														}
														currentByte++;
													}
													
													if (ColorType == ColorType.Palleted)
													{
														// we're always converting to 4bpp in the output, but need the native bpp to find our position in the array, so don't actually set it
														int start = Width * 4 * pixelY + pixelX * 4;
														switch (BitDepth)
														{
															case 1:
																for (int currentBit = 0; currentBit < 8; currentBit++)
																{
																	if ((data[rowStartByte + j * BytesPerPixel] & 1 << (7 - currentBit % 8)) != 0)
																	{
																		pixelBuffer[start++] = colorPalette.Colors[1].R;
																		pixelBuffer[start++] = colorPalette.Colors[1].G;
																		pixelBuffer[start++] = colorPalette.Colors[1].B;
																		pixelBuffer[start++] = colorPalette.Colors[1].A;
																	}
																	else
																	{
																		pixelBuffer[start++] = colorPalette.Colors[0].R;
																		pixelBuffer[start++] = colorPalette.Colors[0].G;
																		pixelBuffer[start++] = colorPalette.Colors[0].B;
																		pixelBuffer[start++] = colorPalette.Colors[0].A;
																	}

																	if (pixelX + currentBit >= Width)
																	{
																		// A single byte contains 8px, but the image may not be of a multiple of this
																		break;
																	}
																}

																break;
															case 2:
																byte firstNibblet = (byte)((data[rowStartByte + j * BytesPerPixel] >> 6) & 0x03); // color of first pix
																pixelBuffer[start] = colorPalette.Colors[firstNibblet].R;
																pixelBuffer[start + 01] = colorPalette.Colors[firstNibblet].G;
																pixelBuffer[start + 02] = colorPalette.Colors[firstNibblet].B;
																pixelBuffer[start + 03] = colorPalette.Colors[firstNibblet].A;
																byte secondNibblet = (byte)((data[rowStartByte + j * BytesPerPixel] >> 4) & 0x03); // color of second pix
																pixelBuffer[start + 04] = colorPalette.Colors[secondNibblet].R;
																pixelBuffer[start + 05] = colorPalette.Colors[secondNibblet].G;
																pixelBuffer[start + 06] = colorPalette.Colors[secondNibblet].B;
																pixelBuffer[start + 07] = colorPalette.Colors[secondNibblet].A;
																byte thirdNibblet = (byte)((data[rowStartByte + j * BytesPerPixel] >> 2) & 0x03); // color of third pix
																pixelBuffer[start + 08] = colorPalette.Colors[thirdNibblet].R;
																pixelBuffer[start + 09] = colorPalette.Colors[thirdNibblet].G;
																pixelBuffer[start + 10] = colorPalette.Colors[thirdNibblet].B;
																pixelBuffer[start + 11] = colorPalette.Colors[thirdNibblet].A;
																byte fourthNibblet = (byte)(data[rowStartByte + j * BytesPerPixel] & 0x03); // color of fourth pix
																pixelBuffer[start + 12] = colorPalette.Colors[fourthNibblet].R;
																pixelBuffer[start + 13] = colorPalette.Colors[fourthNibblet].G;
																pixelBuffer[start + 14] = colorPalette.Colors[fourthNibblet].B;
																pixelBuffer[start + 15] = colorPalette.Colors[fourthNibblet].A;
																break;
															case 4:
																byte leftNibble = (byte)((data[rowStartByte + j * BytesPerPixel] & 0xF0) >> 4); // color of left pixel
																pixelBuffer[start] = colorPalette.Colors[leftNibble].R;
																pixelBuffer[start + 1] = colorPalette.Colors[leftNibble].G;
																pixelBuffer[start + 2] = colorPalette.Colors[leftNibble].B;
																pixelBuffer[start + 3] = colorPalette.Colors[leftNibble].A;
																byte rightNibble = (byte)(data[rowStartByte + j * BytesPerPixel] & 0x0F); // color of right pixel
																pixelBuffer[start + 4] = colorPalette.Colors[rightNibble].R;
																pixelBuffer[start + 5] = colorPalette.Colors[rightNibble].G;
																pixelBuffer[start + 6] = colorPalette.Colors[rightNibble].B;
																pixelBuffer[start + 7] = colorPalette.Colors[rightNibble].A;
																break;
															case 8:
																pixelBuffer[start] = colorPalette.Colors[data[rowStartByte + j * BytesPerPixel]].R;
																pixelBuffer[start + 1] = colorPalette.Colors[data[rowStartByte + j * BytesPerPixel]].G;
																pixelBuffer[start + 2] = colorPalette.Colors[data[rowStartByte + j * BytesPerPixel]].B;
																pixelBuffer[start + 3] = colorPalette.Colors[data[rowStartByte + j * BytesPerPixel]].A;
																break;
														}
													}
													else
													{
														int start = Width * BytesPerPixel * pixelY + pixelX * BytesPerPixel;
														Buffer.BlockCopy(data, rowStartByte + j * BytesPerPixel, pixelBuffer, start, BytesPerPixel);
													}
												}
												previousRowStartByte = rowStartByte;
											}
										}
										break;
									default:
										Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Invalid interlacing method in PNG file " + fileName);
										return false;
							}
						}
					}

					if (ColorType == ColorType.Palleted)
					{
						// need the final bpp to reflect what we've converted the image to, not the bpp in the file used whilst loading
						BytesPerPixel = 4;
					}
					return true;
				}
			}
		}

		/// <summary>Predicts the Paeth value for any given byte</summary>
		/// <param name="a">The left byte</param>
		/// <param name="b">The upper byte</param>
		/// <param name="c">The upper left byte</param>
		private int PaethPredictor(int a, int b, int c)
		{
			int p = a + b - c;
			int pa = Math.Abs(p - a);
			int pb = Math.Abs(p - b);
			int pc = Math.Abs(p - c);

			return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
		}

		/// <summary>Reads the chunk type from the signature bytes</summary>
		private ChunkType ReadChunkType(byte[] headerData)
		{
			string chunkSignature = Encoding.ASCII.GetString(headerData, 4, 4);
			ChunkType type;
			Enum.TryParse(chunkSignature, false, out type);
			return type;
		}

		public void Dispose()
		{
		}
	}
}
