using System.Globalization;
using System.Linq;

namespace OpenBveApi.Math {

	/// <summary>Contains methods required for parsing differently formatted numbers</summary>
	public static partial class NumberFormats
	{
		/// <summary>The host application interface</summary>
		internal static Hosts.HostInterface currentHost;
		/// <summary>Culture used for parsing numbers</summary>
		internal static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

		/// <summary>Returns whether a string contains a valid double, using the supplied unit conversion factor(s)</summary>
		/// <param name="Expression">The expression to parse</param>
		/// <param name="UnitFactors">An array of unit conversion factors</param>
		/// <returns>True if parsing succeds, false otherwise</returns>
		public static bool IsValidDouble(string Expression, double[] UnitFactors)
		{
			double n;
			return TryParseDouble(Expression, UnitFactors, out n);
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
