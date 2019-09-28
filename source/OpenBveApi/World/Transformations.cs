using OpenBveApi.Math;

namespace OpenBveApi.World
{
	/// <summary>Describes a world transformation consisting of three vectors</summary>
	public class Transformation
	{
		/// <summary>The X Vector</summary>
		public Vector3 X;
		/// <summary>The Y Vector</summary>
		public Vector3 Y;
		/// <summary>The Z Vector</summary>
		public Vector3 Z;

		/// <summary>Creates a new empty transformation</summary>
		public Transformation()
		{
			this.X = Vector3.Right;
			this.Y = Vector3.Down;
			this.Z = Vector3.Forward;
		}

		/// <summary>Creates a new transformation, based upon yaw pitch and roll values</summary>
		/// <param name="Yaw">The yaw to apply</param>
		/// <param name="Pitch">The pitch to apply</param>
		/// <param name="Roll">The roll to apply</param>
		public Transformation(double Yaw, double Pitch, double Roll)
		{
			if (Yaw == 0.0 & Pitch == 0.0 & Roll == 0.0)
			{
				this.X = Vector3.Right;
				this.Y = Vector3.Down;
				this.Z = Vector3.Forward;
			}
			else if (Pitch == 0.0 & Roll == 0.0)
			{
				double cosYaw = System.Math.Cos(Yaw);
				double sinYaw = System.Math.Sin(Yaw);
				this.X = new Vector3(cosYaw, 0.0, -sinYaw);
				this.Y = Vector3.Down;
				this.Z = new Vector3(sinYaw, 0.0, cosYaw);
			}
			else
			{
				X = Vector3.Right;
				Y = Vector3.Down;
				Z = Vector3.Forward;
				double cosYaw = System.Math.Cos(Yaw);
				double sinYaw = System.Math.Sin(Yaw);
				double cosPitch = System.Math.Cos(-Pitch);
				double sinPitch = System.Math.Sin(-Pitch);
				double cosRoll = System.Math.Cos(-Roll);
				double sinRoll = System.Math.Sin(-Roll);
				X.Rotate(Y, cosYaw, sinYaw);
				Z.Rotate(Y, cosYaw, sinYaw);
				Y.Rotate(X, cosPitch, sinPitch);
				Z.Rotate(X, cosPitch, sinPitch);
				X.Rotate(Z, cosRoll, sinRoll);
				Y.Rotate(Z, cosRoll, sinRoll);
			}
		}

		/// <summary>Creates a new transformation, based upon an initial transformation, plus secondary yaw pitch and roll values</summary>
		/// <param name="Transformation">The initial transformation</param>
		/// <param name="Yaw">The yaw to apply</param>
		/// <param name="Pitch">The pitch to apply</param>
		/// <param name="Roll">The roll to apply</param>
		public Transformation(Transformation Transformation, double Yaw, double Pitch, double Roll)
		{
			X = new Vector3(Transformation.X);
			Y = new Vector3(Transformation.Y);
			Z = new Vector3(Transformation.Z);
			double cosYaw = System.Math.Cos(Yaw);
			double sinYaw = System.Math.Sin(Yaw);
			double cosPitch = System.Math.Cos(-Pitch);
			double sinPitch = System.Math.Sin(-Pitch);
			double cosRoll = System.Math.Cos(Roll);
			double sinRoll = System.Math.Sin(Roll);
			X.Rotate(Y, cosYaw, sinYaw);
			Z.Rotate(Y, cosYaw, sinYaw);
			Y.Rotate(X, cosPitch, sinPitch);
			Z.Rotate(X, cosPitch, sinPitch);
			X.Rotate(Z, cosRoll, sinRoll);
			Y.Rotate(Z, cosRoll, sinRoll);
		}

		/// <summary>Creates a new transformation, based upon a base transformation and an auxiliary transformation</summary>
		/// <param name="BaseTransformation">The base transformation</param>
		/// <param name="AuxTransformation">The auxiliary transformation</param>
		public Transformation(Transformation BaseTransformation, Transformation AuxTransformation)
		{
			X = new Vector3(BaseTransformation.X);
			Y = new Vector3(BaseTransformation.Y);
			Z = new Vector3(BaseTransformation.Z);
			X.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
			Y.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
			Z.Rotate(AuxTransformation.Z, AuxTransformation.Y, AuxTransformation.X);
		}

		/// <summary>Creates a new transformation, based upon three other vectors</summary>
		/// <param name="firstVector">The first vector</param>
		/// <param name="secondVector">The second vector</param>
		/// <param name="thirdVector">The third vector</param>
		public Transformation(Vector3 firstVector, Vector3 secondVector, Vector3 thirdVector)
		{
			X = thirdVector;
			Y = secondVector;
			Z = firstVector;
		}

		public static explicit operator OpenTK.Matrix4d(Transformation t)
		{
			OpenTK.Quaterniond rot1 = Quaternion.RotationBetweenVectors(OpenTK.Vector3d.UnitZ * -1.0, new OpenTK.Vector3d(t.Z.X, t.Z.Y, -t.Z.Z).Normalized());
			OpenTK.Vector3d newUp = OpenTK.Vector3d.Transform(OpenTK.Vector3d.UnitY, rot1);
			OpenTK.Quaterniond rot2 = Quaternion.RotationBetweenVectors(newUp, new OpenTK.Vector3d(t.Y.X, t.Y.Y, -t.Y.Z).Normalized());
			return OpenTK.Matrix4d.CreateFromQuaternion(rot2 * rot1);
		}
	}

}
