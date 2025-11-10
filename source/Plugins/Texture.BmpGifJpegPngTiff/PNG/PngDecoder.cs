//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023- 2025, Christopher Lees, The OpenBVE Project
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
using System.Runtime.CompilerServices;
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
						if (fileReader.Read(buffer, 0,8) != 8)
						{
							Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Insufficient Header data for Chunk " + currentChunk + " in PNG file " + fileName);
							return false;
						}

						int chunkSize = (int)BinaryExtensions.GetUInt32(buffer, 0, Endianness.Big);
						ChunkType chunkType = ReadChunkType(buffer);

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
									case 16:
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
										BytesPerPixel = BitDepth == 8 ? 4 : 8;
										break;
									case ColorType.GrayscaleAlpha:
										BytesPerPixel = 2;
										break;
									default:
										Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "This decoder does not currently support " + ColorType + " in PNG file " + fileName);
										return false;
								}

								ScanlineLength = Math.Max(1, (int)Math.Ceiling((Width * BytesPerPixel) / (double)ScanlineLength)); // scanline must be a minumum of 1 byte in length. Always round up

								pixelBuffer = ColorType != ColorType.Palleted && ColorType != ColorType.GrayscaleAlpha && BytesPerPixel != 8 ? new byte[Width * Height * BytesPerPixel] : new byte[Width * Height * 4];
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
												case ScanlineFilterAlgorithm.None:
													// ignore
													break;
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
												default:
													// decoder has messed up somewhere, so don't return junk data
													return false;
											}
										}

                                        switch (ColorType)
                                        {
                                            case ColorType.Palleted:
                                                {
                                                    int pixelIndex = 0;
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

                                                                    pixelIndex++;
                                                                    if (pixelIndex >= Width)
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
                                                                pixelIndex++;
                                                                if (pixelIndex >= Width)
                                                                {
                                                                    // A single byte contains 4px, but the image may not be of a multiple of this
                                                                    break;
                                                                }
                                                                byte secondNibblet = (byte)((scanline[px] >> 4) & 0x03); // color of second pix
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].R;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].G;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].B;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[secondNibblet].A;
                                                                pixelIndex++;
                                                                if (pixelIndex >= Width)
                                                                {
                                                                    // A single byte contains 4px, but the image may not be of a multiple of this
                                                                    break;
                                                                }
                                                                byte thirdNibblet = (byte)((scanline[px] >> 2) & 0x03); // color of third pix
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].R;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].G;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].B;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[thirdNibblet].A;
                                                                pixelIndex++;
                                                                if (pixelIndex >= Width)
                                                                {
                                                                    // A single byte contains 4px, but the image may not be of a multiple of this
                                                                    break;
                                                                }
                                                                byte fourthNibblet = (byte)(scanline[px] & 0x03); // color of fourth pix
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].R;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].G;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].B;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[fourthNibblet].A;
                                                                pixelIndex++;
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
                                                                pixelIndex++;
                                                                if (pixelIndex >= Width)
                                                                {
                                                                    // A single byte contains 2px, but the image may not be of a multiple of this
                                                                    break;
                                                                }
                                                                byte rightNibble = (byte)(scanline[px] & 0x0F); // color of right pixel
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].R;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].G;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].B;
                                                                pixelBuffer[pixelsOffset++] = colorPalette.Colors[rightNibble].A;
                                                                pixelIndex++;
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
                                                    break;
                                                }
											case ColorType.GrayscaleAlpha:
												switch (BitDepth)
												{
													case 8:
														for (int px = 0; px < scanline.Length; px++)
														{
															pixelBuffer[pixelsOffset++] = scanline[px];
															pixelBuffer[pixelsOffset++] = scanline[px];
															pixelBuffer[pixelsOffset++] = scanline[px];
															pixelBuffer[pixelsOffset++] = scanline[px + 1];
															px++;
														}
														break;
													case 16:
														Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "16-bit GrayscaleAlpha PNG has not yet been implemented " + fileName);
														return false;
													default:
														Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "A GrayscaleAlpha PNG must have a bit-depth of 8 or 16 in file " + fileName);
														return false;
												}
												break;
                                            default:
												if (BytesPerPixel == 8)
												{
													for (int px = 0; px < scanline.Length; px+= 2)
													{
														// HACK: This discards the upper byte
														//		 Dynamic range may be lost, but otherwise we've got to play with endinan bitshifting and range checking, which is sloow....
														pixelBuffer[pixelsOffset++] = scanline[px];
													}
												}
												else
												{
													// just copy raw data into the pixel buffer
													Buffer.BlockCopy(scanline, 0, pixelBuffer, pixelsOffset, scanline.Length);
													pixelsOffset += scanline.Length;
												}
												break;
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
												
												// find total length of scanline and unfilter data
												int scanlineLength = pixelsPerScanline * BytesPerPixel;
												for (int j = 0; j < scanlineLength; j++)
												{
													// ReSharper disable once TooWideLocalVariableScope
													byte leftByte, upByte, upLeftByte;
													switch (scanlineFilterAlgorithm)
													{
														case ScanlineFilterAlgorithm.None:
															// ignore
															break;
														case ScanlineFilterAlgorithm.Sub:
															leftByte = j >= BytesPerPixel ? data[rowStartByte + j - BytesPerPixel] : (byte)0;
															data[rowStartByte + j] = (byte)((data[rowStartByte + j] + leftByte) % 256);
															break;
														case ScanlineFilterAlgorithm.Up:
															upByte = currentScanline == 0 ? (byte)0 : data[previousRowStartByte + j];
															data[rowStartByte + j] = (byte)((data[rowStartByte + j] + upByte) % 256);
															break;
														case ScanlineFilterAlgorithm.Average:
															leftByte = j >= BytesPerPixel ? data[rowStartByte + j - BytesPerPixel] : (byte)0;
															upByte = currentScanline == 0 ? (byte)0 : data[previousRowStartByte + j];
															data[rowStartByte + j] = (byte)((data[rowStartByte + j] + ((leftByte + upByte) >> 1)) % 256);
															break;
														case ScanlineFilterAlgorithm.Paeth:
															// NOTE: For Paeth filtered Row0 in any given pass, the previous scanline must be all zeroes
															// e.g. NWM_Open\Common\buildm0.png
															leftByte = j - BytesPerPixel >= 0 ? data[rowStartByte + j - BytesPerPixel] : (byte)0;
															upByte = currentScanline == 0 ? (byte)0 : data[previousRowStartByte + j];
															upLeftByte = currentScanline == 0 ? (byte)0 : j >= BytesPerPixel ? data[previousRowStartByte + j - BytesPerPixel] : (byte)0;	
															data[rowStartByte + j] = (byte)((data[rowStartByte + j] + PaethPredictor(leftByte, upByte, upLeftByte)) % 256);
															break;
														default:
															throw new Exception("Decoder error probably...");
													}
												}

												for (int j = 0; j < pixelsPerScanline; j++)
												{
													int pixelX, pixelY; // using two ints as opposed to Vector2 is c. 10% faster
													Adam7.GetPixelIndexForScanlineInPass(currentPass, currentScanline, j, out pixelX, out pixelY);
                                                switch (ColorType)
                                                {
                                                    case ColorType.Palleted:
                                                        {
                                                            // we're always converting to 4bpp in the output, but need the native bpp to find our position in the array, so don't actually set it
                                                            int start = Width * 4 * pixelY + pixelX * 4;
                                                            byte pixelByte = data[rowStartByte + (j * BytesPerPixel)];
                                                            switch (BitDepth)
                                                            {

                                                                case 4:
                                                                    if (j % 2 != 0)
                                                                    {
                                                                        byte leftNibble = (byte)((pixelByte & 0xF0) >> 4); // color of left pixel
                                                                        pixelBuffer[start] = colorPalette.Colors[leftNibble].R;
                                                                        pixelBuffer[start + 1] = colorPalette.Colors[leftNibble].G;
                                                                        pixelBuffer[start + 2] = colorPalette.Colors[leftNibble].B;
                                                                        pixelBuffer[start + 3] = colorPalette.Colors[leftNibble].A;
                                                                        if (j == pixelsPerScanline)
                                                                        {
                                                                            // second nibble of byte discarded
                                                                            currentByte++;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        byte rightNibble = (byte)(pixelByte & 0x0F); // color of right pixel
                                                                        pixelBuffer[start] = colorPalette.Colors[rightNibble].R;
                                                                        pixelBuffer[start + 1] = colorPalette.Colors[rightNibble].G;
                                                                        pixelBuffer[start + 2] = colorPalette.Colors[rightNibble].B;
                                                                        pixelBuffer[start + 3] = colorPalette.Colors[rightNibble].A;
                                                                        currentByte++;
                                                                    }
                                                                    break;
                                                                case 8:
                                                                    pixelBuffer[start] = colorPalette.Colors[pixelByte].R;
                                                                    pixelBuffer[start + 1] = colorPalette.Colors[pixelByte].G;
                                                                    pixelBuffer[start + 2] = colorPalette.Colors[pixelByte].B;
                                                                    pixelBuffer[start + 3] = colorPalette.Colors[pixelByte].A;
                                                                    currentByte++;
                                                                    break;
                                                            }

                                                            break;
                                                        }
													case ColorType.GrayscaleAlpha:
														switch (BitDepth)
														{
															case 8:
																// we're always converting to 4bpp in the output, but need the native bpp to find our position in the array, so don't actually set it
																int start = Width * 4 * pixelY + pixelX * 4;
																byte pixelByte = data[rowStartByte + (j * BytesPerPixel)];
																byte alphaByte = data[(rowStartByte + (j * BytesPerPixel)) + 1];
																pixelBuffer[start] = pixelByte;
																pixelBuffer[start + 1] = pixelByte;
																pixelBuffer[start + 2] = pixelByte;
																pixelBuffer[start + 3] = alphaByte;
																currentByte += 2;
																break;
															case 16:
																Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "16-bit GrayscaleAlpha PNG has not yet been implemented " + fileName);
																return false;
															default:
																Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "A GrayscaleAlpha PNG must have a bit-depth of 8 or 16 in file " + fileName);
																return false;
														}
														break;
                                                    default:
                                                        {
                                                            int start = Width * BytesPerPixel * pixelY + pixelX * BytesPerPixel;
                                                            Buffer.BlockCopy(data, rowStartByte + j * BytesPerPixel, pixelBuffer, start, BytesPerPixel);
                                                            currentByte += BytesPerPixel;
                                                            break;
                                                        }
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

					if (ColorType == ColorType.Palleted || ColorType == ColorType.GrayscaleAlpha || BytesPerPixel == 8)
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
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int PaethPredictor(int a, int b, int c)
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
