using System;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBveShared
{
	public static class Camera
	{
		/// <summary>The currently applied camera restriction</summary>
		public static CameraRestrictionMode CameraRestriction = CameraRestrictionMode.NotAvailable;

		/// <summary>The current camera mode</summary>
		public static CameraViewMode CameraView;

		/// <summary>The current base camera alignment</summary>
		public static CameraAlignment CameraCurrentAlignment;
		
		/// <summary>The current camera alignment direction vectors</summary>
		public static CameraAlignment CameraAlignmentDirection;

		/// <summary>The current camera alignment speed vectors</summary>
		public static CameraAlignment CameraAlignmentSpeed;

		/// <summary>The absolute camera position</summary>
		public static Vector3 AbsoluteCameraPosition;

		/// <summary>The absolute camera direction vector</summary>
		public static Vector3 AbsoluteCameraDirection;

		/// <summary>The absolute camera up vector</summary>
		public static Vector3 AbsoluteCameraUp;

		/// <summary>The absolute camera side vector</summary>
		public static Vector3 AbsoluteCameraSide;
		
		/// <summary>The current camera movement speed</summary>
		public static double CameraSpeed;

		/// <summary>The top camera movement speed when in an interior view</summary>
		public const double CameraInteriorTopSpeed = 5.0;

		/// <summary>The top camera angular movement speed when in an interior view</summary>
		public const double CameraInteriorTopAngularSpeed = 5.0;

		/// <summary>The top camera movement speed when in an exterior view</summary>
		public const double CameraExteriorTopSpeed = 50.0;

		/// <summary>The top camera angular movement speed when in an exterior view</summary>
		public const double CameraExteriorTopAngularSpeed = 10.0;

		/// <summary>The top camera zoom speed</summary>
		public const double CameraZoomTopSpeed = 2.0;

		/// <summary>The bottom-left world coordinate for the camera restriction</summary>
		public static Vector3 CameraRestrictionBottomLeft = new Vector3(-1.0, -1.0, 1.0);

		/// <summary>The top-right world coordinate for the camera restriction</summary>
		public static Vector3 CameraRestrictionTopRight = new Vector3(1.0, 1.0, 1.0);

		/// <summary>Applies the current camera zoom, taking account of the vertical viewing angle</summary>
		public static void ApplyZoom()
		{
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle * Math.Exp(CameraCurrentAlignment.Zoom);
			if (World.VerticalViewingAngle < 0.001) World.VerticalViewingAngle = 0.001;
			if (World.VerticalViewingAngle > 1.5) World.VerticalViewingAngle = 1.5;
			Renderer.UpdateViewport(Renderer.ViewPortChangeMode.NoChange);
		}

		public static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed)
		{
			AdjustAlignment(ref Source, Direction, ref Speed, TimeElapsed, false);
		}

		public static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom)
		{
			if (Direction != 0.0 | Speed != 0.0)
			{
				if (TimeElapsed > 0.0)
				{
					if (Direction == 0.0)
					{
						double d = (0.025 + 5.0 * Math.Abs(Speed)) * TimeElapsed;
						if (Speed >= -d & Speed <= d)
						{
							Speed = 0.0;
						}
						else
						{
							Speed -= (double) Math.Sign(Speed) * d;
						}
					}
					else
					{
						double t = Math.Abs(Direction);
						double d = ((1.15 - 1.0 / (1.0 + 0.025 * Math.Abs(Speed)))) * TimeElapsed;
						Speed += Direction * d;
						if (Speed < -t)
						{
							Speed = -t;
						}
						else if (Speed > t)
						{
							Speed = t;
						}
					}

					double x = Source + Speed * TimeElapsed;
					if (!PerformProgressiveAdjustmentForCameraRestriction(ref Source, x, Zoom))
					{
						Speed = 0.0;
					}
				}
			}
		}

		/// <summary>Progressively adjusts the camera position along a movement vector, taking into account zoom</summary>
		/// <param name="Source">The source position</param>
		/// <param name="Target">The target position</param>
		/// <param name="Zoom">Whether zoom is being applied</param>
		/// <returns>The new position along the vector</returns>
		public static bool PerformProgressiveAdjustmentForCameraRestriction(ref double Source, double Target, bool Zoom)
		{
			if ((Camera.CameraView != CameraViewMode.Interior & Camera.CameraView != CameraViewMode.InteriorLookAhead) | Camera.CameraRestriction != CameraRestrictionMode.On)
			{
				Source = Target;
				return true;
			}

			double best = Source;
			const int Precision = 8;
			double a = Source;
			double b = Target;
			Source = Target;
			if (Zoom) ApplyZoom();
			if (PerformCameraRestrictionTest())
			{
				return true;
			}

			double x = 0.5 * (a + b);
			bool q = true;
			for (int i = 0; i < Precision; i++)
			{
				//Do not remove, this is updated via the ref & causes the panel zoom to bug out
				Source = x;
				if (Zoom) Camera.ApplyZoom();
				q = Camera.PerformCameraRestrictionTest();
				if (q)
				{
					a = x;
					best = x;
				}
				else
				{
					b = x;
				}
				x = 0.5 * (a + b);
			}
			Source = best;
			if (Zoom) ApplyZoom();
			return q;
		}

		/// <summary>Tests whether the new camera position is valid when compared to the current camera restriction</summary>
		/// <returns>True if the new camera position is valid</returns>
		public static bool PerformCameraRestrictionTest()
		{
			if (Camera.CameraRestriction != CameraRestrictionMode.On)
			{
				return true;
			}
			Vector3[] p = new Vector3[] {Camera.CameraRestrictionBottomLeft, Camera.CameraRestrictionTopRight };
			Vector2[] r = new Vector2[2];
			for (int j = 0; j < 2; j++)
			{
				// determine relative world coordinates
				p[j].Rotate(Camera.AbsoluteCameraDirection, Camera.AbsoluteCameraUp, Camera.AbsoluteCameraSide);
				double rx = -Math.Tan(Camera.CameraCurrentAlignment.Yaw) - Camera.CameraCurrentAlignment.Position.X;
				double ry = -Math.Tan(Camera.CameraCurrentAlignment.Pitch) - Camera.CameraCurrentAlignment.Position.Y;
				double rz = -Camera.CameraCurrentAlignment.Position.Z;
				p[j].X += rx * Camera.AbsoluteCameraSide.X + ry * Camera.AbsoluteCameraUp.X + rz * Camera.AbsoluteCameraDirection.X;
				p[j].Y += rx * Camera.AbsoluteCameraSide.Y + ry * Camera.AbsoluteCameraUp.Y + rz * Camera.AbsoluteCameraDirection.Y;
				p[j].Z += rx * Camera.AbsoluteCameraSide.Z + ry * Camera.AbsoluteCameraUp.Z + rz * Camera.AbsoluteCameraDirection.Z;
				// determine screen coordinates
				double ez = Camera.AbsoluteCameraDirection.X * p[j].X + Camera.AbsoluteCameraDirection.Y * p[j].Y + Camera.AbsoluteCameraDirection.Z * p[j].Z;
				if (ez == 0.0) return false;
				double ex = Camera.AbsoluteCameraSide.X * p[j].X + Camera.AbsoluteCameraSide.Y * p[j].Y + Camera.AbsoluteCameraSide.Z * p[j].Z;
				double ey = Camera.AbsoluteCameraUp.X * p[j].X + Camera.AbsoluteCameraUp.Y * p[j].Y + Camera.AbsoluteCameraUp.Z * p[j].Z;
				r[j].X = ex / (ez * Math.Tan(0.5 * World.HorizontalViewingAngle));
				r[j].Y = ey / (ez * Math.Tan(0.5 * World.VerticalViewingAngle));
			}

			return r[0].X <= -1.0025 & r[1].X >= 1.0025 & r[0].Y <= -1.0025 & r[1].Y >= 1.0025;
		}
	}
}
