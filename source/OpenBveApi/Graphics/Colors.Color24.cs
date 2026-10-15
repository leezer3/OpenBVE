using System;
using System.Globalization;
// ReSharper disable UnusedMember.Global
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Colors
{
	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	/// <summary>Represents a 24-bit color with red, green and blue channels at 8 bits each.</summary>
	public struct Color24 {
		// --- members ---
		/// <summary>The red component.</summary>
		public byte R;
		/// <summary>The green component.</summary>
		public byte G;
		/// <summary>The blue component.</summary>
		public byte B;

		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		public Color24(byte r, byte g, byte b) {
			this.R = r;
			this.G = g;
			this.B = b;
		}

		/// <summary>Creates a new color from a Color96</summary>
		///<param name="color">The Color96</param>
		public Color24(Color96 color)
		{
			this.R = (byte)(255 * color.R);
			this.G = (byte)(255 * color.G);
			this.B = (byte)(255 * color.B);
		}

		/// <summary>Interpolates between two Color24 values using a simple Cosine algorithm</summary>
		/// <param name="Color1">The first color</param>
		/// <param name="Color2">The second color</param>
		/// <param name="mu">The position on the curve of the new color</param>
		/// <returns>The interpolated color</returns>
		public static Color24 CosineInterpolate(Color24 Color1, Color24 Color2, double mu)
		{
			var mu2 = (1 - System.Math.Cos(mu * System.Math.PI)) / 2;
			return new Color24((byte)(Color1.R * (1 - mu2) + Color2.R * mu2), (byte)(Color1.G * (1 - mu2) + Color2.G * mu2), (byte)(Color1.B * (1 - mu2) + Color2.B * mu2));
		}
		
		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public static bool operator ==(Color24 a, Color24 b) {
			return a.R == b.R & a.G == b.G & a.B == b.B;
		}
		/// <summary>Checks whether two colors are unequal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are unequal.</returns>
		public static bool operator !=(Color24 a, Color24 b) {
			return a.R != b.R | a.G != b.G | a.B != b.B;
		}

		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public bool Equals (Color24 a, Color24 b)
		{
			return a.R == b.R & a.G == b.G & a.B == b.B;
		}

		/// <summary>Checks whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Color24))
			{
				return false;
			}
			return Equals(this, (Color24)obj);
		}

		/// <summary>Gets the hue-saturation-brightness (HSB) brightness value for this color.</summary>
		/// <returns>The brightness of this color.</returns>
		public float GetBrightness()
		{
			float num1 = R / (float)byte.MaxValue;
			float num2 = G / (float)byte.MaxValue;
			float num3 = B / (float)byte.MaxValue;
			float num4 = num1;
			float num5 = num1;
			if (num2 > num4)
				num4 = num2;
			if (num3 > num4)
				num4 = num3;
			if (num2 < num5)
				num5 = num2;
			if (num3 < num5)
				num5 = num3;
			return (num4 + num5) / 2.0f;
		}

		/// <summary>Returns the hashcode for this instance.</summary>
		/// <returns>An integer representing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.R.GetHashCode();
				hashCode = (hashCode * 397) ^ this.G.GetHashCode();
				hashCode = (hashCode * 397) ^ this.B.GetHashCode();
				return hashCode;
			}
		}

		// --- read-only fields ---
		/// <summary>Represents a black color.</summary>
		public static readonly Color24 Black = new Color24(0, 0, 0);
		/// <summary>Represents a dark grey color</summary>
		public static readonly Color24 DarkGrey = new Color24(85, 85, 85);
		/// <summary>Represents a grey color.</summary>
		public static readonly Color24 LightGrey = new Color24(178, 178, 178);
		/// <summary>Represents a grey color.</summary>
		public static readonly Color24 Grey = new Color24(128, 128, 128);
		/// <summary>Represents a red color.</summary>
		public static readonly Color24 Red = new Color24(255, 0, 0);
		/// <summary>Represents a green color.</summary>
		public static readonly Color24 Green = new Color24(0, 255, 0);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color24 Blue = new Color24(0, 0, 255);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color24 Cyan = new Color24(0, 255, 255);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color24 Magenta = new Color24(255, 0, 255);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color24 Yellow = new Color24(255, 255, 0);
		/// <summary>Represents a white color.</summary>
		public static readonly Color24 White = new Color24(255, 255, 255);

		/// <summary>Parses a hexadecimal string into a Color24</summary>
		/// <param name="Expression">The color in hexadecimal format</param>
		/// <param name="Color">The Color24, updated via 'out'</param>
		/// <remarks>Sets Color to blue if the parse fails</remarks>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		public static bool TryParseHexColor(string Expression, out Color24 Color)
		{
			Color = Blue;
			if (Expression.StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
			{
				string a = Expression.Substring(1).TrimStart();
				if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int x))
				{
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255)
					{
						Color = new Color24((byte)r, (byte)g, (byte)b);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>Parses a Color24 stored in a string</summary>
		/// <param name="stringToParse">The string to parse</param>
		/// <param name="separator">The separator character</param>
		/// <param name="Color">The out Color32</param>
		/// <returns>True if parsing succeeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Color32.
		/// If any part fails parsing, it will be set to 255</remarks>
		public static bool TryParseColor(string stringToParse, char separator, out Color24 Color)
		{
			Color = White;
			bool success = true;
			string[] splitString = stringToParse.Split(separator);
			int i;
			for (i = 0; i < splitString.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (!double.TryParse(splitString[i], out double r) || r < 0 || r > 255)
						{
							success = false;
						}
						else
						{
							Color.R = (byte)r;
						}
						break;
					case 1:
						if (!double.TryParse(splitString[i], out double g) || g < 0 || g > 255)
						{
							success = false;
						}
						else
						{
							Color.G = (byte)g;
						}
						break;
					case 2:
						if (!double.TryParse(splitString[i], out double b) || b < 0 || b > 255)
						{
							success = false;
						}
						else
						{
							Color.B = (byte)b;
						}
						break;
				}
			}

			if (i != 3)
			{
				success = false;
			}
			return success;
		}

		/// <summary>Parses a Hexadecimal string into a Color24</summary>
		/// <param name="Expression">The color in hexadecimal format</param>
		/// <returns>The new Color24</returns>
		public static Color24 ParseHexColor(string Expression)
		{
			if (!TryParseHexColor(Expression, out Color24 color))
			{
				throw new FormatException();
			}

			return color;
		}

		/// <summary>Casts a System.Drawing.Color to a Color24, discarding the alpha component</summary>
		/// <param name="c">The System.Drawing.Color</param>
		/// <returns>The new Color24</returns>
		public static implicit operator Color24(System.Drawing.Color c)
		{
			return new Color24(c.R, c.G, c.B);
		}

		/// <summary>Casts a Color24 to a System.Drawing.Color, discarding the alpha component</summary>
		/// <param name="c">The Color24</param>
		/// <returns>The new System.Drawing.Color</returns>
		public static implicit operator System.Drawing.Color(Color24 c)
		{
			return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
		}

		/// <summary>Returns a string representation of this Color24</summary>
		public override string ToString()
		{
			return $"#{BitConverter.ToString(new[] { R, G, B }).Replace("-", string.Empty)}";
		}

		/// <summary>Returns whether the color is a dark color</summary>
		public bool IsDark()
		{
			double val = (R + G + B) / 3.0;
			return val < 128;
		}
	}
}
