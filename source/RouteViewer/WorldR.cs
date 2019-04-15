// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;

namespace OpenBve {
	public static class World {	

		// display
		internal static double HorizontalViewingAngle;
		internal static double VerticalViewingAngle;
		internal static double OriginalVerticalViewingAngle;
		internal static double AspectRatio;
		internal static double ForwardViewingDistance;
		internal static double BackwardViewingDistance;
		internal static double ExtraViewingDistance;
		internal static double BackgroundImageDistance;
		internal struct Background {
			internal Texture Texture;
			internal int Repetition;
			internal bool KeepAspectRatio;
			internal Background(Texture texture, int Repetition, bool KeepAspectRatio) {
				this.Texture = texture;
				this.Repetition = Repetition;
				this.KeepAspectRatio = KeepAspectRatio;
			}
		}
		internal static Background CurrentBackground = new Background(null, 6, false);
		internal static Background TargetBackground = new Background(null, 6, false);
		internal const double TargetBackgroundDefaultCountdown = 0.8;
		internal static double TargetBackgroundCountdown;

		// relative camera
		internal struct CameraAlignment {
			internal Vector3 Position;
			internal double Yaw;
			internal double Pitch;
			internal double Roll;
			internal double TrackPosition;
			internal double Zoom;
			internal CameraAlignment(Vector3 Position, double Yaw, double Pitch, double Roll, double TrackPosition, double Zoom) {
				this.Position = Position;
				this.Yaw = Yaw;
				this.Pitch = Pitch;
				this.Roll = Roll;
				this.TrackPosition = TrackPosition;
				this.Zoom = Zoom;
			}
		}
		internal static TrackManager.TrackFollower CameraTrackFollower;
		internal static CameraAlignment CameraCurrentAlignment;
		internal static CameraAlignment CameraAlignmentDirection;
		internal static CameraAlignment CameraAlignmentSpeed;
		internal const double CameraExteriorTopSpeed = 50.0;
		internal const double CameraExteriorTopAngularSpeed = 5.0;
		internal const double CameraZoomTopSpeed = 2.0;
		internal static CameraViewMode CameraMode;

		// camera restriction
		internal enum CameraRestrictionMode {
			NotAvailable = -1,
			Off = 0,
			On = 1
		}
		internal static CameraRestrictionMode CameraRestriction = CameraRestrictionMode.NotAvailable;
		
		// absolute camera
		internal static Vector3 AbsoluteCameraPosition;
		internal static Vector3 AbsoluteCameraDirection;
		internal static Vector3 AbsoluteCameraUp;
		internal static Vector3 AbsoluteCameraSide;

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = World.CameraCurrentAlignment.Zoom;
			AdjustAlignment(ref World.CameraCurrentAlignment.Zoom, World.CameraAlignmentDirection.Zoom, ref World.CameraAlignmentSpeed.Zoom, TimeElapsed, World.CameraAlignmentSpeed.Zoom != 0.0);
			if (zm != World.CameraCurrentAlignment.Zoom) {
				ApplyZoom();
			}
			// current alignment
			AdjustAlignment(ref World.CameraCurrentAlignment.Position.X, World.CameraAlignmentDirection.Position.X, ref World.CameraAlignmentSpeed.Position.X, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Position.Y, World.CameraAlignmentDirection.Position.Y, ref World.CameraAlignmentSpeed.Position.Y, TimeElapsed);
			bool q = World.CameraAlignmentSpeed.Yaw != 0.0 | World.CameraAlignmentSpeed.Pitch != 0.0 | World.CameraAlignmentSpeed.Roll != 0.0;
			AdjustAlignment(ref World.CameraCurrentAlignment.Yaw, World.CameraAlignmentDirection.Yaw, ref World.CameraAlignmentSpeed.Yaw, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Pitch, World.CameraAlignmentDirection.Pitch, ref World.CameraAlignmentSpeed.Pitch, TimeElapsed);
			AdjustAlignment(ref World.CameraCurrentAlignment.Roll, World.CameraAlignmentDirection.Roll, ref World.CameraAlignmentSpeed.Roll, TimeElapsed);
			double tr = World.CameraCurrentAlignment.TrackPosition;
			AdjustAlignment(ref World.CameraCurrentAlignment.TrackPosition, World.CameraAlignmentDirection.TrackPosition, ref World.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != World.CameraCurrentAlignment.TrackPosition) {
				TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraCurrentAlignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				UpdateViewingDistances();
			}
			Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(CameraCurrentAlignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			double cx = World.CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z;
			double cy = World.CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z;
			double cz = World.CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z;
			if (World.CameraCurrentAlignment.Yaw != 0.0) {
				double cosa = Math.Cos(World.CameraCurrentAlignment.Yaw);
				double sina = Math.Sin(World.CameraCurrentAlignment.Yaw);
				dF.Rotate(uF, cosa, sina);
				sF.Rotate(uF, cosa, sina);
			}
			double p = World.CameraCurrentAlignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				dF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
			}
			if (World.CameraCurrentAlignment.Roll != 0.0) {
				double cosa = Math.Cos(-World.CameraCurrentAlignment.Roll);
				double sina = Math.Sin(-World.CameraCurrentAlignment.Roll);
				uF.Rotate(dF, cosa, sina);
				sF.Rotate(dF, cosa, sina);
			}
			AbsoluteCameraPosition = new Vector3(cx, cy, cz);
			AbsoluteCameraDirection = dF;
			AbsoluteCameraUp = uF;
			AbsoluteCameraSide = sF;
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed) {
			AdjustAlignment(ref Source, Direction, ref Speed, TimeElapsed, false);
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom) {
			if (TimeElapsed > 0.0) {
				if (Direction == 0.0) {
					double d = (0.025 + 5.0 * Math.Abs(Speed)) * TimeElapsed;
					if (Speed >= -d & Speed <= d) {
						Speed = 0.0;
					} else {
						Speed -= (double)Math.Sign(Speed) * d;
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
				Source += Speed * TimeElapsed;
			}
		}
		private static void ApplyZoom() {
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle * Math.Exp(World.CameraCurrentAlignment.Zoom);
			if (World.VerticalViewingAngle < 0.001) World.VerticalViewingAngle = 0.001;
			if (World.VerticalViewingAngle > 1.5) World.VerticalViewingAngle = 1.5;
			Program.UpdateViewport();
		}

		// update viewing distance
		internal static void UpdateViewingDistances() {
			double f = Math.Atan2(World.CameraTrackFollower.WorldDirection.Z, World.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(World.AbsoluteCameraDirection.Z, World.AbsoluteCameraDirection.X) - f;
			if (c < -Math.PI) {
				c += 2.0 * Math.PI;
			} else if (c > Math.PI) {
				c -= 2.0 * Math.PI;
			}
			double a0 = c - 0.5 * World.HorizontalViewingAngle;
			double a1 = c + 0.5 * World.HorizontalViewingAngle;
			double max;
			if (a0 <= 0.0 & a1 >= 0.0) {
				max = 1.0;
			} else {
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				max = c0 > c1 ? c0 : c1;
				if (max < 0.0) max = 0.0;
			}
			double min;
			if (a0 <= -Math.PI | a1 >= Math.PI) {
				min = -1.0;
			} else {
				double c0 = Math.Cos(a0);
				double c1 = Math.Cos(a1);
				min = c0 < c1 ? c0 : c1;
				if (min > 0.0) min = 0.0;
			}
			double d = World.BackgroundImageDistance + World.ExtraViewingDistance;
			World.ForwardViewingDistance = d * max;
			World.BackwardViewingDistance = -d * min;
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z, true);
		}

		// normalize
		internal static void Normalize(ref double x, ref double y) {
			double t = x * x + y * y;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t; y *= t;
			}
		}
	}
}
