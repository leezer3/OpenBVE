using LibRender2.Cameras;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using System;
using TrainManager.Trains;

namespace OpenBve {
	internal static class World {

		// camera restriction
		internal static void InitializeCameraRestriction() {
			if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On) {
				Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
				UpdateAbsoluteCamera();

				if (Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
				{
					return;
				}
				Program.Renderer.Camera.Alignment = new CameraAlignment();
				Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
				Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
				UpdateAbsoluteCamera();
				Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
				if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction)) {
					Program.Renderer.Camera.Alignment.Position.Z = 0.8;
					UpdateAbsoluteCamera();
					Program.Renderer.Camera.PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Z, 0.0, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction)) {
						Program.Renderer.Camera.Alignment.Position.X = 0.5 * (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction.BottomLeft.X + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction.TopRight.X);
						Program.Renderer.Camera.Alignment.Position.Y = 0.5 * (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction.BottomLeft.Y + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction.TopRight.Y);
						Program.Renderer.Camera.Alignment.Position.Z = 0.0;
						UpdateAbsoluteCamera();
						if (Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction)) {
							Program.Renderer.Camera.PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.X, 0.0, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
							Program.Renderer.Camera.PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Y, 0.0, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
						} else {
							Program.Renderer.Camera.Alignment.Position.Z = 0.8;
							UpdateAbsoluteCamera();
							Program.Renderer.Camera.PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Z, 0.0, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
							if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction)) {
								Program.Renderer.Camera.Alignment = new CameraAlignment();
							}
						}
					}
				}
				UpdateAbsoluteCamera();
			}
		}
		
		/// <summary>Checks whether the camera can move in the selected direction, due to a bounding box.</summary>
		/// <returns>True if we are able to move the camera, false otherwise</returns>
		internal static bool PerformBoundingBoxTest(ref StaticObject bounding, ref Vector3 cameraLocation)
		{
			if (cameraLocation.X < bounding.Mesh.BoundingBox[0].X || cameraLocation.X > bounding.Mesh.BoundingBox[1].X ||
				cameraLocation.Y < bounding.Mesh.BoundingBox[0].Y || cameraLocation.Y > bounding.Mesh.BoundingBox[1].Y ||
				cameraLocation.Z < bounding.Mesh.BoundingBox[0].Z || cameraLocation.Z > bounding.Mesh.BoundingBox[1].Z)
			{
				//Our bounding boxes do not intersect
				return true;
			}
			return false;
		}

		

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed = 0.0) {
			// zoom
			double zm = Program.Renderer.Camera.Alignment.Zoom;
			Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Zoom, Program.Renderer.Camera.AlignmentDirection.Zoom, ref Program.Renderer.Camera.AlignmentSpeed.Zoom, TimeElapsed, true, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
			if (zm != Program.Renderer.Camera.Alignment.Zoom) {
				Program.Renderer.Camera.ApplyZoom();
			}
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy | Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming) {
				// fly-by
				Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.X, Program.Renderer.Camera.AlignmentDirection.Position.X, ref Program.Renderer.Camera.AlignmentSpeed.Position.X, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Y, Program.Renderer.Camera.AlignmentDirection.Position.Y, ref Program.Renderer.Camera.AlignmentSpeed.Position.Y, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				double tr = Program.Renderer.Camera.Alignment.TrackPosition;
				Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.TrackPosition, Program.Renderer.Camera.AlignmentDirection.TrackPosition, ref Program.Renderer.Camera.AlignmentSpeed.TrackPosition, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
				if (tr != Program.Renderer.Camera.Alignment.TrackPosition) {
					Program.Renderer.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.Alignment.TrackPosition, true, false);
					Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
				}
				// position to focus on
				Vector3 focusPosition = Vector3.Zero;
				double zoomMultiplier = 1.0;
				const double heightFactor = 0.75;
				TrainBase bestTrain = null;
				double bestDistanceSquared = double.MaxValue;
				TrainBase secondBestTrain = null;
				double secondBestDistanceSquared = double.MaxValue;
				foreach (TrainBase train in Program.TrainManager.Trains)
				{
					if (train.State != TrainState.Available)
					{
						continue;
					}
					Vector3 focusPos = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition + train.Cars[0].RearAxle.Follower.WorldPosition);
					focusPos.Y += heightFactor * train.Cars[0].Height; // as we want to focus on the approx center height of the car, not the track
					focusPos -= Program.Renderer.CameraTrackFollower.WorldPosition;
					if (focusPos.NormSquared() < bestDistanceSquared)
					{
						secondBestTrain = bestTrain;
						secondBestDistanceSquared = bestDistanceSquared;
						bestTrain = train;
						bestDistanceSquared = focusPos.NormSquared();
					}
					else if (focusPos.NormSquared() < secondBestDistanceSquared)
					{
						secondBestTrain = train;
						secondBestDistanceSquared = focusPos.NormSquared();
					}
				}

				if (bestTrain != null)
				{
					const double maxDistance = 100.0;
					double bestDistance = Math.Sqrt(bestDistanceSquared);
					double secondBestDistance = Math.Sqrt(secondBestDistanceSquared);
					if (secondBestTrain != null && secondBestDistance - bestDistance <= maxDistance)
					{
						Vector3 bestTrainPos = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition + bestTrain.Cars[0].RearAxle.Follower.WorldPosition);
						bestTrainPos.Y += heightFactor * bestTrain.Cars[0].Height;
						Vector3 secondBestTrainPos = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition);
						secondBestTrainPos.Y += heightFactor * secondBestTrain.Cars[0].Height;
						double bt = 0.5 - (secondBestDistance - bestDistance) / (2.0 * maxDistance);
						if (bt < 0.0) bt = 0.0;
						bt = 2.0 * bt * bt; /* in order to change the shape of the interpolation curve */
						focusPosition = (1.0 - bt) * bestTrainPos + bt * secondBestTrainPos;
						zoomMultiplier = 1.0 - 2.0 * bt;
					}
					else
					{
						focusPosition = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition + bestTrain.Cars[0].RearAxle.Follower.WorldPosition);
						focusPosition.Y += heightFactor * bestTrain.Cars[0].Height;
						zoomMultiplier = 1.0;
					}
				}
				// camera
				Program.Renderer.Camera.AbsoluteDirection = new Vector3(Program.Renderer.CameraTrackFollower.WorldDirection);
				Program.Renderer.Camera.AbsolutePosition = Program.Renderer.CameraTrackFollower.WorldPosition + Program.Renderer.CameraTrackFollower.WorldSide * Program.Renderer.Camera.Alignment.Position.X + Program.Renderer.CameraTrackFollower.WorldUp * Program.Renderer.Camera.Alignment.Position.Y + Program.Renderer.Camera.AbsoluteDirection * Program.Renderer.Camera.Alignment.Position.Z;
				Program.Renderer.Camera.AbsoluteDirection = focusPosition - Program.Renderer.Camera.AbsolutePosition;
				double t = Program.Renderer.Camera.AbsoluteDirection.Norm();
				Program.Renderer.Camera.AbsoluteDirection *= Program.Renderer.Camera.AbsoluteDirection.Magnitude();

				Program.Renderer.Camera.AbsoluteSide = new Vector3(Program.Renderer.Camera.AbsoluteDirection.Z, 0.0, -Program.Renderer.Camera.AbsoluteDirection.X);
				Program.Renderer.Camera.AbsoluteSide.Normalize();
				Program.Renderer.Camera.AbsoluteUp = Vector3.Cross(Program.Renderer.Camera.AbsoluteDirection, Program.Renderer.Camera.AbsoluteSide);
				Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
				if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming)
				{
					// zoom
					const double fadeOutDistance = 600.0; /* the distance with the highest zoom factor is half the fade-out distance */
					const double maxZoomFactor = 7.0; /* the zoom factor at half the fade-out distance */
					const double factor = 256.0 / (fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance);
					double zoom;
					if (t < fadeOutDistance)
					{
						double tdist4 = fadeOutDistance - t; tdist4 *= tdist4; tdist4 *= tdist4;
						double t4 = t * t; t4 *= t4;
						zoom = 1.0 + factor * zoomMultiplier * (maxZoomFactor - 1.0) * tdist4 * t4;
					}
					else
					{
						zoom = 1.0;
					}
					Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle / zoom;
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
				}
			} else {
				// non-fly-by
				{
					// current alignment
					Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position, Program.Renderer.Camera.AlignmentDirection.Position, ref Program.Renderer.Camera.AlignmentSpeed.Position, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On) {
						if (Program.Renderer.Camera.Alignment.Position.Z > 0.75) {
							Program.Renderer.Camera.Alignment.Position.Z = 0.75;
						}
					}
					bool q = Program.Renderer.Camera.AlignmentSpeed.Yaw != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Pitch != 0.0 | Program.Renderer.Camera.AlignmentSpeed.Roll != 0.0;
					Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Yaw, Program.Renderer.Camera.AlignmentDirection.Yaw, ref Program.Renderer.Camera.AlignmentSpeed.Yaw, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Pitch, Program.Renderer.Camera.AlignmentDirection.Pitch, ref Program.Renderer.Camera.AlignmentSpeed.Pitch, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.Roll, Program.Renderer.Camera.AlignmentDirection.Roll, ref Program.Renderer.Camera.AlignmentSpeed.Roll, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					double tr = Program.Renderer.Camera.Alignment.TrackPosition;
					Program.Renderer.Camera.AdjustAlignment(ref Program.Renderer.Camera.Alignment.TrackPosition, Program.Renderer.Camera.AlignmentDirection.TrackPosition, ref Program.Renderer.Camera.AlignmentSpeed.TrackPosition, TimeElapsed, false, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction);
					if (tr != Program.Renderer.Camera.Alignment.TrackPosition) {
						Program.Renderer.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.Alignment.TrackPosition, true, false);
						q = true;
					}
					if (q) {
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					}
				}
				// camera
				Vector3 cF = new Vector3(Program.Renderer.CameraTrackFollower.WorldPosition);
				Vector3 dF = new Vector3(Program.Renderer.CameraTrackFollower.WorldDirection);
				Vector3 uF = new Vector3(Program.Renderer.CameraTrackFollower.WorldUp);
				Vector3 sF = new Vector3(Program.Renderer.CameraTrackFollower.WorldSide);
				double lookaheadYaw;
				double lookaheadPitch;
				if (Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) {
					// look-ahead
					double d = 20.0;
					if (TrainManager.PlayerTrain.CurrentSpeed > 0.0) {
						d += 3.0 * (Math.Sqrt(TrainManager.PlayerTrain.CurrentSpeed * TrainManager.PlayerTrain.CurrentSpeed + 1.0) - 1.0);
					}
					d -= TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Position;
					TrackFollower f = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.Clone();
					f.TriggerType = EventTriggerType.None;
					f.UpdateRelative(d, true, false);
					Vector3 r = new Vector3(f.WorldPosition - cF + Program.Renderer.CameraTrackFollower.WorldSide * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + Program.Renderer.CameraTrackFollower.WorldUp * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + Program.Renderer.CameraTrackFollower.WorldDirection * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z);
					r.Normalize();
					double t = dF.Z * (sF.Y * uF.X - sF.X * uF.Y) + dF.Y * (-sF.Z * uF.X + sF.X * uF.Z) + dF.X * (sF.Z * uF.Y - sF.Y * uF.Z);
					if (t != 0.0) {
						t = 1.0 / t;

						double tx = (r.Z * (-dF.Y * uF.X + dF.X * uF.Y) + r.Y * (dF.Z * uF.X - dF.X * uF.Z) + r.X * (-dF.Z * uF.Y + dF.Y * uF.Z)) * t;
						double ty = (r.Z * (dF.Y * sF.X - dF.X * sF.Y) + r.Y * (-dF.Z * sF.X + dF.X * sF.Z) + r.X * (dF.Z * sF.Y - dF.Y * sF.Z)) * t;
						double tz = (r.Z * (sF.Y * uF.X - sF.X * uF.Y) + r.Y * (-sF.Z * uF.X + sF.X * uF.Z) + r.X * (sF.Z * uF.Y - sF.Y * uF.Z)) * t;
						lookaheadYaw = tx * tz != 0.0 ? Math.Atan2(tx, tz) : 0.0;
						if (ty < -1.0) {
							lookaheadPitch = -0.5 * Math.PI;
						} else if (ty > 1.0) {
							lookaheadPitch = 0.5 * Math.PI;
						} else {
							lookaheadPitch = Math.Asin(ty);
						}
					} else {
						lookaheadYaw = 0.0;
						lookaheadPitch = 0.0;
					}
				} else {
					lookaheadYaw = 0.0;
					lookaheadPitch = 0.0;
				}
				{
					// cab pitch and yaw
					Vector3 d2 = new Vector3(dF);
					Vector3 u2 = new Vector3(uF);
					if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
						int c = TrainManager.PlayerTrain.DriverCar;
						if (c >= 0) {
							if (TrainManager.PlayerTrain.Cars[c].CarSections.ContainsKey(CarSectionType.Interior)) {
								d2.Rotate(sF, -TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch);
								u2.Rotate(sF, -TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch);
							}
						}
					}

					cF += sF * Program.Renderer.Camera.Alignment.Position.X + u2 * Program.Renderer.Camera.Alignment.Position.Y + d2 * Program.Renderer.Camera.Alignment.Position.Z;

				}
				// yaw, pitch, roll
				double headYaw = Program.Renderer.Camera.Alignment.Yaw + lookaheadYaw;
				if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headYaw += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverYaw;
					}
				}
				double headPitch = Program.Renderer.Camera.Alignment.Pitch + lookaheadPitch;
				if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headPitch += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
					}
				}
				
				double headRoll = Program.Renderer.Camera.Alignment.Roll;
				// rotation
				if ((Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)  & (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)) {
					double bodyPitch = 0.0;
					double bodyRoll = 0.0;
					// with body and head
					bodyPitch += TrainManager.PlayerTrain.DriverBody.Pitch;
					headPitch -= 0.2 * TrainManager.PlayerTrain.DriverBody.Pitch;
					bodyRoll += TrainManager.PlayerTrain.DriverBody.Roll;
					headRoll += 0.2 * TrainManager.PlayerTrain.DriverBody.Roll;
					// body pitch
					double bpy = (Math.Cos(-bodyPitch) - 1.0) * TrainManager.PlayerTrain.DriverBody.ShoulderHeight;
					double bpz = Math.Sin(-bodyPitch) * TrainManager.PlayerTrain.DriverBody.ShoulderHeight;
					cF += dF * bpz + uF * bpy;
					if (bodyPitch != 0.0)
					{
						dF.Rotate(sF, -bodyPitch);
						uF.Rotate(sF, -bodyPitch);
					}
					// body roll
					double brx = Math.Sin(bodyRoll) * TrainManager.PlayerTrain.DriverBody.ShoulderHeight;
					double bry = (Math.Cos(bodyRoll) - 1.0) * TrainManager.PlayerTrain.DriverBody.ShoulderHeight;
					cF += sF * brx + uF * bry;
					if (bodyRoll != 0.0)
					{
						uF.Rotate(dF, -bodyRoll);
						sF.Rotate(dF, -bodyRoll);
					}
					// head yaw
					double yx = Math.Sin(headYaw) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					double yz = (Math.Cos(headYaw) - 1.0) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					cF += sF * yx + dF * yz;
					if (headYaw != 0.0)
					{
						dF.Rotate(uF, headYaw);
						sF.Rotate(uF, headYaw);
					}
					// head pitch
					double py = (Math.Cos(-headPitch) - 1.0) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					double pz = Math.Sin(-headPitch) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					cF += dF * pz + uF * py;
					if (headPitch != 0.0)
					{
						dF.Rotate(sF, -headPitch);
						uF.Rotate(sF, -headPitch);
					}
					// head roll
					double rx = Math.Sin(headRoll) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					double ry = (Math.Cos(headRoll) - 1.0) * TrainManager.PlayerTrain.DriverBody.HeadHeight;
					cF += sF * rx + uF * ry;
					if (headRoll != 0.0)
					{
						uF.Rotate(dF, -headRoll);
						sF.Rotate(dF, -headRoll);
					}
				} else {
					// without body or head
					if (headYaw != 0.0) {
						dF.Rotate(uF, headYaw);
						sF.Rotate(uF, headYaw);
					}
					if (headPitch != 0.0) {
						dF.Rotate(sF, -headPitch);
						uF.Rotate(sF, -headPitch);
					}
					if (headRoll != 0.0) {
						uF.Rotate(dF, -headRoll);
						sF.Rotate(dF, -headRoll);
					}
				}

				if (Program.Renderer.Camera.CurrentMode < CameraViewMode.Exterior)
				{
					if (TrainManager.PlayerTrain.DriverCar >= 0 && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CarSections.TryGetValue(CarSectionType.Interior, out CarSection interiorSection) && interiorSection.ViewDirection != null)
					{
						dF.Rotate(interiorSection.ViewDirection);
						uF.Rotate(interiorSection.ViewDirection);
						sF.Rotate(interiorSection.ViewDirection);
					}
				}

				// finish
				Program.Renderer.Camera.AbsolutePosition = cF;
				Program.Renderer.Camera.AbsoluteDirection = dF;
				Program.Renderer.Camera.AbsoluteUp = uF;
				Program.Renderer.Camera.AbsoluteSide = sF;
			}
		}
	}
}
