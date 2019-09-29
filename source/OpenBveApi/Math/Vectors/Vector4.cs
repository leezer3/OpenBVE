using System;

namespace OpenBveApi.Math
{
	/// <summary>Represents a 4-dimensional vector</summary>
	public struct Vector4
	{
		/// <summary>The x-coordinate.</summary>
		public double X;
		
		/// <summary>The y-coordinate.</summary>
		public double Y;
		
		/// <summary>The z-coordinate.</summary>
		public double Z;

		/// <summary>The W-component of the vector</summary>
		public double W;

		/// <summary>Creates a new three-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="z">The z-coordinate.</param>
		/// <param name="w">The w-component</param>
		public Vector4(double x, double y, double z, double w) {
			this.X = x;
			this.Y = y;
			this.Z = z;
			this.W = w;
		}

		/// <summary>Creates a clone of a vector</summary>
		/// <param name="v">The vector to clone</param>
		public Vector4(Vector4 v) {
			this.X = v.X;
			this.Y = v.Y;
			this.Z = v.Z;
			this.W = v.W;
		}

		// --- arithmetic operators ---
		
		/// <summary>Adds two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector4 operator +(Vector4 a, Vector4 b)
		{
			a.X += b.X;
			a.Y += b.Y;
			a.Z += b.Z;
			a.W += b.W;
			return a;
		}
		
		/// <summary>Adds a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The sum of the vector and the scalar.</returns>
		public static Vector4 operator +(Vector4 a, double b)
		{
			a.X += b;
			a.Y += b;
			a.Z += b;
			a.W += b;
			return a;
		}
		
		/// <summary>Adds a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The sum of the scalar and the vector.</returns>
		public static Vector4 operator +(double a, Vector4 b) {
			b.X += a;
			b.Y += a;
			b.Z += a;
			b.W += a;
			return b;
		}
		
		/// <summary>Subtracts two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector4 operator -(Vector4 a, Vector4 b)
		{
			a.X -= b.X;
			a.Y -= b.Y;
			a.Z -= b.Z;
			a.W -= b.W;
			return a;
		}
		
		/// <summary>Subtracts a scalar from a vector.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The difference of the vector and the scalar.</returns>
		public static Vector4 operator -(Vector4 a, double b)
		{
			a.X -= b;
			a.Y -= b;
			a.Z -= b;
			a.W -= b;
			return a;
		}
		
		/// <summary>Subtracts a vector from a scalar.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The difference of the scalar and the vector.</returns>
		public static Vector4 operator -(double a, Vector4 b)
		{
			b.X -= a;
			b.Y -= a;
			b.Z -= a;
			b.W -= a;
			return b;
		}
		
		/// <summary>Negates a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The negation of the vector.</returns>
		public static Vector4 operator -(Vector4 vector)
		{
			vector.X = -vector.X;
			vector.Y = -vector.Y;
			vector.Z = -vector.Z;
			vector.W = -vector.W;
			return vector;
		}
		
		/// <summary>Multiplies two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The product of the two vectors.</returns>
		public static Vector4 operator *(Vector4 a, Vector4 b)
		{
			a.X *= b.X;
			a.Y *= b.Y;
			a.Z *= b.Z;
			a.W *= b.W;
			return a;
		}
		/// <summary>Multiplies a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static Vector4 operator *(Vector4 a, double b)
		{
			a.X *= b;
			a.Y *= b;
			a.Z *= b;
			a.W *= b;
			return a;
		}
		
		/// <summary>Multiplies a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The product of the scalar and the vector.</returns>
		public static Vector4 operator *(double a, Vector4 b)
		{
			b.X *= a;
			b.Y *= a;
			b.Z *= a;
			b.W *= a;
			return b;
		}
		
		/// <summary>Divides two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The quotient of the two vectors.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when any member of the second vector is zero.</exception>
		public static Vector4 operator /(Vector4 a, Vector4 b)
		{
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0 | b.W == 0.0) {
				throw new DivideByZeroException();
			}

			a.X /= b.X;
			a.Y /= b.Y;
			a.Z /= b.Z;
			a.W /= b.W;
			return a;
		}
		
		/// <summary>Divides a vector by a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The quotient of the vector and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the scalar is zero.</exception>
		public static Vector4 operator /(Vector4 a, double b) {
			if (b == 0.0) {
				throw new DivideByZeroException();
			}
			double factor = 1.0 / b;
			a.X *= factor;
			a.Y *= factor;
			a.Z *= factor;
			a.W *= factor;
			return a;
		}
		
		/// <summary>Divides a scalar by a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The quotient of the scalar and the vector.</returns>
		/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
		public static Vector4 operator /(double a, Vector4 b)
		{
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0 | b.W == 0.0) {
				throw new DivideByZeroException();
			}

			b.X /= a;
			b.Y /= a;
			b.Z /= a;
			b.W /= a;
			return b;
		}

		
		// --- comparisons ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public static bool operator ==(Vector4 a, Vector4 b) {
			if (a.X != b.X) return false;
			if (a.Y != b.Y) return false;
			if (a.Z != b.Z) return false;
			if (a.W != b.W) return false;
			return true;
		}

		/// <summary>Returns the hashcode for this instance.</summary>
		/// <returns>An integer representing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.X.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Z.GetHashCode();
				hashCode = (hashCode * 397) ^ this.W.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector4))
			{
				return false;
			}

			return this.Equals((Vector4)obj);
		}

		/// <summary>Checks whether the current vector is equal to the specified vector.</summary>
		/// <param name="b">The specified vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public bool Equals(Vector4 b)
		{
			if (this.X != b.X) return false;
			if (this.Y != b.Y) return false;
			if (this.Z != b.Z) return false;
			if (this.W != b.W) return false;
			return true;
		}

		/// <summary>Checks whether the two specified vectors are unequal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are unequal.</returns>
		public static bool operator !=(Vector4 a, Vector4 b) {
			if (a.X != b.X) return true;
			if (a.Y != b.Y) return true;
			if (a.Z != b.Z) return true;
			if (a.W != b.W) return true;
			return false;
		}

		/// <summary>Represents a null vector</summary>
		public static readonly Vector4 Zero = new Vector4(0.0, 0.0, 0.0, 0.0);
	}

	
}
