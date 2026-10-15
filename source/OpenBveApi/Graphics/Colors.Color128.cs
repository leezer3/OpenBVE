// ReSharper disable UnusedMember.Global
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Colors
{
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
			this.R = color.R / 255.0f;
			this.G = color.G / 255.0f;
			this.B = color.B / 255.0f;
			this.A = a;
		}
		/// <summary>Creates a new color.</summary>
		/// <param name="color">The solid color.</param>
		/// <remarks>The alpha component is set to full opacity.</remarks>
		public Color128(Color24 color) {
			this.R = color.R / 255.0f;
			this.G = color.G / 255.0f;
			this.B = color.B / 255.0f;
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
			return Equals(this, (Color128)obj);
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
		/// <summary>Represents a black color</summary>
		public static readonly Color128 Black = new Color128(0.0f, 0.0f, 0.0f);
		/// <summary>Represents a black color</summary>
		public static readonly Color128 Grey = new Color128(0.5f, 0.5f, 0.5f);
		/// <summary>Represents a red color</summary>
		public static readonly Color128 Red = new Color128(1.0f, 0.0f, 0.0f);
		/// <summary>Represents a green color</summary>
		public static readonly Color128 Green = new Color128(0.0f, 1.0f, 0.0f);
		/// <summary>Represents a blue color</summary>
		public static readonly Color128 Blue = new Color128(0.0f, 0.0f, 1.0f);
		/// <summary>Represents a cyan color</summary>
		public static readonly Color128 Cyan = new Color128(0.0f, 1.0f, 1.0f);
		/// <summary>Represents a magenta color</summary>
		public static readonly Color128 Magenta = new Color128(1.0f, 0.0f, 1.0f);
		/// <summary>Represents a yellow color</summary>
		public static readonly Color128 Yellow = new Color128(1.0f, 1.0f, 0.0f);
		/// <summary>Represents a white color</summary>
		public static readonly Color128 White = new Color128(1.0f, 1.0f, 1.0f);
		/// <summary>Represents a transparent black color</summary>
		public static readonly Color128 Transparent = new Color128(0.0f, 0.0f, 0.0f, 0.0f);
		
		/*
		 * Colors used by overlays etc.
		 * Where possible, use the standard web palette names
		 */
		/// <summary>Represents a semi transparent grey color</summary>
		public static readonly Color128 SemiTransparentGrey = new Color128(0.5f, 0.5f, 0.5f, 0.5f);
		/// <summary>Represents a deep sky blue color</summary>
		public static readonly Color128 DeepSkyBlue = new Color128(0.0f, 0.75f, 1.0f);
		/// <summary>Represents an orange color</summary>
		public static readonly Color128 Orange = new Color128(1.0f, 0.69f, 0.0f);

		// --- conversions ---
		/// <summary>Performs a widening conversion from Color24 to Color128.</summary>
		/// <param name="value">The Color96 value.</param>
		/// <returns>The Color128 value.</returns>
		public static implicit operator Color128(Color24 value) {
			return new Color128(value.R, value.G, value.B);
		}

		/// <summary>Performs a widening conversion from Color32 to Color128.</summary>
		/// <param name="value">The Color96 value.</param>
		/// <returns>The Color128 value.</returns>
		public static implicit operator Color128(Color32 value)
		{
			const float inv255 = 1.0f / 255;
			return new Color128(value.R * inv255, value.G * inv255, value.B * inv255, value.A * inv255);
		}

		/// <summary>Performs a narrowing conversion from Color128 to Color96.</summary>
		/// <param name="value">The Color128 value.</param>
		/// <returns>The Color96 value.</returns>
		public static explicit operator Color96(Color128 value) {
			return new Color96(value.R, value.G, value.B);
		}
	}
}
