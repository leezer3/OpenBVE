using System;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenBveApi.Math
{
	/// <summary>Represents a 4x4 double-precision matrix</summary>
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

		// --- comparisons ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
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
			if (!(obj is Vector4))
			{
				return false;
			}

			return this.Equals((Vector4)obj);
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
		public static bool operator !=(Matrix4D a, Matrix4D b) {
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
				Row3 = new Vector4(0.0, 0.0,0.0,1.0)
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
			return new Matrix4D(new Vector4 (2.0 * zNear * inverseRightLeft, 0.0, 0.0, 0.0),
				new Vector4 (0.0, 2.0 * zNear * inverseTopBottom, 0.0, 0.0),
				new Vector4 ((right + left) * inverseRightLeft, (top + bottom) * inverseTopBottom, -(zFar + zNear) * inverseFarNear, -1.0),
				new Vector4 (0.0, 0.0, -2.0 * zFar * zNear * inverseFarNear, 0.0));
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
		

		/// <summary>Represents a zero matrix</summary>
		public static readonly Matrix4D Zero = new Matrix4D(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);

		/// <summary>Represents a matrix which performs no transformation</summary>
		public static readonly Matrix4D NoTransformation = new Matrix4D(new Vector4(1.0, 0.0, 0.0, 0.0),new Vector4(0.0, 1.0, 0.0, 0.0), new Vector4(0.0, 0.0, 1.0, 0.0), new Vector4(0.0, 0.0, 0.0, 1.0) );
	}
}
