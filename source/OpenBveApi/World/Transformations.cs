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
			X = Vector3.Right;
			Y = Vector3.Down;
			Z = Vector3.Forward;
		}

		/// <summary>Creates a new transformation, based upon yaw pitch and roll values</summary>
		/// <param name="Yaw">
		/// <para>The yaw to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the XZ-plane in clock-wise order when viewed from above.</para>
		/// </param>
		/// <param name="Pitch">
		/// <para>The pitch to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the YZ-plane in *counter* clock-wise order when viewed from the right.</para>
		/// </param>
		/// <param name="Roll">
		/// <para>The roll to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the XY-plane in *counter* clock-wise order when viewed from ahead.</para>
		/// </param>
		public Transformation(double Yaw, double Pitch, double Roll)
		{
			if (Yaw == 0.0 & Pitch == 0.0 & Roll == 0.0)
			{
				X = Vector3.Right;
				Y = Vector3.Down;
				Z = Vector3.Forward;
			}
			else if (Pitch == 0.0 & Roll == 0.0)
			{
				double cosYaw = System.Math.Cos(Yaw);
				double sinYaw = System.Math.Sin(Yaw);
				X = new Vector3(cosYaw, 0.0, -sinYaw);
				Y = Vector3.Down;
				Z = new Vector3(sinYaw, 0.0, cosYaw);
			}
			else
			{
				X = Vector3.Right;
				Y = Vector3.Down;
				Z = Vector3.Forward;
				X.Rotate(Y, Yaw);
				Z.Rotate(Y, Yaw);
				// In the left-handed coordinate system, the clock-wise rotation is positive when the origin is viewed from the positive direction of the axis.
				// Therefore, reverse the sign of rotation.
				Y.Rotate(X, -Pitch);
				Z.Rotate(X, -Pitch);
				X.Rotate(Z, -Roll);
				Y.Rotate(Z, -Roll);
			}
		}

		/// <summary>Creates a new transformation, based upon an initial transformation, plus secondary yaw pitch and roll values</summary>
		/// <param name="Transformation">The initial transformation</param>
		/// <param name="Yaw">
		/// <para>The yaw to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the XZ-plane in clock-wise order when viewed from above.</para>
		/// </param>
		/// <param name="Pitch">
		/// <para>The pitch to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the YZ-plane in *counter* clock-wise order when viewed from the right.</para>
		/// </param>
		/// <param name="Roll">
		/// <para>The roll to apply</para>
		/// <para>NOTE: The angle in radians by which the object is rotated in the XY-plane in *counter* clock-wise order when viewed from ahead.</para>
		/// </param>
		public Transformation(Transformation Transformation, double Yaw, double Pitch, double Roll)
		{
			X = new Vector3(Transformation.X);
			Y = new Vector3(Transformation.Y);
			Z = new Vector3(Transformation.Z);
			X.Rotate(Y, Yaw);
			Z.Rotate(Y, Yaw);
			// In the left-handed coordinate system, the clock-wise rotation is positive when the origin is viewed from the positive direction of the axis.
			// Therefore, reverse the sign of rotation.
			Y.Rotate(X, -Pitch);
			Z.Rotate(X, -Pitch);
			X.Rotate(Z, -Roll);
			Y.Rotate(Z, -Roll);
		}

		/// <summary>Creates a new transformation, based upon a base transformation and an additional transformation</summary>
		/// <param name="firstTransformation">The transformation to apply first</param>
		/// <param name="secondTransformation">The transformation to apply second</param>
		public Transformation(Transformation firstTransformation, Transformation secondTransformation)
		{
			X = new Vector3(firstTransformation.X);
			Y = new Vector3(firstTransformation.Y);
			Z = new Vector3(firstTransformation.Z);
			X.Rotate(secondTransformation);
			Y.Rotate(secondTransformation);
			Z.Rotate(secondTransformation);
		}

		/// <summary>Creates a new transformation, based upon three other vectors</summary>
		/// <param name="direction">The vector in the Z axis direction</param>
		/// <param name="up">The vector in the Y axis direction</param>
		/// <param name="side">The vector in the X axis direction</param>
		public Transformation(Vector3 direction, Vector3 up, Vector3 side)
		{
			X = side;
			Y = up;
			Z = direction;
		}

		/// <summary>Creates a new transformation, based upon three other vectors</summary>
		/// <param name="direction">The vector in the Z axis direction</param>
		/// <param name="up">The vector in the Y axis direction</param>
		/// <param name="side">The vector in the X axis direction</param>
		public Transformation(Vector3f direction, Vector3f up, Vector3f side)
		{
			X = side;
			Y = up;
			Z = direction;
		}

		/// <summary>Converts a Transformation into a change-of-basis row-major matrix in the right-handed coordinate system</summary>
		/// <param name="t">The transformation to convert</param>
		public static explicit operator Matrix4D(Transformation t)
		{
			// X, Y and Z represent the basis vector.
			// Arrange them in row-major to create a change-of-basis matrix.
			// And converting from the left-handed coordinate system to the right-handed coordinate system by reversing the Z axis.
			return new Matrix4D(new[]
			{
				t.X.X, t.X.Y, -t.X.Z, 0.0,
				t.Y.X, t.Y.Y, -t.Y.Z, 0.0,
				-t.Z.X, -t.Z.Y, t.Z.Z, 0.0,
				0.0, 0.0, 0.0, 1.0
			});
		}

		/// <summary>A transformation which leaves the input unchanged</summary>
		public static readonly Transformation NullTransformation = new Transformation();
	}
}
