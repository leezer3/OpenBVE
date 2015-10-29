using System;
using System.Drawing;
using System.Drawing.Imaging;
using Tao.OpenGl;

namespace OpenBve {
	internal static class TextureManager {

		// options
		internal enum InterpolationMode {
			NearestNeighbor,
			Bilinear,
			NearestNeighborMipmapped,
			BilinearMipmapped,
			TrilinearMipmapped,
			AnisotropicFiltering
		}

		// textures
		internal enum TextureLoadMode { Normal, Bve4SignalGlow }
		internal enum TextureWrapMode { Repeat, ClampToEdge }
		internal enum TextureTransparencyMode { None, TransparentColor, Alpha }
		internal class Texture {
			internal bool Queried;
			internal bool Loaded;
			internal string FileName;
			internal TextureLoadMode LoadMode;
			internal TextureWrapMode WrapModeX;
			internal TextureWrapMode WrapModeY;
			internal World.ColorRGB TransparentColor;
			internal byte TransparentColorUsed;
			internal TextureTransparencyMode Transparency;
			internal int ClipLeft;
			internal int ClipTop;
			internal int ClipWidth;
			internal int ClipHeight;
			internal int Width;
			internal int Height;
			internal bool IsRGBA;
			internal byte[] Data;
			internal int OpenGlTextureIndex;
			internal int CyclesSurvived;
			internal bool DontAllowUnload;
			internal bool LoadImmediately;
		}
		internal static Texture[] Textures = new Texture[16];
		private const int MaxCyclesUntilUnload = 4;
		private const double CycleInterval = 10.0;
		private static double CycleTime = 0.0;
		
		// use texture
		internal enum UseMode { Normal, QueryDimensions, LoadImmediately }
		internal static int UseTexture(int TextureIndex, UseMode Mode) {
			if (TextureIndex == -1) return 0;
			Textures[TextureIndex].CyclesSurvived = 0;
			if (Textures[TextureIndex].Loaded) {
				return Textures[TextureIndex].OpenGlTextureIndex;
			} else if (Textures[TextureIndex].Data != null) {
				if (Textures[TextureIndex].IsRGBA) {
					LoadTextureRGBAforOpenGl(TextureIndex);
				} else {
					LoadTextureRGBforOpenGl(TextureIndex);
				}
				return Textures[TextureIndex].OpenGlTextureIndex;
			} else {
				if (Renderer.LoadTexturesImmediately == Renderer.LoadTextureImmediatelyMode.Yes | Mode == UseMode.LoadImmediately || (Renderer.LoadTexturesImmediately != Renderer.LoadTextureImmediatelyMode.NotYet & Textures[TextureIndex].LoadImmediately & Mode != UseMode.QueryDimensions)) {
					LoadTextureData(TextureIndex);
					if (Textures[TextureIndex].Loaded) {
						return Textures[TextureIndex].OpenGlTextureIndex;
					} else if (Textures[TextureIndex].Data != null) {
						if (Textures[TextureIndex].IsRGBA) {
							LoadTextureRGBAforOpenGl(TextureIndex);
						} else {
							LoadTextureRGBforOpenGl(TextureIndex);
						}
						return Textures[TextureIndex].OpenGlTextureIndex;
					} else {
						return 0;
					}
				} else {
					if (Mode == UseMode.QueryDimensions & Textures[TextureIndex].FileName != null) {
						try {
							Bitmap b = (Bitmap)Image.FromFile(Textures[TextureIndex].FileName);
							Textures[TextureIndex].ClipWidth = b.Width;
							Textures[TextureIndex].ClipHeight = b.Height;
							b.Dispose();
						} catch (Exception ex) {
							Interface.AddMessage(Interface.MessageType.Error, false, "Internal error in TextureManager.cs::UseTexture: " + ex.Message);
							throw;
						}
					}
					Textures[TextureIndex].Queried = true;
					return 0;
				}
			}
		}

		// unuse texture
		internal static void UnuseTexture(int TextureIndex) {
			if (TextureIndex == -1) return;
			if (Textures[TextureIndex].Loaded) {
				if (Textures[TextureIndex].OpenGlTextureIndex != 0) {
					Gl.glDeleteTextures(1, new int[] { Textures[TextureIndex].OpenGlTextureIndex });
					Textures[TextureIndex].OpenGlTextureIndex = 0;
				}
				Textures[TextureIndex].Loaded = false;
			}
		}
		internal static void UnuseAllTextures() {
			for (int i = 0; i < Textures.Length; i++) {
				if (Textures[i] != null) {
					UnuseTexture(i);
				}
			}
		}

		// unregister texture
		internal static void UnregisterTexture(ref int TextureIndex) {
			if (TextureIndex == -1) return;
			if (Textures[TextureIndex].Loaded) {
				Gl.glDeleteTextures(1, new int[] { Textures[TextureIndex].OpenGlTextureIndex });
			}
			Textures[TextureIndex] = null;
			TextureIndex = -1;
		}

		// validate texture
		internal static void ValidateTexture(ref int TextureIndex) {
			int i = UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
			if (i == 0) TextureIndex = -1;
		}

		// perform asynchronous operations
		internal static void PerformAsynchronousOperations() {
			for (int i = 0; i < Textures.Length; i++) {
				if (Textures[i] != null && Textures[i].Queried & !Textures[i].Loaded & Textures[i].Data == null) {
					LoadTextureData(i);
					System.Threading.Thread.Sleep(0);
				}
			}
		}

		// load texture data
		private static void LoadTextureData(int TextureIndex) {
			if (Textures[TextureIndex].FileName != null && System.IO.File.Exists(Textures[TextureIndex].FileName)) {
				try {
					using (Bitmap Bitmap = (Bitmap)Image.FromFile(Textures[TextureIndex].FileName)) {
						if (Textures[TextureIndex].IsRGBA) {
							LoadTextureRGBAforData(Bitmap, Textures[TextureIndex].TransparentColor, Textures[TextureIndex].TransparentColorUsed, TextureIndex);
						} else {
							LoadTextureRGBforData(Bitmap, TextureIndex);
						}
					}
				} catch {
					using (Bitmap Bitmap = new Bitmap(1, 1, PixelFormat.Format24bppRgb)) {
						if (Textures[TextureIndex].IsRGBA) {
							LoadTextureRGBAforData(Bitmap, Textures[TextureIndex].TransparentColor, Textures[TextureIndex].TransparentColorUsed, TextureIndex);
						} else {
							LoadTextureRGBforData(Bitmap, TextureIndex);
						}
					}
				}
			} else {
				Textures[TextureIndex].Loaded = true;
				Textures[TextureIndex].Data = null;
			}
		}

		// update
		internal static void Update(double TimeElapsed) {
			CycleTime += TimeElapsed;
			if (CycleTime >= CycleInterval) {
				CycleTime = 0.0;
				for (int i = 0; i < Textures.Length; i++) {
					if (Textures[i] != null) {
						if (Textures[i].Loaded & !Textures[i].DontAllowUnload) {
							Textures[i].CyclesSurvived++;
							if (Textures[i].CyclesSurvived >= 2) {
								Textures[i].Queried = false;
							}
							if (Textures[i].CyclesSurvived >= MaxCyclesUntilUnload) {
								UnuseTexture(i);
							}
						} else {
							Textures[i].CyclesSurvived = 0;
						}
					}
				}
			}
		}

		// register texture
		internal static int RegisterTexture(string FileName, TextureWrapMode WrapModeX, TextureWrapMode WrapModeY, bool DontAllowUnload) {
			return RegisterTexture(FileName, new World.ColorRGB(0, 0, 0), 0, TextureLoadMode.Normal, WrapModeX, WrapModeY, DontAllowUnload, 0, 0, 0, 0);
		}
		internal static int RegisterTexture(string FileName, World.ColorRGB TransparentColor, byte TransparentColorUsed, TextureWrapMode WrapModeX, TextureWrapMode WrapModeY, bool DontAllowUnload) {
			return RegisterTexture(FileName, TransparentColor, TransparentColorUsed, TextureLoadMode.Normal, WrapModeX, WrapModeY, DontAllowUnload, 0, 0, 0, 0);
		}
		internal static int RegisterTexture(string FileName, World.ColorRGB TransparentColor, byte TransparentColorUsed, TextureLoadMode LoadMode, TextureWrapMode WrapModeX, TextureWrapMode WrapModeY, bool DontAllowUnload, int ClipLeft, int ClipTop, int ClipWidth, int ClipHeight) {
			int i = FindTexture(FileName, TransparentColor, TransparentColorUsed, LoadMode, WrapModeX, WrapModeY, ClipLeft, ClipTop, ClipWidth, ClipHeight);
			if (i >= 0) {
				return i;
			} else {
				i = GetFreeTexture();
				Textures[i] = new Texture();
				Textures[i].Queried = false;
				Textures[i].Loaded = false;
				Textures[i].FileName = FileName;
				Textures[i].TransparentColor = TransparentColor;
				Textures[i].TransparentColorUsed = TransparentColorUsed;
				Textures[i].LoadMode = LoadMode;
				Textures[i].WrapModeX = WrapModeX;
				Textures[i].WrapModeY = WrapModeY;
				Textures[i].ClipLeft = ClipLeft;
				Textures[i].ClipTop = ClipTop;
				Textures[i].ClipWidth = ClipWidth;
				Textures[i].ClipHeight = ClipHeight;
				Textures[i].DontAllowUnload = DontAllowUnload;
				Textures[i].LoadImmediately = false;
				Textures[i].OpenGlTextureIndex = 0;
				bool alpha = false;
				switch (System.IO.Path.GetExtension(Textures[i].FileName).ToLowerInvariant()) {
					case ".gif":
					case ".png":
						alpha = true;
						Textures[i].LoadImmediately = true;
						break;
				}
				if (alpha) {
					Textures[i].Transparency = TextureTransparencyMode.Alpha;
				} else if (TransparentColorUsed != 0) {
					Textures[i].Transparency = TextureTransparencyMode.TransparentColor;
				} else {
					Textures[i].Transparency = TextureTransparencyMode.None;
				}
				Textures[i].IsRGBA = Textures[i].Transparency != TextureTransparencyMode.None | LoadMode != TextureLoadMode.Normal;
				return i;
			}
		}
		internal static int RegisterTexture(Bitmap Bitmap, bool Alpha) {
			int i = GetFreeTexture();
			int[] a = new int[1];
			Gl.glGenTextures(1, a);
			Textures[i] = new Texture();
			Textures[i].Queried = false;
			Textures[i].OpenGlTextureIndex = a[0];
			Textures[i].Transparency = TextureTransparencyMode.None;
			Textures[i].TransparentColor = new World.ColorRGB(0, 0, 0);
			Textures[i].TransparentColorUsed = 0;
			Textures[i].FileName = null;
			Textures[i].Loaded = true;
			Textures[i].DontAllowUnload = true;
			if (Alpha) {
				LoadTextureRGBAforData(Bitmap, new World.ColorRGB(0, 0, 0), 0, i);
				LoadTextureRGBAforOpenGl(i);
			} else {
				LoadTextureRGBforData(Bitmap, i);
				LoadTextureRGBforOpenGl(i);
			}
			return i;
		}

		// get image dimensions
		internal static void GetImageDimensions(string File, out int Width, out int Height) {
			try {
				Bitmap b = (Bitmap)Image.FromFile(File);
				Width = b.Width;
				Height = b.Height;
				b.Dispose();
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, false, "Internal error in TextureManager.cs::GetImageDimensions: " + ex.Message);
				throw;
			}
		}

		// find texture
		private static int FindTexture(string FileName, World.ColorRGB TransparentColor, byte TransparentColorUsed, TextureLoadMode LoadMode, TextureWrapMode WrapModeX, TextureWrapMode WrapModeY, int ClipLeft, int ClipTop, int ClipWidth, int ClipHeight) {
			for (int i = 1; i < Textures.Length; i++) {
				if (Textures[i] != null && Textures[i].FileName != null) {
					if (string.Compare(Textures[i].FileName, FileName, StringComparison.OrdinalIgnoreCase) == 0) {
						if (Textures[i].LoadMode == LoadMode & Textures[i].WrapModeX == WrapModeX & Textures[i].WrapModeY == WrapModeY) {
							if (Textures[i].ClipLeft == ClipLeft & Textures[i].ClipTop == ClipTop & Textures[i].ClipWidth == ClipWidth & Textures[i].ClipHeight == ClipHeight) {
								if (TransparentColorUsed == 0) {
									if (Textures[i].TransparentColorUsed == 0) {
										return i;
									}
								} else {
									if (Textures[i].TransparentColorUsed != 0) {
										if (Textures[i].TransparentColor.R == TransparentColor.R & Textures[i].TransparentColor.G == TransparentColor.G & Textures[i].TransparentColor.B == TransparentColor.B) {
											return i;
										}
									}
								}
							}
						}
					}
				}
			}
			return -1;
		}

		// get free texture
		private static int GetFreeTexture() {
			int i;
			for (i = 0; i < Textures.Length; i++) {
				if (Textures[i] == null) break;
			}
			if (i >= Textures.Length) {
				Array.Resize<Texture>(ref Textures, Textures.Length << 1);
			}
			return i;
		}

		// load texture rgb
		private static void LoadTextureRGBforData(Bitmap Bitmap, int TextureIndex) {
			try {
				// load bytes
				int Width, Height, Stride; byte[] Data;
				{
					// extract clip into power-of-two bitmap
					if (Textures[TextureIndex].ClipWidth == 0) Textures[TextureIndex].ClipWidth = Bitmap.Width;
					if (Textures[TextureIndex].ClipHeight == 0) Textures[TextureIndex].ClipHeight = Bitmap.Height;
					Width = Interface.RoundToPowerOfTwo(Textures[TextureIndex].ClipWidth);
					Height = Interface.RoundToPowerOfTwo(Textures[TextureIndex].ClipHeight);
					Bitmap c = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
					Graphics g = Graphics.FromImage(c);
					Point[] p = new Point[] { new Point(0, 0), new Point(Width, 0), new Point(0, Height) };
					g.DrawImage(Bitmap, p, new Rectangle(Textures[TextureIndex].ClipLeft, Textures[TextureIndex].ClipTop, Textures[TextureIndex].ClipWidth, Textures[TextureIndex].ClipHeight), GraphicsUnit.Pixel);
					g.Dispose();
					BitmapData d = c.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, c.PixelFormat);
					Stride = d.Stride;
					Data = new byte[Stride * Height];
					System.Runtime.InteropServices.Marshal.Copy(d.Scan0, Data, 0, Stride * Height);
					c.UnlockBits(d);
					c.Dispose();
				}
				Textures[TextureIndex].Width = Width;
				Textures[TextureIndex].Height = Height;
				Textures[TextureIndex].Data = Data;
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, false, "Internal error in TextureManager.cs::LoadTextureRGBForData: " + ex.Message);
				throw;
			}
		}
		private static void LoadTextureRGBforOpenGl(int TextureIndex) {
			// apply to opengl
			int[] a = new int[1];
			Gl.glGenTextures(1, a);
			if (a[0] > 0) {
				Textures[TextureIndex].OpenGlTextureIndex = a[0];
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, a[0]);
				switch (Interface.CurrentOptions.Interpolation) {
					case InterpolationMode.NearestNeighbor:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
						break;
					case InterpolationMode.Bilinear:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					case InterpolationMode.NearestNeighborMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
						break;
					case InterpolationMode.BilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
						break;
					case InterpolationMode.TrilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						break;
					default:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						break;
				}
				if (Interface.CurrentOptions.AnisotropicFilteringLevel > 0) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, Interface.CurrentOptions.AnisotropicFilteringLevel);
				}
				if (Textures[TextureIndex].WrapModeX == TextureWrapMode.Repeat) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
				} else {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
				}
				if (Textures[TextureIndex].WrapModeY == TextureWrapMode.Repeat) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
				} else {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
				}
				byte[] Data = Textures[TextureIndex].Data;
				int Width = Textures[TextureIndex].Width;
				int Height = Textures[TextureIndex].Height;
				if (Interface.CurrentOptions.Interpolation == InterpolationMode.NearestNeighbor | Interface.CurrentOptions.Interpolation == InterpolationMode.Bilinear) {
					Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, Width, Height, 0, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, Data);
				} else {
					Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGB, Width, Height, Gl.GL_BGR, Gl.GL_UNSIGNED_BYTE, Data);
				}
			}
			Textures[TextureIndex].Loaded = true;
			Textures[TextureIndex].Data = null;
		}

		// load texture rgba
		private static void LoadTextureRGBAforData(Bitmap Bitmap, World.ColorRGB TransparentColor, byte TransparentColorUsed, int TextureIndex) {
			try {
				// load bytes
				int Width, Height, Stride; byte[] Data;
				{
					if (Textures[TextureIndex].ClipWidth == 0) Textures[TextureIndex].ClipWidth = Bitmap.Width;
					if (Textures[TextureIndex].ClipHeight == 0) Textures[TextureIndex].ClipHeight = Bitmap.Height;
					Width = Textures[TextureIndex].ClipWidth;
					Height = Textures[TextureIndex].ClipHeight;
					Bitmap c = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
					Graphics g = Graphics.FromImage(c);
					Rectangle dst = new Rectangle(0, 0, Width, Height);
					Rectangle src = new Rectangle(Textures[TextureIndex].ClipLeft, Textures[TextureIndex].ClipTop, Textures[TextureIndex].ClipWidth, Textures[TextureIndex].ClipHeight);
					g.DrawImage(Bitmap, dst, src, GraphicsUnit.Pixel);
					g.Dispose();
					BitmapData d = c.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, c.PixelFormat);
					Stride = d.Stride;
					Data = new byte[Stride * Height];
					System.Runtime.InteropServices.Marshal.Copy(d.Scan0, Data, 0, Stride * Height);
					c.UnlockBits(d);
					c.Dispose();
				}
				// load mode
				if (Textures[TextureIndex].LoadMode == TextureLoadMode.Bve4SignalGlow) {
					// bve 4 signal glow
					int p = 0, pn = Stride - 4 * Width;
					byte tr, tg, tb;
					if (TransparentColorUsed != 0) {
						tr = TransparentColor.R;
						tg = TransparentColor.G;
						tb = TransparentColor.B;
					} else {
						tr = 0; tg = 0; tb = 0;
					}
					// invert lightness
					byte[] Temp = new byte[Stride * Height];
					for (int y = 0; y < Height; y++) {
						for (int x = 0; x < Width; x++) {
							if (Data[p] == tb & Data[p + 1] == tg & Data[p + 2] == tr) {
								Temp[p] = 0;
								Temp[p + 1] = 0;
								Temp[p + 2] = 0;
							} else if (Data[p] != 255 | Data[p + 1] != 255 | Data[p + 2] != 255) {
								int b = Data[p], g = Data[p + 1], r = Data[p + 2];
								InvertLightness(ref r, ref g, ref b);
								int l = r >= g & r >= b ? r : g >= b ? g : b;
								Temp[p] = (byte)(l * b / 255);
								Temp[p + 1] = (byte)(l * g / 255);
								Temp[p + 2] = (byte)(l * r / 255);
							} else {
								Temp[p] = Data[p];
								Temp[p + 1] = Data[p + 1];
								Temp[p + 2] = Data[p + 2];
							}
							p += 4;
						} p += pn;
					} p = 0;
					// blur the image and multiply by lightness
					int s = 4;
					int n = Stride - (2 * s + 1 << 2);
					for (int y = 0; y < Height; y++) {
						for (int x = 0; x < Width; x++) {
							int q = p - s * (Stride + 4);
							int r = 0, g = 0, b = 0, c = 0;
							for (int yr = y - s; yr <= y + s; yr++) {
								if (yr >= 0 & yr < Height) {
									for (int xr = x - s; xr <= x + s; xr++) {
										if (xr >= 0 & xr < Width) {
											b += (int)Temp[q];
											g += (int)Temp[q + 1];
											r += (int)Temp[q + 2];
											c++;
										} q += 4;
									} q += n;
								} else q += Stride;
							} if (c == 0) {
								Data[p] = 0;
								Data[p + 1] = 0;
								Data[p + 2] = 0;
								Data[p + 3] = 255;
							} else {
								r /= c; g /= c; b /= c;
								int l = r >= g & r >= b ? r : g >= b ? g : b;
								Data[p] = (byte)(l * b / 255);
								Data[p + 1] = (byte)(l * g / 255);
								Data[p + 2] = (byte)(l * r / 255);
								Data[p + 3] = 255;
							}
							p += 4;
						} p += pn;
					}
					Textures[TextureIndex].Transparency = TextureTransparencyMode.None;
					Textures[TextureIndex].DontAllowUnload = true;
				} else if (TransparentColorUsed != 0) {
					// transparent color
					int p = 0, pn = Stride - 4 * Width;
					byte tr = TransparentColor.R;
					byte tg = TransparentColor.G;
					byte tb = TransparentColor.B;
					bool used = false;
					// check if alpha is actually used
					int y;
					for (y = 0; y < Height; y++) {
						int x; for (x = 0; x < Width; x++) {
							if (Data[p + 3] != 255) {
								break;
							} p += 4;
						} if (x < Width) break;
						p += pn;
					}
					if (y == Height) {
						Textures[TextureIndex].Transparency = TextureTransparencyMode.TransparentColor;
					}
					// duplicate color data from adjacent pixels
					p = 0; pn = Stride - 4 * Width;
					for (y = 0; y < Height; y++) {
						for (int x = 0; x < Width; x++) {
							if (Data[p] == tb & Data[p + 1] == tg & Data[p + 2] == tr) {
								used = true;
								if (x == 0) {
									int q = p;
									int v; for (v = y; v < Height; v++) {
										int u; for (u = v == y ? x + 1 : 0; u < Width; u++) {
											if (Data[q] != tb | Data[q + 1] != tg | Data[q + 2] != tr) {
												Data[p] = Data[q];
												Data[p + 1] = Data[q + 1];
												Data[p + 2] = Data[q + 2];
												Data[p + 3] = 0;
												break;
											} q += 4;
										} if (u < Width) {
											break;
										} else q += pn;
									} if (v == Height) {
										if (y == 0) {
											Data[p] = 128;
											Data[p + 1] = 128;
											Data[p + 2] = 128;
											Data[p + 3] = 0;
										} else {
											Data[p] = Data[p - Stride];
											Data[p + 1] = Data[p - Stride + 1];
											Data[p + 2] = Data[p - Stride + 2];
											Data[p + 3] = 0;
										}
									}
								} else {
									Data[p] = Data[p - 4];
									Data[p + 1] = Data[p - 3];
									Data[p + 2] = Data[p - 2];
									Data[p + 3] = 0;
								}
							} p += 4;
						} p += pn;
					}
					// transparent color is not actually used
					if (!used & Textures[TextureIndex].Transparency == TextureTransparencyMode.TransparentColor) {
						Textures[TextureIndex].Transparency = TextureTransparencyMode.None;
					}
				} else if (Textures[TextureIndex].Transparency == TextureTransparencyMode.Alpha) {
					// check if alpha is actually used
					int p = 0, pn = Stride - 4 * Width;
					int y; for (y = 0; y < Height; y++) {
						int x; for (x = 0; x < Width; x++) {
							if (Data[p + 3] != 255) {
								break;
							} p += 4;
						} if (x < Width) break;
						p += pn;
					}
					if (y == Height) {
						Textures[TextureIndex].Transparency = TextureTransparencyMode.None;
					}
				}
				// non-power of two
				int TargetWidth = Interface.RoundToPowerOfTwo(Width);
				int TargetHeight = Interface.RoundToPowerOfTwo(Height);
				if (TargetWidth != Width | TargetHeight != Height) {
					Bitmap b = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
					BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, b.PixelFormat);
					System.Runtime.InteropServices.Marshal.Copy(Data, 0, d.Scan0, d.Stride * d.Height);
					b.UnlockBits(d);
					Bitmap c = new Bitmap(TargetWidth, TargetHeight, PixelFormat.Format32bppArgb);
					Graphics g = Graphics.FromImage(c);
					g.DrawImage(b, 0, 0, TargetWidth, TargetHeight);
					g.Dispose();
					b.Dispose();
					d = c.LockBits(new Rectangle(0, 0, TargetWidth, TargetHeight), ImageLockMode.ReadOnly, c.PixelFormat);
					Stride = d.Stride;
					Data = new byte[Stride * TargetHeight];
					System.Runtime.InteropServices.Marshal.Copy(d.Scan0, Data, 0, Stride * TargetHeight);
					c.UnlockBits(d);
					c.Dispose();
				}
				Textures[TextureIndex].Width = TargetWidth;
				Textures[TextureIndex].Height = TargetHeight;
				Textures[TextureIndex].Data = Data;
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Error, false, "Internal error in TextureManager.cs::LoadTextureRGBAForData: " + ex.Message);
				throw;
			}
		}
		
		private static void LoadTextureRGBAforOpenGl(int TextureIndex) {
			int[] a = new int[1];
			Gl.glGenTextures(1, a);
			if (a[0] > 0) {
				Textures[TextureIndex].OpenGlTextureIndex = a[0];
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, a[0]);
				switch (Interface.CurrentOptions.Interpolation) {
					case InterpolationMode.NearestNeighbor:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
						break;
					case InterpolationMode.Bilinear:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					case InterpolationMode.NearestNeighborMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_NEAREST);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
						break;
					case InterpolationMode.BilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					case InterpolationMode.TrilinearMipmapped:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
					default:
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
						Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
						break;
				}
				if (Interface.CurrentOptions.AnisotropicFilteringLevel > 0) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAX_ANISOTROPY_EXT, Interface.CurrentOptions.AnisotropicFilteringLevel);
				}
				if (Textures[TextureIndex].WrapModeX == TextureWrapMode.Repeat) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
				} else {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_CLAMP_TO_EDGE);
				}
				if (Textures[TextureIndex].WrapModeY == TextureWrapMode.Repeat) {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
				} else {
					Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_CLAMP_TO_EDGE);
				}
				byte[] Data = Textures[TextureIndex].Data;
				int Width = Textures[TextureIndex].Width;
				int Height = Textures[TextureIndex].Height;
				if (Interface.CurrentOptions.Interpolation == InterpolationMode.NearestNeighbor | Interface.CurrentOptions.Interpolation == InterpolationMode.Bilinear) {
					Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGBA, Width, Height, 0, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, Data);
				} else {
					Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, Gl.GL_RGBA, Width, Height, Gl.GL_BGRA, Gl.GL_UNSIGNED_BYTE, Data);
				}
			}
			Textures[TextureIndex].Loaded = true;
			Textures[TextureIndex].Data = null;
		}

		// invert lightness
		private static void InvertLightness(ref int R, ref int G, ref int B) {
			int nr, ng, nb;
			if (G <= R & R <= B | B <= R & R <= G) {
				nr = 255 + R - G - B;
				ng = 255 - B;
				nb = 255 - G;
			} else if (R <= G & G <= B | B <= G & G <= R) {
				nr = 255 - B;
				ng = 255 + G - R - B;
				nb = 255 - R;
			} else {
				nr = 255 - G;
				ng = 255 - R;
				nb = 255 + B - R - G;
			}
			R = nr;
			G = ng;
			B = nb;
		}

	}
}