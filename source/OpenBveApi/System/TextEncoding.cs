using Ude;

namespace OpenBveApi
{
	public static class TextEncoding
	{
		/// <summary>Represents a text encoding</summary>
		public struct EncodingValue
		{
			/// <summary>The system codepage for this text encoding</summary>
			public int Codepage;

			/// <summary>The textual name for this text encoding</summary>
			public string Value;
		}

		/// <summary>The understood character encodings</summary>
		public enum Encoding
		{
			/// <summary>The character encoding is unknown</summary>
			Unknown = 0,

			/// <summary>UTF-7</summary>
			Utf7 = 1,

			/// <summary>UTF-8</summary>
			Utf8 = 2,

			/// <summary>UTF-16LE</summary>
			Utf16Le = 3,

			/// <summary>UTF-16BE</summary>
			Utf16Be = 4,

			/// <summary>UTF-32LE</summary>
			Utf32Le = 5,

			/// <summary>UTF-32BE</summary>
			Utf32Be = 6,

			/// <summary>SHIFT_JIS</summary>
			Shift_JIS = 7,

			/// <summary>Basic ASCII</summary>
			ASCII,

			/// <summary>Windows-1252 (Legacy Microsoft)</summary>
			Windows1252,

			/// <summary>Windows-1255 (Legacy Microsoft Hebrew)</summary>
			Windows1255,

			/// <summary>BIG5</summary>
			Big5,

			/// <summary>Legacy Korean</summary>
			EUC_KR,

			/// <summary>Legacy Cyrillic</summary>
			OEM866			
		}

		/// <summary>Gets the character endcoding of a file</summary>
		/// <param name="File">The absolute path to a file</param>
		/// <returns>The character encoding, or the system default encoding if unknown</returns>
		public static System.Text.Encoding GetSystemEncodingFromFile(string File)
		{
			Encoding e = GetEncodingFromFile(File);

			switch (e)
			{
				case Encoding.Utf7:
					return System.Text.Encoding.UTF7;
				case Encoding.Utf8:
					return System.Text.Encoding.UTF8;
				case Encoding.Utf16Le:
					return System.Text.Encoding.Unicode;
				case Encoding.Utf16Be:
					return System.Text.Encoding.BigEndianUnicode;
				case Encoding.Utf32Le:
					return System.Text.Encoding.UTF32;
				case Encoding.Utf32Be:
					return System.Text.Encoding.GetEncoding(12001);
				case Encoding.Shift_JIS:
					return System.Text.Encoding.GetEncoding(932);
				case Encoding.Big5:
					return System.Text.Encoding.GetEncoding(950);
				case Encoding.EUC_KR:
					return System.Text.Encoding.GetEncoding(949);
				default:
					return System.Text.Encoding.Default;
			}
		}

		/// <summary>Gets the character endcoding of a file</summary>
		/// <param name="File">The absolute path to a file</param>
		/// <returns>The character encoding, or unknown</returns>
		public static Encoding GetEncodingFromFile(string File)
		{
			if (File == null || !System.IO.File.Exists(File))
			{
				return Encoding.Unknown;
			}

			try
			{
				System.IO.FileInfo fInfo = new System.IO.FileInfo(File);
				byte[] Data = System.IO.File.ReadAllBytes(File);

				if (Data.Length >= 3)
				{
					if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF)
					{
						return Encoding.Utf8;
					}

					if (Data[0] == 0x2b & Data[1] == 0x2f & Data[2] == 0x76)
					{
						return Encoding.Utf7;
					}
				}

				if (Data.Length >= 2)
				{
					if (Data[0] == 0xFE & Data[1] == 0xFF)
					{
						return Encoding.Utf16Be;
					}

					if (Data[0] == 0xFF & Data[1] == 0xFE)
					{
						return Encoding.Utf16Le;
					}
				}

				if (Data.Length >= 4)
				{
					if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF)
					{
						return Encoding.Utf32Be;
					}

					if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00)
					{
						return Encoding.Utf32Le;
					}
				}

				CharsetDetector Det = new CharsetDetector();
				Det.Feed(Data, 0, Data.Length);
				Det.DataEnd();

				if (Det.Charset == null)
				{
					return Encoding.Unknown;
				}
				
				switch (Det.Charset.ToUpperInvariant())
				{
					case "SHIFT-JIS":
					case "SHIFT_JIS":
						return Encoding.Shift_JIS;
					case "UTF-8":
						return Encoding.Utf8;
					case "UTF-7":
						return Encoding.Utf7;
					case "WINDOWS-1251":
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "585tc1.csv" && fInfo.Length == 37302)
						{
							return Encoding.Shift_JIS;
						}
						return Encoding.Windows1252;
					case "WINDOWS-1252":
						if (fInfo.Length == 62861)
						{
							//HK tram route. Comes in a non-unicode zip, so filename may be subject to mangling
							return Encoding.Big5;
						}
						return Encoding.Windows1252;
					case "WINDOWS-1255":
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "xdbetulasmall.csv" && fInfo.Length == 406)
						{
							//Hungarian birch tree; Actually loads OK with 1255, but use the correct one
							return Encoding.Windows1252;
						}
						return Encoding.Big5;
					case "BIG5":
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "stoklosy.b3d" && fInfo.Length == 18256)
						{
							//Polish Warsaw metro object file uses diacritics in filenames
							return Encoding.Windows1252;
						}
						return Encoding.Big5;
					case "EUC-KR":
						return Encoding.EUC_KR;
					case "ASCII":
						return Encoding.ASCII;
					case "IBM866":
						return Encoding.OEM866;
					case "X-MAC-CYRILLIC":
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "exit01.csv" && fInfo.Length == 752)
						{
							//hira2
							return Encoding.Shift_JIS;
						}
						break;
					case "GB18030":
						//Extended new Chinese charset
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "people6.b3d" && fInfo.Length == 377)
						{
							//Polish Warsaw metro object file uses diacritics in filenames
							return Encoding.Windows1252;
						}
						break;
					case "EUC-JP":
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "xsara.b3d" && fInfo.Length == 3429)
						{
							//Uses an odd character in the comments, ASCII works just fine
							return Encoding.ASCII;
						}
						break;
				}

				Det.Reset();
				return Encoding.Unknown;
			}
			catch
			{
				return Encoding.Unknown;
			}
		}

		/// <summary>Gets the character endcoding of a file within a folder</summary>
		/// <param name="Folder">The absolute path to the folder containing the file</param>
		/// <param name="File">The filename</param>
		/// <returns>The character encoding, or unknown</returns>
		public static Encoding GetEncodingFromFile(string Folder, string File)
		{
			return GetEncodingFromFile(Path.CombineFile(Folder, File));
		}

		/// <summary>Gets the encoding corresponding to a textual name string</summary>
		/// <param name="EncodingName">The textual name of the encoding</param>
		/// <returns>The System.Text.Encoding</returns>
		public static System.Text.Encoding ParseEncoding(string EncodingName)
		{
			switch (EncodingName.ToLowerInvariant())
			{
				case "shift_jis":
					return System.Text.Encoding.GetEncoding(932);
				case "utf-8":
					return System.Text.Encoding.UTF8;
				case "utf-32":
					return System.Text.Encoding.UTF32;
				default:
					return System.Text.Encoding.Default;
			}
		}

		/// <summary>Checks whether the specified System.Text.Encoding is Unicode</summary>
		/// <param name="Encoding">The Encoding</param>
		public static bool IsUtf(System.Text.Encoding Encoding)
		{
			switch (Encoding.WindowsCodePage)
			{
				//UTF codepage numbers
				case 1200:
				case 1201:
				case 65000:
				case 65001:
					return true;
			}

			return false;
		}
	}
}
