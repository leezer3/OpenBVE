using System;
using System.Globalization;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Colors
{
	/// <summary>Represents a 32-bit color with red, green, blue and alpha channels at 8 bits each.</summary>
	public struct Color32 {
		// --- members ---
		/// <summary>The red component.</summary>
		public byte R;
		/// <summary>The green component.</summary>
		public byte G;
		/// <summary>The blue component.</summary>
		public byte B;
		/// <summary>The alpha component.</summary>
		public byte A;

		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="a">The alpha component.</param>
		public Color32(byte r, byte g, byte b, byte a) {
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <remarks>The alpha component is set to full opacity.</remarks>
		public Color32(byte r, byte g, byte b) {
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = 255;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="color">The solid color.</param>
		/// <param name="a">The alpha component.</param>
		public Color32(Color24 color, byte a) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = a;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="color">The solid color.</param>
		/// <remarks>The alpha component is set to full opacity.</remarks>
		public Color32(Color24 color) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = 255;
		}

		/// <summary>Creates a new color from a Color128</summary>
		///<param name="color">The Color128</param>
		public Color32(Color128 color)
		{
			this.R = (byte)(255 * color.R);
			this.G = (byte)(255 * color.G);
			this.B = (byte)(255 * color.B);
			this.A = (byte)(255 * color.A);
		}

		// --- operators ---
		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public static bool operator ==(Color32 a, Color32 b) {
			return a.R == b.R & a.G == b.G & a.B == b.B & a.A == b.A;
		}
		/// <summary>Checks whether two colors are unequal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are unequal.</returns>
		public static bool operator !=(Color32 a, Color32 b) {
			return a.R != b.R | a.G != b.G | a.B != b.B | a.A != b.A;
		}

		/// <summary>Multiplies a Color32 with a scalar.</summary>
		/// <param name="a">The Color32.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The product of the Color32 and the scalar.</returns>
		public static Color32 operator *(Color32 a, double b)
		{
			a.R = (byte) System.Math.Round(a.R * b);
			a.G = (byte) System.Math.Round(a.G * b);
			a.B = (byte) System.Math.Round(a.B * b);
			return a;
		}

		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public bool Equals(Color32 a, Color32 b)
		{
			return a.R == b.R & a.G == b.G & a.B == b.B & a.A == b.A;
		}

		/// <summary>Checks whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Color32))
			{
				return false;
			}
			return Equals(this, (Color32)obj);
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
				hashCode = (hashCode * 397) ^ this.A.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>Defines the size of the Color32 struct in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf((object)new Color32());

		// --- read-only fields ---
		/// <summary>Represents a black color.</summary>
		public static readonly Color32 Black = new Color32(0, 0, 0);
		/// <summary>Represents a red color.</summary>
		public static readonly Color32 Red = new Color32(255, 0, 0);
		/// <summary>Represents a green color.</summary>
		public static readonly Color32 Green = new Color32(0, 255, 0);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color32 Blue = new Color32(0, 0, 255);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color32 Cyan = new Color32(0, 255, 255);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color32 Magenta = new Color32(255, 0, 255);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color32 Yellow = new Color32(255, 255, 0);
		/// <summary>Represents a white color.</summary>
		public static readonly Color32 White = new Color32(255, 255, 255);
		/// <summary>Represents a transparent black color.</summary>
		public static readonly Color32 Transparent = new Color32(0, 0, 0, 0);
		// --- conversions ---
		/// <summary>Performs a widening conversion from Color24 to Color32.</summary>
		/// <param name="value">The Color24 value.</param>
		/// <returns>The Color32 value.</returns>
		public static implicit operator Color32(Color24 value) {
			return new Color32(value.R, value.G, value.B);
		}
		/// <summary>Performs a narrowing conversion from Color32 to Color24.</summary>
		/// <param name="value">The Color32 value.</param>
		/// <returns>The Color24 value.</returns>
		public static explicit operator Color24(Color32 value) {
			return new Color24(value.R, value.G, value.B);
		}

		/// <summary>Parses a hexadecimal string into a Color32</summary>
		/// <param name="Expression">The color in hexadecimal format</param>
		/// <param name="Color">The Color32, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		public static bool TryParseHexColor(string Expression, out Color32 Color)
		{
			if (Expression.StartsWith("#", StringComparison.InvariantCultureIgnoreCase))
			{
				string hexNumber = Expression.Substring(1).TrimStart();
				int r = 0, g = 0, b = 255, a = 255; // legacy yuck - undefined color is pure blue
				if (hexNumber.Length >= 2)
				{
					r = int.Parse(hexNumber.Substring(0, 2), NumberStyles.HexNumber);
				}

				if (hexNumber.Length >= 4)
				{
					g = int.Parse(hexNumber.Substring(2, 2), NumberStyles.HexNumber);
				}

				if (hexNumber.Length >= 6)
				{
					b = int.Parse(hexNumber.Substring(4, 2), NumberStyles.HexNumber);
				}

				if (hexNumber.Length >= 8)
				{
					a = int.Parse(hexNumber.Substring(6, 2), NumberStyles.HexNumber);
				}

				if (r >= 0 && r <= 255 && g >= 0 && g <= 255 && b >= 0 && b <= 255 && a >= 0 && a <= 255)
				{
					Color = new Color32((byte)r, (byte)g, (byte)b, 255);
					return true;
				}
			}
			Color = Blue;
			return false;
		}

		/// <summary>Parses a Color32 stored in a string</summary>
		/// <param name="stringToParse">The string to parse</param>
		/// <param name="separator">The separator character</param>
		/// <param name="Color">The out Color32</param>
		/// <returns>True if parsing succeeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Color32.
		/// If any part fails parsing, it will be set to 255</remarks>
		public static bool TryParseColor(string stringToParse, char separator, out Color32 Color)
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
					case 3:
						if (!double.TryParse(splitString[i], out double a) || a < 0 || a > 255)
						{
							success = false;
						}
						else
						{
							Color.A = (byte)a;
						}
						break;
				}
			}

			if (i != 3 && i != 4)
			{
				success = false;
			}
			return success;
		}
		
		/// <summary>Parses a Color32 stored in a string array</summary>
		/// <param name="arguments">The string array to parse</param>
		/// <param name="Color">The out Color32</param>
		/// <returns>True if parsing succeeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Color32.
		/// If any part fails parsing, it will be set to 255</remarks>
		public static bool TryParseColor(string[] arguments, out Color32 Color)
		{
			Color = White;
			bool success = true;
			int i;
			for (i = 0; i < arguments.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (!double.TryParse(arguments[i], out double r) || r < 0 || r > 255)
						{
							success = false;
						}
						else
						{
							Color.R = (byte)r;
						}
						break;
					case 1:
						if (!double.TryParse(arguments[i], out double g) || g < 0 || g > 255)
						{
							success = false;
						}
						else
						{
							Color.G = (byte)g;
						}
						break;
					case 2:
						if (!double.TryParse(arguments[i], out double b) || b < 0 || b > 255)
						{
							success = false;
						}
						else
						{
							Color.B = (byte)b;
						}
						break;
					case 3:
						if (!double.TryParse(arguments[i], out double a) || a < 0 || a > 255)
						{
							success = false;
						}
						else
						{
							Color.A = (byte)a;
						}
						break;
				}
			}

			if (i != 3 && i != 4)
			{
				success = false;
			}
			return success;
		}

		private const float inv255 = 1.0f / 255.0f;

		/// <summary>Creates the background color for anti-aliasing text</summary>
		/// <param name="SystemColor">The color of the message text</param>
		/// <param name="Alpha">The alpha to apply</param>
		public Color128 CreateBackColor(MessageColor SystemColor, float Alpha)
		{
			Color128 c = new Color128();
			if (this.R == 0 & this.G == 0 & this.B == 0)
			{
				switch (SystemColor)
				{
					case MessageColor.Black:
						c.R = 0.0f; c.G = 0.0f; c.B = 0.0f;
						break;
					case MessageColor.Gray:
						c.R = 0.4f; c.G = 0.4f; c.B = 0.4f;
						break;
					case MessageColor.White:
						c.R = 1.0f; c.G = 1.0f; c.B = 1.0f;
						break;
					case MessageColor.Red:
						c.R = 1.0f; c.G = 0.0f; c.B = 0.0f;
						break;
					case MessageColor.Orange:
						c.R = 0.9f; c.G = 0.7f; c.B = 0.0f;
						break;
					case MessageColor.Green:
						c.R = 0.2f; c.G = 0.8f; c.B = 0.0f;
						break;
					case MessageColor.Blue:
						c.R = 0.0f; c.G = 0.7f; c.B = 1.0f;
						break;
					case MessageColor.Magenta:
						c.R = 1.0f; c.G = 0.0f; c.B = 0.7f;
						break;
					default:
						c.R = 1.0f; c.G = 1.0f; c.B = 1.0f;
						break;
				}
			}
			else
			{
				c.R = inv255 * R;
				c.G = inv255 * G;
				c.B = inv255 * B;
			}
			c.A = inv255 * A * Alpha;
			return c;
		}

		/// <summary>Creates the foreground color for anti-aliasing text</summary>
		/// <param name="SystemColor">The color of the message text</param>
		/// <param name="Alpha">The alpha to apply</param>
		public Color128 CreateTextColor(MessageColor SystemColor, float Alpha)
		{
			Color128 c = new Color128();
			if (this.R == 0 & this.G == 0 & this.B == 0)
			{
				switch (SystemColor)
				{
					case MessageColor.Black:
						c.R = 0.0f; c.G = 0.0f; c.B = 0.0f;
						break;
					case MessageColor.Gray:
						c.R = 0.4f; c.G = 0.4f; c.B = 0.4f;
						break;
					case MessageColor.White:
						c.R = 1.0f; c.G = 1.0f; c.B = 1.0f;
						break;
					case MessageColor.Red:
						c.R = 1.0f; c.G = 0.0f; c.B = 0.0f;
						break;
					case MessageColor.Orange:
						c.R = 0.9f; c.G = 0.7f; c.B = 0.0f;
						break;
					case MessageColor.Green:
						c.R = 0.3f; c.G = 1.0f; c.B = 0.0f;
						break;
					case MessageColor.Blue:
						c.R = 0.0f; c.G = 0.0f; c.B = 1.0f;
						break;
					case MessageColor.Magenta:
						c.R = 1.0f; c.G = 0.0f; c.B = 0.7f;
						break;
					default:
						c.R = 1.0f; c.G = 1.0f; c.B = 1.0f;
						break;
				}
			}
			else
			{
				c.R = inv255 * R;
				c.G = inv255 * G;
				c.B = inv255 * B;
			}
			c.A = inv255 * A * Alpha;
			return c;
		}

		/// <summary>Casts a System.Drawing.Color to a Color32</summary>
		/// <param name="c">The System.Drawing.Color</param>
		/// <returns>The new Color32</returns>
		public static implicit operator Color32(System.Drawing.Color c)
		{
			return new Color32(c.R, c.G, c.B, c.A);
		}

		/// <summary>Casts a Color32 to a System.Drawing.Color, preserving the alpha channel</summary>
		/// <param name="c">The Color32</param>
		/// <returns>The new System.Drawing.Color</returns>
		public static implicit operator System.Drawing.Color(Color32 c)
		{
			return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
		}

		/// <summary>Returns a string representation of this Color32</summary>
		public override string ToString()
		{
			return $"#{BitConverter.ToString(new[] { R, G, B, A }).Replace("-", string.Empty)}";
		}
	}
}
