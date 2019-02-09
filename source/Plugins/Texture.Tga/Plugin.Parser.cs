#pragma warning disable 169, 414 //We don't use a bunch of fields, but keep them in case required later
// ReSharper disable NotAccessedField.Local
// ReSharper disable UnusedMember.Local
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;

namespace Plugin
{
	public partial class Plugin : TextureInterface
	{
		//Extension area information
		private int ExtensionAreaSize;
		//Date, time, author name, job name, job time and comments not needed....
		//Version number
		private Color ExtensionAreaKeyColor;
		private int PixelAspectRatioNumerator;
		private int PixelAspectRatioDenominator;
		private int GammaNumerator;
		private int GammaDenominator;
		private int ColorCorrectionOffset;
		private int PostageStampOffset;
		private int ScanLineOffset;
		private int AttributesType;


		//Basic footer information
		private bool NewTGA;
		private int ExtensionAreaOffset;
		private int DeveloperDirectoryOffset;
		private string ReservedCharacter;
		//Basic header information
		private byte ImageIDLength;
		private byte PixelDepth;
		private byte ImageDescriptor;
		private bool HasColorMap;
		private int AttributeBits;
		private int VerticalTransferOrder;
		private int HorizontalTransferOrder;
		private string ImageID;
		private short ImageWidth;
		private short ImageHeight;

		//Color map
		private int ColorMapType;
		private ImageTypes ImageType;
		private int ColorMapLength;
		private byte ColorMapEntrySize;
		private readonly List<Color> ColorMap = new List<Color>();

		//Scan line table
		private readonly List<int> ScanLineTable = new List<int>();

		//Color correction table
		private readonly List<Color> ColorCorrectionTable = new List<Color>();

		//Other
		private GCHandle ByteArrayHandle;
		private Bitmap bitmap;

		private enum ImageTypes : byte
		{
			None = 0,
			UncompressedColorMapped = 1,
			UncompressedTrueColor = 2,
			UncompressedGreyscale = 3,
			CompressedColorMapped = 9,
			CompressedRGB = 10,
			CompressedGreyscale = 11
		}

		internal bool Parse(string fileName, out Texture texture)
		{

			byte[] filebytes = System.IO.File.ReadAllBytes(fileName);
			if (filebytes.Length > 0)
			{
				using (MemoryStream filestream = new MemoryStream(filebytes))
				{
					if (filestream.Length > 0 && filestream.CanSeek == true)
					{
						using (BinaryReader binReader = new BinaryReader(filestream))
						{
							//First we need to read the footer
							try
							{
								//Start reading at 18 bytes from the end of the file
								binReader.BaseStream.Seek(-18, SeekOrigin.End);
								//Read the signature string to it's textual representation
								var Signature = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(16)).TrimEnd('\0');
								if (String.CompareOrdinal(Signature, "TRUEVISION-XFILE") == 0)
								{
									//This is a new TGA format file, so we must read the footer information
									NewTGA = true;
									//Reset seek position
									binReader.BaseStream.Seek((-26), SeekOrigin.End);
									//Read the extension area offset
									ExtensionAreaOffset = binReader.ReadInt32();
									//Read the developer directory offset
									DeveloperDirectoryOffset = binReader.ReadInt32();
									//Skip signature
									binReader.ReadBytes(16);
									//Read the reserved character
									ReservedCharacter = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(1)).TrimEnd('\0');
								}
								else
								{
									//This is an old TGA format file
								}
							}
							catch
							{
								//Failed to load the footer information from the TGA file....
								CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Failed to load TGA footer.");
								texture = null;
								return false;
							}
							//Now read the basic header....
							try
							{
								//Start reading at the start of the file
								binReader.BaseStream.Seek(0, SeekOrigin.Begin);
								//ImageID Length
								ImageIDLength = binReader.ReadByte();
								//Byte showing the color map type ==> Either none or has a color map
								var ColorMapByte = binReader.ReadByte();
								if (ColorMapByte == 0)
								{
									HasColorMap = false;
								}
								else
								{
									HasColorMap = true;
								}
								//Byte showing the image type
								ImageType = (ImageTypes)binReader.ReadByte();
								//16-bit integer showing the first entry in the color map
								binReader.ReadInt16();
								//16-bit integer showing the length of the color map
								ColorMapLength = binReader.ReadInt16();
								//Byte showing the size of a color map entry
								ColorMapEntrySize = binReader.ReadByte();
								//16-bit integer showing the X-origin
								binReader.ReadInt16();
								//16-bit integer showing the Y-origin
								binReader.ReadInt16();
								//16-bit integer showing the image width
								ImageWidth = binReader.ReadInt16();
								//16-bit integer showing the image height
								ImageHeight = binReader.ReadInt16();
								//Byte showing the pixel depth
								PixelDepth = binReader.ReadByte();

								ImageDescriptor = binReader.ReadByte();
								AttributeBits = (ImageDescriptor >> 0) & ((1 << 4) - 1);
								HorizontalTransferOrder = (ImageDescriptor >> 5) & ((1 << 1) - 1);
								VerticalTransferOrder = (ImageDescriptor >> 4) & ((1 << 1) - 1);

								// load ImageID value if any
								if (ImageIDLength > 0)
								{
									byte[] ImageIDValueBytes = binReader.ReadBytes(ImageIDLength);
									ImageID = System.Text.Encoding.ASCII.GetString(ImageIDValueBytes).TrimEnd('\0');
								}
							}
							catch
							{
								//Failed to load the basic header information from the TGA file....
								CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Failed to load TGA header.");
								texture = null;
								return false;
							}
							//Next, we need to read the color map if one is included
							if (HasColorMap)
							{
								if (ColorMapLength > 0)
								{
									try
									{
										for (int i = 0; i < ColorMapLength; i++)
										{
											int a, b, g, r;
											// load each color map entry based on the ColorMapEntrySize value
											switch (ColorMapEntrySize)
											{
												case 15:
													byte[] color15 = binReader.ReadBytes(2);
													ColorMap.Add(GetColorFrom2Bytes(color15[1], color15[0]));
													break;
												case 16:
													byte[] color16 = binReader.ReadBytes(2);
													ColorMap.Add(GetColorFrom2Bytes(color16[1], color16[0]));
													break;
												case 24:
													b = Convert.ToInt32(binReader.ReadByte());
													g = Convert.ToInt32(binReader.ReadByte());
													r = Convert.ToInt32(binReader.ReadByte());
													ColorMap.Add(System.Drawing.Color.FromArgb(r, g, b));
													break;
												case 32:
													a = Convert.ToInt32(binReader.ReadByte());
													b = Convert.ToInt32(binReader.ReadByte());
													g = Convert.ToInt32(binReader.ReadByte());
													r = Convert.ToInt32(binReader.ReadByte());
													ColorMap.Add(System.Drawing.Color.FromArgb(a, r, g, b));
													break;
												default:
													//No other entry sizes than 15, 16, 24 & 32 are supported....
													throw new Exception();

											}


										}
									}
									catch
									{
										CurrentHost.ReportProblem(ProblemType.InvalidOperation, "TGA ColorMap not correctly formatted.");
										//Color map not correctly formatted
										texture = null;
										return false;
									}
								}
								else
								{
									//Image requires a color map, and one is not present
									CurrentHost.ReportProblem(ProblemType.InvalidOperation, "TGA Image requires a color map which is not present.");
									texture = null;
									return false;
								}
							}
							//Read the extension area if applicable
							if (NewTGA)
							{
								try
								{
									//Reset seek position to the start of the extension area
									binReader.BaseStream.Seek(ExtensionAreaOffset, SeekOrigin.Begin);
									//Get the size of the extension area
									ExtensionAreaSize = (int)binReader.ReadInt16();
									//Skip the author name- We don't need this
									binReader.ReadBytes(41);
									//Skip the author comments- We don't need this
									binReader.ReadBytes(324);
									//Date & time values, again we don't need these
									binReader.ReadInt16();
									binReader.ReadInt16();
									binReader.ReadInt16();
									binReader.ReadInt16();
									binReader.ReadInt16();
									binReader.ReadInt16();
									//Skip the job name
									binReader.ReadBytes(41);
									//Job date & time values
									binReader.ReadInt16();
									binReader.ReadInt16();
									binReader.ReadInt16();
									//ID name of software which wrote the file, skip
									binReader.ReadBytes(41);
									//Software version number & letter
									binReader.ReadInt16();
									binReader.ReadBytes(1);
									//Aha- Something useful, set the key color
									var a = (int)binReader.ReadByte();
									var r = (int)binReader.ReadByte();
									var b = (int)binReader.ReadByte();
									var g = (int)binReader.ReadByte();
									ExtensionAreaKeyColor = Color.FromArgb(a, r, g, b);
									PixelAspectRatioNumerator = (int)binReader.ReadInt16();
									PixelAspectRatioDenominator = (int)binReader.ReadInt16();
									GammaNumerator = (int)binReader.ReadInt16();
									GammaDenominator = (int)binReader.ReadInt16();
									ColorCorrectionOffset = binReader.ReadInt32();
									PostageStampOffset = binReader.ReadInt32();
									ScanLineOffset = binReader.ReadInt32();
									AttributesType = (int)binReader.ReadByte();
									//If a scan line table offset is included, read this
									if (ScanLineOffset > 0)
									{
										binReader.BaseStream.Seek(ScanLineOffset, SeekOrigin.Begin);
										for (int i = 0; i < ImageHeight; i++)
										{
											ScanLineTable.Add(binReader.ReadInt32());
										}
									}
									//If a color correction table is included, read this
									if (ColorCorrectionOffset > 0)
									{
										binReader.BaseStream.Seek(ColorCorrectionOffset, SeekOrigin.Begin);
										for (int i = 0; i < 256; i++)
										{
											a = (int)binReader.ReadInt16();
											r = (int)binReader.ReadInt16();
											b = (int)binReader.ReadInt16();
											g = (int)binReader.ReadInt16();
											ColorCorrectionTable.Add(Color.FromArgb(a, r, g, b));
										}
									}
								}
								catch
								{
									//Extension area not correctly formatted
									CurrentHost.ReportProblem(ProblemType.InvalidOperation, "TGA Extension Area not correctly formatted.");
									texture = null;
									return false;
								}
								//We should now have all the data required to read our image into memory

								//Calculate the stride value
								var stride = (((int)ImageWidth * (int)PixelDepth + 31) & ~31) >> 3;
								//Calculate the padding value
								var padding = stride - ((((int)ImageWidth * (int)PixelDepth) + 7) / 8);
								//Next, load the image data into memory

								//Create the padding array, as stride must be a multiple of 4
								byte[] paddingBytes = new byte[padding];
								//Create the temporary row lists
								var rows = new System.Collections.Generic.List<System.Collections.Generic.List<byte>>();
								var row = new System.Collections.Generic.List<byte>();

								//Calculate the offset & seek to the start of the image data
								var ImageDataOffset = 18 + ImageIDLength;
								int Bytes = 0;
								switch (ColorMapEntrySize)
								{
									case 15:
										ImageDataOffset += 2 * ColorMapLength;
										break;
									case 16:
										Bytes = 2 * ColorMapLength;
										break;
									case 24:
										Bytes = 3 * ColorMapLength;
										break;
									case 32:
										Bytes = 4 * ColorMapLength;
										break;

								}
								ImageDataOffset += Bytes;
								binReader.BaseStream.Seek(ImageDataOffset, SeekOrigin.Begin);

								//Size in bytes for each row
								int ImageRowByteSize = (int)ImageWidth * ((int)(PixelDepth / 8));

								//Size in bytes for whole image
								int ImageByteSize = ImageRowByteSize * (int)ImageHeight;

								if (ImageType == ImageTypes.CompressedColorMapped || ImageType == ImageTypes.CompressedRGB || ImageType == ImageTypes.CompressedGreyscale)
								{
									var BytesRead = 0;
									var RowBytesRead = 0;
									//Read image
									while (BytesRead < ImageByteSize)
									{
										var Packet = binReader.ReadByte();
										var PacketType = (Packet >> 7) & ((1 << 1) - 1);
										var PixelCount = ((Packet >> 0) & ((1 << 7) - 1)) + 1;
										if (PacketType == 1)
										{
											byte[] Pixel = binReader.ReadBytes((int)PixelDepth / 8);
											for (int i = 0; i < PixelCount; i++)
											{

												foreach (byte b in Pixel)
												{
													row.Add(b);
												}
												RowBytesRead += Pixel.Length;
												BytesRead += Pixel.Length;

												//If this is a full row, addit to the list of rows, clear it and restart the counter
												if (RowBytesRead == ImageRowByteSize)
												{
													rows.Add(row);
													row = new System.Collections.Generic.List<byte>();
													RowBytesRead = 0;
												}
											}
										}
										else
										{
											int BytesToRead = PixelCount * (int)((int)PixelDepth / 8);
											for (int i = 0; i < BytesToRead; i++)
											{
												row.Add(binReader.ReadByte());
												BytesRead++;
												RowBytesRead++;

												//If this is a full row, addit to the list of rows, clear it and restart the counter
												if (RowBytesRead == ImageRowByteSize)
												{
													rows.Add(row);
													row = new System.Collections.Generic.List<byte>();
													RowBytesRead = 0;
												}

											}
										}

									}
								}
								else
								{
									for (int i = 0; i < (int)ImageHeight; i++)
									{
										for (int j = 0; j < ImageRowByteSize; j++)
										{
											row.Add(binReader.ReadByte());
										}
										rows.Add(row);
										row = new System.Collections.Generic.List<byte>();
									}
								}
								bool reverseRows = false;
								bool reverseBytes = false;
								//We now need to get the location of the first pixel to see if the rows need to be reversed when converted to bitmap
								if (VerticalTransferOrder == -1 && HorizontalTransferOrder == -1)
								{
									//Unknown==>
									reverseRows = true;
								}
								else if (VerticalTransferOrder == 0 && HorizontalTransferOrder == 1)
								{
									//Bottom Left
									reverseRows = true;
									reverseBytes = true;
								}
								else if (VerticalTransferOrder == 0 && HorizontalTransferOrder == 0)
								{
									//Bottom right
									reverseRows = true;
								}
								else if (VerticalTransferOrder == 1 && HorizontalTransferOrder == 1)
								{
									//Top left
									reverseBytes = true;
								}
								else
								{
									//Top right
									//No reversions
								}
								//Check and reverse rows & bytes if necessary
								if (reverseRows)
								{
									rows.Reverse();
								}
								if (reverseBytes)
								{
									for (int i = 0; i < rows.Count; i++)
									{
										rows[i].Reverse();
									}
								}
								byte[] ImageData;
								//Now create the final array using MemoryStream
								using (MemoryStream memoryStream = new MemoryStream())
								{
									for (int i = 0; i < rows.Count; i++)
									{
										byte[] RowBytes = rows[i].ToArray();
										//Write out the row and padding into the memorystream
										memoryStream.Write(RowBytes, 0, RowBytes.Length);
										memoryStream.Write(paddingBytes, 0, paddingBytes.Length);
									}
									//Convert the contents of the memorystream to our array
									ImageData = memoryStream.ToArray();

								}
								//Convert the byte array into a bitmap

								//First, calculate the stride
								var Stride = (((int)ImageWidth * (int)PixelDepth + 31) & ~31) >> 3;
								//We now need to calculate the pixel depth from the image attributes
								PixelFormat CurrentPixelFormat = PixelFormat.Undefined;
								switch (PixelDepth)
								{
									case 8:
										CurrentPixelFormat = PixelFormat.Format8bppIndexed;
										break;
									case 16:
										if (NewTGA == true && ExtensionAreaOffset > 0)
										{
											switch (AttributesType)
											{
												case 0:
												case 1:
												case 2:
													CurrentPixelFormat = PixelFormat.Format16bppRgb555;
													break;

												case 3:
													CurrentPixelFormat = PixelFormat.Format16bppArgb1555;
													break;
											}
										}
										else
										{
											if (AttributeBits == 0)
											{
												CurrentPixelFormat = PixelFormat.Format16bppRgb555;
											}
											if (AttributeBits == 1)
											{
												CurrentPixelFormat = PixelFormat.Format16bppArgb1555;
											}
										}
										break;
									case 24:
										CurrentPixelFormat = PixelFormat.Format24bppRgb;
										break;
									case 32:
										if (NewTGA == true && ExtensionAreaOffset > 0)
										{
											switch (AttributesType)
											{

												case 0:
												case 1:
												case 2:
													CurrentPixelFormat = PixelFormat.Format32bppRgb;
													break;

												case 3:
													CurrentPixelFormat = PixelFormat.Format32bppArgb;
													break;

												case 4:
													CurrentPixelFormat = PixelFormat.Format32bppPArgb;
													break;

											}
										}
										else
										{
											if (AttributeBits == 0)
											{
												CurrentPixelFormat = PixelFormat.Format32bppRgb;
											}
											if (AttributeBits == 8)
											{
												CurrentPixelFormat = PixelFormat.Format32bppArgb;
											}
										}
										break;
								}
								//Create a pinned GC handle to our new byte array
								ByteArrayHandle = GCHandle.Alloc(ImageData, GCHandleType.Pinned);
								bitmap = new Bitmap((int)ImageWidth, (int)ImageHeight, Stride, CurrentPixelFormat, ByteArrayHandle.AddrOfPinnedObject());
								//Free it again
								ByteArrayHandle.Free();
								//Load color map
								if (ColorMap.Count > 0)
								{
									ColorPalette currentPalette = bitmap.Palette;
									for (int i = 0; i < ColorMap.Count; i++)
									{
										bool forceopaque = false;
										if (NewTGA == true && ExtensionAreaOffset > 0)
										{
											if (AttributesType == 0 || AttributesType == 1)
											{
												forceopaque = true;
											}
										}
										else if (AttributeBits == 0 || AttributeBits == 1)
										{
											forceopaque = true;
										}
										if (forceopaque)
										{
											// use 255 for alpha ( 255 = opaque/visible ) so we can see the image
											currentPalette.Entries[i] = Color.FromArgb(255, ColorMap[i].R, ColorMap[i].G, ColorMap[i].B);
										}
										else
										{
											// use whatever value is there
											currentPalette.Entries[i] = ColorMap[i];
										}
									}

									// set the new palette back to the Bitmap object
									bitmap.Palette = currentPalette;
								}
								else
								{
									if (PixelDepth == 8 && (ImageType == ImageTypes.UncompressedGreyscale || ImageType == ImageTypes.CompressedGreyscale))
									{
										// get the current palette
										ColorPalette currentPalette = bitmap.Palette;

										// create the Greyscale palette
										for (int i = 0; i < 256; i++)
										{
											currentPalette.Entries[i] = Color.FromArgb(i, i, i);
										}

										// set the new palette back to the Bitmap object
										bitmap.Palette = currentPalette;
									}
								}
							}


						}
					}
				}
			}


			/*
			 * Read the bitmap. This will be a bitmap of just
			 * any format, not necessarily the one that allows
			 * us to extract the bitmap data easily.
			 * */
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			/* 
			 * If the bitmap format is not already 32-bit BGRA,
			 * then convert it to 32-bit BGRA.
			 * */
			if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				Bitmap compatibleBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
				Graphics graphics = Graphics.FromImage(compatibleBitmap);
				graphics.DrawImage(bitmap, rect, rect, GraphicsUnit.Pixel);
				graphics.Dispose();
				bitmap.Dispose();
				bitmap = compatibleBitmap;
			}
			/*
			 * Extract the raw bitmap data.
			 * */
			BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			if (data.Stride == 4 * data.Width)
			{
				/*
				 * Copy the data from the bitmap
				 * to the array in BGRA format.
				 * */
				byte[] raw = new byte[data.Stride * data.Height];
				Marshal.Copy(data.Scan0, raw, 0, data.Stride * data.Height);
				bitmap.UnlockBits(data);
				int width = bitmap.Width;
				int height = bitmap.Height;
				bitmap.Dispose();
				/*
				 * Change the byte order from BGRA to RGBA.
				 * */
				for (int i = 0; i < raw.Length; i += 4)
				{
					byte temp = raw[i];
					raw[i] = raw[i + 2];
					raw[i + 2] = temp;
				}
				texture = new Texture(width, height, 32, raw, null);
				return true;
			}
			else
			{
				/*
				 * The stride is invalid. This indicates that the
				 * CLI either does not implement the conversion to
				 * 32-bit BGRA correctly, or that the CLI has
				 * applied additional padding that we do not
				 * support.
				 * */
				bitmap.UnlockBits(data);
				bitmap.Dispose();
				CurrentHost.ReportProblem(ProblemType.InvalidOperation, "Invalid stride encountered.");
				texture = null;
				return false;
			}
		}
		internal static Color GetColorFrom2Bytes(byte one, byte two)
		{
			// get the 5 bits used for the RED value from the first byte
			int r1 = GetBits(one, 2, 5);
			int r = r1 << 3;

			// get the two high order bits for GREEN from the from the first byte
			int bit = GetBits(one, 0, 2);
			// shift bits to the high order
			int g1 = bit << 6;

			// get the 3 low order bits for GREEN from the from the second byte
			bit = GetBits(two, 5, 3);
			// shift the low order bits
			int g2 = bit << 3;
			// add the shifted values together to get the full GREEN value
			int g = g1 + g2;

			// get the 5 bits used for the BLUE value from the second byte
			int b1 = GetBits(two, 0, 5);
			int b = b1 << 3;

			// get the 1 bit used for the ALPHA value from the first byte
			int a1 = GetBits(one, 7, 1);
			int a = a1 * 255;

			// return the resulting Color
			return Color.FromArgb(a, r, g, b);
		}
		internal static int GetBits(byte b, int offset, int count)
		{
			return (b >> offset) & ((1 << count) - 1);
		}
	}
}
