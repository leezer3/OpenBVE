using System;
using Ude;

namespace OpenBveApi
{
	/// <summary>Contains helper functions for working with TextEncodings</summary>
	public static class TextEncoding
	{
		/// <summary>Represents a file and the associated encoding</summary>
		public struct EncodingValue
		{
			/// <summary>The system codepage for this text encoding</summary>
			public Encoding Codepage;

			/// <summary>The filename</summary>
			public string Value;
		}

		/// <summary>The understood character encodings</summary>
		public enum Encoding
		{
			/// <summary>The character encoding is unknown</summary>
			Unknown,

			/// <summary>IBM855 (OEM Cyrillic)</summary>
			IBM855 = 855,

			/// <summary>IBM866 (Legacy Cyrillic)</summary>
			IBM866 = 866,

			/// <summary>Shift_JIS</summary>
			SHIFT_JIS = 932,

			/// <summary>EUC-KR (EUC-KR is a subset of KS_C_5601-1987 and Legacy Korean)</summary>
			EUC_KR = 949,

			/// <summary>BIG5 (Traditional Chinese)</summary>
			BIG5 = 950,

			/// <summary>UTF-16LE</summary>
			UTF16_LE = 1200,

			/// <summary>UTF-16BE</summary>
			UTF16_BE = 1201,

			/// <summary>Windows-1251 (Legacy Microsoft Cyrillic)</summary>
			WIN1251 = 1251,

			/// <summary>Windows-1252 (Legacy Microsoft Western European)</summary>
			WIN1252 = 1252,

			/// <summary>Windows-1253 (Legacy Microsoft Greek)</summary>
			WIN1253 = 1253,

			/// <summary>Windows-1255 (Legacy Microsoft Hebrew)</summary>
			WIN1255 = 1255,

			/// <summary>x-mac-cyrillic (Legacy Mac Cyrillic)</summary>
			MAC_CYRILLIC = 10007,

			/// <summary>UTF-32LE</summary>
			UTF32_LE = 12000,

			/// <summary>UTF-32BE</summary>
			UTF32_BE = 12001,

			/// <summary>Basic ASCII</summary>
			ASCII = 20127,

			/// <summary>KOI8-R (Legacy Cyrillic)</summary>
			KOI8_R = 20866,

			/// <summary>EUC-JP</summary>
			EUC_JP = 20932,

			/// <summary>ISO-8859-2 (ISO 8859 Central European)</summary>
			ISO8859_2 = 28592,

			/// <summary>ISO-8859-5 (ISO 8859 Cyrillic)</summary>
			ISO8859_5 = 28595,

			/// <summary>ISO-8859-7 (ISO 8859 Greek)</summary>
			ISO8859_7 = 28597,

			/// <summary>ISO-8859-8 (ISO 8859 Visual Hebrew)</summary>
			ISO8859_8 = 28598,

			/// <summary>ISO-2022-JP (ISO 2022 Japanese)</summary>
			ISO2022_JP = 50220,

			/// <summary>ISO-2022-KR (ISO 2022 Korean)</summary>
			ISO2022_KR = 50225,

			/// <summary>ISO-2022-CN (ISO 2022 Chinese)</summary>
			ISO2022_CN = 50227,

			/// <summary>HZ_GB_2312 (Simplified Chinese)</summary>
			HZ_GB_2312 = 52936,

			/// <summary>GB18030 (Simplified Chinese)</summary>
			GB18030 = 54936,

			/// <summary>UTF-7</summary>
			UTF7 = 65000,

			/// <summary>UTF-8</summary>
			UTF8 = 65001
		}

		/// <summary>
		/// Gets the character system encoding of the bytes array
		/// </summary>
		/// <param name="Data">The bytes array</param>
		/// <param name="DefaultEncoding">The encoding to use if the encoding could not be determined. If not specified, the system default encoding is used.</param>
		/// <returns>The character system encoding, or default encoding if unknown</returns>
		public static System.Text.Encoding GetSystemEncodingFromBytes(byte[] Data, System.Text.Encoding DefaultEncoding = null)
		{
			Encoding encoding = GetEncodingFromBytes(Data);
			return ConvertToSystemEncoding(encoding, DefaultEncoding);
		}

		/// <summary>Gets the character system encoding of a file</summary>
		/// <param name="File">The absolute path to a file</param>
		/// <param name="DefaultEncoding">The encoding to use if the encoding could not be determined. If not specified, the system default encoding is used.</param>
		/// <returns>The character system encoding, or default encoding if unknown</returns>
		public static System.Text.Encoding GetSystemEncodingFromFile(string File, System.Text.Encoding DefaultEncoding = null)
		{
			Encoding encoding = GetEncodingFromFile(File);
			return ConvertToSystemEncoding(encoding, DefaultEncoding);
		}

		/// <summary>Gets the character system encoding of a file within a folder</summary>
		/// <param name="Folder">The absolute path to the folder containing the file</param>
		/// <param name="File">The filename</param>
		/// <param name="DefaultEncoding">The encoding to use if the encoding could not be determined. If not specified, the system default encoding is used.</param>
		/// <returns>The character system encoding, or default encoding if unknown</returns>
		public static System.Text.Encoding GetSystemEncodingFromFile(string Folder, string File, System.Text.Encoding DefaultEncoding = null)
		{
			return GetSystemEncodingFromFile(Path.CombineFile(Folder, File), DefaultEncoding);
		}

		/// <summary>
		/// Converts to the character system encoding
		/// </summary>
		/// <param name="encoding">The character encoding, or unknown</param>
		/// <param name="DefaultEncoding">The encoding to use if the encoding could not be determined. If not specified, the system default encoding is used.</param>
		/// <returns>The character system encoding, or default encoding if unknown</returns>
		public static System.Text.Encoding ConvertToSystemEncoding(Encoding encoding, System.Text.Encoding DefaultEncoding = null)
		{
			if (encoding == Encoding.Unknown)
			{
				return DefaultEncoding ?? System.Text.Encoding.Default;
			}

			System.Text.Encoding systemEncoding;

			try
			{
				systemEncoding = System.Text.Encoding.GetEncoding((int)encoding);
			}
			catch (SystemException e) when (e is ArgumentException || e is NotSupportedException)
			{
				// MAC_CYRILLIC under Mono (Missing codepage?)
				systemEncoding = DefaultEncoding ?? System.Text.Encoding.Default;
			}

			return systemEncoding;
		}

		/// <summary>
		/// Gets the character encoding of the bytes array
		/// </summary>
		/// <param name="Data">The bytes array</param>
		/// <returns>The character encoding, or unknown</returns>
		public static Encoding GetEncodingFromBytes(byte[] Data)
		{
			if (Data.Length >= 3)
			{
				if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF)
				{
					return Encoding.UTF8;
				}

				if (Data[0] == 0x2b & Data[1] == 0x2f & Data[2] == 0x76)
				{
					return Encoding.UTF7;
				}
			}

			if (Data.Length >= 2)
			{
				if (Data[0] == 0xFE & Data[1] == 0xFF)
				{
					return Encoding.UTF16_BE;
				}

				if (Data[0] == 0xFF & Data[1] == 0xFE)
				{
					return Encoding.UTF16_LE;
				}
			}

			if (Data.Length >= 4)
			{
				if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF)
				{
					return Encoding.UTF32_BE;
				}

				if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00)
				{
					return Encoding.UTF32_LE;
				}
			}

			CharsetDetector Det = new CharsetDetector();
			Det.Feed(Data, 0, Data.Length);
			Det.DataEnd();

			if (Det.Charset == null)
			{
				return Encoding.Unknown;
			}

			switch (Det.Charset)
			{
				case Charsets.IBM855:
					return Encoding.IBM855;
				case Charsets.IBM866:
					return Encoding.IBM866;
				case Charsets.SHIFT_JIS:
					return Encoding.SHIFT_JIS;
				case Charsets.EUCKR:
					return Encoding.EUC_KR;
				case Charsets.BIG5:
					return Encoding.BIG5;
				case Charsets.UTF16_LE:
					return Encoding.UTF16_LE;
				case Charsets.UTF16_BE:
					return Encoding.UTF16_BE;
				case Charsets.WIN1251:
					return Encoding.WIN1251;
				case Charsets.WIN1252:
					return Encoding.WIN1252;
				case Charsets.WIN1253:
					return Encoding.WIN1253;
				case Charsets.WIN1255:
					return Encoding.WIN1255;
				case Charsets.MAC_CYRILLIC:
					return Encoding.MAC_CYRILLIC;
				case Charsets.UTF32_LE:
					return Encoding.UTF32_LE;
				case Charsets.UTF32_BE:
					return Encoding.UTF32_BE;
				case Charsets.ASCII:
					return Encoding.ASCII;
				case Charsets.KOI8R:
					return Encoding.KOI8_R;
				case Charsets.EUCJP:
					return Encoding.EUC_JP;
				case Charsets.ISO8859_2:
					return Encoding.ISO8859_2;
				case Charsets.ISO8859_5:
					return Encoding.ISO8859_5;
				case Charsets.ISO_8859_7:
					return Encoding.ISO8859_7;
				case Charsets.ISO8859_8:
					return Encoding.ISO8859_8;
				case Charsets.ISO2022_JP:
					return Encoding.ISO2022_JP;
				case Charsets.ISO2022_KR:
					return Encoding.ISO2022_KR;
				case Charsets.ISO2022_CN:
					return Encoding.ISO2022_CN;
				case Charsets.HZ_GB_2312:
					return Encoding.HZ_GB_2312;
				case Charsets.GB18030:
					return Encoding.GB18030;
				case Charsets.UTF8:
					return Encoding.UTF8;
			}

			Det.Reset();
			return Encoding.Unknown;
		}

		/// <summary>Gets the character encoding of a file</summary>
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
				Encoding encoding = GetEncodingFromBytes(System.IO.File.ReadAllBytes(File));

				switch (encoding)
				{
					case Encoding.BIG5:
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "stoklosy.b3d" && fInfo.Length == 18256)
						{
							//Polish Warsaw metro object file uses diacritics in filenames
							return Encoding.WIN1252;
						}
						break;
					case Encoding.WIN1251:
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "585tc1.csv" && fInfo.Length == 37302)
						{
							return Encoding.SHIFT_JIS;
						}
						break;
					case Encoding.WIN1252:
						switch (fInfo.Length)
						{
							case 62861:
								//HK tram route. Comes in a non-unicode zip, so filename may be subject to mangling
								return Encoding.BIG5;
							case 2101:
							case 2107:
							case 5330:
								// railtypes from Lrt720
							case 506:
								// HK_LRT
								return Encoding.BIG5;
						}
						break;
					case Encoding.WIN1255:
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "xdbetulasmall.csv" && fInfo.Length == 406)
						{
							//Hungarian birch tree; Actually loads OK with 1255, but use the correct one
							return Encoding.WIN1252;
						}
						break;
					case Encoding.MAC_CYRILLIC:
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "exit01.csv" && fInfo.Length == 752)
						{
							//hira2
							return Encoding.SHIFT_JIS;
						}
						break;
					case Encoding.EUC_JP:
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "xsara.b3d" && fInfo.Length == 3429)
						{
							//Uses an odd character in the comments, ASCII works just fine
							return Encoding.ASCII;
						}
						break;
					case Encoding.GB18030:
						//Extended new Chinese charset
						if (System.IO.Path.GetFileName(File).ToLowerInvariant() == "people6.b3d" && fInfo.Length == 377)
						{
							//Polish Warsaw metro object file uses diacritics in filenames
							return Encoding.WIN1252;
						}
						break;
					case Encoding.IBM866:
						if (fInfo.Length == 348)
						{
							// HK_LRT again
							return Encoding.BIG5;
						}
						break;
				}

				return encoding;
			}
			catch
			{
				return Encoding.Unknown;
			}
		}

		/// <summary>Gets the character encoding of a file within a folder</summary>
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
