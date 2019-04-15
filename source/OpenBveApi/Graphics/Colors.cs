using System;
using System.Globalization;

namespace OpenBveApi.Colors {
	
	/* ----------------------------------------
	 * TODO: This part of the API is unstable.
	 *       Modifications can be made at will.
	 * ---------------------------------------- */

	// --- color 24 ---
	
	/// <summary>Represents a 24-bit color with red, green and blue channels at 8 bits each.</summary>
	public struct Color24 {
		// --- members ---
		/// <summary>The red component.</summary>
		public byte R;
		/// <summary>The green component.</summary>
		public byte G;
		/// <summary>The blue component.</summary>
		public byte B;
		// --- constructors ---
		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		public Color24(byte r, byte g, byte b) {
			this.R = r;
			this.G = g;
			this.B = b;
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
		// --- operators ---
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
			return this.Equals((Color24)obj);
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
		/// <returns>True if the parse succeds, false if it does not</returns>
		public static bool TryParseHexColor(string Expression, out Color24 Color)
		{
			Color = Blue;
			if (Expression.StartsWith("#"))
			{
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x))
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
				return false;
			}
			return false;
		}

		/// <summary>Casts a System.Drawing.Color to a Color24, discarding the alpha component</summary>
		/// <param name="c">The System.Drawing.Color</param>
		/// <returns>The new Color24</returns>
		public static implicit operator Color24(System.Drawing.Color c)
		{
			return new Color24(c.R, c.G, c.B);
		}
	}
	
	
	// --- color 32 ---
	
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
		// --- constructors ---
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

		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public bool Equals(Color32 a, Color32 b)
		{
			return a.R != b.R | a.G != b.G | a.B != b.B | a.A != b.A;
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
			return this.Equals((Color32)obj);
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
		/// <returns>True if the parse succeds, false if it does not</returns>
		public static bool TryParseHexColor(string Expression, out Color32 Color)
		{
			if (Expression.StartsWith("#"))
			{
				string a = Expression.Substring(1).TrimStart();
				int x; if (Int32.TryParse(a, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x))
				{
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255)
					{
						Color = new Color32((byte)r, (byte)g, (byte)b, 255);
						return true;
					}
					Color = Blue;
					return false;
				}
				Color = Blue;
				return false;
			}
			Color = Blue;
			return false;
		}

		/// <summary>Casts a System.Drawing.Color to a Color32</summary>
		/// <param name="c">The System.Drawing.Color</param>
		/// <returns>The new Color32</returns>
		public static implicit operator Color32(System.Drawing.Color c)
		{
			return new Color32(c.R, c.G, c.B, c.A);
		}
	}
	
	
	// --- color 96 ---

	/// <summary>Represents a 96-bit color with red, green and blue channels at 32 bits each.</summary>
	public struct Color96 {
		// --- members ---
		/// <summary>The red component.</summary>
		public float R;
		/// <summary>The green component.</summary>
		public float G;
		/// <summary>The blue component.</summary>
		public float B;
		// --- constructors ---
		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		public Color96(float r, float g, float b) {
			this.R = r;
			this.G = g;
			this.B = b;
		}
		// --- operators ---
		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public static bool operator ==(Color96 a, Color96 b) {
			return a.R == b.R & a.G == b.G & a.B == b.B;
		}
		/// <summary>Checks whether two colors are unequal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are unequal.</returns>
		public static bool operator !=(Color96 a, Color96 b) {
			return a.R != b.R | a.G != b.G | a.B != b.B;
		}

		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public bool Equals(Color96 a, Color96 b)
		{
			return a.R != b.R | a.G != b.G | a.B != b.B;
		}

		/// <summary>Checks whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Color96))
			{
				return false;
			}
			return this.Equals((Color96)obj);
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
		public static readonly Color96 Black = new Color96(0.0f, 0.0f, 0.0f);
		/// <summary>Represents a red color.</summary>
		public static readonly Color96 Red = new Color96(1.0f, 0.0f, 0.0f);
		/// <summary>Represents a green color.</summary>
		public static readonly Color96 Green = new Color96(0.0f, 1.0f, 0.0f);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color96 Blue = new Color96(0.0f, 0.0f, 1.0f);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color96 Cyan = new Color96(0.0f, 1.0f, 1.0f);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color96 Magenta = new Color96(1.0f, 0.0f, 1.0f);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color96 Yellow = new Color96(1.0f, 1.0f, 0.0f);
		/// <summary>Represents a white color.</summary>
		public static readonly Color96 White = new Color96(1.0f, 1.0f, 1.0f);
	}
	
	
	// --- color 128 ---
	
	/// <summary>Represents a 128-bit color with red, green, blue and alpha channels at 32 bits each.</summary>
	public struct Color128 {
		// --- members ---
		/// <summary>The red component.</summary>
		public float R;
		/// <summary>The green component.</summary>
		public float G;
		/// <summary>The blue component.</summary>
		public float B;
		/// <summary>The alpha component.</summary>
		public float A;
		// --- constructors ---
		/// <summary>Creates a new color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="a">The alpha component.</param>
		public Color128(float r, float g, float b, float a) {
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
		public Color128(float r, float g, float b) {
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = 1.0f;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="color">The solid color.</param>
		/// <param name="a">The alpha component.</param>
		public Color128(Color24 color, float a) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = a;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="color">The solid color.</param>
		/// <remarks>The alpha component is set to full opacity.</remarks>
		public Color128(Color24 color) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = 1.0f;
		}
		// --- operators ---
		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public static bool operator ==(Color128 a, Color128 b) {
			return a.R == b.R & a.G == b.G & a.B == b.B & a.A == b.A;
		}
		/// <summary>Checks whether two colors are unequal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are unequal.</returns>
		public static bool operator !=(Color128 a, Color128 b) {
			return a.R != b.R | a.G != b.G | a.B != b.B | a.A != b.A;
		}

		/// <summary>Checks whether two colors are equal.</summary>
		/// <param name="a">The first color.</param>
		/// <param name="b">The second color.</param>
		/// <returns>Whether the two colors are equal.</returns>
		public bool Equals(Color128 a, Color128 b)
		{
			return a.R != b.R | a.G != b.G | a.B != b.B | a.A != b.A;
		}

		/// <summary>Checks whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Color128))
			{
				return false;
			}
			return this.Equals((Color128)obj);
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

		// --- read-only fields ---
		/// <summary>Represents a black color.</summary>
		public static readonly Color128 Black = new Color128(0.0f, 0.0f, 0.0f);
		/// <summary>Represents a red color.</summary>
		public static readonly Color128 Red = new Color128(1.0f, 0.0f, 0.0f);
		/// <summary>Represents a green color.</summary>
		public static readonly Color128 Green = new Color128(0.0f, 1.0f, 0.0f);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color128 Blue = new Color128(0.0f, 0.0f, 1.0f);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color128 Cyan = new Color128(0.0f, 1.0f, 1.0f);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color128 Magenta = new Color128(1.0f, 0.0f, 1.0f);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color128 Yellow = new Color128(1.0f, 1.0f, 0.0f);
		/// <summary>Represents a white color.</summary>
		public static readonly Color128 White = new Color128(1.0f, 1.0f, 1.0f);
		/// <summary>Represents a transparent black color.</summary>
		public static readonly Color128 Transparent = new Color128(0.0f, 0.0f, 0.0f, 0.0f);
		// --- conversions ---
		/// <summary>Performs a widening conversion from Color96 to Color128.</summary>
		/// <param name="value">The Color96 value.</param>
		/// <returns>The Color128 value.</returns>
		public static implicit operator Color128(Color24 value) {
			return new Color128(value.R, value.G, value.B);
		}
		/// <summary>Performs a narrowing conversion from Color128 to Color96.</summary>
		/// <param name="value">The Color128 value.</param>
		/// <returns>The Color96 value.</returns>
		public static explicit operator Color96(Color128 value) {
			return new Color96(value.R, value.G, value.B);
		}

	}
	/// <summary>Represents the available colors for in-game messages.</summary>
	public enum MessageColor
	{
		/// <summary>No color.</summary>
		None = 0,
		/// <summary>Black.</summary>
		Black = 1,
		/// <summary>Gray.</summary>
		Gray = 2,
		/// <summary>White.</summary>
		White = 3,
		/// <summary>Red.</summary>
		Red = 4,
		/// <summary>Orange.</summary>
		Orange = 5,
		/// <summary>Green.</summary>
		Green = 6,
		/// <summary>Blue.</summary>
		Blue = 7,
		/// <summary>Magenta.</summary>
		Magenta = 8
	}
}
