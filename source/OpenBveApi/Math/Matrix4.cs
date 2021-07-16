/*
Some Matrix math code derived from OpenTK-

Copyright (c) 2006 - 2008 The Open Toolkit library.
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenBveApi.Math
{
	/// <summary>Represents a 4x4 double-precision row-major matrix</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Matrix4D
	{
		/// <summary>The top row of the matrix</summary>
		public Vector4 Row0;

		/// <summary>The first row of the matrix</summary>
		public Vector4 Row1;

		/// <summary>The second row of the matrix</summary>
		public Vector4 Row2;

		/// <summary>The third row of the matrix</summary>
		public Vector4 Row3;

		/// <summary>The first column of the matrix</summary>
		public Vector4 Column0
		{
			get
			{
				return new Vector4(Row0.X, Row1.X, Row2.X, Row3.X);
			}
			set
			{
				Row0.X = value.X;
				Row1.X = value.Y;
				Row2.X = value.Z;
				Row3.X = value.W;
			}
		}

		/// <summary>The second column of the matrix</summary>
		public Vector4 Column1
		{
			get
			{
				return new Vector4(Row0.Y, Row1.Y, Row2.Y, Row3.Y);
			}
			set
			{
				Row0.Y = value.X;
				Row1.Y = value.Y;
				Row2.Y = value.Z;
				Row3.Y = value.W;
			}
		}

		/// <summary>The third column of the matrix</summary>
		public Vector4 Column2
		{
			get
			{
				return new Vector4(Row0.Z, Row1.Z, Row2.Z, Row3.Z);
			}
			set
			{
				Row0.Z = value.X;
				Row1.Z = value.Y;
				Row2.Z = value.Z;
				Row3.Z = value.W;
			}
		}

		/// <summary>The fourth column of the matrix</summary>
		public Vector4 Column3
		{
			get
			{
				return new Vector4(Row0.W, Row1.W, Row2.W, Row3.W);
			}
			set
			{
				Row0.W = value.X;
				Row1.W = value.Y;
				Row2.W = value.Z;
				Row3.W = value.W;
			}
		}

		/// <summary>Creates a new Matrix4D</summary>
		/// <param name="topRow"></param>
		/// <param name="row1"></param>
		/// <param name="row2"></param>
		/// <param name="bottomRow"></param>
		public Matrix4D(Vector4 topRow, Vector4 row1, Vector4 row2, Vector4 bottomRow)
		{
			this.Row0 = topRow;
			this.Row1 = row1;
			this.Row2 = row2;
			this.Row3 = bottomRow;
		}

		/// <summary>Creates a new Matrix4D</summary>
		public Matrix4D(double[] matrixValues)
		{
			if (matrixValues.Length != 16)
			{
				throw new InvalidDataException("matrixValues must contain exactly 16 doubles");
			}

			Row0 = new Vector4(matrixValues[0], matrixValues[1], matrixValues[2], matrixValues[3]);
			Row1 = new Vector4(matrixValues[4], matrixValues[5], matrixValues[6], matrixValues[7]);
			Row2 = new Vector4(matrixValues[8], matrixValues[9], matrixValues[10], matrixValues[11]);
			Row3 = new Vector4(matrixValues[12], matrixValues[13], matrixValues[14], matrixValues[15]);
		}

		/// <summary>Creates a clone of a matrix</summary>
		/// <param name="matrix">The matrix to clone</param>
		public Matrix4D(Matrix4D matrix)
		{
			Row0 = new Vector4(matrix.Row0);
			Row1 = new Vector4(matrix.Row1);
			Row2 = new Vector4(matrix.Row2);
			Row3 = new Vector4(matrix.Row3);
		}

		// --- comparisons ---

		/// <summary>Checks whether the two specified matrices are equal.</summary>
		/// <param name="a">The first matrix.</param>
		/// <param name="b">The second matrix.</param>
		/// <returns>Whether the two matricies are equal.</returns>
		public static bool operator ==(Matrix4D a, Matrix4D b)
		{
			if (a.Row0 != b.Row0) return false;
			if (a.Row1 != b.Row1) return false;
			if (a.Row2 != b.Row2) return false;
			if (a.Row3 != b.Row3) return false;
			return true;
		}

		/// <summary>Returns the hashcode for this instance.</summary>
		/// <returns>An integer representing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = this.Row0.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Row1.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Row2.GetHashCode();
				hashCode = (hashCode * 397) ^ this.Row3.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Matrix4D))
			{
				return false;
			}

			return this.Equals((Matrix4D) obj);
		}

		/// <summary>Checks whether the current vector is equal to the specified vector.</summary>
		/// <param name="b">The specified vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public bool Equals(Matrix4D b)
		{
			if (this.Row0 != b.Row0) return false;
			if (this.Row1 != b.Row1) return false;
			if (this.Row2 != b.Row2) return false;
			if (this.Row3 != b.Row3) return false;
			return true;
		}

		/// <summary>Checks whether the two specified vectors are unequal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are unequal.</returns>
		public static bool operator !=(Matrix4D a, Matrix4D b)
		{
			if (a.Row0 != b.Row0) return true;
			if (a.Row1 != b.Row1) return true;
			if (a.Row2 != b.Row2) return true;
			if (a.Row3 != b.Row3) return true;
			return false;
		}

		/// <summary>Builds a camera-space to world-space matrix</summary>
		/// <param name="eyePosition">The eye position</param>
		/// <param name="targetPosition">The target position in world-space</param>
		/// <param name="Up">The world-space up vector</param>
		/// <returns>The matrix which transforms camera space to world-space</returns>
		public static Matrix4D LookAt(Vector3 eyePosition, Vector3 targetPosition, Vector3 Up)
		{
			Vector3 z;
			if (eyePosition == Vector3.Zero && targetPosition == Vector3.Zero)
			{
				z = Vector3.Zero;
			}
			else
			{
				z = Vector3.Normalize(eyePosition - targetPosition);
			}

			Vector3 x = Vector3.Cross(Up, z);
			Vector3 y = Vector3.Cross(z, x);


			Matrix4D rotationMatrix = new Matrix4D
			{
				Row0 = new Vector4(x.X, y.X, z.X, 0.0),
				Row1 = new Vector4(x.Y, y.Y, z.Y, 0.0),
				Row2 = new Vector4(x.Z, y.Z, z.Z, 0.0),
				Row3 = Vector4.UnitW
			};
			Matrix4D translationMatrix = NoTransformation;
			translationMatrix.Row3 = new Vector4(-eyePosition.X, -eyePosition.Y, -eyePosition.Z, 1.0);
			return translationMatrix * rotationMatrix;
		}

		/// <summary>Creates a perspective matrix</summary>
		/// <param name="fieldOfViewY">The field of view Y angle in radians</param>
		/// <param name="aspectRatio">The aspect ratio</param>
		/// <param name="zNear">Distance to the near clip plane</param>
		/// <param name="zFar">Distance to the far clip plane</param>
		/// <returns></returns>
		public static Matrix4D Perspective(double fieldOfViewY, double aspectRatio, double zNear, double zFar)
		{
			double yMax = zNear * System.Math.Tan(fieldOfViewY * System.Math.PI / 6.28319); //360 degrees in radians
			double yMin = -yMax;
			double xMin = yMin * aspectRatio;
			double xMax = yMax * aspectRatio;

			return Frustum(xMin, xMax, yMin, yMax, zNear, zFar);
		}

		/// <summary>Creates a matrix which transforms camera space to the viewing frustrum</summary>
		/// <param name="left">The left side of the frustrum</param>
		/// <param name="right">The right side of the frustrum</param>
		/// <param name="bottom">The bottom of the frustrum</param>
		/// <param name="top">The top of the frustrum</param>
		/// <param name="zNear">Distance to the near clip plane</param>
		/// <param name="zFar">Distance to the far clip plane</param>
		/// <returns></returns>
		public static Matrix4D Frustum(double left, double right, double bottom, double top, double zNear, double zFar)
		{
			double inverseRightLeft = 1.0 / (right - left);
			double inverseTopBottom = 1.0 / (top - bottom);
			double inverseFarNear = 1.0 / (zFar - zNear);
			return new Matrix4D(new Vector4(2.0 * zNear * inverseRightLeft, 0.0, 0.0, 0.0),
				new Vector4(0.0, 2.0 * zNear * inverseTopBottom, 0.0, 0.0),
				new Vector4((right + left) * inverseRightLeft, (top + bottom) * inverseTopBottom, -(zFar + zNear) * inverseFarNear, -1.0),
				new Vector4(0.0, 0.0, -2.0 * zFar * zNear * inverseFarNear, 0.0));
		}

		/// <summary>Creates a perspective projection matrix</summary>
		/// <param name="fieldOfViewY">The field of view Y angle in radians</param>
		/// <param name="aspectRatio">The aspect ratio</param>
		/// <param name="zNear">Distance to the near clip plane</param>
		/// <param name="zFar">Distance to the far clip plane</param>
		/// <returns>The perspective projection matrix</returns>
		public static Matrix4D CreatePerspectiveFieldOfView(double fieldOfViewY, double aspectRatio, double zNear, double zFar)
		{
			//https://www.cs.rit.edu/usr/local/pub/wrc/graphics/doc/opengl/books/blue/gluPerspective.html
			if (fieldOfViewY <= 0 || fieldOfViewY > System.Math.PI)
			{
				throw new ArgumentOutOfRangeException("fieldOfViewY", "fieldOfViewY must be positive and less than Pi");
			}

			if (aspectRatio <= 0)
			{
				throw new ArgumentOutOfRangeException("aspectRatio", "aspectRatio must be positive");
			}

			if (zNear <= 0)
			{
				throw new ArgumentOutOfRangeException("zNear", "zNear must be positive");
			}

			if (zFar <= 0)
			{
				throw new ArgumentOutOfRangeException("zFar", "zFar must be positive");
			}

			double yMax = zNear * System.Math.Tan(fieldOfViewY * System.Math.PI / 6.28319); //360 degrees in radians
			double yMin = -yMax;
			double xMin = yMin * aspectRatio;
			double xMax = yMax * aspectRatio;

			return CreatePerspectiveOffCenter(xMin, xMax, yMin, yMax, zNear, zFar);
		}

		/// <summary>Creates a perspective projection matrix, assuming that the eye is located at 0,0,0</summary>
		/// <param name="left">The left side of the frustrum</param>
		/// <param name="right">The right side of the frustrum</param>
		/// <param name="bottom">The bottom of the frustrum</param>
		/// <param name="top">The top of the frustrum</param>
		/// <param name="zNear">Distance to the near clip plane</param>
		/// <param name="zFar">Distance to the far clip plane</param>
		/// <returns>The perspective projection matrix</returns>
		public static Matrix4D CreatePerspectiveOffCenter(double left, double right, double bottom, double top, double zNear, double zFar)
		{
			//https://www.cs.rit.edu/usr/local/pub/wrc/graphics/doc/opengl/books/blue/glFrustum.html
			if (zNear <= 0)
			{
				throw new ArgumentOutOfRangeException("zNear", "zNear must be positive");
			}

			if (zFar <= 0)
			{
				throw new ArgumentOutOfRangeException("zFar", "zFar must be positive");
			}

			if (zNear >= zFar)
			{
				throw new ArgumentOutOfRangeException("zNear", "zNear must be less than or equal to zFar");
			}

			double x = (2.0 * zNear) / (right - left);
			double y = (2.0 * zNear) / (top - bottom);
			double a = (right + left) / (right - left);
			double b = (top + bottom) / (top - bottom);
			double c = -(zFar + zNear) / (zFar - zNear);
			double d = -(2.0 * zFar * zNear) / (zFar - zNear);
			//Remember that we assume the eye is located at 0,0,0
			return new Matrix4D(new Vector4(x, 0, 0, 0), new Vector4(0, y, 0, 0), new Vector4(a, b, c, -1), new Vector4(0, 0, d, 0));
		}

		/// <summary>Multiplies two matrices</summary>
		/// <param name="left">The left matrix</param>
		/// <param name="right">The right matrix</param>
		/// <returns>The new multiplied matrix</returns>
		public static Matrix4D operator *(Matrix4D left, Matrix4D right)
		{
			Matrix4D result = new Matrix4D();
			result.Row0.X = (((left.Row0.X * right.Row0.X) + (left.Row0.Y * right.Row1.X)) + (left.Row0.Z * right.Row2.X)) + (left.Row0.W * right.Row3.X);
			result.Row0.Y = (((left.Row0.X * right.Row0.Y) + (left.Row0.Y * right.Row1.Y)) + (left.Row0.Z * right.Row2.Y)) + (left.Row0.W * right.Row3.Y);
			result.Row0.Z = (((left.Row0.X * right.Row0.Z) + (left.Row0.Y * right.Row1.Z)) + (left.Row0.Z * right.Row2.Z)) + (left.Row0.W * right.Row3.Z);
			result.Row0.W = (((left.Row0.X * right.Row0.W) + (left.Row0.Y * right.Row1.W)) + (left.Row0.Z * right.Row2.W)) + (left.Row0.W * right.Row3.W);
			result.Row1.X = (((left.Row1.X * right.Row0.X) + (left.Row1.Y * right.Row1.X)) + (left.Row1.Z * right.Row2.X)) + (left.Row1.W * right.Row3.X);
			result.Row1.Y = (((left.Row1.X * right.Row0.Y) + (left.Row1.Y * right.Row1.Y)) + (left.Row1.Z * right.Row2.Y)) + (left.Row1.W * right.Row3.Y);
			result.Row1.Z = (((left.Row1.X * right.Row0.Z) + (left.Row1.Y * right.Row1.Z)) + (left.Row1.Z * right.Row2.Z)) + (left.Row1.W * right.Row3.Z);
			result.Row1.W = (((left.Row1.X * right.Row0.W) + (left.Row1.Y * right.Row1.W)) + (left.Row1.Z * right.Row2.W)) + (left.Row1.W * right.Row3.W);
			result.Row2.X = (((left.Row2.X * right.Row0.X) + (left.Row2.Y * right.Row1.X)) + (left.Row2.Z * right.Row2.X)) + (left.Row2.W * right.Row3.X);
			result.Row2.Y = (((left.Row2.X * right.Row0.Y) + (left.Row2.Y * right.Row1.Y)) + (left.Row2.Z * right.Row2.Y)) + (left.Row2.W * right.Row3.Y);
			result.Row2.Z = (((left.Row2.X * right.Row0.Z) + (left.Row2.Y * right.Row1.Z)) + (left.Row2.Z * right.Row2.Z)) + (left.Row2.W * right.Row3.Z);
			result.Row2.W = (((left.Row2.X * right.Row0.W) + (left.Row2.Y * right.Row1.W)) + (left.Row2.Z * right.Row2.W)) + (left.Row2.W * right.Row3.W);
			result.Row3.X = (((left.Row3.X * right.Row0.X) + (left.Row3.Y * right.Row1.X)) + (left.Row3.Z * right.Row2.X)) + (left.Row3.W * right.Row3.X);
			result.Row3.Y = (((left.Row3.X * right.Row0.Y) + (left.Row3.Y * right.Row1.Y)) + (left.Row3.Z * right.Row2.Y)) + (left.Row3.W * right.Row3.Y);
			result.Row3.Z = (((left.Row3.X * right.Row0.Z) + (left.Row3.Y * right.Row1.Z)) + (left.Row3.Z * right.Row2.Z)) + (left.Row3.W * right.Row3.Z);
			result.Row3.W = (((left.Row3.X * right.Row0.W) + (left.Row3.Y * right.Row1.W)) + (left.Row3.Z * right.Row2.W)) + (left.Row3.W * right.Row3.W);
			return result;
		}

		/// <summary>Creates a translation matrix.</summary>
		/// <param name="x">X translation.</param>
		/// <param name="y">Y translation.</param>
		/// <param name="z">Z translation.</param>
		/// <param name="result">The resulting Matrix4.</param>
		public static void CreateTranslation(double x, double y, double z, out Matrix4D result)
		{
			result = Identity;
			result.Row3 = new Vector4(x, y, z, 1);
		}

		/// <summary>Creates a translation matrix.</summary>
		/// <param name="vector">The translation vector.</param>
		/// <param name="result">The resulting Matrix4.</param>
		public static void CreateTranslation(ref Vector3 vector, out Matrix4D result)
		{
			result = Identity;
			result.Row3 = new Vector4(vector.X, vector.Y, vector.Z, 1);
		}

		/// <summary>Creates a translation matrix.</summary>
		/// <param name="x">X translation.</param>
		/// <param name="y">Y translation.</param>
		/// <param name="z">Z translation.</param>
		/// <returns>The resulting Matrix4.</returns>
		public static Matrix4D CreateTranslation(double x, double y, double z)
		{
			Matrix4D result;
			CreateTranslation(x, y, z, out result);
			return result;
		}

		/// <summary>Creates a translation matrix.</summary>
		/// <param name="vector">The translation vector.</param>
		/// <returns>The resulting Matrix4.</returns>
		public static Matrix4D CreateTranslation(Vector3 vector)
		{
			Matrix4D result;
			CreateTranslation(vector.X, vector.Y, vector.Z, out result);
			return result;
		}

		/// <summary>Returns the translation component of this Matrix4.</summary>
		public Vector3 ExtractTranslation()
		{
			return new Vector3(Row3.X, Row3.Y, Row3.Z);
		}

		/// <summary>Build a rotation matrix from the specified axis/angle rotation.</summary>
		/// <param name="axis">The axis to rotate about.</param>
		/// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
		/// <param name="result">A matrix instance.</param>
		public static void CreateFromAxisAngle(Vector3 axis, double angle, out Matrix4D result)
		{
			axis.Normalize();
			Vector3 cloneAxis = new Vector3(axis);
			double cos = System.Math.Cos(-angle);
			double sin = System.Math.Sin(-angle);
			double t = 1.0f - cos;
			double tXX = t * cloneAxis.X * cloneAxis.X,
			tXY = t * cloneAxis.X * cloneAxis.Y,
			tXZ = t * cloneAxis.X * cloneAxis.Z,
			tYY = t * cloneAxis.Y * cloneAxis.Y,
			tYZ = t * cloneAxis.Y * cloneAxis.Z,
			tZZ = t * cloneAxis.Z * cloneAxis.Z;

			double sinX = sin * cloneAxis.X,
			sinY = sin * cloneAxis.Y,
			sinZ = sin * cloneAxis.Z;

			result.Row0.X = tXX + cos;
			result.Row0.Y = tXY - sinZ;
			result.Row0.Z = tXZ + sinY;
			result.Row0.W = 0;
			result.Row1.X = tXY + sinZ;
			result.Row1.Y = tYY + cos;
			result.Row1.Z = tYZ - sinX;
			result.Row1.W = 0;
			result.Row2.X = tXZ - sinY;
			result.Row2.Y = tYZ + sinX;
			result.Row2.Z = tZZ + cos;
			result.Row2.W = 0;
			result.Row3 = Vector4.UnitW;
		}

		/// <summary>Build a rotation matrix from the specified axis/angle rotation.</summary>
		/// <param name="axis">The axis to rotate about.</param>
		/// <param name="angle">Angle in radians to rotate counter-clockwise (looking in the direction of the given axis).</param>
		/// <returns>A matrix instance.</returns>
		public static Matrix4D CreateFromAxisAngle(Vector3 axis, double angle)
		{
			Matrix4D result;
			CreateFromAxisAngle(axis, angle, out result);
			return result;
		}

		/// <summary>
		/// Build a rotation matrix from the specified quaternion.
		/// </summary>
		/// <param name="q">Quaternion to translate.</param>
		/// <param name="result">Matrix result.</param>
		public static void CreateFromQuaternion(ref Quaternion q, out Matrix4D result)
		{
			Vector3 axis;
			double angle;
			q.ToAxisAngle(out axis, out angle);
			CreateFromAxisAngle(axis, angle, out result);
		}

		/// <summary>
		/// Builds a rotation matrix from a quaternion.
		/// </summary>
		/// <param name="q">The quaternion to rotate by.</param>
		/// <returns>A matrix instance.</returns>
		public static Matrix4D CreateFromQuaternion(Quaternion q)
		{
			Matrix4D result;
			CreateFromQuaternion(ref q, out result);
			return result;
		}

		/// <summary>
		/// Calculate the transpose of the given matrix
		/// </summary>
		/// <param name="mat">The matrix to transpose</param>
		/// <returns>The transpose of the given matrix</returns>
		public static Matrix4D Transpose(Matrix4D mat)
		{
			return new Matrix4D(mat.Column0, mat.Column1, mat.Column2, mat.Column3);
		}


		/// <summary>
		/// Calculate the transpose of the given matrix
		/// </summary>
		/// <param name="mat">The matrix to transpose</param>
		/// <param name="result">The result of the calculation</param>
		public static void Transpose(ref Matrix4D mat, out Matrix4D result)
		{
			result.Row0 = mat.Column0;
			result.Row1 = mat.Column1;
			result.Row2 = mat.Column2;
			result.Row3 = mat.Column3;
		}


		/// <summary>
		/// The determinant of this matrix
		/// </summary>
		public double Determinant
		{
			get
			{
				return
					Row0.X * Row1.Y * Row2.Z * Row3.W - Row0.X * Row1.Y * Row2.W * Row3.Z + Row0.X * Row1.Z * Row2.W * Row3.Y - Row0.X * Row1.Z * Row2.Y * Row3.W
					+ Row0.X * Row1.W * Row2.Y * Row3.Z - Row0.X * Row1.W * Row2.Z * Row3.Y - Row0.Y * Row1.Z * Row2.W * Row3.X + Row0.Y * Row1.Z * Row2.X * Row3.W
					- Row0.Y * Row1.W * Row2.X * Row3.Z + Row0.Y * Row1.W * Row2.Z * Row3.X - Row0.Y * Row1.X * Row2.Z * Row3.W + Row0.Y * Row1.X * Row2.W * Row3.Z
					                                                                                                            + Row0.Z * Row1.W * Row2.X * Row3.Y - Row0.Z * Row1.W * Row2.Y * Row3.X + Row0.Z * Row1.X * Row2.Y * Row3.W - Row0.Z * Row1.X * Row2.W * Row3.Y
					+ Row0.Z * Row1.Y * Row2.W * Row3.X - Row0.Z * Row1.Y * Row2.X * Row3.W - Row0.W * Row1.X * Row2.Y * Row3.Z + Row0.W * Row1.X * Row2.Z * Row3.Y
					- Row0.W * Row1.Y * Row2.Z * Row3.X + Row0.W * Row1.Y * Row2.X * Row3.Z - Row0.W * Row1.Z * Row2.X * Row3.Y + Row0.W * Row1.Z * Row2.Y * Row3.X;
			}
		}
		
        /// <summary>
        /// Calculate the inverse of the given matrix
        /// </summary>
        /// <param name="mat">The matrix to invert</param>
        /// <returns>The inverse of the given matrix if it has one, or the input if it is singular</returns>
        /// <exception cref="InvalidOperationException">Thrown if the Matrix4d is singular.</exception>
        public static Matrix4D Invert(Matrix4D mat)
        {
            
            if (mat.Determinant != 0)
            {
	            Matrix4D result = new Matrix4D ();
	            mat.Invert(ref result);
	            return result;
            }
            return mat;
        }

        /// <summary>
        /// Inverts a matrix
        /// </summary>
        public void Invert(ref Matrix4D result)
        {
            double m41 = Row3.X, m42 = Row3.Y, m43 = Row3.Z, m44 = Row3.W;
            if (m41 == 0 && m42 == 0 && m43 == 0 && m44 == 1.0f) {
                InvertAffine(ref result);
                return;
            }

            double d = Determinant;
            if (d == 0.0f)
                throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            double d1 = 1 / d;
            double m11 = Row0.X, m12 = Row0.Y, m13 = Row0.Z, m14 = Row0.W,
            m21 = Row1.X, m22 = Row1.Y, m23 = Row1.Z, m24 = Row1.W,
            m31 = Row2.X, m32 = Row2.Y, m33 = Row2.Z, m34 = Row2.W;

            result.Row0.X = d1 * (m22 * m33 * m44 + m23 * m34 * m42 + m24 * m32 * m43 - m22 * m34 * m43 - m23 * m32 * m44 - m24 * m33 * m42);
            result.Row0.Y = d1 * (m12 * m34 * m43 + m13 * m32 * m44 + m14 * m33 * m42 - m12 * m33 * m44 - m13 * m34 * m42 - m14 * m32 * m43);
            result.Row0.Z = d1 * (m12 * m23 * m44 + m13 * m24 * m42 + m14 * m22 * m43 - m12 * m24 * m43 - m13 * m22 * m44 - m14 * m23 * m42);
            result.Row0.W = d1 * (m12 * m24 * m33 + m13 * m22 * m34 + m14 * m23 * m32 - m12 * m23 * m34 - m13 * m24 * m32 - m14 * m22 * m33);
            result.Row1.X = d1 * (m21 * m34 * m43 + m23 * m31 * m44 + m24 * m33 * m41 - m21 * m33 * m44 - m23 * m34 * m41 - m24 * m31 * m43);
            result.Row1.Y = d1 * (m11 * m33 * m44 + m13 * m34 * m41 + m14 * m31 * m43 - m11 * m34 * m43 - m13 * m31 * m44 - m14 * m33 * m41);
            result.Row1.Z = d1 * (m11 * m24 * m43 + m13 * m21 * m44 + m14 * m23 * m41 - m11 * m23 * m44 - m13 * m24 * m41 - m14 * m21 * m43);
            result.Row1.W = d1 * (m11 * m23 * m34 + m13 * m24 * m31 + m14 * m21 * m33 - m11 * m24 * m33 - m13 * m21 * m34 - m14 * m23 * m31);
            result.Row2.X = d1 * (m21 * m32 * m44 + m22 * m34 * m41 + m24 * m31 * m42 - m21 * m34 * m42 - m22 * m31 * m44 - m24 * m32 * m41);
            result.Row2.Y = d1 * (m11 * m34 * m42 + m12 * m31 * m44 + m14 * m32 * m41 - m11 * m32 * m44 - m12 * m34 * m41 - m14 * m31 * m42);
            result.Row2.Z = d1 * (m11 * m22 * m44 + m12 * m24 * m41 + m14 * m21 * m42 - m11 * m24 * m42 - m12 * m21 * m44 - m14 * m22 * m41);
            result.Row2.W = d1 * (m11 * m24 * m32 + m12 * m21 * m34 + m14 * m22 * m31 - m11 * m22 * m34 - m12 * m24 * m31 - m14 * m21 * m32);
            result.Row3.X = d1 * (m21 * m33 * m42 + m22 * m31 * m43 + m23 * m32 * m41 - m21 * m32 * m43 - m22 * m33 * m41 - m23 * m31 * m42);
            result.Row3.Y = d1 * (m11 * m32 * m43 + m12 * m33 * m41 + m13 * m31 * m42 - m11 * m33 * m42 - m12 * m31 * m43 - m13 * m32 * m41);
            result.Row3.Z = d1 * (m11 * m23 * m42 + m12 * m21 * m43 + m13 * m22 * m41 - m11 * m22 * m43 - m12 * m23 * m41 - m13 * m21 * m42);
            result.Row3.W = d1 * (m11 * m22 * m33 + m12 * m23 * m31 + m13 * m21 * m32 - m11 * m23 * m32 - m12 * m21 * m33 - m13 * m22 * m31);
        }

        void InvertAffine(ref Matrix4D result)
        {
            double m11 = Row0.X, m12 = Row0.Y, m13 = Row0.Z, m14 = Row0.W,
            m21 = Row1.X, m22 = Row1.Y, m23 = Row1.Z, m24 = Row1.W,
            m31 = Row2.X, m32 = Row2.Y, m33 = Row2.Z, m34 = Row2.W;

            double d = m11 * m22 * m33 + m21 * m32 * m13 + m31 * m12 * m23 -
                    m11 * m32 * m23 - m31 * m22 * m13 - m21 * m12 * m33;

            if (d == 0.0f)
                    throw new InvalidOperationException("Matrix is singular and cannot be inverted.");

            double d1 = 1 / d;

            // sub 3x3 inv
            result.Row0.X = d1 * (m22 * m33 - m23 * m32);
            result.Row0.Y = d1 * (m13 * m32 - m12 * m33);
            result.Row0.Z = d1 * (m12 * m23 - m13 * m22);
            result.Row1.X = d1 * (m23 * m31 - m21 * m33);
            result.Row1.Y = d1 * (m11 * m33 - m13 * m31);
            result.Row1.Z = d1 * (m13 * m21 - m11 * m23);
            result.Row2.X = d1 * (m21 * m32 - m22 * m31);
            result.Row2.Y = d1 * (m12 * m31 - m11 * m32);
            result.Row2.Z = d1 * (m11 * m22 - m12 * m21);

            // - sub 3x3 inv * b
            result.Row0.W = - result.Row0.X * m14 - result.Row0.Y * m24 - result.Row0.Z * m34;
            result.Row1.W = - result.Row1.X * m14 - result.Row1.Y * m24 - result.Row1.Z * m34;
            result.Row2.W = - result.Row2.X * m14 - result.Row2.Y * m24 - result.Row2.Z * m34;

            // last row remains 0 0 0 1
            result.Row3.X = result.Row3.Y = result.Row3.Z = 0.0f;
            result.Row3.W = 1.0f;
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Single scale factor for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4D Scale(double scale)
        {
	        return Scale(scale, scale, scale);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="scale">Scale factors for x,y and z axes</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4D Scale(Vector3 scale)
        {
	        return Scale(scale.X, scale.Y, scale.Z);
        }

        /// <summary>
        /// Build a scaling matrix
        /// </summary>
        /// <param name="x">Scale factor for x-axis</param>
        /// <param name="y">Scale factor for y-axis</param>
        /// <param name="z">Scale factor for z-axis</param>
        /// <returns>A scaling matrix</returns>
        public static Matrix4D Scale(double x, double y, double z)
        {
	        Matrix4D result;
	        result.Row0 = Vector4.UnitX * x;
	        result.Row1 = Vector4.UnitY * y;
	        result.Row2 = Vector4.UnitZ * z;
	        result.Row3 = Vector4.UnitW;
	        return result;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="width">The width of the projection volume.</param>
        /// <param name="height">The height of the projection volume.</param>
        /// <param name="depthNear">The near edge of the projection volume.</param>
        /// <param name="depthFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4d instance.</param>
        public static void CreateOrthographic(double width, double height, double depthNear, double depthFar, out Matrix4D result)
        {
	        CreateOrthographicOffCenter(-width / 2, width / 2, -height / 2, height / 2, depthNear, depthFar, out result);
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        public static void CreateOrthographicOffCenter(double left, double right, double bottom, double top, double zNear, double zFar, out Matrix4D result)
        {
	        result = new Matrix4D();

	        double invRL = 1 / (right - left);
	        double invTB = 1 / (top - bottom);
	        double invFN = 1 / (zFar - zNear);

	        result.Row0.X = 2 * invRL;
	        result.Row1.Y = 2 * invTB;
	        result.Row2.Z = -2 * invFN;

	        result.Row3.X = -(right + left) * invRL;
	        result.Row3.Y = -(top + bottom) * invTB;
	        result.Row3.Z = -(zFar + zNear) * invFN;
	        result.Row3.W = 1;
        }

        /// <summary>
        /// Creates an orthographic projection matrix.
        /// </summary>
        /// <param name="left">The left edge of the projection volume.</param>
        /// <param name="right">The right edge of the projection volume.</param>
        /// <param name="bottom">The bottom edge of the projection volume.</param>
        /// <param name="top">The top edge of the projection volume.</param>
        /// <param name="zNear">The near edge of the projection volume.</param>
        /// <param name="zFar">The far edge of the projection volume.</param>
        /// <param name="result">The resulting Matrix4 instance.</param>
        public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNear, float zFar, out Matrix4D result)
        {
	        result = new Matrix4D();

	        float invRL = 1 / (right - left);
	        float invTB = 1 / (top - bottom);
	        float invFN = 1 / (zFar - zNear);

	        result.Row0.X = 2 * invRL;
	        result.Row1.Y = 2 * invTB;
	        result.Row2.Z = -2 * invFN;

	        result.Row3.X = -(right + left) * invRL;
	        result.Row3.Y = -(top + bottom) * invTB;
	        result.Row3.Z = -(zFar + zNear) * invFN;
	        result.Row3.W = 1;
        }


		/// <summary>Represents a zero matrix</summary>
		public static readonly Matrix4D Zero = new Matrix4D(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);

		/// <summary>Represents a matrix which performs no transformation</summary>
		public static readonly Matrix4D NoTransformation = new Matrix4D(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);

		/// <summary>Represents an identity matrix</summary>
		public static Matrix4D Identity = new Matrix4D(Vector4.UnitX, Vector4.UnitY, Vector4.UnitZ, Vector4.UnitW);
	}
}
