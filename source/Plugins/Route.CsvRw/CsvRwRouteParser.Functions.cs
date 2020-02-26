using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
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
						string b = a.Substring(Name.Length).TrimStart(new char[] { });
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
													texture.InvertLightness();
												}
												Program.CurrentHost.RegisterTexture(texture, new TextureParameters(null, null), out Textures[j]);
											}
										}
										else
										{
											Program.CurrentHost.RegisterTexture(Files[i], new TextureParameters(null, Color24.Black), out Textures[j]);
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

		/// <summary>Parses a string into OpenBVE's internal time representation (Seconds since midnight on the first day)</summary>
		/// <param name="Expression">The time in string format</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		internal static bool TryParseTime(string Expression, out double Value)
		{
			Expression = Expression.TrimInside();
			if (Expression.Length != 0) {
				CultureInfo Culture = CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i == -1)
				{
					i = Expression.IndexOf(':');
				}
				if (i >= 1) {
					int h; if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Program.CurrentHost.AddMessage(MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out m)) {
								uint s;
								string ss = Expression.Substring(i + 3, n - 2);
								if (Program.CurrentOptions.EnableBveTsHacks)
								{
									/*
									 * Handles values in the following format:
									 * HH.MM.SS
									 */
									if (ss.StartsWith("."))
									{
										ss = ss.Substring(1, ss.Length - 1);
									}
								}
								if (uint.TryParse(ss, NumberStyles.None, Culture, out s)) {
									Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					int h; if (int.TryParse(Expression, NumberStyles.Integer, Culture, out h)) {
						Value = 3600.0 * (double)h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}

		private static void Normalize(ref double x, ref double y)
		{
			double t = x * x + y * y;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}

		private static StaticObject LoadStaticObject(string fileName, System.Text.Encoding encoding, bool preserveVertices)
		{
			//FIXME: This needs to be removed
			//Hack to allow loading objects via the API into an array
			StaticObject staticObject;
			Program.CurrentHost.LoadStaticObject(fileName, encoding, preserveVertices, out staticObject);
			return staticObject;
		}

		internal static string GetChecksum(string file)
		{
			if (string.IsNullOrEmpty(file) || !File.Exists(file))
			{
				return string.Empty;
			}
			using (FileStream stream = File.OpenRead(file))
			{
				SHA256Managed sha = new SHA256Managed();
				byte[] checksum = sha.ComputeHash(stream);
				return BitConverter.ToString(checksum).Replace("-", string.Empty);
			}
		}
	}
}
