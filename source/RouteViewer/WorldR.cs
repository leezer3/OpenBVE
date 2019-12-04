// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Math;
using OpenBveApi.Routes;

namespace OpenBve {
	public static class World
	{
		internal static TrackFollower CameraTrackFollower = new TrackFollower(Program.CurrentHost);

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = Program.Renderer.Camera.Alignment.Zoom;
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Zoom, Program.Renderer.Camera.AlignmentDirection.Zoom, ref Program.Renderer.Camera.AlignmentSpeed.Zoom, TimeElapsed, Program.Renderer.Camera.AlignmentSpeed.Zoom != 0.0);
			if (zm != Program.Renderer.Camera.Alignment.Zoom) {
				ApplyZoom();
			}
			// current alignment
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.X, Program.Renderer.Camera.AlignmentDirection.Position.X, ref Program.Renderer.Camera.AlignmentSpeed.Position.X, TimeElapsed);
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Y, Program.Renderer.Camera.AlignmentDirection.Position.Y, ref Program.Renderer.Camera.AlignmentSpeed.Position.Y, TimeElapsed);
			bool q = Program.Renderer.Camera.AlignmentSpeed.Yaw != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Pitch != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Roll != 0.0;
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Yaw, Program.Renderer.Camera.AlignmentDirection.Yaw, ref Program.Renderer.Camera.AlignmentSpeed.Yaw, TimeElapsed);
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Pitch, Program.Renderer.Camera.AlignmentDirection.Pitch, ref Program.Renderer.Camera.AlignmentSpeed.Pitch, TimeElapsed);
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Roll, Program.Renderer.Camera.AlignmentDirection.Roll, ref Program.Renderer.Camera.AlignmentSpeed.Roll, TimeElapsed);
			double tr = Program.Renderer.Camera.Alignment.TrackPosition;
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.TrackPosition, Program.Renderer.Camera.AlignmentDirection.TrackPosition, ref Program.Renderer.Camera.AlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != Program.Renderer.Camera.Alignment.TrackPosition) {
				World.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.Alignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				UpdateViewingDistances();
			}
			Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(Program.Renderer.Camera.Alignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			double cx = World.CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z;
			double cy = World.CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z;
			double cz = World.CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z;
			if (Program.Renderer.Camera.Alignment.Yaw != 0.0) {
				double cosa = Math.Cos(Program.Renderer.Camera.Alignment.Yaw);
				double sina = Math.Sin(Program.Renderer.Camera.Alignment.Yaw);
				dF.Rotate(uF, cosa, sina);
				sF.Rotate(uF, cosa, sina);
			}
			double p = Program.Renderer.Camera.Alignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				dF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
			}
			if (Program.Renderer.Camera.Alignment.Roll != 0.0) {
				double cosa = Math.Cos(-Program.Renderer.Camera.Alignment.Roll);
				double sina = Math.Sin(-Program.Renderer.Camera.Alignment.Roll);
				uF.Rotate(dF, cosa, sina);
				sF.Rotate(dF, cosa, sina);
			}

			Program.Renderer.Camera.AbsolutePosition = new Vector3(cx, cy, cz);
			Program.Renderer.Camera.AbsoluteDirection = dF;
			Program.Renderer.Camera.AbsoluteUp = uF;
			Program.Renderer.Camera.AbsoluteSide = sF;
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
			Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle * Math.Exp(Program.Renderer.Camera.Alignment.Zoom);
			if (Program.Renderer.Camera.VerticalViewingAngle < 0.001) Program.Renderer.Camera.VerticalViewingAngle = 0.001;
			if (Program.Renderer.Camera.VerticalViewingAngle > 1.5) Program.Renderer.Camera.VerticalViewingAngle = 1.5;
			Program.Renderer.UpdateViewport();
		}

		// update viewing distance
		internal static void UpdateViewingDistances() {
			double f = Math.Atan2(World.CameraTrackFollower.WorldDirection.Z, World.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(Program.Renderer.Camera.AbsoluteDirection.Z, Program.Renderer.Camera.AbsoluteDirection.X) - f;
			if (c < -Math.PI) {
				c += 2.0 * Math.PI;
			} else if (c > Math.PI) {
				c -= 2.0 * Math.PI;
			}
			double a0 = c - 0.5 * Program.Renderer.Camera.HorizontalViewingAngle;
			double a1 = c + 0.5 * Program.Renderer.Camera.HorizontalViewingAngle;
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
			double d = Program.CurrentRoute.CurrentBackground.BackgroundImageDistance + Program.Renderer.Camera.ExtraViewingDistance;
			Program.Renderer.Camera.ForwardViewingDistance = d * max;
			Program.Renderer.Camera.BackwardViewingDistance = -d * min;
			Program.Renderer.UpdateVisibility(World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z, true);
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
