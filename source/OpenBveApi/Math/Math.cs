using System.Globalization;
using System.Linq;

namespace OpenBveApi.Math {

	/// <summary>Contains methods required for parsing differently formatted numbers</summary>
	public class NumberFormats
	{
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
				string[] parameters = Expression.Split(':');
				if (parameters.Length <= UnitFactors.Length)
				{
					Value = 0.0;
					for (int i = 0; i < parameters.Length; i++)
					{
						if (double.TryParse(parameters[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out a))
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
				string[] parameters = Expression.Split(':');
				Value = 0.0;
				if (parameters.Length <= UnitFactors.Length)
				{
					for (int i = 0; i < parameters.Length; i++)
					{
						if (TryParseDoubleVb6(parameters[i].Trim(), out a))
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
