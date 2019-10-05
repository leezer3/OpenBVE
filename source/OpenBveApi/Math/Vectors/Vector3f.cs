using System;

namespace OpenBveApi.Math {
	/// <summary>Represents a three-dimensional vector.</summary>
	public struct Vector3f {
		
		// --- members ---
		
		/// <summary>The x-coordinate.</summary>
		public float X;
		
		/// <summary>The y-coordinate.</summary>
		public float Y;
		
		/// <summary>The z-coordinate.</summary>
		public float Z;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new three-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="z">The z-coordinate.</param>
		/// <remarks>This discards the double precision</remarks>
		public Vector3f(double x, double y, double z) {
			this.X = (float)x;
			this.Y = (float)y;
			this.Z = (float)z;
		}

		/// <summary>Creates a new three-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="z">The z-coordinate.</param>
		public Vector3f(float x, float y, float z) {
			this.X = x;
			this.Y = y;
			this.Z = z;
		}

		/// <summary>Creates a clone of a vector</summary>
		/// <param name="v">The vector to clone</param>
		public Vector3f(Vector3f v) {
			this.X = v.X;
			this.Y = v.Y;
			this.Z = v.Z;
		}

		/// <summary>Creates a clone of a vector</summary>
		/// <param name="v">The vector to clone</param>
		/// <remarks>This discards the double precision</remarks>
		public Vector3f(Vector3 v) {
			this.X = (float)v.X;
			this.Y = (float)v.Y;
			this.Z = (float)v.Z;
		}

		/// <summary>Converts a Vector3f to a Vector3</summary>
		public static implicit operator Vector3(Vector3f v)
		{
			return new Vector3(v.X, v.Y, v.Z);
		}
		
		/// <summary>Parses a Vector3f stored in a string</summary>
		/// <param name="stringToParse">The string to parse</param>
		/// <param name="separator">The separator character</param>
		/// <param name="v">The out Vector</param>
		/// <returns>True if parsing succeded with no errors, false otherwise</returns>
		/// <remarks>This will always return a Vector3f.
		/// If any part fails parsing, it will be set to zero</remarks>
		public static bool TryParse(string stringToParse, char separator, out Vector3f v)
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
						if (!float.TryParse(splitString[i], out v.X))
						{
							success = false;
						}
						break;
					case 1:
						if (!float.TryParse(splitString[i], out v.Y))
						{
							success = false;
						}
						break;
					case 2:
						if (!float.TryParse(splitString[i], out v.Z))
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

		/// <summary>Interpolates between two Vector3f values using a simple Cosine algorithm</summary>
		/// <param name="Vector1">The first vector</param>
		/// <param name="Vector2">The second vector</param>
		/// <param name="mu">The position on the curve of the new vector</param>
		/// <returns>The interpolated vector</returns>
		public static Vector3f CosineInterpolate(Vector3f Vector1, Vector3f Vector2, double mu)
		{
			double mu2 = (1 - System.Math.Cos(mu * System.Math.PI)) / 2;
			return new Vector3f((Vector1.X * (1 - mu2) + Vector2.X * mu2), (Vector1.Y * (1 - mu2) + Vector2.Y * mu2), (Vector1.Z * (1 - mu2) + Vector2.Z * mu2));
		}

		/// <summary>Linearly interpolates between two vectors</summary>
		/// <param name="Vector1">The first vector</param>
		/// <param name="Vector2">The second vector</param>
		/// <param name="mu">The position on the interpolation curve of the new vector</param>
		/// <returns>The interpolated vector</returns>
		public static Vector3f LinearInterpolate(Vector3f Vector1, Vector3f Vector2, double mu)
		{
			return new Vector3f(Vector1.X + ((Vector2.X - Vector1.X) * mu), Vector1.Y + ((Vector2.Y - Vector1.Y) * mu), Vector1.Z + ((Vector2.Z - Vector1.Z) * mu));
		}
		
		
		// --- arithmetic operators ---
		
		/// <summary>Adds two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The sum of the two vectors.</returns>
		public static Vector3f operator +(Vector3f a, Vector3f b)
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
		public static Vector3f operator +(Vector3f a, double b)
		{
			a.X += (float)b;
			a.Y += (float)b;
			a.Z += (float)b;
			return a;
		}
		
		/// <summary>Adds a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The sum of the scalar and the vector.</returns>
		public static Vector3f operator +(double a, Vector3f b) {
			b.X += (float)a;
			b.Y += (float)a;
			b.Z += (float)a;
			return b;
		}
		
		/// <summary>Subtracts two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The difference of the two vectors.</returns>
		public static Vector3f operator -(Vector3f a, Vector3f b)
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
		public static Vector3f operator -(Vector3f a, double b)
		{
			a.X -= (float)b;
			a.Y -= (float)b;
			a.Z -= (float)b;
			return a;
		}
		
		/// <summary>Subtracts a vector from a scalar.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The difference of the scalar and the vector.</returns>
		public static Vector3f operator -(double a, Vector3f b)
		{
			b.X -= (float)a;
			b.Y -= (float)a;
			b.Z -= (float)a;
			return b;
		}
		
		/// <summary>Negates a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The negation of the vector.</returns>
		public static Vector3f operator -(Vector3f vector)
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
		public static Vector3f operator *(Vector3f a, Vector3f b)
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
		public static Vector3f operator *(Vector3f a, double b)
		{
			a.X *= (float)b;
			a.Y *= (float)b;
			a.Z *= (float)b;
			return a;
		}
		
		/// <summary>Multiplies a scalar and a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The product of the scalar and the vector.</returns>
		public static Vector3f operator *(double a, Vector3f b)
		{
			b.X *= (float)a;
			b.Y *= (float)a;
			b.Z *= (float)a;
			return b;
		}
		
		/// <summary>Divides two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The quotient of the two vectors.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when any member of the second vector is zero.</exception>
		public static Vector3f operator /(Vector3f a, Vector3f b)
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
		public static Vector3f operator /(Vector3f a, double b) {
			if (b == 0.0) {
				throw new DivideByZeroException();
			}
			double factor = 1.0 / b;
			a.X *= (float)factor;
			a.Y *= (float)factor;
			a.Z *= (float)factor;
			return a;
		}
		
		/// <summary>Divides a scalar by a vector.</summary>
		/// <param name="a">The scalar.</param>
		/// <param name="b">The vector.</param>
		/// <returns>The quotient of the scalar and the vector.</returns>
		/// <exception cref="DivideByZeroException">Raised when any member of the vector is zero.</exception>
		public static Vector3f operator /(double a, Vector3f b)
		{
			if (b.X == 0.0 | b.Y == 0.0 | b.Z == 0.0) {
				throw new DivideByZeroException();
			}

			b.X /= (float)a;
			b.Y /= (float)a;
			b.Z /= (float)a;
			return b;
		}

		
		// --- comparisons ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public static bool operator ==(Vector3f a, Vector3f b) {
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
			if (!(obj is Vector3f))
			{
				return false;
			}

			return this.Equals((Vector3f)obj);
		}

		/// <summary>Checks whether the current vector is equal to the specified vector.</summary>
		/// <param name="b">The specified vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public bool Equals(Vector3f b)
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
		public static bool operator !=(Vector3f a, Vector3f b) {
			if (a.X != b.X) return true;
			if (a.Y != b.Y) return true;
			if (a.Z != b.Z) return true;
			return false;
		}
		
		
		// --- instance functions ---
		
		/// <summary>Normalizes the vector.</summary>
		public void Normalize() {
			float norm = this.X * this.X + this.Y * this.Y + this.Z * this.Z;
			if (norm == 0.0)
			{
				return;
			}
			float factor = 1.0f / (float)System.Math.Sqrt(norm);
			this.X *= factor;
			this.Y *= factor;
			this.Z *= factor;
		}
		
		/// <summary>Checks whether the vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public bool IsNullVector() {
			return this.X == 0.0f & this.Y == 0.0f & this.Z == 0.0f;
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
		public static double Dot(Vector3f a, Vector3f b) {
			return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
		}
		
		/// <summary>Gives the cross product of two vectors.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>The cross product of the two vectors.</returns>
		public static Vector3f Cross(Vector3f a, Vector3f b) {
			return new Vector3f(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
		}
		
		/// <summary>Normalizes a vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The normalized vector.</returns>
		/// <exception cref="System.DivideByZeroException">Raised when the vector is a null vector.</exception>
		public static Vector3f Normalize(Vector3f vector) {
			double norm = vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
			if (norm == 0.0) {
				throw new DivideByZeroException();
			} else {
				double factor = 1.0 / System.Math.Sqrt(norm);
				return new Vector3f(vector.X * factor, vector.Y * factor, vector.Z * factor);
			}
		}
		
		/// <summary>Translates a vector by a specified offset.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="offset">The offset.</param>
		/// <returns>The translated vector.</returns>
		public static Vector3f Translate(Vector3f vector, Vector3f offset) {
			double x = vector.X + offset.X;
			double y = vector.Y + offset.Y;
			double z = vector.Z + offset.Z;
			return new Vector3f(x, y, z);
		}
		
		/// <summary>Translates a vector by a specified offset that is measured along a specified orientation.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="orientation">The orientation.</param>
		/// <param name="offset">The offset measured in the specified orientation.</param>
		public static Vector3f Translate(Vector3f vector, Orientation3 orientation, Vector3f offset) {
			double x = vector.X + orientation.X.X * offset.X + orientation.Y.X * offset.Y + orientation.Z.X * offset.Z;
			double y = vector.Y + orientation.X.Y * offset.X + orientation.Y.Y * offset.Y + orientation.Z.Y * offset.Z;
			double z = vector.Z + orientation.X.Z * offset.X + orientation.Y.Z * offset.Y + orientation.Z.Z * offset.Z;
			return new Vector3f(x, y, z);
		}
		
		/// <summary>Scales a vector by a specified factor.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="factor">The factor.</param>
		/// <returns>The scaled vector.</returns>
		public static Vector3f Scale(Vector3f vector, Vector3f factor) {
			double x = vector.X * factor.X;
			double y = vector.Y * factor.Y;
			double z = vector.Z * factor.Z;
			return new Vector3f(x, y, z);
		}
		
		/// <summary>Rotates a vector on the plane perpendicular to a specified direction by a specified angle.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="direction">The direction perpendicular to the plane on which to rotate.</param>
		/// <param name="cosineOfAngle">The cosine of the angle.</param>
		/// <param name="sineOfAngle">The sine of the angle.</param>
		/// <returns>The rotated vector.</returns>
		public static Vector3f Rotate(Vector3f vector, Vector3f direction, double cosineOfAngle, double sineOfAngle) {
			double cosineComplement = 1.0 - cosineOfAngle;
			double x = (cosineOfAngle + cosineComplement * direction.X * direction.X) * vector.X + (cosineComplement * direction.X * direction.Y - sineOfAngle * direction.Z) * vector.Y + (cosineComplement * direction.X * direction.Z + sineOfAngle * direction.Y) * vector.Z;
			double y = (cosineOfAngle + cosineComplement * direction.Y * direction.Y) * vector.Y + (cosineComplement * direction.X * direction.Y + sineOfAngle * direction.Z) * vector.X + (cosineComplement * direction.Y * direction.Z - sineOfAngle * direction.X) * vector.Z;
			double z = (cosineOfAngle + cosineComplement * direction.Z * direction.Z) * vector.Z + (cosineComplement * direction.X * direction.Z - sineOfAngle * direction.Y) * vector.X + (cosineComplement * direction.Y * direction.Z + sineOfAngle * direction.X) * vector.Y;
			return new Vector3f(x, y, z);
		}
		
		/// <summary>Rotates a vector from the default orientation into a specified orientation.</summary>
		/// <param name="vector">The vector.</param>
		/// <param name="orientation">The orientation.</param>
		/// <returns>The rotated vector.</returns>
		/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
		public static Vector3f Rotate(Vector3f vector, Orientation3 orientation) {
			double x = orientation.X.X * vector.X + orientation.Y.X * vector.Y + orientation.Z.X * vector.Z;
			double y = orientation.X.Y * vector.X + orientation.Y.Y * vector.Y + orientation.Z.Y * vector.Z;
			double z = orientation.X.Z * vector.X + orientation.Y.Z * vector.Y + orientation.Z.Z * vector.Z;
			return new Vector3f(x, y, z);
		}


		
		/// <summary>Creates a unit vector perpendicular to the plane described by three spatial coordinates, suitable for being a surface normal.</summary>
		/// <param name="a">The first spatial coordinate.</param>
		/// <param name="b">The second spatial coordinate.</param>
		/// <param name="c">The third spatial coordinate.</param>
		/// <param name="normal">On success, receives the vector perpendicular to the described plane. On failure, receives Vector3f.Up.</param>
		/// <returns>The success of the operation. This operation fails if the specified three vectors are colinear.</returns>
		public static bool CreateNormal(Vector3f a, Vector3f b, Vector3f c, out Vector3f normal) {
			normal = Vector3f.Cross(b - a, c - a);
			double norm = normal.X * normal.X + normal.Y * normal.Y + normal.Z * normal.Z;
			if (norm != 0.0) {
				normal *= 1.0 / System.Math.Sqrt(norm);
				return true;
			} else {
				normal = Vector3f.Up;
				return false;
			}
		}
		
		/// <summary>Checks whether three spatial coordinates are colinear.</summary>
		/// <param name="a">The first spatial coordinate.</param>
		/// <param name="b">The second spatial coordinate.</param>
		/// <param name="c">The third spatial coordinate.</param>
		/// <returns>A boolean indicating whether the three spatial coordinates are colinear.</returns>
		public static bool AreColinear(Vector3f a, Vector3f b, Vector3f c) {
			Vector3f normal = Vector3f.Cross(b - a, c - a);
			return IsNullVector(normal);
		}
		
		/// <summary>Checks whether a vector is a null vector.</summary>
		/// <returns>A boolean indicating whether the vector is a null vector.</returns>
		public static bool IsNullVector(Vector3f vector) {
			return vector.X == 0.0 & vector.Y == 0.0 & vector.Z == 0.0;
		}
		
		/// <summary>Gets the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The euclidean norm.</returns>
		public static double Norm(Vector3f vector) {
			return System.Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		}
		
		/// <summary>Gets the square of the euclidean norm of the specified vector.</summary>
		/// <param name="vector">The vector.</param>
		/// <returns>The square of the euclidean norm.</returns>
		public static double NormSquared(Vector3f vector) {
			return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
		}

		/// <summary>Returns a normalized vector based on a 2D vector in the XZ plane and an additional Y-coordinate.</summary>
		/// <param name="Vector">The vector in the XZ-plane. The X and Y components in Vector represent the X- and Z-coordinates, respectively.</param>
		/// <param name="Y">The Y-coordinate.</param>
		public static Vector3f GetVector3f(Vector2 Vector, double Y)
		{
			double t = 1.0 / System.Math.Sqrt(Vector.X * Vector.X + Vector.Y * Vector.Y + Y * Y);
			return new Vector3f(t * Vector.X, t * Y, t * Vector.Y);
		}


		/// <summary>Determines whether this is a zero (0,0,0) vector</summary>
		/// <param name="Vector"></param>
		/// <returns>True if this is a zero vector, false otherwise</returns>
		public static bool IsZero(Vector3f Vector)
		{
			if (Vector.X != 0.0f) return false;
			if (Vector.Y != 0.0f) return false;
			if (Vector.Z != 0.0f) return false;
			return true;
		}
		
		// --- read-only fields ---
		
		/// <summary>Represents a null vector.</summary>
		public static readonly Vector3f Zero = new Vector3f(0.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing left.</summary>
		public static readonly Vector3f Left = new Vector3f(-1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing right.</summary>
		public static readonly Vector3f Right = new Vector3f(1.0, 0.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector3f Up = new Vector3f(0.0, -1.0, 0.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector3f Down = new Vector3f(0.0 , 1.0, 0.0);
		
		/// <summary>Represents a vector pointing up.</summary>
		public static readonly Vector3f Backward = new Vector3f(0.0, 0.0, -1.0);
		
		/// <summary>Represents a vector pointing down.</summary>
		public static readonly Vector3f Forward = new Vector3f(0.0, 0.0, 1.0);

		/// <summary>Returns the representation of the vector in string format</summary>
		public override string ToString()
		{
			string toString = this.X + " , " + this.Y + " , " + this.Z;
			return toString;
		}
	}
}
