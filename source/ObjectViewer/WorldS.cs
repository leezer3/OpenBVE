// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using LibRender;
using OpenBveApi.Math;

namespace OpenBve {
	public static class World {
		// display
		
		
		internal static double ForwardViewingDistance = 100000.0;
		internal static double BackwardViewingDistance = 100000.0;
		internal static double ExtraViewingDistance = 1000.0;
		internal static double BackgroundImageDistance = 100000.0;
		
#pragma warning disable 0649
		internal static TrackManager.TrackFollower CameraTrackFollower;


#pragma warning restore 0649

		

		// absolute camera
		

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = Camera.CurrentAlignment.Zoom;
			AdjustAlignment(ref Camera.CurrentAlignment.Zoom, Camera.AlignmentDirection.Zoom, ref Camera.AlignmentSpeed.Zoom, TimeElapsed, Camera.AlignmentSpeed.Zoom != 0.0);
			if (zm != Camera.CurrentAlignment.Zoom) {
				ApplyZoom();
			}
			// current alignment
			AdjustAlignment(ref Camera.CurrentAlignment.Position.X, Camera.AlignmentDirection.Position.X, ref Camera.AlignmentSpeed.Position.X, TimeElapsed);
			AdjustAlignment(ref Camera.CurrentAlignment.Position.Y, Camera.AlignmentDirection.Position.Y, ref Camera.AlignmentSpeed.Position.Y, TimeElapsed);
			bool q = Camera.AlignmentSpeed.Yaw != 0.0 | Camera.AlignmentSpeed.Pitch != 0.0 | Camera.AlignmentSpeed.Roll != 0.0;
			AdjustAlignment(ref Camera.CurrentAlignment.Yaw, Camera.AlignmentDirection.Yaw, ref Camera.AlignmentSpeed.Yaw, TimeElapsed);
			AdjustAlignment(ref Camera.CurrentAlignment.Pitch, Camera.AlignmentDirection.Pitch, ref Camera.AlignmentSpeed.Pitch, TimeElapsed);
			AdjustAlignment(ref Camera.CurrentAlignment.Roll, Camera.AlignmentDirection.Roll, ref Camera.AlignmentSpeed.Roll, TimeElapsed);
			double tr = Camera.CurrentAlignment.TrackPosition;
			AdjustAlignment(ref Camera.CurrentAlignment.TrackPosition, Camera.AlignmentDirection.TrackPosition, ref Camera.AlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != Camera.CurrentAlignment.TrackPosition) {
				TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, Camera.CurrentAlignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				UpdateViewingDistances();
			}

			Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(Camera.CurrentAlignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			if (Camera.CurrentAlignment.Yaw != 0.0) {
				double cosa = Math.Cos(Camera.CurrentAlignment.Yaw);
				double sina = Math.Sin(Camera.CurrentAlignment.Yaw);
				dF.Rotate(uF, cosa, sina);
				sF.Rotate(uF, cosa, sina);
			}
			double p = Camera.CurrentAlignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				dF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
			}
			if (Camera.CurrentAlignment.Roll != 0.0) {
				double cosa = Math.Cos(-Camera.CurrentAlignment.Roll);
				double sina = Math.Sin(-Camera.CurrentAlignment.Roll);
				uF.Rotate(dF, cosa, sina);
				sF.Rotate(dF, cosa, sina);
			}

			Camera.AbsolutePosition = new Vector3(CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z, CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z, CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z);
			Camera.AbsoluteDirection = dF;
			Camera.AbsoluteUp = uF;
			Camera.AbsoluteSide = sF;
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
			Camera.VerticalViewingAngle = Camera.OriginalVerticalViewingAngle * Math.Exp(Camera.CurrentAlignment.Zoom);
			if (Camera.VerticalViewingAngle < 0.001) Camera.VerticalViewingAngle = 0.001;
			if (Camera.VerticalViewingAngle > 1.5) Camera.VerticalViewingAngle = 1.5;
			Program.UpdateViewport();
		}

		// update viewing distance
		internal static void UpdateViewingDistances() {
			double f = Math.Atan2(World.CameraTrackFollower.WorldDirection.Z, World.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(Camera.AbsoluteDirection.Z, Camera.AbsoluteDirection.X) - f;
			if (c < -Math.PI) {
				c += 2.0 * Math.PI;
			} else if (c > Math.PI) {
				c -= 2.0 * Math.PI;
			}
			double a0 = c - 0.5 * Camera.HorizontalViewingAngle;
			double a1 = c + 0.5 * Camera.HorizontalViewingAngle;
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
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + Camera.CurrentAlignment.Position.Z, true);
		}
	}
}
