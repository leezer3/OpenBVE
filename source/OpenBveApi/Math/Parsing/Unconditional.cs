using OpenBveApi.Colors;
using OpenBveApi.Interface;

namespace OpenBveApi.Math
{

	/// <summary>Contains unconditional methods required for parsing differently formatted numbers</summary>
	public static partial class NumberFormats
	{
		/// <summary>Parses a double formatted as a Visual Basic 6 string</summary>
		/// <param name="Value">The string value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		public static int ParseInt(string Value, string Key, string Section, int Line, string FileName)
		{
			int parsedNumber = 0;
			if (string.IsNullOrEmpty(Value))
			{
				currentHost.AddMessage(MessageType.Error, false, Key + " is empty in " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				return parsedNumber;
			}
			
			if (!TryParseIntVb6(Value, out parsedNumber))
			{
				currentHost.AddMessage(MessageType.Error, false, Key + " is invalid in " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			return parsedNumber;
		}

		/// <summary>Parses a double formatted as a Visual Basic 6 string</summary>
		/// <param name="Value">The string value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		public static double ParseDouble(string Value, string Key, string Section, int Line, string FileName)
		{
			double parsedNumber = 0.0;
			if (string.IsNullOrEmpty(Value))
			{
				currentHost.AddMessage(MessageType.Error, false, Key + " is empty in " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				return parsedNumber;
			}
			
			if (!TryParseDoubleVb6(Value, out parsedNumber))
			{
				currentHost.AddMessage(MessageType.Error, false, Key + " is invalid in " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			return parsedNumber;
		}

		/// <summary>Parses a Vector2 formatted as a Visual Basic 6 string</summary>
		/// <param name="Value">The string value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether two arguments are expected</param>
		public static Vector2 ParseVector2(string Value, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			Vector2 parsedVector = new Vector2();
			int k = Value.IndexOf(',');
			if (k >= 0)
			{
				string a = Value.Substring(0, k).TrimEnd(new char[] { });
				string b = Value.Substring(k + 1).TrimStart(new char[] { });
				if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out parsedVector.X))
				{
					currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				}

				if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out parsedVector.Y))
				{
					currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				}
			}
			else
			{
				if (ExpectedArguments)
				{
					currentHost.AddMessage(MessageType.Error, false, "Two arguments are expected in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				}

			}

			return parsedVector;
		}

		/// <summary>Parses a Vector3 formatted as a Visual Basic 6 string</summary>
		/// <param name="Value">The string value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether a minimum of three arguments is expected</param>
		public static Vector3 ParseVector3(string Value, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			string[] Arguments = Value.Split(',');
			return ParseVector3(Arguments, Key, Section, Line, FileName, ExpectedArguments);
		}

		/// <summary>Parses a Vector3 formatted as an array of strings</summary>
		/// <param name="Arguments">The vector values</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether a minimum of three arguments is expected</param>
		public static Vector3 ParseVector3(string[] Arguments, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			Vector3 parsedVector = new Vector3();

			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out parsedVector.X))
			{
				currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}

			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out parsedVector.Y))
			{
				currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}

			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out parsedVector.Z))
			{
				currentHost.AddMessage(MessageType.Error, false, "Z is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}

			if (Arguments.Length < 3 && ExpectedArguments)
			{
				currentHost.AddMessage(MessageType.Error, false, "Three arguments are expected in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}

			return parsedVector;
		}

		/// <summary>Parses a Color32 formatted as a Visual Basic 6 string</summary>
		/// <param name="Arguments">The color value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether a minimum of three arguments is expected</param>
		public static Color24 ParseColor24(string[] Arguments, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			int r = 0, g = 0, b = 0;
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (r < 0 | r > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				r = r < 0 ? 0 : 255;
			}

			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (g < 0 | g > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				g = g < 0 ? 0 : 255;
			}

			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (b < 0 | b > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				b = b < 0 ? 0 : 255;
			}

			if (Arguments.Length < 3 && ExpectedArguments)
			{
				currentHost.AddMessage(MessageType.Error, false, "Three arguments are expected in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			return new Color24(r, g, b);
		}

		/// <summary>Parses a Color32 formatted as a Visual Basic 6 string</summary>
		/// <param name="Arguments">The color value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether a minimum of three arguments is expected</param>
		public static Color32 ParseColor32(string[] Arguments, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			int r = 0, g = 0, b = 0, a = 255;
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (r < 0 | r > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				r = r < 0 ? 0 : 255;
			}

			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (g < 0 | g > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				g = g < 0 ? 0 : 255;
			}

			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (b < 0 | b > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				b = b < 0 ? 0 : 255;
			}

			if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out a))
			{
				currentHost.AddMessage(MessageType.Error, false, "Invalid argument Alpha in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
			}
			else if (a < 0 | a > 255)
			{
				currentHost.AddMessage(MessageType.Error, false, "Alpha is required to be within the range from 0 to 255 in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in file " + FileName);
				a = a < 0 ? 0 : 255;
			}
			if (Arguments.Length < 4 && ExpectedArguments)
			{
				currentHost.AddMessage(MessageType.Error, false, "Three arguments are expected in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			return new Color32(r, g, b, a);
		}
	}
}
