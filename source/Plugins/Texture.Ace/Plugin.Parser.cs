using System;
using System.IO;
using System.IO.Compression;
using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace Plugin {
	public partial class Plugin : TextureInterface {
		
		// --- get colors ---
		
		/// <summary>Gets a color from the specified integer.</summary>
		/// <param name="color">The color comprised of 5 red bits in the most significant bits, 6 green bits, and 5 blue bits in the least significant bits.</param>
		/// <returns></returns>
		private static Color32 GetColor(ushort color) {
			return new Color32(
				(byte)((color >> 11) << 3),
				(byte)(((color >> 5) & 0x3F) << 2),
				(byte)((color & 0x1F) << 3),
				255
			);
		}
		
		/// <summary>Gets the color that is half-way between the two specified colors.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>The mixed color.</returns>
		private static Color32 GetInterpolatedColor11(Color32 a, Color32 b) {
			return new Color32(
				(byte)((uint)a.R + (uint)b.R >> 1),
				(byte)((uint)a.G + (uint)b.G >> 1),
				(byte)((uint)a.B + (uint)b.B >> 1),
				(byte)((uint)a.A + (uint)b.A >> 1)
			);
		}
		
		/// <summary>Gets the color that is one third the way between the two specified colors.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>The mixed color.</returns>
		private static Color32 GetInterpolatedColor12(Color32 a, Color32 b) {
			return new Color32(
				(byte)(((uint)a.R + 2 * (uint)b.R) / 3),
				(byte)(((uint)a.G + 2 * (uint)b.G) / 3),
				(byte)(((uint)a.B + 2 * (uint)b.B) / 3),
				(byte)(((uint)a.A + 2 * (uint)b.A) / 3)
			);
		}
		
		/// <summary>Gets the color that is two thirds the way between the two specified colors.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>The mixed color.</returns>
		private static Color32 GetInterpolatedColor21(Color32 a, Color32 b) {
			return new Color32(
				(byte)((2 * (uint)a.R + (uint)b.R) / 3),
				(byte)((2 * (uint)a.G + (uint)b.G) / 3),
				(byte)((2 * (uint)a.B + (uint)b.B) / 3),
				(byte)((2 * (uint)a.A + (uint)b.A) / 3)
			);
		}
		
		
		// --- can load file ---
		
		/// <summary>Checks whether the specified file can be loaded as an ACE texture.</summary>
		/// <param name="file">The path to the file.</param>
		/// <returns>Whether the file can be load as an ACE texture.</returns>
		private static bool CanLoadFile(string file) {
			ulong identifier;
			using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					if (stream.Length < 8)
					{
						return false;
					}
					identifier = reader.ReadUInt64();
				}
			}
			if (identifier == 0x40404153494D4953) {
				byte[] bytes = File.ReadAllBytes(file);
				return CanLoadUncompressedData(bytes);
			}
		    if (identifier == 0x46404153494D4953) {
		        byte[] bytes = File.ReadAllBytes(file);
		        return CanLoadUncompressedData(DecompressAce(bytes));
		    }
		    return false;
		}
		
		/// <summary>Checks whether the specified uncompressed data can be loaded as an ACE texture.</summary>
		/// <param name="data">The uncompressed data.</param>
		/// <returns>Whether the uncompressed data can be load as an ACE texture.</returns>
		private static bool CanLoadUncompressedData(byte[] data) {
			using (MemoryStream stream = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					ulong identifier = reader.ReadUInt64();
					if (identifier != 0x40404153494D4953) {
						return false;
					}
					identifier = reader.ReadUInt64();
					if (identifier != 0x4040404040404040) {
						return false;
					}
					int unknown1 = reader.ReadInt32();
					if (unknown1 != 1) {
						return false;
					}
					return true;
				}
			}
		}
		
		
		// --- query dimensions ---

		/// <summary>Queries the texture dimensions of the specified file.</summary>
		/// <param name="file">The path to the file.</param>
		/// <param name="width">Receives the width.</param>
		/// <param name="height">Receives the height.</param>
		private static void QueryDimensionsFromFile(string file, out int width, out int height) {
			byte[] bytes = File.ReadAllBytes(file);
			ulong identifier;
			using (MemoryStream stream = new MemoryStream(bytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					identifier = reader.ReadUInt64();
				}
			}
			if (identifier == 0x40404153494D4953) {
				QueryDimensionsFromUncompressedData(bytes, out width, out height);
			} else if (identifier == 0x46404153494D4953) {
				QueryDimensionsFromUncompressedData(DecompressAce(bytes), out width, out height);
			} else {
				throw new InvalidDataException();
			}
		}

		/// <summary>Queries the texture dimensions of the specified uncompressed data.</summary>
		/// <param name="data">The byte data.</param>
		/// <param name="width">Receives the width.</param>
		/// <param name="height">Receives the height.</param>
		private static void QueryDimensionsFromUncompressedData(byte[] data, out int width, out int height) {
			using (MemoryStream stream = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					ulong identifier = reader.ReadUInt64();
					if (identifier != 0x40404153494D4953) {
						throw new InvalidDataException();
					}
					identifier = reader.ReadUInt64();
					if (identifier != 0x4040404040404040) {
						throw new InvalidDataException();
					}
					int unknown1 = reader.ReadInt32();
					if (unknown1 != 1) {
						throw new InvalidDataException();
					}
					//We don't know what this is, so reading it into a variable just generates a compiler warning....
					reader.ReadInt32();
					width = reader.ReadInt32();
					height = reader.ReadInt32();
				}
			}
		}
		
		
		// --- load from file ---
		
		/// <summary>Loads an ACE texture from the specified file.</summary>
		/// <param name="file">The path to the file.</param>
		/// <returns>The texture.</returns>
		private static Texture LoadFromFile(string file) {
			byte[] bytes = File.ReadAllBytes(file);
			ulong identifier;
			using (MemoryStream stream = new MemoryStream(bytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					identifier = reader.ReadUInt64();
				}
			}
			if (identifier == 0x40404153494D4953) {
				return LoadFromUncompressedData(bytes);
			} else if (identifier == 0x46404153494D4953) {
				return LoadFromUncompressedData(DecompressAce(bytes));
			} else {
				throw new InvalidDataException();
			}
		}
		
		/// <summary>Loads an ACE texture from uncompressed data.</summary>
		/// <param name="data">The uncompressed data.</param>
		/// <returns>The texture.</returns>
		private static Texture LoadFromUncompressedData(byte[] data) {
			using (MemoryStream stream = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// --- header ---
					ulong identifier = reader.ReadUInt64();
					if (identifier != 0x40404153494D4953) {
						throw new InvalidDataException();
					}
					identifier = reader.ReadUInt64();
					if (identifier != 0x4040404040404040) {
						throw new InvalidDataException();
					}
					int unknown1 = reader.ReadInt32();
					if (unknown1 != 1) {
						throw new InvalidDataException();
					}
					reader.ReadInt32();
					int width = reader.ReadInt32();
					int height = reader.ReadInt32();
					int type = reader.ReadInt32();
					/* 14 = 24 bits
					 * 16 = 24 bit 1 bit alpha
					 * 17 = 24 bit 8 bit alpha
					 * 18 = dtx1 */
					if (type != 14 & type != 16 & type != 17 & type != 18) {
						throw new InvalidDataException();
					}
					int channels = reader.ReadInt32();
					/* 3 = 24 bit
					 * 4 = 24 bits 1 bit alpha
					 * 5 = 24 bit 8 bit alpha */
					if (channels != 3 & channels != 4 & channels != 5) {
						throw new InvalidDataException();
					}
					//We don't know what this is, so reading it into a variable just generates a compiler warning....
					reader.ReadInt32();
					//16 bytes representing the author name
					reader.ReadBytes(16);
					//72 bytes of copyright information
					reader.ReadBytes(72);
					//We don't know what this is, so reading it into a variable just generates a compiler warning....
					reader.ReadInt32();
					//Skip the appropriate number of padding bytes
					switch (channels)
					{
						case 3:
							reader.ReadBytes(80);
							break;
						case 4:
							reader.ReadBytes(96);
							break;
						case 5:
							reader.ReadBytes(112);
							break;
					}
					// --- actual pixel data ---
					byte[] bytes = new byte[4 * width * height];
					if (type == 14 & channels == 3) {
						// --- rgb ---
						int[] streamOffsets = new int[height];
						for (int y = 0; y < height; y++) {
							streamOffsets[y] = 16 + reader.ReadInt32();
						}
						int offset = 0;
						int offsetIncrement = -4 * width + 1;
						for (int y = 0; y < height; y++) {
							stream.Position = streamOffsets[y];
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = 255;
								offset += 4;
							}
							offset -= 3;
						}
					} else if (type == 16 & channels == 4) {
						// --- rgb (1-bit transparency) ---
						int[] streamOffsets = new int[height];
						for (int y = 0; y < height; y++) {
							streamOffsets[y] = 16 + reader.ReadInt32();
						}
						int offset = 0;
						int offsetIncrement = -4 * width + 1;
						for (int y = 0; y < height; y++) {
							stream.Position = streamOffsets[y];
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							int value = 0;
							int counter = 0;
							for (int x = 0; x < width; x++) {
								var mask = (byte)(0x80 >> (x & 0x7));
								if (counter == 0) {
									value = reader.ReadByte();
									counter = 7;
								} else {
									counter--;
								}
								bytes[offset] = (value & mask) == 0 ? (byte)0 : (byte)255;
								offset += 4;
							}
							offset -= 3;
						}
					} else if (type == 17 & channels == 5) {
						// --- rgb (8-bit alpha) ---
						int[] streamOffsets = new int[height];
						for (int y = 0; y < height; y++) {
							streamOffsets[y] = 16 + reader.ReadInt32();
						}
						int offset = 0;
						int offsetIncrement = -4 * width + 1;
						for (int y = 0; y < height; y++) {
							stream.Position = streamOffsets[y];
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset += offsetIncrement;
							stream.Position += (width + 7) / 8;
							for (int x = 0; x < width; x++) {
								bytes[offset] = reader.ReadByte();
								offset += 4;
							}
							offset -= 3;
						}
					} else if (type == 18 & (channels == 3 | channels == 4)) {
						// --- dxt1 ---
						int mipmapOffset0 = reader.ReadInt32() + 20;
						stream.Position = mipmapOffset0;
						int offset = 0;
						int offsetIncrementY = 12 * width;
						int offsetIncrementX = -16 * width + 16;
						int offsetIncrementDy = 4 * width - 16;
						Color32[] colors = new Color32[4];
						Color32 black = channels == 4 ? Color32.Transparent : Color32.Black;
						for (int y = 0; y < height; y += 4) {
							for (int x = 0; x < width; x += 4) {
								ushort entry0 = reader.ReadUInt16();
								ushort entry1 = reader.ReadUInt16();
								colors[0] = GetColor(entry0);
								colors[1] = GetColor(entry1);
								if (entry0 > entry1) {
									colors[2] = GetInterpolatedColor21(colors[0], colors[1]);
									colors[3] = GetInterpolatedColor12(colors[0], colors[1]);
								} else {
									colors[2] = GetInterpolatedColor11(colors[0], colors[1]);
									colors[3] = black;
								}
								uint lookup = reader.ReadUInt32();
								for (int dy = 0; dy < 4; dy++) {
									for (int dx = 0; dx < 4; dx++) {
										uint index = lookup & 3;
										lookup >>= 2;
										bytes[offset + 0] = colors[index].R;
										bytes[offset + 1] = colors[index].G;
										bytes[offset + 2] = colors[index].B;
										bytes[offset + 3] = colors[index].A;
										offset += 4;
									}
									offset += offsetIncrementDy;
								}
								offset += offsetIncrementX;
							}
							offset += offsetIncrementY;
						}
					} else {
						// --- not supported ---
						throw new NotSupportedException();
					}
					// --- return texture ---
					return new Texture(width, height, 32, bytes, null);
				}
			}
		}
		
		
		// --- decompress ace ---
		
		/// <summary>Decompresses the specified zlib-compressed data.</summary>
		/// <param name="data">The compressed data including the ACE header.</param>
		/// <returns>The uncompressed data including the ACE header.</returns>
		private static byte[] DecompressAce(byte[] data) {
			// --- decompress data ---
			byte[] result;
			using (MemoryStream stream = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// --- ACE header ---
					ulong identifier = reader.ReadUInt64();
					if (identifier != 0x46404153494D4953) {
						throw new InvalidDataException();
					}
					int uncompressedLength = reader.ReadInt32();
					identifier = reader.ReadUInt32();
					if (identifier != 0x40404040) {
						throw new InvalidDataException();
					}
					// --- zlib header ---
					byte cmf = reader.ReadByte();
					int cm = cmf & 15;
					if (cm != 8) {
						throw new InvalidDataException();
					}
					byte flg = reader.ReadByte();
					// int fcheck = flg & 31;
					if ((256 * cmf + flg) % 31 != 0) {
						throw new InvalidDataException();
					}
					// --- deflate data ---
					result = new byte[uncompressedLength + 16];
					Array.Copy(new byte[] { 0x53, 0x49, 0x4D, 0x49, 0x53, 0x41, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40, 0x40 }, result, 16);
					using (DeflateStream deflate = new DeflateStream(stream, CompressionMode.Decompress, true)) {
						int length = deflate.Read(result, 16, uncompressedLength);
						if (length != uncompressedLength) {
							throw new InvalidDataException();
						}
					}
				}
			}
			return result;
		}
		
	}
}
