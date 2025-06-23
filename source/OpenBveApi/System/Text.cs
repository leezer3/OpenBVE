using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OpenBveApi
{
	/// <summary>Provides various extension methods for working with text</summary>
	public static class Text
	{
		/// <summary>Unescapes control characters used in a string</summary>
		/// <param name="Text">The raw string on which unescaping should be performed</param>
		/// <returns>The unescaped string</returns>
		public static string Unescape(this string Text) {
			StringBuilder Builder = new StringBuilder(Text.Length);
			int Start = 0;
			for (int i = 0; i < Text.Length; i++) {
				if (Text[i] == '\\') {
					Builder.Append(Text, Start, i - Start);
					if (i + 1 < Text.Length) {
						switch (Text[i + 1]) {
								case 'a': Builder.Append('\a'); break;
								case 'b': Builder.Append('\b'); break;
								case 't': Builder.Append('\t'); break;
								case 'n': Builder.Append('\n'); break;
								case 'v': Builder.Append('\v'); break;
								case 'f': Builder.Append('\f'); break;
								case 'r': Builder.Append('\r'); break;
								case 'e': Builder.Append('\x1B'); break;
							case 'c':
								if (i + 2 < Text.Length) {
									int CodePoint = char.ConvertToUtf32(Text, i + 2);
									if (CodePoint >= 0x40 & CodePoint <= 0x5F) {
										Builder.Append(char.ConvertFromUtf32(CodePoint - 64));
									} else if (CodePoint == 0x3F) {
										Builder.Append('\x7F');
									} else {
										return Text;
									} i++;
								} else {
									return Text;
								} break;
							case '"':
								Builder.Append('"');
								break;
							case '\\':
								Builder.Append('\\');
								break;
							case 'x':
								if (i + 3 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 2), 16)));
									i += 2;
								} else {
									return Text;
								} break;
							case 'u':
								if (i + 5 < Text.Length) {
									Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 4), 16)));
									i += 4;
								} else {
									return Text;
								} break;
							default:
								return Text;
						}
						i++;
						Start = i + 1;
					} else {
						return Text;
					}
				}
			}
			Builder.Append(Text, Start, Text.Length - Start);
			return Builder.ToString();
		}

		/// <summary>Provides an escaped copy of a string</summary>
		public static string Escape(this string text)
		{
			return new XElement("t", text).LastNode.ToString();
		}

		/// <summary>Determines whether the specified string is encoded using Shift_JIS (Japanese)</summary>
		/// <param name="Name">The string to check</param>
		/// <returns>True if Shift_JIS encoded, false otherwise</returns>
		public static bool IsJapanese(this string Name) {
			for (int i = 0; i < Name.Length; i++) {
				int a = char.ConvertToUtf32(Name, i);
				if (a < 0x10000) {
					bool q = false;
					while (true) {
						if (a >= 0x2E80 & a <= 0x2EFF) break;
						if (a >= 0x3000 & a <= 0x30FF) break;
						if (a >= 0x31C0 & a <= 0x4DBF) break;
						if (a >= 0x4E00 & a <= 0x9FFF) break;
						if (a >= 0xF900 & a <= 0xFAFF) break;
						if (a >= 0xFE30 & a <= 0xFE4F) break;
						if (a >= 0xFF00 & a <= 0xFFEF) break;
						q = true; break;
					} if (q) return false;
				} else {
					return false;
				}
			} return true;
		}

		/// <summary>Converts various line-endings to CR-LF format</summary>
		/// <param name="Text">The string for which all line-endings should be converted to CR-LF</param>
		/// <returns>The converted StringBuilder</returns>
		public static string ConvertNewlinesToCrLf(this string Text) {
			StringBuilder Builder = new StringBuilder();
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				if (a == 0xD & i < Text.Length - 1) {
					int b = char.ConvertToUtf32(Text, i + 1);
					if (b == 0xA) {
						Builder.Append("\r\n");
						i++;
					} else {
						Builder.Append("\r\n");
					}
				} else if (a == 0xA | a == 0xC | a == 0xD | a == 0x85 | a == 0x2028 | a == 0x2029) {
					Builder.Append("\r\n");
				} else if (a < 0x10000) {
					Builder.Append(Text[i]);
				} else {
					Builder.Append(Text.Substring(i, 2));
					i++;
				}
			}
			Builder = Builder.Replace("\\n\\n", "\r\n\r\n");
			return Builder.ToString();
		}

		/// <summary>Trims any multiple whitespace characters within the string</summary>
		/// <param name="Expression">The string to trim</param>
		/// <returns>The trimmed string</returns>
		public static string TrimInside(this string Expression) {
			StringBuilder Builder = new StringBuilder(Expression.Length);
			foreach (char c in Expression.Where(c => !char.IsWhiteSpace(c)))
			{
				Builder.Append(c);
			} return Builder.ToString();
		}

		/// <summary>Trims BVE5 comments from a string.</summary>
		/// <param name="String">The string to trim.</param>
		public static string TrimBVE5Comments(this string String)
		{
			int j = String.IndexOf('#');
			if (j >= 0)
			{
				String = String.Substring(0, j);
			}
			int k = String.IndexOf("//", StringComparison.Ordinal);
			if (k >= 0)
			{
				String = String.Substring(0, k);
			}
			//remove any excess commas from the end, commonly found in motor noise tables edited with excel
			String = String.TrimEnd(',');
			return String;
		}

		/// <summary>Trims BVE5 comments from a string array</summary>
		/// <param name="Strings">The strings to trim</param>
		public static string[] TrimBVE5Comments(this string[] Strings)
		{
			for (int i = 0; i < Strings.Length; i++)
			{
				Strings[i].TrimBVE5Comments();
			}
			return Strings;
		}

		/// <summary>Attempts to determine the System.Text.Encoding value for a given BVE5 file</summary>
		/// <param name="FileName">The filename</param>
		/// <returns>The detected encoding, or UTF-8 if this is not found</returns>
		public static Encoding DetermineBVE5FileEncoding(string FileName)
		{
			try
			{
				using (StreamReader reader = new StreamReader(FileName))
				{
					var firstLine = reader.ReadLine();
					if (firstLine == null)
					{
						return Encoding.UTF8;
					}

					string[] Header = firstLine.Split(':');
					if (Header.Length == 1)
					{
						return Encoding.UTF8;
					}

					string[] Arguments = Header[1].Split(',');
					try
					{
						return Encoding.GetEncoding(Arguments[0].ToLowerInvariant().Trim());
					}
					catch
					{
						return Encoding.UTF8;
					}
				}
			}
			catch
			{
				return Encoding.UTF8;
			}
		}

		/// <summary>Removes single or double quotes enclosing a string.</summary>
		/// <param name="String">The string to trim.</param>
		public static string RemoveEnclosingQuotes(this string String)
		{
			if (String.StartsWith("'", StringComparison.InvariantCultureIgnoreCase) && String.EndsWith("'", StringComparison.InvariantCultureIgnoreCase))
			{
				String = String.Substring(1, String.Length - 2);
			}

			if (String.StartsWith("\"", StringComparison.InvariantCultureIgnoreCase) && String.EndsWith("\"", StringComparison.InvariantCultureIgnoreCase))
			{
				String = String.Substring(1, String.Length - 2);
			}
			return String;
		}

		/// <summary>Performs a string split to an array with a consistant length</summary>
		/// <param name="text">The string to split</param>
		/// <param name="separator">The separator character</param>
		/// <param name="desiredLength">The desired length</param>
		/// <returns>The split string</returns>
		public static string[] ConsistantSplit(this string text, char separator, int desiredLength)
		{
			string[] result = new string[desiredLength];
			string[] splitString = text.Split(separator);
			Array.Copy(splitString, result, System.Math.Min(desiredLength, splitString.Length));
			return result;
		}
	}
}
