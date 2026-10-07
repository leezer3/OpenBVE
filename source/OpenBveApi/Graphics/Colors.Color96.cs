using System;
using System.IO;
// ReSharper disable UnusedMember.Global
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Colors
{
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
			if (r > 1 || r < 0 || g > 1 || g < 0 || b > 1 || b < 0)
			{
				throw new InvalidDataException();
			}
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
			return Equals(this, (Color96)obj);
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
}
