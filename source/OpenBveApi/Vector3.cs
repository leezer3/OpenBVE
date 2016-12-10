﻿#pragma warning disable 0660, 0661

using System;

namespace OpenBveApi.Math {
	/// <summary>Represents a three-dimensional vector.</summary>
	public struct Vector3 {
		
		// --- members ---
		
		/// <summary>The x-coordinate.</summary>
		public double X;
		
		/// <summary>The y-coordinate.</summary>
		public double Y;
		
		/// <summary>The z-coordinate.</summary>
		public double Z;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new three-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="z">The z-coordinate.</param>
		public Vector3(double x, double y, double z) {
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		/// <summary>Interpolates between two Vector3 values using a simple Cosine algorithm</summary>
		/// <param name="Vector1">The first vector</param>
		/// <param name="Vector2">The second vector</param>
		/// <param name="mu">The position on the curve of the new vector</param>
		/// <returns>The interpolated vector</returns>
		public static Vector3 CosineInterpolate(Vector3 Vector1, Vector3 Vector2, double mu)
		{
			double mu2 = (1 - System.Math.Cos(mu * System.Math.PI)) / 2;
			return new Vector3((Vector1.X * (1 - mu2) + Vector2.X * mu2), (Vector1.Y * (1 - mu2) + Vector2.Y * mu2), (Vector1.Z * (1 - mu2) + Vector2.Z * mu2));
		}

		/// <summary>Linearly interpolates between two vectors</summary>
		/// <param name="Vector1">The first vector</param>
		/// <param name="Vector2">The second vector</param>
		/// <param name="mu">The position on the interpolation curve of the new vector</param>
		/// <returns>The interpolated vector</returns>
		public static Vector3 LinearInterpolate(Vector3 Vector1, Vector3 Vector2, double mu)
		{
			return new Vector3(Vector1.X + ((Vector2.X - Vector1.X) * mu), Vector1.Y + ((Vector2.Y - Vector1.Y) * mu), Vector1.Z + ((Vector2.Z - Vector1.Z) * mu));
		}
		
		
		// --- arithmetic operators ---
		
		/// <summary>Adds two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector3 operator +(Vector3 a, Vector3 b) {
			return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}
		
		/// <summary>Adds a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The sum of the vector and the scalar.</returns>
		public static Vector3 operator +(Vector3 a, double b) {
			return new Vector3(a.X + b, a.Y + b, a.Z + b);
		}
		
		/// <summary>Adds a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The sum of the scalar and the vector.</returns>
		public static Vector3 operator +(double a, Vector3 b) {
			return new Vector3(a + b.X, a + b.Y, a + b.Z);
		}
		
		/// <summary>Subtracts two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector3 operator -(Vector3 a, Vector3 b) {
			return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}
		
		/// <summary>Subtracts a scalar from a vector.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The difference of the vector and the scalar.</returns>
		public static Vector3 operator -(Vector3 a, double b) {
			return new Vector3(a.X - b, a.Y - b, a.Z - b);
		}
		
		/// <summary>Subtracts a vector from a scalar.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The difference of the scalar and the vector.</returns>
		public static Vector3 operator -(double a, Vector3 b) {
			return new Vector3(a - b.X, a - b.Y, a - b.Z);
		}
		
		/// <summary>Negates a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The negation of the vector.</returns>
		public static Vector3 operator -(Vector3 vector) {
			return new Vector3(-vector.X, -vector.Y, -vector.Z);
		}
		
		/// <summary>Multiplies two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The product of the two vectors.</returns>
		public static Vector3 operator *(Vector3 a, Vector3 b) {
			return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
		}
		/// <summary>Multiplies a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static Vector3 operator *(Vector3 a, double b) {
			return new Vector3(a.X * b, a.Y * b, a.Z * b);
		}
		
		/// <summary>Multiplies a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The product of the scalar and the vector.</returns>
		public static Vector3 operator *(double a, Vector3 b) {
			return new Vector3(a * b.X, a * b.Y, a * b.Z);
		}
		
		/// <summary>Divides two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The quotient of the two vectors.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when any member of the second vector is zero.</exception>
		public static Vector3 operator /(Vector3 a, Vector3 b) {
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
				throw new DivideByZeroException();
			} else {
				return new Vector3(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
			}
		}
		
		/// <summary>Divides a vector by a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The quotient of the vector and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the scalar is zero.</exception>
		public static Vector3 operator /(Vector3 a, double b) {
			if (b == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / b;
				return new Vector3(a.X * factor, a.Y * factor, a.Z * factor);
			}
		}
		
		/// <summary>Divides a scalar by a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The quotient of the scalar and the vector.</returns>
		/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
		public static Vector3 operator /(double a, Vector3 b) {
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
				throw new DivideByZeroException();
			} else {
				return new Vector3(a / b.X, a / b.Y, a / b.Z);
			}
		}

		
		// --- comparisons ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public static bool operator ==(Vector3 a, Vector3 b) {
			if (a.X != b.X) return false;
			if (a.Y != b.Y) return false;
			if (a.Z != b.Z) return false;
			return true;
		}
		
		/// <summary>Checks whether the two specified vectors are unequal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are unequal.</returns>
		public static bool operator !=(Vector3 a, Vector3 b) {
			if (a.X != b.X) return true;
			if (a.Y != b.Y) return true;
			if (a.Z != b.Z) return true;
			return false;
		}
		
		
		// --- instance functions ---
		
		/// <summary>Normalizes the vector.</summary>
		/// <exception cref="System.DivideByZeroException">Raised when the vector is a null vector.</exception>
		public void Normalize() {
			double norm = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
			if (norm == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / System.Math.Sqrt(norm);
				this.X *= factor;
				this.Y *= factor;
				this.Z *= factor;
			}
		}
		
		/// <summary>Translates the vector by a specified offset.</summary>
		/// <param name="offset">The offset.</param>
		public void Translate(Vector3 offset) {
			this.X += offset.X;
			this.Y += offset.Y;
			this.Z += offset.Z;
		}
		
		/// <summary>Translates the vector by a specified offset that is measured in a specified orientation.</summary>
		/// <param name="orientation">The orientation.</param>
		/// <param name="offset">The offset measured in the specified orientation.</param>
		public void Translate(Orientation3 orientation, Vector3 offset) {
			this.X += orientation.X.X * offset.X + orientation.Y.X * offset.Y + orientation.Z.X * offset.Z;
			this.Y += orientation.X.Y * offset.X + orientation.Y.Y * offset.Y + orientation.Z.Y * offset.Z;
			this.Z += orientation.X.Z * offset.X + orientation.Y.Z * offset.Y + orientation.Z.Z * offset.Z;
		}
		
		/// <summary>Scales the vector by a specified factor.</summary>
		/// <param name="factor">The factor.</param>
		public void Scale(Vector3 factor) {
			this.X *= factor.X;
			this.Y *= factor.Y;
			this.Z *= factor.Z;
		}
		
		/// <summary>Rotates the vector on the plane perpendicular to a specified direction by a specified angle.</summary>
		/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
		/// <param name="cosineOfAngle">The cosine of the angle.</param>
		/// <param name="sineOfAngle">The sine of the angle.</param>
		public void Rotate(Vector3 direction, double cosineOfAngle, double sineOfAngle) {
			double cosineComplement = 1.0 - cosineOfAngle;
			double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * this.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * this.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * this.Z;
			double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * this.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * this.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * this.Z;
			double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * this.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * this.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * this.Y;
			this = new Vector3(x, y, z);
		}
		
		/// <summary>Rotates the vector from the default orientation into a specified orientation.</summary>
		/// <param name="orientation">The orientation.</param>
		/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
		public void Rotate(Orientation3 orientation) {
			double x = orientation.X.X * this.X + orientation.Y.X * this.Y + orientation.Z.X * this.Z;
			double y = orientation.X.Y * this.X + orientation.Y.Y * this.Y + orientation.Z.Y * this.Z;
			double z = orientation.X.Z * this.X + orientation.Y.Z * this.Y + orientation.Z.Z * this.Z;
			this = new Vector3(x, y, z);
		}
		
		/// <summary>Checks whether the vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public bool IsNullVector() {
			return this.X == 0.0 & this.Y == 0.0 & this.Z == 0.0;
		}
		
		/// <summary>Checks whether the vector is considered a null vector.</summary>
		/// <param name="tolerance">The highest absolute value that each component of the vector may have before the vector is not considered a null vector.</param>
		/// <returns>A boolean indicating whether the vector is considered a null vector.</returns>
		public bool IsNullVector(double tolerance) {
			if (this.X < -tolerance) return false;
			if (this.X > tolerance) return false;
			if (this.Y < -tolerance) return false;
			if (this.Y > tolerance) return false;
			if (this.Z < -tolerance) return false;
			if (this.Z > tolerance) return false;
			return true;
		}
		
		/// <summary>Gets the euclidean norm.</summary>
		/// <returns>The euclidean norm.</returns>
		public double Norm() {
			return System.Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
		}
		
		/// <summary>Gets the square of the euclidean norm.</summary>
		/// <returns>The square of the euclidean norm.</returns>
		public double NormSquared() {
			return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
		}

		
		// --- static functions ---
		
		/// <summary>Gives the dot product of two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The dot product of the two vectors.</returns>
		public static double Dot(Vector3 a, Vector3 b) {
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}
		
		/// <summary>Gives the cross product of two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The cross product of the two vectors.</returns>
		public static Vector3 Cross(Vector3 a, Vector3 b) {
			return new Vector3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}
		
		/// <summary>Normalizes a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The normalized vector.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the vector is a null vector.</exception>
		public static Vector3 Normalize(Vector3 vector) {
			double norm = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
			if (norm == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / System.Math.Sqrt(norm);
				return new Vector3(vector.X * factor, vector.Y * factor, vector.Z * factor);
			}
		}
		
		/// <summary>Translates a vector by a specified offset.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="offset">The offset.</param>
		/// <returns>The translated vector.</returns>
		public static Vector3 Translate(Vector3 vector, Vector3 offset) {
			double x = vector.X + offset.X;
			double y = vector.Y + offset.Y;
			double z = vector.Z + offset.Z;
			return new Vector3(x, y, z);
		}
		
		/// <summary>Translates a vector by a specified offset that is measured along a specified orientation.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="orientation">The orientation.</param>
		/// <param name="offset">The offset measured in the specified orientation.</param>
		public static Vector3 Translate(Vector3 vector, Orientation3 orientation, Vector3 offset) {
			double x = vector.X + orientation.X.X * offset.X + orientation.Y.X * offset.Y + orientation.Z.X * offset.Z;
			double y = vector.Y + orientation.X.Y * offset.X + orientation.Y.Y * offset.Y + orientation.Z.Y * offset.Z;
			double z = vector.Z + orientation.X.Z * offset.X + orientation.Y.Z * offset.Y + orientation.Z.Z * offset.Z;
			return new Vector3(x, y, z);
		}
		
		/// <summary>Scales a vector by a specified factor.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="factor">The factor.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector3 Scale(Vector3 vector, Vector3 factor) {
			double x = vector.X * factor.X;
			double y = vector.Y * factor.Y;
			double z = vector.Z * factor.Z;
			return new Vector3(x, y, z);
		}
		
		/// <summary>Rotates a vector on the plane perpendicular to a specified direction by a specified angle.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
		/// <param name="cosineOfAngle">The cosine of the angle.</param>
		/// <param name="sineOfAngle">The sine of the angle.</param>
		/// <returns>The rotated vector.</returns>
		public static Vector3 Rotate(Vector3 vector, Vector3 direction, double cosineOfAngle, double sineOfAngle) {
			double cosineComplement = 1.0 - cosineOfAngle;
			double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * vector.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * vector.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * vector.Z;
			double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * vector.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * vector.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * vector.Z;
			double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * vector.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * vector.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * vector.Y;
			return new Vector3(x, y, z);
		}
		
		/// <summary>Rotates a vector from the default orientation into a specified orientation.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="orientation">The orientation.</param>
		/// <returns>The rotated vector.</returns>
		/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
		public static Vector3 Rotate(Vector3 vector, Orientation3 orientation) {
			double x = orientation.X.X * vector.X + orientation.Y.X * vector.Y + orientation.Z.X * vector.Z;
			double y = orientation.X.Y * vector.X + orientation.Y.Y * vector.Y + orientation.Z.Y * vector.Z;
			double z = orientation.X.Z * vector.X + orientation.Y.Z * vector.Y + orientation.Z.Z * vector.Z;
			return new Vector3(x, y, z);
		}
		
		/// <summary>Creates a unit vector perpendicular to the plane described by three spatial coordinates, suitable for being a surface normal.</summary>
		/// <param name="a">The first spatial coordinate.</param>
		/// <param name="b">The second spatial coordinate.</param>
		/// <param name="c">The third spatial coordinate.</param>
		/// <param name="normal">On success, receives the vector perpendicular to the described plane. On failure, receives Vector3.Up.</param>
		/// <returns>The success of the operation. This operation fails if the specified three vectors are colinear.</returns>
		public static bool CreateNormal(Vector3 a, Vector3 b, Vector3 c, out Vector3 normal) {
			normal = Vector3.Cross(b - a, c - a);
			double norm = normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z;
			if (norm != 0.0) {
				normal *= 1.0 / System.Math.Sqrt(norm);
				return true;
			} else {
				normal = Vector3.Up;
				return false;
			}
		}
		
		/// <summary>Checks whether three spatial coordinates are colinear.</summary>
		/// <param name="a">The first spatial coordinate.</param>
		/// <param name="b">The second spatial coordinate.</param>
		/// <param name="c">The third spatial coordinate.</param>
		/// <returns>A boolean indicating whether the three spatial coordinates are colinear.</returns>
		public static bool AreColinear(Vector3 a, Vector3 b, Vector3 c) {
			Vector3 normal = Vector3.Cross(b - a, c - a);
			return IsNullVector(normal);
		}
		
		/// <summary>Checks whether a vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public static bool IsNullVector(Vector3 vector) {
			return vector.X == 0.0 & vector.Y == 0.0 & vector.Z == 0.0;
		}
		
		/// <summary>Gets the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The euclidean norm.</returns>
		public static double Norm(Vector3 vector) {
			return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		}
		
		/// <summary>Gets the square of the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The square of the euclidean norm.</returns>
		public static double NormSquared(Vector3 vector) {
			return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
		}

		/// <summary>Returns a normalized vector based on a 2D vector in the XZ plane and an additional Y-coordinate.</summary>
		/// <param name="Vector">The vector in the XZ-plane. The X and Y components in Vector represent the X- and Z-coordinates, respectively.</param>
		/// <param name="Y">The Y-coordinate.</param>
		public static Vector3 GetVector3(Vector2 Vector, double Y)
		{
			double t = 1.0 / System.Math.Sqrt(Vector.X * Vector.X + Vector.Y * Vector.Y + Y * Y);
			return new Vector3(t * Vector.X, t * Y, t * Vector.Y);
		}
		/// <summary>Determines whether this is a zero (0,0,0) vector</summary>
		/// <param name="Vector"></param>
		/// <returns>True if this is a zero vector, false otherwise</returns>
		public static bool IsZero(Vector3 Vector)
		{
			if (Vector.X != 0.0f) return false;
			if (Vector.Y != 0.0f) return false;
			if (Vector.Z != 0.0f) return false;
			return true;
		}
		
		// --- read-only fields ---
		
		/// <summary>Represents a null vector.</summary>
		public static readonly Vector3 Null = new Vector3(0.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing left.</summary>
		public static readonly Vector3 Left = new Vector3(-1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing right.</summary>
		public static readonly Vector3 Right = new Vector3(1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector3 Up = new Vector3(0.0, -1.0, 0.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector3 Down = new Vector3(0.0, 1.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector3 Backward = new Vector3(0.0, 0.0, -1.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector3 Forward = new Vector3(0.0, 0.0, 1.0);

		/// <summary>Returns the representation of the vector in string format</summary>
		public override string ToString()
		{
			string toString = this.X + " , " + this.Y + " , " + this.Z;
			return toString;
		}
	}
}