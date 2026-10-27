using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace LibRender2.Text
{
	/// <summary>Finds a font file for a font family name.</summary>
	internal static class FontFileResolver
	{
		private const string BundledFontResource = "LibRender2.NotoSans-Regular.ttf";
		private static readonly string[] FontExtensions = { ".ttf", ".otf", ".ttc" };
		private static byte[] bundledFontData;

		internal static byte[] GetFontData(string fontName)
		{
			string path = Resolve(fontName);

			if (path != null)
			{
				try
				{
					return File.ReadAllBytes(path);
				}
				catch
				{
					// Fall back to the bundled font if the file cannot be read
				}
			}

			return BundledFontData;
		}

		internal static byte[] BundledFontData
		{
			get
			{
				if (bundledFontData == null)
				{
					using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(BundledFontResource))
					{
						if (stream == null)
						{
							throw new InvalidOperationException("Bundled font resource was not found.");
						}

						using (MemoryStream memoryStream = new MemoryStream())
						{
							stream.CopyTo(memoryStream);
							bundledFontData = memoryStream.ToArray();
						}
					}
				}

				return bundledFontData;
			}
		}

		private static string Resolve(string fontName)
		{
			if (string.IsNullOrEmpty(fontName))
			{
				return null;
			}

			if (File.Exists(fontName))
			{
				return fontName;
			}

			if (Normalize(fontName).Length == 0)
			{
				return null;
			}

			string path = ResolveWindowsRegistry(fontName);

			if (path != null)
			{
				return path;
			}

			path = ResolveFontConfig(fontName);

			if (path != null)
			{
				return path;
			}

			return ResolveKnownDirectory(fontName);
		}

		private static string ResolveWindowsRegistry(string fontName)
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				return null;
			}

			string target = Normalize(fontName);
			string bestPath = null;
			int bestScore = int.MaxValue;

			foreach (RegistryKey root in new[] { Registry.LocalMachine, Registry.CurrentUser })
			{
				try
				{
					using (RegistryKey key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts"))
					{
						if (key == null)
						{
							continue;
						}

						foreach (string valueName in key.GetValueNames())
						{
							int score = GetMatchScore(Normalize(StripTypeSuffix(valueName)), target);

							if (score < 0 || score >= bestScore)
							{
								continue;
							}

							string value = key.GetValue(valueName) as string;

							if (string.IsNullOrEmpty(value))
							{
								continue;
							}

							if (!Path.IsPathRooted(value))
							{
								value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), value);
							}

							if (File.Exists(value))
							{
								bestPath = value;
								bestScore = score;
							}
						}
					}
				}
				catch
				{
					// Registry access is not available
				}
			}

			return bestPath;
		}

		private static string ResolveFontConfig(string fontName)
		{
			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = "fc-match",
					Arguments = "-f \"%{file}\" \"" + fontName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};

				using (Process process = Process.Start(startInfo))
				{
					if (process == null)
					{
						return null;
					}

					string path = process.StandardOutput.ReadToEnd().Trim();
					process.WaitForExit(3000);
					return File.Exists(path) ? path : null;
				}
			}
			catch
			{
				return null;
			}
		}

		private static string ResolveKnownDirectory(string fontName)
		{
			string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			string[] directories =
			{
				Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts"),
				Path.Combine(home, ".fonts"),
				Path.Combine(home, ".local", "share", "fonts"),
				"/usr/share/fonts",
				"/usr/local/share/fonts",
				"/Library/Fonts",
				"/System/Library/Fonts"
			};

			string target = Normalize(fontName);
			string bestPath = null;
			int bestLength = int.MaxValue;

			foreach (string directory in directories)
			{
				if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
				{
					continue;
				}

				foreach (string extension in FontExtensions)
				{
					string candidate = Path.Combine(directory, fontName + extension);

					if (File.Exists(candidate))
					{
						return candidate;
					}
				}

				try
				{
					foreach (string candidate in Directory.EnumerateFiles(directory))
					{
						string extension = Path.GetExtension(candidate).ToLowerInvariant();

						if (Array.IndexOf(FontExtensions, extension) < 0)
						{
							continue;
						}

						string name = Normalize(Path.GetFileNameWithoutExtension(candidate));

						if (!name.StartsWith(target, StringComparison.Ordinal))
						{
							continue;
						}

						// Prefer the shortest name, which is usually the regular style
						if (name.Length < bestLength)
						{
							bestPath = candidate;
							bestLength = name.Length;
						}
					}
				}
				catch
				{
					// Directory cannot be enumerated
				}
			}

			return bestPath;
		}

		private static int GetMatchScore(string name, string target)
		{
			if (name == target)
			{
				return 0;
			}

			if (name == target + "regular" || name == target + "normal")
			{
				return 1;
			}

			return name.StartsWith(target, StringComparison.Ordinal) ? 2 : -1;
		}

		private static string StripTypeSuffix(string name)
		{
			int index = name.LastIndexOf(" (", StringComparison.Ordinal);

			if (index > 0 && name.EndsWith(")", StringComparison.Ordinal))
			{
				return name.Substring(0, index);
			}

			return name;
		}

		private static string Normalize(string value)
		{
			char[] buffer = new char[value.Length];
			int length = 0;

			foreach (char character in value)
			{
				if (char.IsLetterOrDigit(character))
				{
					buffer[length++] = char.ToLowerInvariant(character);
				}
			}

			return new string(buffer, 0, length);
		}
	}
}
