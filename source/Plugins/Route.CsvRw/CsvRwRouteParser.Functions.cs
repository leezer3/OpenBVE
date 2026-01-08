using System;
using System.Globalization;
using System.IO;
using System.Text;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using Path = System.IO.Path;

namespace CsvRwRouteParser
{
	/*
	 * This file contains functions used by the CSV & RW route parser
	 */
	internal partial class Parser
	{
		/// <summary>Loads all BVE4 signal or glow textures (Non animated file)</summary>
		/// <param name="BaseFile">The base file.</param>
		/// <param name="IsGlowTexture">Whether to load glow textures. If false, black is the transparent color. If true, the texture is edited according to the CSV route documentation.</param>
		/// <returns>All textures matching the base file.</returns>
		private static Texture[] LoadAllTextures(string BaseFile, bool IsGlowTexture)
		{
			string Folder = Path.GetDirectoryName(BaseFile);
			if (Folder != null && !Directory.Exists(Folder))
			{
				return new Texture[] { };
			}
			string Name = Path.GetFileNameWithoutExtension(BaseFile);
			Texture[] Textures = { };
			if (Folder == null) return Textures;
			string[] Files = Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++)
			{
				string a = Path.GetFileNameWithoutExtension(Files[i]);
				if (a == null)
				{
					return Textures;
				}
				if (a.StartsWith(Name, StringComparison.OrdinalIgnoreCase))
				{
					if (a.Length > Name.Length)
					{
						string b = a.Substring(Name.Length).TrimStart();
						if (int.TryParse(b, NumberStyles.Integer, CultureInfo.InvariantCulture, out int j))
						{
							if (j >= 0)
							{
								string c = Path.GetExtension(Files[i]);
								
								switch (c.ToLowerInvariant())
								{
									case ".ace":
									case ".bmp":
									case ".dds":
									case ".gif":
									case ".jpg":
									case ".jpeg":
									case ".png":
									case ".tif":
									case ".tiff":
										if (j >= Textures.Length)
										{
											int n = Textures.Length;
											Array.Resize(ref Textures, j + 1);
											for (int k = n; k < j; k++)
											{
												Textures[k] = null;
											}
										}
										if (IsGlowTexture)
										{
											if (Plugin.CurrentHost.LoadTexture(Files[i], null, out Texture texture))
											{
												if (texture.PixelFormat == PixelFormat.RGBAlpha)
												{
													texture.InvertLightness();
												}
												Plugin.CurrentHost.RegisterTexture(texture, TextureParameters.NoChange, out Textures[j]);
											}
										}
										else
										{
											Plugin.CurrentHost.RegisterTexture(Files[i], new TextureParameters(null, Color24.Black), out Textures[j]);
										}
										break;
									case "":
										return Textures;
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
			CultureInfo Culture = CultureInfo.InvariantCulture;
			Expression = Expression.TrimInside();
			if (Expression.Length != 0) {
				int i = Expression.IndexOf('.');
				if (i == -1)
				{
					i = Expression.IndexOf(':');
				}
				if (i >= 1) {
					if (int.TryParse(Expression.Substring(0, i), NumberStyles.Integer, Culture, out int h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							if (uint.TryParse(Expression.Substring(i + 1, n), NumberStyles.None, Culture, out uint m)) {
								Value = 3600.0 * h + 60.0 * m;
								return true;
							}
						} else if (n >= 3) {
							if (n > 4)
							{
								Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "A maximum of 4 digits of precision are supported in TIME values");
								n = 4;
							}
							if (uint.TryParse(Expression.Substring(i + 1, 2), NumberStyles.None, Culture, out uint m)) {
								string ss = Expression.Substring(i + 3, n - 2);
								if (Plugin.CurrentOptions.EnableBveTsHacks)
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
								if (uint.TryParse(ss, NumberStyles.None, Culture, out uint s)) {
									Value = 3600.0 * h + 60.0 * m + s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					if (int.TryParse(Expression, NumberStyles.Integer, Culture, out int h)) {
						Value = 3600.0 * h;
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

		private static StaticObject LoadStaticObject(string fileName, Encoding encoding, bool preserveVertices)
		{
			//FIXME: This needs to be removed
			//Hack to allow loading objects via the API into an array
			Plugin.CurrentHost.LoadStaticObject(fileName, encoding, preserveVertices, out StaticObject staticObject);
			return staticObject;
		}

		private string[] SplitArguments(string ArgumentSequence)
		{
			string[] Arguments;
			{
				int n = 0;
				for (int k = 0; k < ArgumentSequence.Length; k++) {
					if ((IsRW && ArgumentSequence[k] == ',') || ArgumentSequence[k] == ';') 
					{
						n++;
					}
				}
				Arguments = new string[n + 1];
				int a = 0, h = 0;
				for (int k = 0; k < ArgumentSequence.Length; k++) {
					if ((IsRW && ArgumentSequence[k] == ',') || ArgumentSequence[k] == ';') 
					{
						Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
						a = k + 1; h++;
					}
				}
				if (ArgumentSequence.Length - a > 0) {
					Arguments[h] = ArgumentSequence.Substring(a).Trim();
					h++;
				}
				Array.Resize(ref Arguments, h);
			}
			return Arguments;
		}

		private int[] FindIndices(ref string Command, Expression Expression)
		{
			int[] commandIndices = { 0, 0};
			if (Command != null && Command.EndsWith(")")) {
				for (int k = Command.Length - 2; k >= 0; k--) {
					if (Command[k] == '(')
					{
						string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
						Command = Command.Substring(0, k).TrimEnd();
						int h = Indices.IndexOf(";", StringComparison.Ordinal);
						if (h >= 0)
						{
							string a = Indices.Substring(0, h).TrimEnd();
							string b = Indices.Substring(h + 1).TrimStart();
							if (a.Length > 0 && !NumberFormats.TryParseIntVb6(a, out commandIndices[0])) {
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid first index appeared at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File + ".");
								Command = null;
							} 
							if (b.Length > 0 && !NumberFormats.TryParseIntVb6(b, out commandIndices[1])) {
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid second index appeared at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File + ".");
								Command = null;
							}
						} else {
							if (Indices.Length > 0 && !NumberFormats.TryParseIntVb6(Indices, out commandIndices[0]))
							{
								if (Indices.ToLowerInvariant() != "c" || Command.ToLowerInvariant() != "route.comment")
								{
									// (C) used in route comment to represent copyright symbol, so not an error
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid index appeared at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + Expression.File + ".");
									Command = null;
								}
							}
						}
						break;
					}
				}
			}

			return commandIndices;
		}
	}
}
