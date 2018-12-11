// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using OpenBveApi.Math;
using OpenBveShared;
using TrackManager;

namespace OpenBve {
	public static class World {
		internal static TrackFollower CameraTrackFollower;
		
		
		// absolute camera
		

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = OpenBveShared.Camera.CameraCurrentAlignment.Zoom;
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Zoom, OpenBveShared.Camera.CameraAlignmentDirection.Zoom, ref OpenBveShared.Camera.CameraAlignmentSpeed.Zoom, TimeElapsed, OpenBveShared.Camera.CameraAlignmentSpeed.Zoom != 0.0);
			if (zm != OpenBveShared.Camera.CameraCurrentAlignment.Zoom) {
				ApplyZoom();
			}
			// current alignment
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Position.X, OpenBveShared.Camera.CameraAlignmentDirection.Position.X, ref OpenBveShared.Camera.CameraAlignmentSpeed.Position.X, TimeElapsed);
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Position.Y, OpenBveShared.Camera.CameraAlignmentDirection.Position.Y, ref OpenBveShared.Camera.CameraAlignmentSpeed.Position.Y, TimeElapsed);
			bool q = OpenBveShared.Camera.CameraAlignmentSpeed.Yaw != 0.0 | OpenBveShared.Camera.CameraAlignmentSpeed.Pitch != 0.0 | OpenBveShared.Camera.CameraAlignmentSpeed.Roll != 0.0;
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Yaw, OpenBveShared.Camera.CameraAlignmentDirection.Yaw, ref OpenBveShared.Camera.CameraAlignmentSpeed.Yaw, TimeElapsed);
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Pitch, OpenBveShared.Camera.CameraAlignmentDirection.Pitch, ref OpenBveShared.Camera.CameraAlignmentSpeed.Pitch, TimeElapsed);
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.Roll, OpenBveShared.Camera.CameraAlignmentDirection.Roll, ref OpenBveShared.Camera.CameraAlignmentSpeed.Roll, TimeElapsed);
			double tr = OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition;
			AdjustAlignment(ref OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition, OpenBveShared.Camera.CameraAlignmentDirection.TrackPosition, ref OpenBveShared.Camera.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition) {
				World.CameraTrackFollower.Update(TrackManager.CurrentTrack, OpenBveShared.Camera.CameraCurrentAlignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				UpdateViewingDistances();
			}
			Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(OpenBveShared.Camera.CameraCurrentAlignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			double cx = World.CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z;
			double cy = World.CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z;
			double cz = World.CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z;
			if (OpenBveShared.Camera.CameraCurrentAlignment.Yaw != 0.0) {
				double cosa = Math.Cos(OpenBveShared.Camera.CameraCurrentAlignment.Yaw);
				double sina = Math.Sin(OpenBveShared.Camera.CameraCurrentAlignment.Yaw);
				dF.Rotate(uF, cosa, sina);
				sF.Rotate(uF, cosa, sina);
			}
			double p = OpenBveShared.Camera.CameraCurrentAlignment.Pitch;
			if (p != 0.0) {
				double cosa = Math.Cos(-p);
				double sina = Math.Sin(-p);
				dF.Rotate(sF, cosa, sina);
				uF.Rotate(sF, cosa, sina);
			}
			if (OpenBveShared.Camera.CameraCurrentAlignment.Roll != 0.0) {
				double cosa = Math.Cos(-OpenBveShared.Camera.CameraCurrentAlignment.Roll);
				double sina = Math.Sin(-OpenBveShared.Camera.CameraCurrentAlignment.Roll);
				uF.Rotate(dF, cosa, sina);
				sF.Rotate(dF, cosa, sina);
			}

			Camera.AbsoluteCameraPosition = new Vector3(cx, cy, cz);
			Camera.AbsoluteCameraDirection = new Vector3(dF.X, dF.Y, dF.Z);
			Camera.AbsoluteCameraUp = new Vector3(uF.X, uF.Y, uF.Z);
			Camera.AbsoluteCameraSide = new Vector3(sF.X, sF.Y, sF.Z);
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
			OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle * Math.Exp(OpenBveShared.Camera.CameraCurrentAlignment.Zoom);
			if (OpenBveShared.World.VerticalViewingAngle < 0.001) OpenBveShared.World.VerticalViewingAngle = 0.001;
			if (OpenBveShared.World.VerticalViewingAngle > 1.5) OpenBveShared.World.VerticalViewingAngle = 1.5;
			OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
		}

		// update viewing distance
		internal static void UpdateViewingDistances() {
			double f = Math.Atan2(World.CameraTrackFollower.WorldDirection.Z, World.CameraTrackFollower.WorldDirection.X);
			double c = Math.Atan2(Camera.AbsoluteCameraDirection.Z, Camera.AbsoluteCameraDirection.X) - f;
			if (c < -Math.PI) {
				c += 2.0 * Math.PI;
			} else if (c > Math.PI) {
				c -= 2.0 * Math.PI;
			}
			double a0 = c - 0.5 * OpenBveShared.World.HorizontalViewingAngle;
			double a1 = c + 0.5 * OpenBveShared.World.HorizontalViewingAngle;
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
			double d = OpenBveShared.Renderer.BackgroundImageDistance + OpenBveShared.World.ExtraViewingDistance;
			OpenBveShared.World.ForwardViewingDistance = d * max;
			OpenBveShared.World.BackwardViewingDistance = -d * min;
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + OpenBveShared.Camera.CameraCurrentAlignment.Position.Z, true);
		}

		// ================================

		internal static void RotatePlane(ref Vector3 Vector, double cosa, double sina) {
			double u = Vector.X * cosa - Vector.Z * sina;
			double v = Vector.X * sina + Vector.Z * cosa;
			Vector.X = u;
			Vector.Z = v;
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
