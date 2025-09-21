#pragma warning disable IDE0017
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

#pragma warning disable 660, 661
using System;
using System.Runtime.InteropServices;

namespace OpenBveApi.Math
{
	/// <summary>Represents a Quaternion</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Quaternion : IEquatable<Quaternion>
	{
		/// <summary>The X, Y and Z components</summary>
		public Vector3 Xyz;

		/// <summary>The W component</summary>
		public double W;

		/// <summary>Creates a new quaternion from four doubles.</summary>
		/// <param name="x">The X component.</param>
		/// <param name="y">The Y component.</param>
		/// <param name="z">The Z component.</param>
		/// <param name="w">The W component.</param>
		public Quaternion(double x, double y, double z, double w)
		{
			Xyz = new Vector3(x, y, z);
			W = w;
		}

		/// <summary>Creates a new quaternion from a Vector3 and a double.</summary>
		/// <param name="xyz">The Xyz component</param>
		/// <param name="w">The W component.</param>
		public Quaternion(Vector3 xyz, double w)
		{
			Xyz = xyz;
			W = w;
		}

		/// <summary>Gets / sets the X component</summary>
		public double X
		{
			get => Xyz.X;
			set => Xyz.X = value;
		}

		/// <summary>Gets / sets the Y component</summary>
		public double Y
		{
			get => Xyz.Y;
			set => Xyz.Y = value;
		}

		/// <summary>Gets / sets the Z component</summary>
		public double Z
		{
			get => Xyz.Z;
			set => Xyz.Z = value;
		}

		/// <summary>Creates a new quaternion describing the rotation between two vectors</summary>
		/// <param name="Start">The start vector</param>
		/// <param name="Dest">The destination vector</param>
		/// <returns></returns>
		public static Quaternion RotationBetweenVectors(Vector3 Start, Vector3 Dest)
		{
			Vector3 v0 = new Vector3(Start);
			Vector3 v1 = new Vector3(Dest);
			v0.Normalize();
			v1.Normalize();
			double d = Vector3.Dot(Start, Dest);
			if (d >= 1.0f)
			{
				//Dot is 1.0f so Vectors are identical
				return Quaternion.Identity;
				
			}
			if (d <= -1.0f)
			{
				//Dot is -1.0f so Vectors represent a 180 degree flip
				Vector3 axis = Vector3.Cross(new Vector3(1,0,0), Start);
				if (axis.Norm() == 0)
				{
					//Re-generate if colinear
					axis = Vector3.Cross(new Vector3(0,1,0), Start);
				}
				axis.Normalize();
				return Quaternion.FromAxisAngle(axis, System.Math.PI);
			}
			double s = System.Math.Sqrt((1+d) * 2);
			double invs = 1 / s;

			Vector3 c = Vector3.Cross(v0, v1);
			return new Quaternion(c.X * invs, c.Y * invs, c.Z * invs, s * 0.5f).Normalized();
		}

		/// <summary>Convert this instance to an axis-angle representation.</summary>
		/// <returns>A Vector4 that is the axis-angle representation of this quaternion.</returns>
		public Vector4 ToAxisAngle()
		{
			Quaternion q = this;
			if (System.Math.Abs(q.W) > 1.0f)
			{
				q.Normalize();
			}


            Vector4 result = new Vector4();
            result.W = 2.0f * (float)System.Math.Acos(q.W); // angle
			float den = (float)System.Math.Sqrt(1.0 - q.W * q.W);
			if (den > 0.0001f)
			{
				result.Xyz = q.Xyz / den;
			}
			else
			{
				// This occurs when the angle is zero.
				// Not a problem: just set an arbitrary normalized axis.
				result.Xyz = new Vector3(1,0,0);
			}

			return result;
		}

		/// <summary>Convert the current quaternion to axis angle representation</summary>
		/// <param name="axis">The resultant axis</param>
		/// <param name="angle">The resultant angle</param>
		public void ToAxisAngle(out Vector3 axis, out double angle)
		{
			Vector4 result = ToAxisAngle();
			axis = new Vector3(result.X, result.Y, result.Z);
			angle = result.W;
		}

		/// <summary>
		/// Build a Quaternion from the given axis and angle
		/// </summary>
		/// <param name="axis">The axis to rotate about</param>
		/// <param name="angle">The rotation angle in radians</param>
		/// <returns></returns>
		public static Quaternion FromAxisAngle(Vector3 axis, double angle)
		{
			if (axis.NormSquared() == 0.0f)
			{
				return Identity;
			}
				
			Quaternion result = Identity;
			angle *= 0.5f;
			axis.Normalize();
			result.Xyz = axis * System.Math.Sin(angle);
			result.W = System.Math.Cos(angle);
			result.Normalize();
			return result;
		}

		/// <summary>
		/// Gets the length (magnitude) of the Quaternion.
		/// </summary>
		/// <seealso cref="LengthSquared"/>
		public double Length => System.Math.Sqrt(W * W + Xyz.NormSquared());

		/// <summary>
		/// Gets the square of the Quaternion length (magnitude).
		/// </summary>
		public double LengthSquared => W * W + Xyz.NormSquared();

		/// <summary>
		/// Returns a copy of the Quaternion scaled to unit length.
		/// </summary>
		public Quaternion Normalized()
		{
			Quaternion q = this;
			q.Normalize();
			return q;
		}

		/// <summary>
		/// Reverses the rotation angle of this Quaternion.
		/// </summary>
		public void Invert()
		{
			W = -W;
		}

		/// <summary>
		/// Returns a copy of this Quaternion with its rotation angle reversed.
		/// </summary>
		public Quaternion Inverted()
		{
			var q = this;
			q.Invert();
			return q;
		}

		/// <summary>
		/// Scales the Quaternion to unit length.
		/// </summary>
		public void Normalize()
		{
			double scale = 1.0f / this.Length;
			Xyz *= scale;
			W *= scale;
		}

		/// <summary>
		/// Multiplies this quaternion by another
		/// </summary>
		/// <param name="Quaternion">The quaternion to multiply by</param>
		public void Multiply(ref Quaternion Quaternion)
        {
            double w = W * Quaternion.W - X * Quaternion.X - Y * Quaternion.Y - Z * Quaternion.Z;
            double x = W * Quaternion.X + X * Quaternion.W + Y * Quaternion.Z - Z * Quaternion.Y;
            double y = W * Quaternion.Y + Y * Quaternion.W + Z * Quaternion.X - X * Quaternion.Z;
            Z = W * Quaternion.Z + Z * Quaternion.W + X * Quaternion.Y - Y * Quaternion.X;
            W = w;
            X = x;
            Y = y;
        }

        /// <summary>
        /// Multiplies this quaternion by another and returns the result as a new quaternion
        /// </summary>
        /// <param name="Quaternion">The quaternion to multiply by</param>
        /// <param name="result">The new quaternion</param>
        public void Multiply(ref Quaternion Quaternion, out Quaternion result)
        {
			result = new Quaternion(W * Quaternion.X + X * Quaternion.W + Y * Quaternion.Z - Z * Quaternion.Y, 
									W * Quaternion.Y + Y * Quaternion.W + Z * Quaternion.X - X * Quaternion.Z, 
									W * Quaternion.Z + Z * Quaternion.W + X * Quaternion.Y - Y * Quaternion.X,
									W * Quaternion.W - X * Quaternion.X - Y * Quaternion.Y - Z * Quaternion.Z);
        }

        /// <summary>
        /// Multiplies one quaternion by another and returns the result as a new quaternion
        /// </summary>
        /// <param name="left">The first quaternion multiply by</param>
        /// <param name="right">The quaternion to multiply by</param>
        /// <param name="result">The new quaternion</param>
        public static void Multiply(ref Quaternion left, ref Quaternion right, out Quaternion result)
        {
			result = new Quaternion(left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
									left.W * right.Y + left.Y * right.W + left.Z * right.X - left.X * right.Z,
									left.W * right.Z + left.Z * right.W + left.X * right.Y - left.Y * right.X,
									left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z);
        }

        /// <summary>
        /// Multiplies all components of this quaternion by a scalar
        /// </summary>
        /// <param name="scalar">The scalar</param>
        public void Multiply(double scalar)
        {
            W *= scalar;
            X *= scalar;
            Y *= scalar;
            Z *= scalar;
        }

        /// <summary>
        /// Multiplies this quaternion by a scalar and returns the result as a new quaternion
        /// </summary>
        /// <param name="scalar">The scalar</param>
        /// <param name="result">The new quaternion</param>
        public void Multiply(double scalar, out Quaternion result)
        {
	        result = new Quaternion(X * scalar, Y * scalar, Z * scalar, W * scalar);
        }

		/// <summary>
		/// Multiplies a quaternion by a scalar and returns the result as a new quaternion
		/// </summary>
		/// <param name="Quaternion">The quaternion</param>
		/// <param name="scalar">The scalar</param>
		/// <param name="result">The new quaternion</param>
        public static void Multiply(ref Quaternion Quaternion, double scalar, out Quaternion result)
        {
	        result = new Quaternion(Quaternion.X * scalar, Quaternion.Y * scalar, Quaternion.Z * scalar, Quaternion.W * scalar);
        }

		/// <summary>
        /// Adds two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator +(Quaternion left, Quaternion right)
        {
            left.Xyz += right.Xyz;
            left.W += right.W;
            return left;
        }

        /// <summary>
        /// Subtracts two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator -(Quaternion left, Quaternion right)
        {
            left.Xyz -= right.Xyz;
            left.W -= right.W;
            return left;
        }

        /// <summary>
        /// Multiplies two instances.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>The result of the calculation.</returns>
        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            Multiply(ref left, ref right, out left);
            return left;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(Quaternion quaternion, double scale)
        {
            Multiply(ref quaternion, scale, out quaternion);
            return quaternion;
        }

        /// <summary>
        /// Multiplies an instance by a scalar.
        /// </summary>
        /// <param name="quaternion">The instance.</param>
        /// <param name="scale">The scalar.</param>
        /// <returns>A new instance containing the result of the calculation.</returns>
        public static Quaternion operator *(double scale, Quaternion quaternion)
        {
            return new Quaternion(quaternion.X * scale, quaternion.Y * scale, quaternion.Z * scale, quaternion.W * scale);
        }

        /// <summary>
        /// Compares two instances for equality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left equals right; false otherwise.</returns>
        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances for inequality.
        /// </summary>
        /// <param name="left">The first instance.</param>
        /// <param name="right">The second instance.</param>
        /// <returns>True, if left does not equal right; false otherwise.</returns>
        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !left.Equals(right);
        }

		/// <summary>A quaterion representing an identity matrix.</summary>
		public static readonly Quaternion Identity = new Quaternion(0, 0, 0, 1);

		/// <summary>A quaterion representing an identity matrix in DirectX format.</summary>
		public static readonly Quaternion DirectXIdentity = new Quaternion(0, 0, 0, -1);

		/// <summary>
		/// Do Spherical linear interpolation between two quaternions 
		/// </summary>
		/// <param name="q1">The first quaternion</param>
		/// <param name="q2">The second quaternion</param>
		/// <param name="blend">The blend factor</param>
		/// <returns>A smooth blend between the given quaternions</returns>
		public static Quaternion Slerp(Quaternion q1, Quaternion q2, float blend)
		{
			// if either input is zero, return the other.
			if (q1.LengthSquared == 0.0f)
			{
				if (q2.LengthSquared == 0.0f)
				{
					return Identity;
				}
				return q2;
			}
			else if (q2.LengthSquared == 0.0f)
			{
				return q1;
			}


			double cosHalfAngle = q1.W * q2.W + Vector3.Dot(q1.Xyz, q2.Xyz);

			if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
			{
				// angle = 0.0f, so just return one input.
				return q1;
			}
			else if (cosHalfAngle < 0.0f)
			{
				q2.Xyz = -q2.Xyz;
				q2.W = -q2.W;
				cosHalfAngle = -cosHalfAngle;
			}

			float blendA;
			float blendB;
			if (cosHalfAngle < 0.99f)
			{
				// do proper slerp for big angles
				float halfAngle = (float)System.Math.Acos(cosHalfAngle);
				float sinHalfAngle = (float)System.Math.Sin(halfAngle);
				float oneOverSinHalfAngle = 1.0f / sinHalfAngle;
				blendA = (float)System.Math.Sin(halfAngle * (1.0f - blend)) * oneOverSinHalfAngle;
				blendB = (float)System.Math.Sin(halfAngle * blend) * oneOverSinHalfAngle;
			}
			else
			{
				// do lerp if angle is really small.
				blendA = 1.0f - blend;
				blendB = blend;
			}

			Quaternion result = new Quaternion(blendA * q1.Xyz + blendB * q2.Xyz, blendA * q1.W + blendB * q2.W);
			if (result.LengthSquared > 0.0f)
			{
				result.Normalize();
				return result;
			}
			return Identity;
		}


		/// <summary>
		/// Convert this instance to an Euler angle representation.
		/// </summary>
		/// <returns>The Euler angles in radians.</returns>
		public Vector3 ToEulerAngles()
		{
			/*
            reference
            http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
            */

			var q = this;

			Vector3 eulerAngles;

			// Threshold for the singularities found at the north/south poles.
			const float SINGULARITY_THRESHOLD = 0.4999995f;

			var sqw = q.W * q.W;
			var sqx = q.X * q.X;
			var sqy = q.Y * q.Y;
			var sqz = q.Z * q.Z;
			var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
			var singularityTest = (q.X * q.Z) + (q.W * q.Y);

			double PiOver2 = System.Math.PI / 2;

			if (singularityTest > SINGULARITY_THRESHOLD * unit)
			{
				eulerAngles.Z = 2 * System.Math.Atan2(q.X, q.W);
				eulerAngles.Y = PiOver2;
				eulerAngles.X = 0;
			}
			else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
			{
				eulerAngles.Z = -2 * System.Math.Atan2(q.X, q.W);
				eulerAngles.Y = -PiOver2;
				eulerAngles.X = 0;
			}
			else
			{
				eulerAngles.Z = System.Math.Atan2(2 * ((q.W * q.Z) - (q.X * q.Y)), sqw + sqx - sqy - sqz);
				eulerAngles.Y = System.Math.Asin(2 * singularityTest / unit);
				eulerAngles.X = System.Math.Atan2(2 * ((q.W * q.X) - (q.Y * q.Z)), sqw - sqx - sqy + sqz);
			}

			return eulerAngles;
		}

		/// <summary>
		/// Builds a Quaternion from the given euler angles in radians.
		/// The rotations will get applied in following order:
		/// 1. Around X, 2. Around Y, 3. Around Z.
		/// </summary>
		/// <param name="eulerAngles">The counterclockwise euler angles a vector.</param>
		public static Quaternion FromEulerAngles(Vector3 eulerAngles)
		{
			var c1 = System.Math.Cos(eulerAngles.X * 0.5f);
			var c2 = System.Math.Cos(eulerAngles.Y * 0.5f);
			var c3 = System.Math.Cos(eulerAngles.Z * 0.5f);
			var s1 = System.Math.Sin(eulerAngles.X * 0.5f);
			var s2 = System.Math.Sin(eulerAngles.Y * 0.5f);
			var s3 = System.Math.Sin(eulerAngles.Z * 0.5f);
			Quaternion q = new Quaternion((s1 * c2 * c3) + (c1 * s2 * s3), (c1 * s2 * c3) - (s1 * c2 * s3), (c1 * c2 * s3) + (s1 * s2 * c3), (c1 * c2 * c3) - (s1 * s2 * s3));
			return q;
		}

		/// <summary>Tests whether this Quaternion is equal to another</summary>
		/// <param name="other">The other quaternion</param>
		public bool Equals(Quaternion other)
		{
			return Xyz.Equals(other.Xyz) && W.Equals(other.W);
		}

		/// <summary>Tests whether this Quaternion is equal to the specified object</summary>
		/// <param name="obj">The object</param>
		public override bool Equals(object obj)
		{
			return obj is Quaternion other && Equals(other);
		}

		/// <summary>Gets the hash-code for this quaternion</summary>
		public override int GetHashCode()
		{
			unchecked
			{
				return (Xyz.GetHashCode() * 397) ^ W.GetHashCode();
			}
		}
	}
}
#pragma warning restore 660,661
