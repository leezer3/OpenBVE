using System;
using System.Globalization;

#pragma warning disable IDE0064
using System.Runtime.InteropServices;
// ReSharper disable MergeCastWithTypeCheck

namespace OpenBveApi.Math {
	/// <summary>Represents a two-dimensional vector.</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector2 {
		
		// --- members ---
		
		/// <summary>The x-coordinate.</summary>
		public double X;
		
		/// <summary>The y-coordinate.</summary>
		public double Y;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new two-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		public Vector2(double x, double y) {
			this.X = x;
			this.Y = y;
		}

		/// <summary>Creates a clone of a vector</summary>
		/// <param name="v">The vector to clone</param>
		public Vector2(Vector2 v)
		{
			this.X = v.X;
			this.Y = v.Y;
		}

		/// <summary>Converts a Vector2 to a Vector2f</summary>
		///	<remarks>This discards the double precision</remarks>
		public static implicit operator Vector2f(Vector2 v)
		{
			return new Vector2f(v.X, v.Y);
		}

		/// <summary>Parses a Vector2 from a list of strings</summary>
		/// <param name="arguments">The list of strings</param>
		/// <param name="v">The out Vector</param>
		/// <returns>True if parsing succeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Vector3.
		/// If any part fails parsing, it will be set to zero</remarks>
		public static bool TryParse(string[] arguments, out Vector2 v)
		{
			bool success = arguments.Length == 2;
			v.X = 0; v.Y = 0; 
			for (int i = 0; i < arguments.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (!double.TryParse(arguments[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v.X))
						{
							success = false;
						}
						break;
					case 1:
						if (!double.TryParse(arguments[i], NumberStyles.Float, CultureInfo.InvariantCulture, out v.Y))
						{
							success = false;
						}
						break;
				}
			}
			return success;
		}

		/// <summary>Unconditionally parses a vector stored in string format</summary>
		/// <param name="v">The string</param>
		public static Vector2 Parse(string v)
		{
			TryParse(v.Split(','), out Vector2 vv);
			return vv;
		}
		
		// --- arithmetic operators ---
		
		/// <summary>Adds two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector2 operator +(Vector2 a, Vector2 b) {
			return new Vector2(a.X + b.X, a.Y + b.Y);
		}
		
		/// <summary>Adds a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The sum of the vector and the scalar.</returns>
		public static Vector2 operator +(Vector2 a, double b) {
			return new Vector2(a.X + b, a.Y + b);
		}
		
		/// <summary>Adds a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The sum of the scalar and the vector.</returns>
		public static Vector2 operator +(double a, Vector2 b) {
			return new Vector2(a + b.X, a + b.Y);
		}
		
		/// <summary>Subtracts two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector2 operator -(Vector2 a, Vector2 b) {
			return new Vector2(a.X - b.X, a.Y - b.Y);
		}
		
		/// <summary>Subtracts a scalar from a vector.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The difference of the vector and the scalar.</returns>
		public static Vector2 operator -(Vector2 a, double b) {
			return new Vector2(a.X - b, a.Y - b);
		}
		
		/// <summary>Subtracts a vector from a scalar.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The difference of the scalar and the vector.</returns>
		public static Vector2 operator -(double a, Vector2 b) {
			return new Vector2(a - b.X, a - b.Y);
		}
		
		/// <summary>Negates a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The negation of the vector.</returns>
		public static Vector2 operator -(Vector2 vector) {
			return new Vector2(-vector.X, -vector.Y);
		}
		
		/// <summary>Multiplies two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The product of the two vectors.</returns>
		public static Vector2 operator *(Vector2 a, Vector2 b) {
			return new Vector2(a.X * b.X, a.Y * b.Y);
		}
		/// <summary>Multiplies a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static Vector2 operator *(Vector2 a, double b) {
			return new Vector2(a.X * b, a.Y * b);
		}
		
		/// <summary>Multiplies a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The product of the scalar and the vector.</returns>
		public static Vector2 operator *(double a, Vector2 b) {
			return new Vector2(a * b.X, a * b.Y);
		}
		
		/// <summary>Divides two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The quotient of the two vectors.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when any member of the second vector is zero.</exception>
		public static Vector2 operator /(Vector2 a, Vector2 b) {
			if (b.X == 0.0 | b.Y == 0.0) {
				throw new DivideByZeroException();
			} else {
				return new Vector2(a.X / b.X, a.Y / b.Y);
			}
		}
		
		/// <summary>Divides a vector by a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The quotient of the vector and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the scalar is zero.</exception>
		public static Vector2 operator /(Vector2 a, double b) {
			if (b == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / b;
				return new Vector2(a.X * factor, a.Y * factor);
			}
		}
		
		/// <summary>Divides a scalar by a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The quotient of the scalar and the vector.</returns>
		/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
		public static Vector2 operator /(double a, Vector2 b) {
			if (b.X == 0.0 | b.Y == 0.0) {
				throw new DivideByZeroException();
			} else {
				return new Vector2(a / b.X, a / b.Y);
			}
		}
		
		
		// --- comparisons ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public static bool operator ==(Vector2 a, Vector2 b) {
			if (a.X != b.X) return false;
			if (a.Y != b.Y) return false;
			return true;
		}
		
		/// <summary>Checks whether the two specified vectors are unequal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are unequal.</returns>
		public static bool operator !=(Vector2 a, Vector2 b) {
			if (a.X != b.X) return true;
			if (a.Y != b.Y) return true;
			return false;
		}

		/// <summary>Returns the hashcode for this instance.</summary>
		/// <returns>An integer containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
			}
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector2))
			{
				return false;
			}

			return this.Equals((Vector2)obj);
		}

		/// <summary>Checks whether the current vector is equal to the specified vector.</summary>
		/// <param name="b">The specified vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public bool Equals(Vector2 b)
		{
			if (this.X != b.X) return false;
			if (this.Y != b.Y) return false;
			return true;
		}


		// --- instance functions ---

		/// <summary>Normalizes the vector.</summary>
		/// <exception cref="System.DivideByZeroException">Raised when the vector is a null vector.</exception>
		public void Normalize() {
			double norm = this.X * this.X + this.Y * this.Y;
			if (norm == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / System.Math.Sqrt(norm);
				this.X *= factor;
				this.Y *= factor;
			}
		}
		
		/// <summary>Translates the vector by a specified offset.</summary>
		/// <param name="offset">The offset.</param>
		public void Translate(Vector2 offset) {
			this.X += offset.X;
			this.Y += offset.Y;
		}
		
		/// <summary>Scales the vector by a specified factor.</summary>
		/// <param name="factor">The factor.</param>
		public void Scale(Vector2 factor) {
			this.X *= factor.X;
			this.Y *= factor.Y;
		}
		
		/// <summary>Rotates the vector by the specified angle.</summary>
		/// <param name="cosineOfAngle">The cosine of the angle.</param>
		/// <param name="sineOfAngle">The sine of the angle.</param>
		public void Rotate(double cosineOfAngle, double sineOfAngle) {
			double x = cosineOfAngle * this.X - sineOfAngle * this.Y;
			double y = sineOfAngle * this.X + cosineOfAngle * this.Y;
			this = new Vector2(x, y);
		}

		/// <summary>Rotates the vector by the specified angle.</summary>
		/// <param name="angle">The angle.</param>
		public void Rotate(double angle)
		{
			if (angle == 0)
			{
				return;
			}
			double cosineOfAngle = System.Math.Cos(angle);
			double sineOfAngle = System.Math.Sin(angle);
			Rotate(cosineOfAngle, sineOfAngle);
		}

		/// <summary>Checks whether the vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public bool IsNullVector() {
			return this.X == 0.0 & this.Y == 0.0;
		}

		/// <summary>Tests to see whether the vector is finite (no components are double or infinity.</summary>
		/// <returns>A boolean indicating whether the vector is finite</returns>
		public static bool IsFinite(Vector2 Vector)
		{
			return !double.IsNaN(Vector.X) && !double.IsInfinity(Vector.X) && !double.IsNaN(Vector.Y) && !double.IsInfinity(Vector.Y);
		}

		/// <summary>Checks whether the vector is considered a null vector.</summary>
		/// <param name="tolerance">The highest absolute value that each component of the vector may have before the vector is not considered a null vector.</param>
		/// <returns>A boolean indicating whether the vector is considered a null vector.</returns>
		public bool IsNullVector(double tolerance) {
			if (this.X < -tolerance) return false;
			if (this.X > tolerance) return false;
			if (this.Y < -tolerance) return false;
			if (this.Y > tolerance) return false;
			return true;
		}
		
		/// <summary>Gets the euclidean norm.</summary>
		/// <returns>The euclidean norm.</returns>
		public double Norm() {
			return System.Math.Sqrt(this.X * this.X + this.Y * this.Y);
		}
		
		/// <summary>Gets the square of the euclidean norm.</summary>
		/// <returns>The square of the euclidean norm.</returns>
		public double NormSquared() {
			return this.X * this.X + this.Y * this.Y;
		}

		
		// --- static functions ---
		
		/// <summary>Gives the dot product of two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static double Dot(Vector2 a, Vector2 b) {
			return a.X * b.X + a.Y * b.Y;
		}

		/// <summary>Gives the determinant product of two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The determinant product of the two vectors.</returns>
		public static double Determinant(Vector2 a, Vector2 b)
		{
			return a.X * b.Y - a.Y * b.X;
		}

		/// <summary>Normalizes a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The normalized vector.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the vector is a null vector.</exception>
		public static Vector2 Normalize(Vector2 vector) {
			double norm = vector.X * vector.X + vector.Y * vector.Y;
			if (norm == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / System.Math.Sqrt(norm);
				return new Vector2(vector.X * factor, vector.Y * factor);
			}
		}
		
		/// <summary>Translates a vector by a specified offset.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="offset">The offset.</param>
		/// <returns>The translated vector.</returns>
		public static Vector2 Translate(Vector2 vector, Vector2 offset) {
			double x = vector.X + offset.X;
			double y = vector.Y + offset.Y;
			return new Vector2(x, y);
		}
		
		/// <summary>Scales a vector by a specified factor.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="factor">The factor.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector2 Scale(Vector2 vector, Vector2 factor) {
			double x = vector.X * factor.X;
			double y = vector.Y * factor.Y;
			return new Vector2(x, y);
		}
		
		/// <summary>Rotates a vector by a specified angle.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="cosineOfAngle">The cosine of the angle.</param>
		/// <param name="sineOfAngle">The sine of the angle.</param>
		/// <returns>The rotated vector.</returns>
		public static Vector2 Rotate(Vector2 vector, double cosineOfAngle, double sineOfAngle) {
			double x = cosineOfAngle * vector.X - sineOfAngle * vector.Y;
			double y = sineOfAngle * vector.X + cosineOfAngle * vector.Y;
			return new Vector2(x, y);

		}
		
		/// <summary>Checks whether a vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public static bool IsNullVector(Vector2 vector) {
			return vector.X == 0.0 & vector.Y == 0.0;
		}
		
		/// <summary>Gets the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The euclidean norm.</returns>
		public static double Norm(Vector2 vector) {
			return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
		}
		
		/// <summary>Gets the square of the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The square of the euclidean norm.</returns>
		public static double NormSquared(Vector2 vector) {
			return vector.X * vector.X + vector.Y * vector.Y;
		}
		
		
		// --- read-only fields ---
		
		/// <summary>Represents a null vector.</summary>
		public static readonly Vector2 Null = new Vector2(0.0, 0.0);
		
		/// <summary>Represents a vector pointing left.</summary>
		public static readonly Vector2 Left = new Vector2(-1.0, 0.0);
		
		/// <summary>Represents a vector pointing right.</summary>
		public static readonly Vector2 Right = new Vector2(1.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector2 Up = new Vector2(0.0, -1.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector2 Down = new Vector2(0.0, 1.0);

		/// <summary>Represents a unary vector.</summary>
		public static readonly Vector2 One = new Vector2(1.0, 1.0);

		/// <summary>Returns the representation of the vector in string format</summary>
		public override string ToString()
		{
			CultureInfo c = CultureInfo.InvariantCulture;
			string toString = this.X.ToString(c) + " , " + this.Y.ToString(c);
			return toString;
		}
		
	}
}
