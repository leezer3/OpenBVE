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
								ColorType = (ColorType)chunkBuffer[9];
								CompressionMethod = chunkBuffer[10];
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

								pixelBuffer = new byte[Width * Height * 8];
								break;
							case ChunkType.PLTE:
								colorPalette = new Palette(chunkBuffer);
								return false;
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
						}
						// Chunk is followed by it's CRC
						byte[] crcBytes = new byte[4];
						if (fileReader.Read(crcBytes, 0, 4) != 4)
						{
							Plugin.CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Chunk should have been followed by it's CRC- Decoder error or truncated data in PNG file " + fileName);
							return false;
						}
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
						byte[] scanline = new byte[Width * BytesPerPixel];
						byte[] previousScanline = new byte[scanline.Length];
						
						using (DeflateStream deflate = new DeflateStream(chunkDataStream, CompressionMode.Decompress))
						{
							for (int i = 0; i < Height; i++)
							{
								ScanlineFilterAlgorithm scanlineFilterAlgorithm = (ScanlineFilterAlgorithm)deflate.ReadByte();
								if (deflate.Read(scanline, 0, scanline.Length) != scanline.Length)
								{
									Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "Insufficient data decompressed from IDAT chunk in PNG file " + fileName);
									return false;
								}

								switch (interlaceMethod)
								{
									case InterlaceMethod.Disabled:
										if (scanlineFilterAlgorithm != ScanlineFilterAlgorithm.None)
										{
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
										}

										if (ColorType == ColorType.Palleted)
										{
											for (int px = 0; px < scanline.Length; px++)
											{
												pixelBuffer[pixelsOffset] = colorPalette.Colors[scanline[px]].R;
												pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].G;
												pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].B;
												pixelBuffer[pixelsOffset++] = colorPalette.Colors[scanline[px]].A;
											}
										}
										else
										{
											// just copy raw data into the pixel buffer
											Buffer.BlockCopy(scanline, 0, pixelBuffer, pixelsOffset, scanline.Length);
											pixelsOffset += scanline.Length;
										}
										
										
										Buffer.BlockCopy(scanline, 0, previousScanline, 0, scanline.Length);
										break;
									case InterlaceMethod.Adam7:
										Plugin.CurrentHost.ReportProblem(ProblemType.UnsupportedData, "This decoder does not currently support " + interlaceMethod + " in PNG file " + fileName);
										return false;
									default:
										Plugin.CurrentHost.ReportProblem(ProblemType.InvalidData, "Invalid interlacing method in PNG file " + fileName);
										return false;
								}
							}
						}
					}
					
					return true;
				}
			}
		}

		private int PaethPredictor(int a, int b, int c)
		{
			int p = a + b - c;
			int pa = Math.Abs(p - a);
			int pb = Math.Abs(p - b);
			int pc = Math.Abs(p - c);

			return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
		}

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
