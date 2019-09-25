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

		public static OpenTK.Quaternion RotationBetweenVectors(OpenTK.Vector3 Start, OpenTK.Vector3 Dest)
		{
			float m = (float)System.Math.Sqrt((1.0 + OpenTK.Vector3.Dot(Start, Dest)) * 2.0);
			OpenTK.Vector3 w = (1.0f / m) * OpenTK.Vector3.Cross(Start, Dest);
			return new OpenTK.Quaternion(w.X, w.Y, w.Z, 0.5f * m);
		}
	}
}
