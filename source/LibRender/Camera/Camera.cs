using System;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace LibRender
{
	public static class Camera
	{
		/// <summary>Whether the camera has reached the end of the world</summary>
		public static bool AtWorldEnd;
		/// <summary>The current horizontal viewing angle in radians</summary>
		public static double HorizontalViewingAngle;
		/// <summary>The current vertical viewing angle in radians</summary>
		public static double VerticalViewingAngle;
		/// <summary>The original vertical viewing angle in radians</summary>
		public static double OriginalVerticalViewingAngle;
		/// <summary>The absolute in-world camera position</summary>
		public static Vector3 AbsolutePosition;
		/// <summary>The absolute in-world camera Direction vector</summary>
		public static Vector3 AbsoluteDirection;
		/// <summary>The absolute in-world camera Up vector</summary>
		public static Vector3 AbsoluteUp;
		/// <summary>The absolute in-world camera Side vector</summary>
		public static Vector3 AbsoluteSide;
		/// <summary>The current relative camera alignment</summary>
		public static CameraAlignment CurrentAlignment;
		/// <summary>The current relative camera Direction</summary>
		public static CameraAlignment AlignmentDirection;
		/// <summary>The current relative camera Speed</summary>
		public static CameraAlignment AlignmentSpeed;
		/// <summary>The current camera movement speed</summary>
		public static double CurrentSpeed;
		/// <summary>The top speed when moving in a straight line in an interior view</summary>
		public const double InteriorTopSpeed = 5.0;
		/// <summary>The top speed when moving at an angle in an interior view</summary>
		public const double InteriorTopAngularSpeed = 5.0;
		/// <summary>The top speed when moving in a straight line in an exterior view</summary>
		public const double ExteriorTopSpeed = 50.0;
		/// <summary>The top speed when moving in an angle in an exterior view</summary>
		public const double ExteriorTopAngularSpeed = 10.0;
		/// <summary>The top speed when zooming in or out</summary>
		public const double ZoomTopSpeed = 2.0;
		/// <summary>The current camera mode</summary>
		public static CameraViewMode CurrentMode;
		/// <summary>The current camera restriction mode</summary>
		public static CameraRestrictionMode CurrentRestriction = CameraRestrictionMode.NotAvailable;
		/// <summary>The top left vector when the camera is restricted</summary>
		/// <remarks>2D panel</remarks>
		public static Vector3 RestrictionBottomLeft = new Vector3(-1.0, -1.0, 1.0);
		/// <summary>The bottom right vector when the camera is restricted</summary>
		/// <remarks>2D panel</remarks>
		public static Vector3 RestrictionTopRight = new Vector3(1.0, 1.0, 1.0);

		/// <summary>Tests whether the camera may move further in the current direction</summary>
		public static bool PerformRestrictionTest()
		{
			if (CurrentRestriction == CameraRestrictionMode.On) {
				Vector3[] p = new Vector3[] {RestrictionBottomLeft, RestrictionTopRight };
				Vector2[] r = new Vector2[2];
				for (int j = 0; j < 2; j++) {
					// determine relative world coordinates
					p[j].Rotate(AbsoluteDirection, AbsoluteUp, AbsoluteSide);
					double rx = -Math.Tan(CurrentAlignment.Yaw) - CurrentAlignment.Position.X;
					double ry = -Math.Tan(CurrentAlignment.Pitch) - CurrentAlignment.Position.Y;
					double rz = -CurrentAlignment.Position.Z;
					p[j] += rx * AbsoluteSide + ry * AbsoluteUp + rz * AbsoluteDirection;
					// determine screen coordinates
					double ez = AbsoluteDirection.X * p[j].X + AbsoluteDirection.Y * p[j].Y + AbsoluteDirection.Z * p[j].Z;
					if (ez == 0.0) return false;
					double ex = AbsoluteSide.X * p[j].X + AbsoluteSide.Y * p[j].Y + AbsoluteSide.Z * p[j].Z;
					double ey = AbsoluteUp.X * p[j].X + AbsoluteUp.Y * p[j].Y + AbsoluteUp.Z * p[j].Z;
					r[j].X = ex / (ez * Math.Tan(0.5 * HorizontalViewingAngle));
					r[j].Y = ey / (ez * Math.Tan(0.5 * VerticalViewingAngle));
				}
				return r[0].X <= -1.0025 & r[1].X >= 1.0025 & r[0].Y <= -1.0025 & r[1].Y >= 1.0025;
			}
			return true;
		}


	}
}
