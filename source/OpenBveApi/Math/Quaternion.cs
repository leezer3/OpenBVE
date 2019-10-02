using System;
using OpenTK;

namespace OpenBveApi.Math
{
	/// <summary>Represents a Quaternion</summary>
	public struct Quaternion
	{
		/// <summary>The X, Y and Z components</summary>
		public Vector3 Xyz;

		/// <summary>The W component</summary>
		public double W;

		/// <summary>Gets / sets the X component</summary>
		public double X
		{
			get
			{
				return Xyz.X;
			}
			set
			{
				Xyz.X = value;
			}
		}

		/// <summary>Gets / sets the Y component</summary>
		public double Y
		{
			get
			{
				return Xyz.Y;
			}
			set
			{
				Xyz.Y = value;
			}
		}

		/// <summary>Gets / sets the Z component</summary>
		public double Z
		{
			get
			{
				return Xyz.Z;
			}
			set
			{
				Xyz.Z = value;
			}
		}

		public static OpenTK.Quaterniond RotationBetweenVectors(OpenTK.Vector3d Start, OpenTK.Vector3d Dest)
		{
			Vector3d v0 = new Vector3d(Start);
			Vector3d v1 = new Vector3d(Dest);
			v0.Normalize();
			v1.Normalize();
			double d = Vector3d.Dot(Start, Dest);
			if (d >= 1.0f)
			{
				//Dot is 1.0f so Vectors are identical
				return Quaterniond.Identity;
			}
			if (d <= -1.0f)
			{
				//Dot is -1.0f so Vectors represent a 180 degree flip
				Vector3d axis = Vector3d.Cross(Vector3d.UnitX, Start);
				if (axis.Length == 0)
				{
					//Re-generate if colinear
					axis = Vector3d.Cross(Vector3d.UnitY, Start);
				}
				axis.Normalize();
				return Quaterniond.FromAxisAngle(axis, System.Math.PI);
			}
			double s = System.Math.Sqrt((1+d) * 2);
			double invs = 1 / s;

			Vector3d c = Vector3d.Cross(v0, v1);
			return new Quaterniond(c.X * invs, c.Y * invs, c.Z * invs, s * 0.5f).Normalized();
		}
	}
}
