using System;
using OpenBveApi.World;

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

		/// <summary>Creates a clone of a vector</summary>
		/// <param name="v">The vector to clone</param>
		public Vector3(Vector3 v) {
			this.X = v.X;
			this.Y = v.Y;
			this.Z = v.Z;
		}

		/// <summary>Parses a Vector3 stored in a string</summary>
		/// <param name="stringToParse">The string to parse</param>
		/// <param name="separator">The separator character</param>
		/// <param name="v">The out Vector</param>
		/// <returns>True if parsing succeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Vector3.
		/// If any part fails parsing, it will be set to zero</remarks>
		public static bool TryParse(string stringToParse, char separator, out Vector3 v)
		{
			bool success = true;
			v.X = 0; v.Y = 0; v.Z = 0; //Don't generate a new struct- Important in parsing objects with large numbers of verts
			string[] splitString = stringToParse.Split(new char[] { separator });
			int i;
			for (i = 0; i < splitString.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (!double.TryParse(splitString[i], out v.X))
						{
							success = false;
						}
						break;
					case 1:
						if (!double.TryParse(splitString[i], out v.Y))
						{
							success = false;
						}
						break;
					case 2:
						if (!double.TryParse(splitString[i], out v.Z))
						{
							success = false;
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
		
		/// <summary>Converts a Vector3 to a Vector3f</summary>
		///	<remarks>This discards the double precision</remarks>
		public static implicit operator Vector3f(Vector3 v)
		{
			return new Vector3f(v.X, v.Y, v.Z);
		}
		
		// --- arithmetic operators ---
		
		/// <summary>Adds two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector3 operator +(Vector3 a, Vector3 b)
		{
			a.X += b.X;
			a.Y += b.Y;
			a.Z += b.Z;
			return a;
		}
		
		/// <summary>Adds a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The sum of the vector and the scalar.</returns>
		public static Vector3 operator +(Vector3 a, double b)
		{
			a.X += b;
			a.Y += b;
			a.Z += b;
			return a;
		}
		
		/// <summary>Adds a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The sum of the scalar and the vector.</returns>
		public static Vector3 operator +(double a, Vector3 b) {
			b.X += a;
			b.Y += a;
			b.Z += a;
			return b;
		}
		
		/// <summary>Subtracts two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			a.X -= b.X;
			a.Y -= b.Y;
			a.Z -= b.Z;
			return a;
		}
		
		/// <summary>Subtracts a scalar from a vector.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The difference of the vector and the scalar.</returns>
		public static Vector3 operator -(Vector3 a, double b)
		{
			a.X -= b;
			a.Y -= b;
			a.Z -= b;
			return a;
		}
		
		/// <summary>Subtracts a vector from a scalar.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The difference of the scalar and the vector.</returns>
		public static Vector3 operator -(double a, Vector3 b)
		{
			b.X -= a;
			b.Y -= a;
			b.Z -= a;
			return b;
		}
		
		/// <summary>Negates a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The negation of the vector.</returns>
		public static Vector3 operator -(Vector3 vector)
		{
			vector.X = -vector.X;
			vector.Y = -vector.Y;
			vector.Z = -vector.Z;
			return vector;
		}
		
		/// <summary>Multiplies two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The product of the two vectors.</returns>
		public static Vector3 operator *(Vector3 a, Vector3 b)
		{
			a.X *= b.X;
			a.Y *= b.Y;
			a.Z *= b.Z;
			return a;
		}
		/// <summary>Multiplies a vector and a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The product of the vector and the scalar.</returns>
		public static Vector3 operator *(Vector3 a, double b)
		{
			a.X *= b;
			a.Y *= b;
			a.Z *= b;
			return a;
		}
		
		/// <summary>Multiplies a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The product of the scalar and the vector.</returns>
		public static Vector3 operator *(double a, Vector3 b)
		{
			b.X *= a;
			b.Y *= a;
			b.Z *= a;
			return b;
		}
		
		/// <summary>Divides two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The quotient of the two vectors.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when any member of the second vector is zero.</exception>
		public static Vector3 operator /(Vector3 a, Vector3 b)
		{
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
				throw new DivideByZeroException();
			}

			a.X /= b.X;
			a.Y /= b.Y;
			a.Z /= b.Z;
			return a;
		}
		
		/// <summary>Divides a vector by a scalar.</summary>
		/// <param name="a">The vector.</param>
		/// <param name="b">The scalar.</param>
		/// <returns>The quotient of the vector and the scalar.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the scalar is zero.</exception>
		public static Vector3 operator /(Vector3 a, double b) {
			if (b == 0.0) {
				throw new DivideByZeroException();
			}
			double factor = 1.0 / b;
			a.X *= factor;
			a.Y *= factor;
			a.Z *= factor;
			return a;
		}
		
		/// <summary>Divides a scalar by a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The quotient of the scalar and the vector.</returns>
		/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
		public static Vector3 operator /(double a, Vector3 b)
		{
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
				throw new DivideByZeroException();
			}

			b.X /= a;
			b.Y /= a;
			b.Z /= a;
			return b;
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

		/// <summary>Returns the hashcode for this instance.</summary>
		/// <returns>An integer representing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.X.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Y.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Z.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector3))
			{
				return false;
			}

			return this.Equals((Vector3)obj);
		}

		/// <summary>Checks whether the current vector is equal to the specified vector.</summary>
		/// <param name="b">The specified vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public bool Equals(Vector3 b)
		{
			if (this.X != b.X) return false;
			if (this.Y != b.Y) return false;
			if (this.Z != b.Z) return false;
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
		public void Normalize() {
			double norm = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
			if (norm == 0.0)
			{
				return;
			}
			double factor = 1.0 / System.Math.Sqrt(norm);
			this.X *= factor;
			this.Y *= factor;
			this.Z *= factor;
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
		/// <param name="angle">The angle to rotate by.</param>
		public void Rotate(Vector3 direction, double angle)
		{
			Rotate(direction, System.Math.Cos(angle), System.Math.Sin(angle));
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
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Rotates the vector on the perpendicular world plane (Used by the .Turn command)</summary>
		/// <param name="cosa">The cosine of the angle.</param>
		/// <param name="sina">The sine of the angle.</param>
		public void RotatePlane(double cosa, double sina)
		{
			double u = X * cosa - Z * sina;
			double v = X * sina + Z * cosa;
			X = u;
			Z = v;
		}

		/// <summary>Rotates the vector based upon three other vectors</summary>
		/// <param name="direction">The vector in the Z axis direction</param>
		/// <param name="up">The vector in the Y axis direction</param>
		/// <param name="side">The vector in the X axis direction</param>
		public void Rotate(Vector3 direction, Vector3 up, Vector3 side) {
			var x = side.X * this.X + up.X * this.Y + direction.X * this.Z;
			var y = side.Y * this.X + up.Y * this.Y + direction.Y * this.Z;
			var z = side.Z * this.X + up.Z * this.Y + direction.Z * this.Z;
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Rotates the vector from the default orientation into a specified orientation.</summary>
		/// <param name="orientation">The orientation.</param>
		/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
		public void Rotate(Orientation3 orientation) {
			double x = orientation.X.X * this.X + orientation.Y.X * this.Y + orientation.Z.X * this.Z;
			double y = orientation.X.Y * this.X + orientation.Y.Y * this.Y + orientation.Z.Y * this.Z;
			double z = orientation.X.Z * this.X + orientation.Y.Z * this.Y + orientation.Z.Z * this.Z;
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Rotates the vector using the specified transformation</summary>
		/// <param name="transformation">The transformation</param>
		public void Rotate(Transformation transformation)
		{
			double x = transformation.X.X * X + transformation.Y.X * Y + transformation.Z.X * Z;
			double y = transformation.X.Y * X + transformation.Y.Y * Y + transformation.Z.Y * Z;
			double z = transformation.X.Z * X + transformation.Y.Z * Y + transformation.Z.Z * Z;
			X = x;
			Y = y;
			Z = z;
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

		/// <summary>Transforms the Vector based upon the given transform matrix</summary>
		/// <param name="transformMatrix">The matrix by which to transform the Vector</param>
		/// <param name="ignoreW">Whether the W component of the matrix should be ignored</param>
		public void Transform(Matrix4D transformMatrix, bool ignoreW = true)
		{
			double x = (X * transformMatrix.Row0.X) + (Y * transformMatrix.Row1.X) + (Z * transformMatrix.Row2.X);
			double y = (X * transformMatrix.Row0.Y) + (Y * transformMatrix.Row1.Y) + (Z * transformMatrix.Row2.Y);
			double z = (X * transformMatrix.Row0.Z) + (Y * transformMatrix.Row1.Z) + (Z * transformMatrix.Row2.Z);
			if (!ignoreW)
			{
				/*
				 * Multiplying a Vector3 by a Matrix4 is actually mathematically undefined behaviour
				 * Some implementations appear to expect a constant W value to be added to the vector,
				 * whereas others ignore it
				 */
				x += (1 * transformMatrix.Row3.X);
				y += (1 * transformMatrix.Row3.Y);
				z += (1 * transformMatrix.Row3.Z);
			}
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>Transforms a vector by a quaternion rotation.</summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector3 Transform(Vector3 vec, Quaternion quat)
		{
			Vector3 result;
			Transform(ref vec, ref quat, out result);
			return result;
		}

		/// <summary>Transforms a vector by a quaternion rotation.</summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <param name="result">The result of the operation.</param>
		public static void Transform(ref Vector3 vec, ref Quaternion quat, out Vector3 result)
		{
			// Since vec.W == 0, we can optimize quat * vec * quat^-1 as follows:
			// vec + 2.0 * cross(quat.xyz, cross(quat.xyz, vec) + quat.w * vec)
			Vector3 xyz = quat.Xyz, temp, temp2;
			temp = Cross(xyz, vec);
			temp2 = vec * quat.W;
			temp += temp2;
			temp2 = Cross(xyz, temp);
			temp2 *= 2f;
			result = vec + temp2;
		}

		/// <summary>Projects a vector onto a second vector</summary>
		/// <param name="firstVector">The first vector</param>
		/// <param name="secondVector">The second vector</param>
		/// <returns>The projected vector</returns>
		public static Vector3 Project(Vector3 firstVector, Vector3 secondVector)
		{
			double squareMagnitude = Dot(secondVector, secondVector);
			if (squareMagnitude < double.Epsilon)
			{
				return Zero;
			}
			double dot = Dot(firstVector, secondVector);
			return new Vector3(secondVector.X * dot / squareMagnitude, secondVector.Y * dot / squareMagnitude, secondVector.Z * dot / squareMagnitude);
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
		public static readonly Vector3 Zero = new Vector3(0.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing left.</summary>
		public static readonly Vector3 Left = new Vector3(-1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing right.</summary>
		public static readonly Vector3 Right = new Vector3(1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector3 Up = new Vector3(0.0, -1.0, 0.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector3 Down = new Vector3(0.0 , 1.0, 0.0);
		
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
