using System;
using LibRender2.Camera;
using LibRender2.Viewports;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace LibRender2.Cameras
{
	public class CameraProperties
	{
		private readonly BaseRenderer Renderer;
		/// <summary>The current viewing distance in the forward direction.</summary>
		public double ForwardViewingDistance;
		/// <summary>The current viewing distance in the backward direction.</summary>
		public double BackwardViewingDistance;
		/// <summary>The current viewing distance</summary>
		public double ViewingDistance
		{
			get
			{
				return Math.Max(ForwardViewingDistance, BackwardViewingDistance);
			}
		}

		/// <summary>The extra viewing distance used for determining visibility of animated objects.</summary>
		public double ExtraViewingDistance;
		/// <summary>Whether the camera has reached the end of the world</summary>
		public bool AtWorldEnd;
		/// <summary>The current horizontal viewing angle in radians</summary>
		public double HorizontalViewingAngle;
		/// <summary>The current vertical viewing angle in radians</summary>
		public double VerticalViewingAngle;
		/// <summary>The original vertical viewing angle in radians</summary>
		public double OriginalVerticalViewingAngle;
		/// <summary>A Matrix4D describing the current camera translation</summary>
		public Matrix4D TranslationMatrix;
		/// <summary>The absolute in-world camera position</summary>
		public Vector3 AbsolutePosition
		{
			get
			{
				return absolutePosition;
			}
			set
			{
				absolutePosition = value;
				TranslationMatrix = Matrix4D.CreateTranslation(-value.X, -value.Y, value.Z);
			}
		}
		/// <summary>The absolute in-world camera Direction vector</summary>
		public Vector3 AbsoluteDirection;
		/// <summary>The absolute in-world camera Up vector</summary>
		public Vector3 AbsoluteUp;
		/// <summary>The absolute in-world camera Side vector</summary>
		public Vector3 AbsoluteSide;
		/// <summary>The current relative camera alignment</summary>
		public CameraAlignment Alignment;
		/// <summary>The current relative camera Direction</summary>
		public CameraAlignment AlignmentDirection;
		/// <summary>The current relative camera Speed</summary>
		public CameraAlignment AlignmentSpeed;
		/// <summary>The current camera movement speed</summary>
		public double CurrentSpeed;
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
		public CameraViewMode CurrentMode;
		/// <summary>The current camera restriction mode</summary>
		public CameraRestrictionMode CurrentRestriction = CameraRestrictionMode.NotAvailable;
		/// <summary>The saved exterior camera alignment</summary>
		public CameraAlignment SavedExterior;
		/// <summary>The saved track camera alignment</summary>
		public CameraAlignment SavedTrack;

		private Vector3 absolutePosition;

		internal CameraProperties(BaseRenderer renderer)
		{
			Renderer = renderer;
		}

		/// <summary>Tests whether the camera may move further in the current direction</summary>
		public bool PerformRestrictionTest(CameraRestriction Restriction)
		{
			if (CurrentRestriction == CameraRestrictionMode.On)
			{
				Vector3[] p = { Restriction.BottomLeft, Restriction.TopRight };
				Vector2[] r = new Vector2[2];

				for (int j = 0; j < 2; j++)
				{
					// determine relative world coordinates
					p[j].Rotate(AbsoluteDirection, AbsoluteUp, AbsoluteSide);
					double rx = -Math.Tan(Alignment.Yaw) - Alignment.Position.X;
					double ry = -Math.Tan(Alignment.Pitch) - Alignment.Position.Y;
					double rz = -Alignment.Position.Z;
					p[j] += rx * AbsoluteSide + ry * AbsoluteUp + rz * AbsoluteDirection;

					// determine screen coordinates
					double ez = AbsoluteDirection.X * p[j].X + AbsoluteDirection.Y * p[j].Y + AbsoluteDirection.Z * p[j].Z;

					if (ez == 0.0)
					{
						return false;
					}

					double ex = AbsoluteSide.X * p[j].X + AbsoluteSide.Y * p[j].Y + AbsoluteSide.Z * p[j].Z;
					double ey = AbsoluteUp.X * p[j].X + AbsoluteUp.Y * p[j].Y + AbsoluteUp.Z * p[j].Z;
					r[j].X = ex / (ez * Math.Tan(0.5 * HorizontalViewingAngle));
					r[j].Y = ey / (ez * Math.Tan(0.5 * VerticalViewingAngle));
				}

				return r[0].X <= -1.0025 & r[1].X >= 1.0025 & r[0].Y <= -1.0025 & r[1].Y >= 1.0025;
			}
			if (CurrentRestriction == CameraRestrictionMode.Restricted3D)
			{
				Vector3[] p = { Restriction.BottomLeft, Restriction.TopRight };

				for (int j = 0; j < 2; j++)
				{
					// determine relative world coordinates
					p[j].Rotate(AbsoluteDirection, AbsoluteUp, AbsoluteSide);
					double rx = -Math.Tan(Alignment.Yaw) - Alignment.Position.X;
					double ry = -Math.Tan(Alignment.Pitch) - Alignment.Position.Y;
					double rz = -Alignment.Position.Z;
					p[j] += rx * AbsoluteSide + ry * AbsoluteUp + rz * AbsoluteDirection;
				}

				if (AlignmentDirection.Position.X > 0)
				{
					//moving right
					if (AbsolutePosition.X >= Restriction.AbsoluteTopRight.X)
					{
						return false;
					}
				}
				if (AlignmentDirection.Position.X < 0)
				{
					//moving left
					if (AbsolutePosition.X <= Restriction.AbsoluteBottomLeft.X)
					{
						return false;
					}
				}

				if (AlignmentDirection.Position.Y > 0)
				{
					//moving up
					if (AbsolutePosition.Y >= Restriction.AbsoluteTopRight.Y)
					{
						return false;
					}
				}
				if (AlignmentDirection.Position.Y < 0)
				{
					//moving down
					if (AbsolutePosition.Y <= Restriction.AbsoluteBottomLeft.Y)
					{
						return false;
					}
				}

				if (AlignmentDirection.Position.Z > 0)
				{
					//moving forwards
					if (AbsolutePosition.Z >= Restriction.AbsoluteTopRight.Z)
					{
						return false;
					}
				}
				if (AlignmentDirection.Position.Z < 0)
				{
					//moving back
					if (AbsolutePosition.Z <= Restriction.AbsoluteBottomLeft.Z)
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>Performs progressive adjustments taking into account the specified camera restriction</summary>
		public bool PerformProgressiveAdjustmentForCameraRestriction(ref double Source, double Target, bool Zoom, CameraRestriction Restriction)
		{
			if ((CurrentMode != CameraViewMode.Interior & CurrentMode != CameraViewMode.InteriorLookAhead) | (CurrentRestriction != CameraRestrictionMode.On && CurrentRestriction != CameraRestrictionMode.Restricted3D))
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
			if (PerformRestrictionTest(Restriction))
			{
				return true;
			}

			double x = 0.5 * (a + b);
			bool q = true;
			for (int i = 0; i < Precision; i++)
			{

#pragma warning disable IDE0059 //IDE is wrong, best may never be updated if q is never true
				Source = x;
#pragma warning restore IDE0059
				if (Zoom) ApplyZoom();
				q = PerformRestrictionTest(Restriction);
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

		/// <summary>Adjusts the camera alignment based upon the specified parameters</summary>
		public void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom = false, CameraRestriction? Restriction = null) {
			if (Direction != 0.0 | Speed != 0.0) {
				if (TimeElapsed > 0.0) {
					if (Direction == 0.0) {
						double d = (0.025 + 5.0 * Math.Abs(Speed)) * TimeElapsed;
						if (Speed >= -d & Speed <= d) {
							Speed = 0.0;
						} else {
							Speed -= Math.Sign(Speed) * d;
						}
					} else {
						double t = Math.Abs(Direction);
						double d = ((1.15 - 1.0 / (1.0 + 0.025 * Math.Abs(Speed)))) * TimeElapsed;
						Speed += Direction * d;
						if (Speed < -t) {
							Speed = -t;
						} else if (Speed > t) {
							Speed = t;
						}
					}

					if (Restriction != null)
					{
						double x = Source + Speed * TimeElapsed;
						if (!PerformProgressiveAdjustmentForCameraRestriction(ref Source, x, Zoom, (CameraRestriction)Restriction))
						{
							Speed = 0.0;
						}
					}
					else
					{
						Source += Speed * TimeElapsed;
					}
				}
			}
		}

		/// <summary>Applies the current zoom settings after a change</summary>
		public void ApplyZoom()
		{
			VerticalViewingAngle = OriginalVerticalViewingAngle * Math.Exp(Alignment.Zoom);
			if (VerticalViewingAngle < 0.001) VerticalViewingAngle = 0.001;
			if (VerticalViewingAngle > 1.5) VerticalViewingAngle = 1.5;
			Renderer.UpdateViewport(ViewportChangeMode.NoChange);
		}

		/// <summary>Resets the camera to an absolute position</summary>
		public void Reset(Vector3 Position)
		{
			AbsolutePosition = Position;
			AbsoluteDirection = new Vector3(-AbsolutePosition.X, -AbsolutePosition.Y, -AbsolutePosition.Z);
			AbsoluteSide = new Vector3(-AbsolutePosition.Z, 0.0, AbsolutePosition.X);
			AbsoluteDirection.Normalize();
			AbsoluteSide.Normalize();
			AbsoluteUp = Vector3.Cross(AbsoluteDirection, AbsoluteSide);
			VerticalViewingAngle = 45.0.ToRadians();
			HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * VerticalViewingAngle) * Renderer.Screen.AspectRatio);
			OriginalVerticalViewingAngle = VerticalViewingAngle;
		}

		/// <summary>Unconditionally resets the camera</summary>
		public void Reset()
		{
			Alignment.Yaw = 0.0;
			Alignment.Pitch = 0.0;
			Alignment.Roll = 0.0;
			Alignment.Position = new Vector3(0.0, 2.5, 0.0);
			Alignment.Zoom = 0.0;
			AlignmentDirection = new CameraAlignment();
			AlignmentSpeed = new CameraAlignment();
			VerticalViewingAngle = OriginalVerticalViewingAngle;
		}
	}
}
