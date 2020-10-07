using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OpenBveApi.Interface;

namespace OpenBveApi.Math {

	/// <summary>Contains methods required for parsing differently formatted numbers</summary>
	public static class NumberFormats
	{
		/// <summary>The host application interface</summary>
		internal static Hosts.HostInterface currentHost;
		/// <summary>Parses a double formatted as a Visual Basic 6 string</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="Value">The value to return (Default 0.0)</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool TryParseDoubleVb6(string Expression, out double Value)
		{
			if (Expression.Length == 0)
			{
				Value = 0.0;
				return false;
			}
			if (Expression[0] == 65533 || Expression[0] == 8212 || Expression[0] == 8211)
			{
				/*
				 * Handle the use of EM-DASH instead of the minus sign
				 * 65533: ANSI EM-DASH read as Unicode
				 *  8212: ANSI EM-DASH read as ANSI
				 *  8211: Unicode EM-DASH
				 */
				Expression = '-' + Expression.Substring(1, Expression.Length - 1);
			}
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--)
			{
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a))
				{
					Value = a;
					return true;
				}
			}
			Value = 0.0;
			return false;
		}

		/// <summary>Parses a float formatted as a Visual Basic 6 string</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="Value">The value to return (Default 0.0)</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool TryParseFloatVb6(string Expression, out float Value)
		{
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--)
			{
				float a;
				if (float.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a))
				{
					Value = a;
					return true;
				}
			}
			Value = 0.0f;
			return false;
		}

		/// <summary>Parses an integer formatted as a Visual Basic 6 string</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="Value">The value to return (Default 0.0)</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool TryParseIntVb6(string Expression, out int Value)
		{
			Expression = TrimInside(Expression);
			CultureInfo Culture = CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--)
			{
				double a;
				if (double.TryParse(Expression.Substring(0, n), NumberStyles.Float, Culture, out a))
				{
					if (a >= -2147483648.0 & a <= 2147483647.0)
					{
						Value = (int)System.Math.Round(a);
						return true;
					}
					else break;
				}
			}
			Value = 0;
			return false;
		}

		/// <summary>Returns whether a string contains a valid double, using the supplied unit conversion factor(s)</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="UnitFactors">An array of unit conversion factors</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool IsValidDouble(string Expression, double[] UnitFactors)
		{
			double n;
			return TryParseDouble(Expression, UnitFactors, out n);
		}

		/// <summary>Parses a double from a string, using the supplied unit conversion factor(s)</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="UnitFactors">An array of unit conversion factors</param>
		/// <param name="Value">The value to return (Default 0.0)</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool TryParseDouble(string Expression, double[] UnitFactors, out double Value)
		{
			double a;
			if (double.TryParse(Expression, NumberStyles.Number, CultureInfo.InvariantCulture, out a))
			{
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			}
			else
			{
				string[] parameters = Expression.Split(new char[] {':'});
				if (parameters.Length <= UnitFactors.Length)
				{
					Value = 0.0;
					for (int i = 0; i < parameters.Length; i++)
					{
						if (double.TryParse(parameters[i].Trim(new char[] { }), NumberStyles.Float, CultureInfo.InvariantCulture, out a))
						{
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						}
						else
						{
							return false;
						}
					}
					return true;
				}
				else
				{
					Value = 0.0;
					return false;
				}
			}
		}

		/// <summary>Parses an integer formatted as a Visual Basic 6 string, using the supplied unit conversion factor(s)</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="UnitFactors">An array of unit conversion factors</param>
		/// <param name="Value">The value to return (Default 0.0)</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool TryParseDoubleVb6(string Expression, double[] UnitFactors, out double Value)
		{
			double a;
			if (double.TryParse(Expression, NumberStyles.Number, CultureInfo.InvariantCulture, out a))
			{
				Value = a * UnitFactors[UnitFactors.Length - 1];
				return true;
			}
			else
			{
				string[] parameters = Expression.Split(new char[] {':'});
				Value = 0.0;
				if (parameters.Length <= UnitFactors.Length)
				{
					for (int i = 0; i < parameters.Length; i++)
					{
						if (TryParseDoubleVb6(parameters[i].Trim(new char[] { }), out a))
						{
							int j = i + UnitFactors.Length - parameters.Length;
							Value += a * UnitFactors[j];
						}
						else
						{
							return false;
						}
					}
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>Parses a Vector2 formatted as a Visual Basic 6 string</summary>
		/// <param name="Value">The string value</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether two arguments are expected</param>
		/// <returns>True if parsing succeeds, false otherwise</returns>
		public static Vector2 TryParseVector2(string Value, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			Vector2 parsedVector = new Vector2();
			CultureInfo Culture = CultureInfo.InvariantCulture;
			int k = Value.IndexOf(',');
			if (k >= 0)
			{
				string a = Value.Substring(0, k).TrimEnd(new char[] { });
				string b = Value.Substring(k + 1).TrimStart(new char[] { });
				if (a.Length != 0 && !NumberFormats.TryParseDoubleVb6(a, out parsedVector.X)) {
					currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				}
				if (b.Length != 0 && !NumberFormats.TryParseDoubleVb6(b, out parsedVector.Y)) {
					currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
				}
			} else {
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
		/// <returns>True if parsing succeeds, false otherwise</returns>
		public static Vector3 TryParseVector3(string Value, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			string[] Arguments = Value.Split(',');
			return TryParseVector3(Arguments, Key, Section, Line, FileName, ExpectedArguments);
		}

		/// <summary>Parses a Vector3 formatted as an array of strings</summary>
		/// <param name="Arguments">The vector values</param>
		/// <param name="Key">The key value</param>
		/// <param name="Section">The section</param>
		/// <param name="Line">The line</param>
		/// <param name="FileName">The filename</param>
		/// <param name="ExpectedArguments">Whether a minimum of three arguments is expected</param>
		/// <returns>True if parsing succeeds, false otherwise</returns>
		public static Vector3 TryParseVector3(string[] Arguments, string Key, string Section, int Line, string FileName, bool ExpectedArguments = false)
		{
			Vector3 parsedVector = new Vector3();
			CultureInfo Culture = CultureInfo.InvariantCulture;
			
			if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out parsedVector.X)) {
				currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out parsedVector.Y)) {
				currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out parsedVector.Z)) {
				currentHost.AddMessage(MessageType.Error, false, "Z is invalid in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}

			if (Arguments.Length < 3 && ExpectedArguments)
			{
				currentHost.AddMessage(MessageType.Error, false, "Three arguments are expected in " + Key + " - " + Section + " at line " + (Line + 1).ToString(Culture) + " in " + FileName);
			}
			return parsedVector;
		}

		/// <summary>Converts a value given in degrees to Radians</summary>
		public static double ToRadians(this double degrees)
		{
			return degrees * 0.0174532925199433;
		}

		/// <summary>Gets the modulous of two numbers</summary>
		/// <returns>The modulous</returns>
		public static double Mod(double a, double b)
		{
			return a - b * System.Math.Floor(a / b);
		}

		private static string TrimInside(string Expression)
		{
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			foreach (char c in Expression.Where(c => !char.IsWhiteSpace(c)))
			{
				Builder.Append(c);
			} return Builder.ToString();
		}
	}
	
}
