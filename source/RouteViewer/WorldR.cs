// ╔═════════════════════════════════════════════════════════════╗
// ║ World.cs for Object Viewer and Route Viewer                 ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using OpenBveApi.Math;

namespace OpenBve {
	public static class World
	{
		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = Program.Renderer.Camera.Alignment.Zoom;
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Zoom, Program.Renderer.Camera.AlignmentDirection.Zoom, ref Program.Renderer.Camera.AlignmentSpeed.Zoom, TimeElapsed, Program.Renderer.Camera.AlignmentSpeed.Zoom != 0.0);
			if (zm != Program.Renderer.Camera.Alignment.Zoom) {
				Program.Renderer.Camera.ApplyZoom();
			}
			// current alignment
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.X, Program.Renderer.Camera.AlignmentDirection.Position.X, ref Program.Renderer.Camera.AlignmentSpeed.Position.X, TimeElapsed);
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Y, Program.Renderer.Camera.AlignmentDirection.Position.Y, ref Program.Renderer.Camera.AlignmentSpeed.Position.Y, TimeElapsed);
			bool q = Program.Renderer.Camera.AlignmentSpeed.Yaw != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Pitch != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Roll != 0.0;
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Yaw, Program.Renderer.Camera.AlignmentDirection.Yaw, ref Program.Renderer.Camera.AlignmentSpeed.Yaw, TimeElapsed);
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Pitch, Program.Renderer.Camera.AlignmentDirection.Pitch, ref Program.Renderer.Camera.AlignmentSpeed.Pitch, TimeElapsed);
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Roll, Program.Renderer.Camera.AlignmentDirection.Roll, ref Program.Renderer.Camera.AlignmentSpeed.Roll, TimeElapsed);
			double tr = Program.Renderer.Camera.Alignment.TrackPosition;
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.TrackPosition, Program.Renderer.Camera.AlignmentDirection.TrackPosition, ref Program.Renderer.Camera.AlignmentSpeed.TrackPosition, TimeElapsed);
			if (tr != Program.Renderer.Camera.Alignment.TrackPosition) {
				Program.Renderer.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.Alignment.TrackPosition, true, false);
				q = true;
			}
			if (q) {
				Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
			}
			Vector3 dF = new Vector3(Program.Renderer.CameraTrackFollower.WorldDirection);
			Vector3 uF = new Vector3(Program.Renderer.CameraTrackFollower.WorldUp);
			Vector3 sF = new Vector3(Program.Renderer.CameraTrackFollower.WorldSide);
			Vector3 pF = new Vector3(Program.Renderer.Camera.Alignment.Position);
			Vector3 dx2 = new Vector3(dF);
			Vector3 ux2 = new Vector3(uF);
			double cx = Program.Renderer.CameraTrackFollower.WorldPosition.X + sF.X * pF.X + ux2.X * pF.Y + dx2.X * pF.Z;
			double cy = Program.Renderer.CameraTrackFollower.WorldPosition.Y + sF.Y * pF.X + ux2.Y * pF.Y + dx2.Y * pF.Z;
			double cz = Program.Renderer.CameraTrackFollower.WorldPosition.Z + sF.Z * pF.X + ux2.Z * pF.Y + dx2.Z * pF.Z;
			if (Program.Renderer.Camera.Alignment.Yaw != 0.0) {
				dF.Rotate(uF, Program.Renderer.Camera.Alignment.Yaw);
				sF.Rotate(uF, Program.Renderer.Camera.Alignment.Yaw);
			}
			double p = Program.Renderer.Camera.Alignment.Pitch;
			if (p != 0.0) {
				dF.Rotate(sF, -p);
				uF.Rotate(sF, -p);
			}
			if (Program.Renderer.Camera.Alignment.Roll != 0.0) {
				uF.Rotate(dF, -Program.Renderer.Camera.Alignment.Roll);
				sF.Rotate(dF, -Program.Renderer.Camera.Alignment.Roll);
			}

			Program.Renderer.Camera.AbsolutePosition = new Vector3(cx, cy, cz);
			Program.Renderer.Camera.AbsoluteDirection = dF;
			Program.Renderer.Camera.AbsoluteUp = uF;
			Program.Renderer.Camera.AbsoluteSide = sF;
		}
	}
}
