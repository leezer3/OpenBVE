﻿using Mozilla.NUniversalCharDet;

namespace OpenBve
{
	class TextEncoding
	{
		/// <summary>Represents a text encoding</summary>
		internal struct EncodingValue
		{
			/// <summary>The system codepage for this text encoding</summary>
			internal int Codepage;
			/// <summary>The textual name for this text encoding</summary>
			internal string Value;
		}
		/// <summary>The understood character encodings</summary>
		internal enum Encoding
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
		}
		/// <summary>Gets the character endcoding of a file</summary>
		/// <param name="File">The absolute path to a file</param>
		/// <returns>The character encoding, or unknown</returns>
		internal static Encoding GetEncodingFromFile(string File)
		{
			try
			{
				byte[] Data = System.IO.File.ReadAllBytes(File);
				if (Data.Length >= 3)
				{
					if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF) return Encoding.Utf8;
					if (Data[0] == 0x2b & Data[1] == 0x2f & Data[2] == 0x76) return Encoding.Utf7;
				}
				if (Data.Length >= 2)
				{
					if (Data[0] == 0xFE & Data[1] == 0xFF) return Encoding.Utf16Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE) return Encoding.Utf16Le;
				}
				if (Data.Length >= 4)
				{
					if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF) return Encoding.Utf32Be;
					if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00) return Encoding.Utf32Le;
				}

				UniversalDetector Det = new UniversalDetector(null);
				Det.HandleData(Data, 0, Data.Length);
				Det.DataEnd();
				switch (Det.GetDetectedCharset())
				{
					case "SHIFT_JIS":
						return Encoding.Shift_JIS;
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
		internal static Encoding GetEncodingFromFile(string Folder, string File)
		{
			return GetEncodingFromFile(OpenBveApi.Path.CombineFile(Folder, File));
		}
	}
}
