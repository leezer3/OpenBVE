using System;
using System.IO;
using System.Security.Cryptography;
using OpenBveApi.Colors;
using OpenBveApi.Textures;

namespace OpenBve
{
	/*
	 * This file contains functions used by the CSV & RW route parser
	 */
	internal partial class CsvRwRouteParser
	{
		/// <summary>This method checks whether a given route file is in the .rw format</summary>
		/// <param name="FileName">The file to check</param>
		/// <returns>True if this file is a RW, false otherwise</returns>
		internal static bool isRWFile(string FileName)
		{
			//TODO: Write a RW check function, which *doesn't* requite checking the extension
			//This is a kludge, and likely to fall over with misnamed files
			return string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Sets the brightness value for the specified track position</summary>
		/// <param name="Data">The route data (Accessed via 'ref') which we wish to query the brightnes value from</param>
		/// <param name="TrackPosition">The track position to get the brightness value for</param>
		/// <returns>The brightness value</returns>
		private static double GetBrightness(ref RouteData Data, double TrackPosition)
		{
			double tmin = double.PositiveInfinity;
			double tmax = double.NegativeInfinity;
			double bmin = 1.0, bmax = 1.0;
			for (int i = 0; i < Data.Blocks.Length; i++)
			{
				for (int j = 0; j < Data.Blocks[i].BrightnessChanges.Length; j++)
				{
					if (Data.Blocks[i].BrightnessChanges[j].TrackPosition <= TrackPosition)
					{
						tmin = Data.Blocks[i].BrightnessChanges[j].TrackPosition;
						bmin = (double)Data.Blocks[i].BrightnessChanges[j].Value;
					}
				}
			}
			for (int i = Data.Blocks.Length - 1; i >= 0; i--)
			{
				for (int j = Data.Blocks[i].BrightnessChanges.Length - 1; j >= 0; j--)
				{
					if (Data.Blocks[i].BrightnessChanges[j].TrackPosition >= TrackPosition)
					{
						tmax = Data.Blocks[i].BrightnessChanges[j].TrackPosition;
						bmax = (double)Data.Blocks[i].BrightnessChanges[j].Value;
					}
				}
			}
			if (tmin == double.PositiveInfinity & tmax == double.NegativeInfinity)
			{
				return 1.0;
			}
			else if (tmin == double.PositiveInfinity)
			{
				return (bmax - 1.0) * TrackPosition / tmax + 1.0;
			}
			else if (tmax == double.NegativeInfinity)
			{
				return bmin;
			}
			else if (tmin == tmax)
			{
				return 0.5 * (bmin + bmax);
			}
			else
			{
				double n = (TrackPosition - tmin) / (tmax - tmin);
				return (1.0 - n) * bmin + n * bmax;
			}
		}

		/// <summary>Loads all BVE4 signal or glow textures (Non animated file)</summary>
		/// <param name="BaseFile">The base file.</param>
		/// <param name="IsGlowTexture">Whether to load glow textures. If false, black is the transparent color. If true, the texture is edited according to the CSV route documentation.</param>
		/// <returns>All textures matching the base file.</returns>
		private static Texture[] LoadAllTextures(string BaseFile, bool IsGlowTexture)
		{
			string Folder = System.IO.Path.GetDirectoryName(BaseFile);
			if (Folder != null && !System.IO.Directory.Exists(Folder))
			{
				return new Texture[] { };
			}
			string Name = System.IO.Path.GetFileNameWithoutExtension(BaseFile);
			Texture[] Textures = new Texture[] { };
			if (Folder == null) return Textures;
			string[] Files = System.IO.Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++)
			{
				string a = System.IO.Path.GetFileNameWithoutExtension(Files[i]);
				if (a == null || Name == null) return Textures;
				if (a.StartsWith(Name, StringComparison.OrdinalIgnoreCase))
				{
					if (a.Length > Name.Length)
					{
						string b = a.Substring(Name.Length).TrimStart();
						int j; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out j))
						{
							if (j >= 0)
							{
								string c = System.IO.Path.GetExtension(Files[i]);
								if (c == null) return Textures;
								switch (c.ToLowerInvariant())
								{
									case ".bmp":
									case ".gif":
									case ".jpg":
									case ".jpeg":
									case ".png":
									case ".tif":
									case ".tiff":
										if (j >= Textures.Length)
										{
											int n = Textures.Length;
											Array.Resize<Texture>(ref Textures, j + 1);
											for (int k = n; k < j; k++)
											{
												Textures[k] = null;
											}
										}
										if (IsGlowTexture)
										{
											Texture texture;
											if (Program.CurrentHost.LoadTexture(Files[i], null, out texture))
											{
												if (texture.BitsPerPixel == 32)
												{
													byte[] bytes = texture.Bytes;
													InvertLightness(bytes);
													texture = new Texture(texture.Width, texture.Height, 32, bytes, texture.Palette);
												}
												Textures[j] = OpenBve.Textures.RegisterTexture(texture);
											}
										}
										else
										{
											OpenBve.Textures.RegisterTexture(Files[i], new TextureParameters(null, Color24.Black), out Textures[j]);
										}
										break;
								}
							}
						}
					}
				}
			}
			return Textures;
		}
		/// <summary>Inverts the lightness values of a texture used for a glow</summary>
		/// <param name="bytes">A byte array containing the texture to invert</param>
		private static void InvertLightness(byte[] bytes)
		{
			for (int i = 0; i < bytes.Length; i += 4)
			{
				if (bytes[i] != 0 | bytes[i + 1] != 0 | bytes[i + 2] != 0)
				{
					int r = bytes[i + 0];
					int g = bytes[i + 1];
					int b = bytes[i + 2];
					if (g <= r & r <= b | b <= r & r <= g)
					{
						bytes[i + 0] = (byte)(255 + r - g - b);
						bytes[i + 1] = (byte)(255 - b);
						bytes[i + 2] = (byte)(255 - g);
					}
					else if (r <= g & g <= b | b <= g & g <= r)
					{
						bytes[i + 0] = (byte)(255 - b);
						bytes[i + 1] = (byte)(255 + g - r - b);
						bytes[i + 2] = (byte)(255 - r);
					}
					else
					{
						bytes[i + 0] = (byte)(255 - g);
						bytes[i + 1] = (byte)(255 - r);
						bytes[i + 2] = (byte)(255 + b - r - g);
					}
				}
			}
		}

		private static string GetChecksum(string file)
		{
			using (FileStream stream = File.OpenRead(file))
			{
				SHA256Managed sha = new SHA256Managed();
				byte[] checksum = sha.ComputeHash(stream);
				return BitConverter.ToString(checksum).Replace("-", string.Empty);
			}
		}
	}
}
