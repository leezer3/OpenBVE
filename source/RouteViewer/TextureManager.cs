using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Interface;
using OpenBveApi.Textures;
using OpenTK.Graphics.OpenGL;
using GDIPixelFormat = System.Drawing.Imaging.PixelFormat;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace OpenBve
{
	internal static class TextureManager
	{
		// textures
		internal enum TextureLoadMode { Normal, Bve4SignalGlow }

		internal class Texture
		{
			internal bool Queried;
			internal bool Loaded;
			internal string FileName;
			internal TextureLoadMode LoadMode;
			internal OpenGlTextureWrapMode WrapModeX;
			internal OpenGlTextureWrapMode WrapModeY;
			internal Color24 TransparentColor;
			internal byte TransparentColorUsed;
			internal TextureTransparencyType Transparency;
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
		internal static int UseTexture(int TextureIndex, UseMode Mode)
		{
			if (TextureIndex > Textures.Length || Textures[TextureIndex] == null || TextureIndex == -1)
			{
				return 0;
			}
			Textures[TextureIndex].CyclesSurvived = 0;
			if (Textures[TextureIndex].Loaded)
			{
				return Textures[TextureIndex].OpenGlTextureIndex;
			}
			else if (Textures[TextureIndex].Data != null)
			{
				if (Textures[TextureIndex].IsRGBA)
				{
					LoadTextureRGBAforOpenGl(TextureIndex);
				}
				else
				{
					LoadTextureRGBforOpenGl(TextureIndex);
				}
				return Textures[TextureIndex].OpenGlTextureIndex;
			}
			else
			{
				if (Renderer.LoadTexturesImmediately == Renderer.LoadTextureImmediatelyMode.Yes | Mode == UseMode.LoadImmediately || (Renderer.LoadTexturesImmediately != Renderer.LoadTextureImmediatelyMode.NotYet & Textures[TextureIndex].LoadImmediately & Mode != UseMode.QueryDimensions))
				{
					LoadTextureData(TextureIndex);
					if (Textures[TextureIndex].Loaded)
					{
						return Textures[TextureIndex].OpenGlTextureIndex;
					}
					else if (Textures[TextureIndex].Data != null)
					{
						if (Textures[TextureIndex].IsRGBA)
						{
							LoadTextureRGBAforOpenGl(TextureIndex);
						}
						else
						{
							LoadTextureRGBforOpenGl(TextureIndex);
						}
						return Textures[TextureIndex].OpenGlTextureIndex;
					}
					else
					{
						return 0;
					}
				}
				else
				{
					if (Mode == UseMode.QueryDimensions & Textures[TextureIndex].FileName != null)
					{
						try
						{
							Bitmap b = (Bitmap)Image.FromFile(Textures[TextureIndex].FileName);
							Textures[TextureIndex].ClipWidth = b.Width;
							Textures[TextureIndex].ClipHeight = b.Height;
							b.Dispose();
						}
						catch (Exception ex)
						{
							Interface.AddMessage(MessageType.Error, false, "Internal error in TextureManager.cs::UseTexture: " + ex.Message);
							throw;
						}
					}
					Textures[TextureIndex].Queried = true;
					return 0;
				}
			}
		}

		// unuse texture
		internal static void UnuseTexture(int TextureIndex)
		{
			if (TextureIndex == -1) return;
			if (Textures[TextureIndex].Loaded)
			{
				if (Textures[TextureIndex].OpenGlTextureIndex != 0)
				{
					GL.DeleteTextures(1, new int[] { Textures[TextureIndex].OpenGlTextureIndex });
					Textures[TextureIndex].OpenGlTextureIndex = 0;
				}
				Textures[TextureIndex].Loaded = false;
			}
		}
		internal static void UnuseAllTextures()
		{
			for (int i = 0; i < Textures.Length; i++)
			{
				if (Textures[i] != null)
				{
					UnuseTexture(i);
				}
			}
		}

		internal static void ClearTextures()
		{
			UnuseAllTextures();
			//Textures = new Texture[1];
		}

		// unregister texture
		internal static void UnregisterTexture(ref int TextureIndex)
		{
			if (TextureIndex == -1) return;
			if (TextureIndex > Textures.Length ||Textures[TextureIndex] == null)
			{
				TextureIndex = -1;
				return;
			}
			if (Textures[TextureIndex].Loaded)
			{
				GL.DeleteTextures(1, new int[] { Textures[TextureIndex].OpenGlTextureIndex });
			}
			Textures[TextureIndex] = null;
			TextureIndex = -1;
		}

		// validate texture
		internal static void ValidateTexture(ref int TextureIndex)
		{
			int i = UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
			if (i == 0) TextureIndex = -1;
		}

		// load texture data
		private static void LoadTextureData(int TextureIndex)
		{
			if (Textures[TextureIndex].FileName != null && System.IO.File.Exists(Textures[TextureIndex].FileName))
			{
				try
				{
					using (Bitmap Bitmap = (Bitmap)Image.FromFile(Textures[TextureIndex].FileName))
					{
						if (Textures[TextureIndex].IsRGBA)
						{
							LoadTextureRGBAforData(Bitmap, Textures[TextureIndex].TransparentColor, Textures[TextureIndex].TransparentColorUsed, TextureIndex);
						}
						else
						{
							LoadTextureRGBforData(Bitmap, TextureIndex);
						}
					}
				}
				catch
				{
					using (Bitmap Bitmap = new Bitmap(1, 1, GDIPixelFormat.Format24bppRgb))
					{
						if (Textures[TextureIndex].IsRGBA)
						{
							LoadTextureRGBAforData(Bitmap, Textures[TextureIndex].TransparentColor, Textures[TextureIndex].TransparentColorUsed, TextureIndex);
						}
						else
						{
							LoadTextureRGBforData(Bitmap, TextureIndex);
						}
					}
				}
			}
			else
			{
				Textures[TextureIndex].Loaded = true;
				Textures[TextureIndex].Data = null;
			}
		}

		// update
		internal static void Update(double TimeElapsed)
		{
			CycleTime += TimeElapsed;
			if (CycleTime >= CycleInterval)
			{
				CycleTime = 0.0;
				for (int i = 0; i < Textures.Length; i++)
				{
					if (Textures[i] != null)
					{
						if (Textures[i].Loaded & !Textures[i].DontAllowUnload)
						{
							Textures[i].CyclesSurvived++;
							if (Textures[i].CyclesSurvived >= 2)
							{
								Textures[i].Queried = false;
							}
							if (Textures[i].CyclesSurvived >= MaxCyclesUntilUnload)
							{
								UnuseTexture(i);
							}
						}
						else
						{
							Textures[i].CyclesSurvived = 0;
						}
					}
				}
			}
		}

		// register texture
		internal static int RegisterTexture(string FileName, OpenGlTextureWrapMode WrapModeX, OpenGlTextureWrapMode WrapModeY, bool DontAllowUnload)
		{
			return RegisterTexture(FileName, Color24.Black, 0, TextureLoadMode.Normal, WrapModeX, WrapModeY, DontAllowUnload, 0, 0, 0, 0);
		}
		internal static int RegisterTexture(string FileName, Color24 TransparentColor, byte TransparentColorUsed, OpenGlTextureWrapMode WrapModeX, OpenGlTextureWrapMode WrapModeY, bool DontAllowUnload)
		{
			return RegisterTexture(FileName, TransparentColor, TransparentColorUsed, TextureLoadMode.Normal, WrapModeX, WrapModeY, DontAllowUnload, 0, 0, 0, 0);
		}
		internal static int RegisterTexture(string FileName, Color24 TransparentColor, byte TransparentColorUsed, TextureLoadMode LoadMode, OpenGlTextureWrapMode WrapModeX, OpenGlTextureWrapMode WrapModeY, bool DontAllowUnload, int ClipLeft, int ClipTop, int ClipWidth, int ClipHeight)
		{
			if (FileName == null)
			{
				//Need to find out why the object parser sometimes decides to pass a null filename, but this works around it
				return -1;
			}
			
			int i = FindTexture(FileName, TransparentColor, TransparentColorUsed, LoadMode, WrapModeX, WrapModeY, ClipLeft, ClipTop, ClipWidth, ClipHeight);
			if (i >= 0)
			{
				return i;
			}
			else
			{
				i = GetFreeTexture();
				Textures[i] = new Texture
				{
					Queried = false,
					Loaded = false,
					FileName = FileName,
					TransparentColor = TransparentColor,
					TransparentColorUsed = TransparentColorUsed,
					LoadMode = LoadMode,
					WrapModeX = WrapModeX,
					WrapModeY = WrapModeY,
					ClipLeft = ClipLeft,
					ClipTop = ClipTop,
					ClipWidth = ClipWidth,
					ClipHeight = ClipHeight,
					DontAllowUnload = DontAllowUnload,
					LoadImmediately = false,
					OpenGlTextureIndex = 0
				};
				bool alpha = false;
				switch (System.IO.Path.GetExtension(Textures[i].FileName).ToLowerInvariant())
				{
					case ".gif":
					case ".png":
						alpha = true;
						Textures[i].LoadImmediately = true;
						break;
				}
				if (alpha)
				{
					Textures[i].Transparency = TextureTransparencyType.Alpha;
				}
				else if (TransparentColorUsed != 0)
				{
					Textures[i].Transparency = TextureTransparencyType.Partial;
				}
				else
				{
					Textures[i].Transparency = TextureTransparencyType.Opaque;
				}
				Textures[i].IsRGBA = Textures[i].Transparency != TextureTransparencyType.Opaque | LoadMode != TextureLoadMode.Normal;
				return i;
			}
		}
		internal static int RegisterTexture(Bitmap Bitmap, bool Alpha)
		{
			int i = GetFreeTexture();
			int[] a = new int[1];
			GL.GenTextures(1, a);
			Textures[i] = new Texture
			{
				Queried = false,
				OpenGlTextureIndex = a[0],
				Transparency = TextureTransparencyType.Opaque,
				TransparentColor = Color24.Black,
				TransparentColorUsed = 0,
				FileName = null,
				Loaded = true,
				Width = Bitmap.Width,
				Height = Bitmap.Height,
				DontAllowUnload = true
			};
			if (Alpha)
			{
				Textures[i].Transparency = TextureTransparencyType.Alpha;
				LoadTextureRGBAforData(Bitmap, Color24.Black, 0, i);
				LoadTextureRGBAforOpenGl(i);
			}
			else
			{
				LoadTextureRGBforData(Bitmap, i);
				LoadTextureRGBforOpenGl(i);
			}
			return i;
		}

		internal static int RegisterTexture(Bitmap Bitmap, Color24 TransparentColor)
		{
			int i = GetFreeTexture();
			int[] a = new int[1];
			GL.GenTextures(1, a);
			Textures[i] = new Texture
			{
				Queried = false,
				OpenGlTextureIndex = a[0],
				Transparency = TextureTransparencyType.Partial,
				TransparentColor = TransparentColor,
				TransparentColorUsed = 1,
				FileName = null,
				Loaded = true,
				Width =  Bitmap.Width,
				Height = Bitmap.Height,
				DontAllowUnload = true
			};
			LoadTextureRGBAforData(Bitmap, Textures[i].TransparentColor, Textures[i].TransparentColorUsed, i);
			LoadTextureRGBAforOpenGl(i);
			return i;
		}


		// get image dimensions
		internal static void GetImageDimensions(string File, out int Width, out int Height)
		{
			try
			{
				Bitmap b = (Bitmap)Image.FromFile(File);
				Width = b.Width;
				Height = b.Height;
				b.Dispose();
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, "Internal error in TextureManager.cs::GetImageDimensions: " + ex.Message);
				throw;
			}
		}

		// find texture
		private static int FindTexture(string FileName, Color24 TransparentColor, byte TransparentColorUsed, TextureLoadMode LoadMode, OpenGlTextureWrapMode WrapModeX, OpenGlTextureWrapMode WrapModeY, int ClipLeft, int ClipTop, int ClipWidth, int ClipHeight)
		{
			for (int i = 1; i < Textures.Length; i++)
			{
				if (Textures[i] != null && Textures[i].FileName != null)
				{
					if (string.Compare(Textures[i].FileName, FileName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						if (Textures[i].LoadMode == LoadMode & Textures[i].WrapModeX == WrapModeX & Textures[i].WrapModeY == WrapModeY)
						{
							if (Textures[i].ClipLeft == ClipLeft & Textures[i].ClipTop == ClipTop & Textures[i].ClipWidth == ClipWidth & Textures[i].ClipHeight == ClipHeight)
							{
								if (TransparentColorUsed == 0)
								{
									if (Textures[i].TransparentColorUsed == 0)
									{
										return i;
									}
								}
								else
								{
									if (Textures[i].TransparentColorUsed != 0)
									{
										if (Textures[i].TransparentColor.R == TransparentColor.R & Textures[i].TransparentColor.G == TransparentColor.G & Textures[i].TransparentColor.B == TransparentColor.B)
										{
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
		private static int GetFreeTexture()
		{
			int i;
			for (i = 0; i < Textures.Length; i++)
			{
				if (Textures[i] == null) break;
			}
			if (i >= Textures.Length)
			{
				Array.Resize<Texture>(ref Textures, Textures.Length << 1);
			}
			return i;
		}

		// load texture rgb
		private static void LoadTextureRGBforData(Bitmap Bitmap, int TextureIndex)
		{
			try
			{
				// load bytes
				int Width, Height, Stride; byte[] Data;
				{
					// extract clip into power-of-two bitmap
					if (Textures[TextureIndex].ClipWidth == 0) Textures[TextureIndex].ClipWidth = Bitmap.Width;
					if (Textures[TextureIndex].ClipHeight == 0) Textures[TextureIndex].ClipHeight = Bitmap.Height;
					Width = Interface.RoundToPowerOfTwo(Textures[TextureIndex].ClipWidth);
					Height = Interface.RoundToPowerOfTwo(Textures[TextureIndex].ClipHeight);
					Bitmap c = new Bitmap(Width, Height, GDIPixelFormat.Format24bppRgb);
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
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, "Internal error in TextureManager.cs::LoadTextureRGBForData: " + ex.Message);
				throw;
			}
		}
		private static void LoadTextureRGBforOpenGl(int TextureIndex)
		{
			// apply to opengl
			int[] a = new int[1];
			GL.GenTextures(1, a);
			if (a[0] > 0)
			{
				Textures[TextureIndex].OpenGlTextureIndex = a[0];
				GL.BindTexture(TextureTarget.Texture2D, a[0]);
				switch (Interface.CurrentOptions.Interpolation)
				{
					case InterpolationMode.NearestNeighbor:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
						break;
					case InterpolationMode.Bilinear:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					case InterpolationMode.NearestNeighborMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
						break;
					case InterpolationMode.BilinearMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					case InterpolationMode.TrilinearMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					default:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
				}
				if (Interface.CurrentOptions.Interpolation == InterpolationMode.AnisotropicFiltering && Interface.CurrentOptions.AnisotropicFilteringLevel > 0)
				{
					GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, Interface.CurrentOptions.AnisotropicFilteringLevel);
				}
				if (Textures[TextureIndex].WrapModeX == OpenGlTextureWrapMode.RepeatRepeat)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge);
				}
				if (Textures[TextureIndex].WrapModeY == OpenGlTextureWrapMode.RepeatRepeat)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge);
				}
				byte[] Data = Textures[TextureIndex].Data;
				int Width = Textures[TextureIndex].Width;
				int Height = Textures[TextureIndex].Height;
				bool generateMipmap = Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor && Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, generateMipmap ? 1 : 0);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, Width, Height, 0, GLPixelFormat.Bgr, PixelType.UnsignedByte, Data);
			}
			Textures[TextureIndex].Loaded = true;
			Textures[TextureIndex].Data = null;
		}

		// load texture rgba
		private static void LoadTextureRGBAforData(Bitmap Bitmap, Color24 TransparentColor, byte TransparentColorUsed, int TextureIndex)
		{
			try
			{
				// load bytes
				int Width, Height, Stride; byte[] Data;
				{
					if (Textures[TextureIndex].ClipWidth == 0) Textures[TextureIndex].ClipWidth = Bitmap.Width;
					if (Textures[TextureIndex].ClipHeight == 0) Textures[TextureIndex].ClipHeight = Bitmap.Height;
					Width = Textures[TextureIndex].ClipWidth;
					Height = Textures[TextureIndex].ClipHeight;
					Bitmap c = new Bitmap(Width, Height, GDIPixelFormat.Format32bppArgb);
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
				if (Textures[TextureIndex].LoadMode == TextureLoadMode.Bve4SignalGlow)
				{
					// bve 4 signal glow
					int p = 0, pn = Stride - 4 * Width;
					byte tr, tg, tb;
					if (TransparentColorUsed != 0)
					{
						tr = TransparentColor.R;
						tg = TransparentColor.G;
						tb = TransparentColor.B;
					}
					else
					{
						tr = 0; tg = 0; tb = 0;
					}
					// invert lightness
					byte[] Temp = new byte[Stride * Height];
					for (int y = 0; y < Height; y++)
					{
						for (int x = 0; x < Width; x++)
						{
							if (Data[p] == tb & Data[p + 1] == tg & Data[p + 2] == tr)
							{
								Temp[p] = 0;
								Temp[p + 1] = 0;
								Temp[p + 2] = 0;
							}
							else if (Data[p] != 255 | Data[p + 1] != 255 | Data[p + 2] != 255)
							{
								int b = Data[p], g = Data[p + 1], r = Data[p + 2];
								InvertLightness(ref r, ref g, ref b);
								int l = r >= g & r >= b ? r : g >= b ? g : b;
								Temp[p] = (byte)(l * b / 255);
								Temp[p + 1] = (byte)(l * g / 255);
								Temp[p + 2] = (byte)(l * r / 255);
							}
							else
							{
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
					for (int y = 0; y < Height; y++)
					{
						for (int x = 0; x < Width; x++)
						{
							int q = p - s * (Stride + 4);
							int r = 0, g = 0, b = 0, c = 0;
							for (int yr = y - s; yr <= y + s; yr++)
							{
								if (yr >= 0 & yr < Height)
								{
									for (int xr = x - s; xr <= x + s; xr++)
									{
										if (xr >= 0 & xr < Width)
										{
											b += (int)Temp[q];
											g += (int)Temp[q + 1];
											r += (int)Temp[q + 2];
											c++;
										} q += 4;
									} q += n;
								}
								else q += Stride;
							} if (c == 0)
							{
								Data[p] = 0;
								Data[p + 1] = 0;
								Data[p + 2] = 0;
								Data[p + 3] = 255;
							}
							else
							{
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
					Textures[TextureIndex].Transparency = TextureTransparencyType.Opaque;
					Textures[TextureIndex].DontAllowUnload = true;
				}
				else if (TransparentColorUsed != 0)
				{
					// transparent color
					int p = 0, pn = Stride - 4 * Width;
					byte tr = TransparentColor.R;
					byte tg = TransparentColor.G;
					byte tb = TransparentColor.B;
					bool used = false;
					// check if alpha is actually used
					int y;
					for (y = 0; y < Height; y++)
					{
						int x; for (x = 0; x < Width; x++)
						{
							if (Data[p + 3] != 255)
							{
								break;
							} p += 4;
						} if (x < Width) break;
						p += pn;
					}
					if (y == Height)
					{
						Textures[TextureIndex].Transparency = TextureTransparencyType.Partial;
					}
					// duplicate color data from adjacent pixels
					p = 0; pn = Stride - 4 * Width;
					for (y = 0; y < Height; y++)
					{
						for (int x = 0; x < Width; x++)
						{
							if (Data[p] == tb & Data[p + 1] == tg & Data[p + 2] == tr)
							{
								used = true;
								if (x == 0)
								{
									int q = p;
									int v; for (v = y; v < Height; v++)
									{
										int u; for (u = v == y ? x + 1 : 0; u < Width; u++)
										{
											if (Data[q] != tb | Data[q + 1] != tg | Data[q + 2] != tr)
											{
												Data[p] = Data[q];
												Data[p + 1] = Data[q + 1];
												Data[p + 2] = Data[q + 2];
												Data[p + 3] = 0;
												break;
											} q += 4;
										} if (u < Width)
										{
											break;
										}
										else q += pn;
									} if (v == Height)
									{
										if (y == 0)
										{
											Data[p] = 128;
											Data[p + 1] = 128;
											Data[p + 2] = 128;
											Data[p + 3] = 0;
										}
										else
										{
											Data[p] = Data[p - Stride];
											Data[p + 1] = Data[p - Stride + 1];
											Data[p + 2] = Data[p - Stride + 2];
											Data[p + 3] = 0;
										}
									}
								}
								else
								{
									Data[p] = Data[p - 4];
									Data[p + 1] = Data[p - 3];
									Data[p + 2] = Data[p - 2];
									Data[p + 3] = 0;
								}
							} p += 4;
						} p += pn;
					}
					// transparent color is not actually used
					if (!used & Textures[TextureIndex].Transparency == TextureTransparencyType.Partial)
					{
						Textures[TextureIndex].Transparency = TextureTransparencyType.Opaque;
					}
				}
				else if (Textures[TextureIndex].Transparency == TextureTransparencyType.Alpha)
				{
					// check if alpha is actually used
					int p = 0, pn = Stride - 4 * Width;
					int y; for (y = 0; y < Height; y++)
					{
						int x; for (x = 0; x < Width; x++)
						{
							if (Data[p + 3] != 255)
							{
								break;
							} p += 4;
						} if (x < Width) break;
						p += pn;
					}
					if (y == Height)
					{
						Textures[TextureIndex].Transparency = TextureTransparencyType.Opaque;
					}
				}
				// non-power of two
				int TargetWidth = Interface.RoundToPowerOfTwo(Width);
				int TargetHeight = Interface.RoundToPowerOfTwo(Height);
				if (TargetWidth != Width | TargetHeight != Height)
				{
					Bitmap b = new Bitmap(Width, Height, GDIPixelFormat.Format32bppArgb);
					BitmapData d = b.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, b.PixelFormat);
					System.Runtime.InteropServices.Marshal.Copy(Data, 0, d.Scan0, d.Stride * d.Height);
					b.UnlockBits(d);
					Bitmap c = new Bitmap(TargetWidth, TargetHeight, GDIPixelFormat.Format32bppArgb);
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
			}
			catch (Exception ex)
			{
				Interface.AddMessage(MessageType.Error, false, "Internal error in TextureManager.cs::LoadTextureRGBAForData: " + ex.Message);
				throw;
			}
		}

		private static void LoadTextureRGBAforOpenGl(int TextureIndex)
		{
			int[] a = new int[1];
			GL.GenTextures(1, a);
			if (a[0] > 0)
			{
				Textures[TextureIndex].OpenGlTextureIndex = a[0];
				GL.BindTexture(TextureTarget.Texture2D, a[0]);
				switch (Interface.CurrentOptions.Interpolation)
				{
					case InterpolationMode.NearestNeighbor:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
						break;
					case InterpolationMode.Bilinear:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					case InterpolationMode.NearestNeighborMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
						break;
					case InterpolationMode.BilinearMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					case InterpolationMode.TrilinearMipmapped:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
					default:
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
						GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						break;
				}
				if (Interface.CurrentOptions.Interpolation == InterpolationMode.Bilinear || Interface.CurrentOptions.Interpolation == InterpolationMode.NearestNeighbor)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
				}
				if (Interface.CurrentOptions.Interpolation == InterpolationMode.AnisotropicFiltering)
				{
					GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, Interface.CurrentOptions.AnisotropicFilteringLevel);
				}
				if (Textures[TextureIndex].WrapModeX == OpenGlTextureWrapMode.RepeatRepeat)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge);
				}
				if (Textures[TextureIndex].WrapModeY == OpenGlTextureWrapMode.RepeatRepeat)
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
				}
				else
				{
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge);
				}
				byte[] Data = Textures[TextureIndex].Data;
				int Width = Textures[TextureIndex].Width;
				int Height = Textures[TextureIndex].Height;
				bool generateMipmap = Interface.CurrentOptions.Interpolation != InterpolationMode.NearestNeighbor && Interface.CurrentOptions.Interpolation != InterpolationMode.Bilinear;
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, generateMipmap ? 1 : 0);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, GLPixelFormat.Bgra, PixelType.UnsignedByte, Data);
			}
			Textures[TextureIndex].Loaded = true;
			Textures[TextureIndex].Data = null;
		}

		// invert lightness
		private static void InvertLightness(ref int R, ref int G, ref int B)
		{
			int nr, ng, nb;
			if (G <= R & R <= B | B <= R & R <= G)
			{
				nr = 255 + R - G - B;
				ng = 255 - B;
				nb = 255 - G;
			}
			else if (R <= G & G <= B | B <= G & G <= R)
			{
				nr = 255 - B;
				ng = 255 + G - R - B;
				nb = 255 - R;
			}
			else
			{
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
