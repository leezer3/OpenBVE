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

		/// <summary>Applies the current zoom settings after a change</summary>
		public void ApplyZoom()
		{
			VerticalViewingAngle = OriginalVerticalViewingAngle * Math.Exp(Alignment.Zoom);
			if (VerticalViewingAngle < 0.001) VerticalViewingAngle = 0.001;
			if (VerticalViewingAngle > 1.5) VerticalViewingAngle = 1.5;
			Renderer.UpdateViewport(ViewportChangeMode.NoChange);
		}
	}
}
