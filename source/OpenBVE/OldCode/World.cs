#pragma warning disable 0660 // Defines == or != but does not override Object.Equals
#pragma warning disable 0661 // Defines == or != but does not override Object.GetHashCode

using System;
using LibRender2.Cameras;
using LibRender2.Viewports;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;

namespace OpenBve {
	internal static class World {
		// display
		
		
		
		
		

		// driver body
		
		
		
		// mouse grab
		internal static bool MouseGrabEnabled = false;
		internal static bool MouseGrabIgnoreOnce = false;
		internal static Vector2 MouseGrabTarget = new Vector2(0.0, 0.0);
		internal static void UpdateMouseGrab(double TimeElapsed) {
			if (MouseGrabEnabled) {
				double factor;
				if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) {
					factor = 1.0;
				} else {
					factor = 3.0;
				}

				Program.Renderer.Camera.AlignmentDirection.Yaw += factor * MouseGrabTarget.X;
				Program.Renderer.Camera.AlignmentDirection.Pitch -= factor * MouseGrabTarget.Y;
				MouseGrabTarget = Vector2.Null;
			}
		}
		
		// relative camera
		
		internal static TrackFollower CameraTrackFollower;
		
		
		
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		internal static int CameraCar;
		
		// camera memory
		internal static CameraAlignment CameraSavedExterior;
		internal static CameraAlignment CameraSavedTrack;

		// camera restriction
		


		// absolute camera
		

		// camera restriction
		internal static void InitializeCameraRestriction() {
			if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On) {
				Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
				UpdateAbsoluteCamera(0.0);
				if (!Program.Renderer.Camera.PerformRestrictionTest()) {
					Program.Renderer.Camera.Alignment = new CameraAlignment();
					Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
					Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					UpdateAbsoluteCamera(0.0);
					UpdateViewingDistances();
					if (!Program.Renderer.Camera.PerformRestrictionTest()) {
						Program.Renderer.Camera.Alignment.Position.Z = 0.8;
						UpdateAbsoluteCamera(0.0);
						PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Z, 0.0, true);
						if (!Program.Renderer.Camera.PerformRestrictionTest()) {
							Program.Renderer.Camera.Alignment.Position.X = 0.5 * (Program.Renderer.Camera.RestrictionBottomLeft.X + Program.Renderer.Camera.RestrictionTopRight.X);
							Program.Renderer.Camera.Alignment.Position.Y = 0.5 * (Program.Renderer.Camera.RestrictionBottomLeft.Y + Program.Renderer.Camera.RestrictionTopRight.Y);
							Program.Renderer.Camera.Alignment.Position.Z = 0.0;
							UpdateAbsoluteCamera(0.0);
							if (Program.Renderer.Camera.PerformRestrictionTest()) {
								PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.X, 0.0, true);
								PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Y, 0.0, true);
							} else {
								Program.Renderer.Camera.Alignment.Position.Z = 0.8;
								UpdateAbsoluteCamera(0.0);
								PerformProgressiveAdjustmentForCameraRestriction(ref Program.Renderer.Camera.Alignment.Position.Z, 0.0, true);
								if (!Program.Renderer.Camera.PerformRestrictionTest()) {
									Program.Renderer.Camera.Alignment = new CameraAlignment();
								}
							}
						}
					}
					UpdateAbsoluteCamera(0.0);
				}
			}
		}
		internal static bool PerformProgressiveAdjustmentForCameraRestriction(ref double Source, double Target, bool Zoom) {
			if ((Program.Renderer.Camera.CurrentMode != CameraViewMode.Interior & Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead) | Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.On) {
				Source = Target;
				return true;
			}
			double best = Source;
			const int Precision = 8;
			double a = Source;
			double b = Target;
			Source = Target;
			if (Zoom) ApplyZoom();
			if (Program.Renderer.Camera.PerformRestrictionTest()) {
				return true;
			}
			double x = 0.5 * (a + b);
			bool q = true;
			for (int i = 0; i < Precision; i++) {
				//Do not remove, this is updated via the ref & causes the panel zoom to bug out
				Source = x;
				if (Zoom) ApplyZoom();
				q = Program.Renderer.Camera.PerformRestrictionTest();
				if (q) {
					a = x;
					best = x;
				} else {
					b = x;
				}
				x = 0.5 * (a + b);
			}
			Source = best;
			if (Zoom) ApplyZoom();
			return q;
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
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = Program.Renderer.Camera.Alignment.Zoom;
			AdjustAlignment(ref Program.Renderer.Camera.Alignment.Zoom, Program.Renderer.Camera.AlignmentDirection.Zoom, ref Program.Renderer.Camera.AlignmentSpeed.Zoom, TimeElapsed, true);
			if (zm != Program.Renderer.Camera.Alignment.Zoom) {
				ApplyZoom();
			}
			if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy | Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming) {
				// fly-by
				AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.X, Program.Renderer.Camera.AlignmentDirection.Position.X, ref Program.Renderer.Camera.AlignmentSpeed.Position.X, TimeElapsed);
				AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Y, Program.Renderer.Camera.AlignmentDirection.Position.Y, ref Program.Renderer.Camera.AlignmentSpeed.Position.Y, TimeElapsed);
				double tr = Program.Renderer.Camera.Alignment.TrackPosition;
				AdjustAlignment(ref Program.Renderer.Camera.Alignment.TrackPosition, Program.Renderer.Camera.AlignmentDirection.TrackPosition, ref Program.Renderer.Camera.AlignmentSpeed.TrackPosition, TimeElapsed);
				if (tr != Program.Renderer.Camera.Alignment.TrackPosition) {
					World.CameraTrackFollower.UpdateAbsolute(Program.Renderer.Camera.Alignment.TrackPosition, true, false);
					UpdateViewingDistances();
				}
				// position to focus on
				Vector3 focusPosition = Vector3.Zero;
				double zoomMultiplier;
				{
					const double heightFactor = 0.75;
					TrainManager.Train bestTrain = null;
					double bestDistanceSquared = double.MaxValue;
					TrainManager.Train secondBestTrain = null;
					double secondBestDistanceSquared = double.MaxValue;
					foreach (TrainManager.Train train in TrainManager.Trains) {
						if (train.State == TrainState.Available) {
							double x = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.X + train.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Y + train.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * train.Cars[0].Height;
							double z = 0.5 * (train.Cars[0].FrontAxle.Follower.WorldPosition.Z + train.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double dx = x - CameraTrackFollower.WorldPosition.X;
							double dy = y - CameraTrackFollower.WorldPosition.Y;
							double dz = z - CameraTrackFollower.WorldPosition.Z;
							double d = dx * dx + dy * dy + dz * dz;
							if (d < bestDistanceSquared) {
								secondBestTrain = bestTrain;
								secondBestDistanceSquared = bestDistanceSquared;
								bestTrain = train;
								bestDistanceSquared = d;
							} else if (d < secondBestDistanceSquared) {
								secondBestTrain = train;
								secondBestDistanceSquared = d;
							}
						}
					}
					if (bestTrain != null) {
						const double maxDistance = 100.0;
						double bestDistance = Math.Sqrt(bestDistanceSquared);
						double secondBestDistance = Math.Sqrt(secondBestDistanceSquared);
						if (secondBestTrain != null && secondBestDistance - bestDistance <= maxDistance) {
							double x1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							double z1 = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double x2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							double y2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * secondBestTrain.Cars[0].Height;
							double z2 = 0.5 * (secondBestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + secondBestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							double t = 0.5 - (secondBestDistance - bestDistance) / (2.0 * maxDistance);
							if (t < 0.0) t = 0.0;
							t = 2.0 * t * t; /* in order to change the shape of the interpolation curve */
							focusPosition.X = (1.0 - t) * x1 + t * x2;
							focusPosition.Y = (1.0 - t) * y1 + t * y2;
							focusPosition.Z = (1.0 - t) * z1 + t * z2;
							zoomMultiplier = 1.0 - 2.0 * t;
						} else {
							focusPosition.X = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							focusPosition.Y = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							focusPosition.Z = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							zoomMultiplier = 1.0;
						}
					} else {
						zoomMultiplier = 1.0;
					}
				}
				// camera
				{
					Program.Renderer.Camera.AbsoluteDirection = new Vector3(CameraTrackFollower.WorldDirection);
					Program.Renderer.Camera.AbsolutePosition = CameraTrackFollower.WorldPosition + CameraTrackFollower.WorldSide * Program.Renderer.Camera.Alignment.Position.X + CameraTrackFollower.WorldUp * Program.Renderer.Camera.Alignment.Position.Y + Program.Renderer.Camera.AbsoluteDirection * Program.Renderer.Camera.Alignment.Position.Z;
					Program.Renderer.Camera.AbsoluteDirection = focusPosition - Program.Renderer.Camera.AbsolutePosition;
					double t = Program.Renderer.Camera.AbsoluteDirection.Norm();
					double ti = 1.0 / t;
					Program.Renderer.Camera.AbsoluteDirection *= ti;

					Program.Renderer.Camera.AbsoluteSide = new Vector3(Program.Renderer.Camera.AbsoluteDirection.Z, 0.0, -Program.Renderer.Camera.AbsoluteDirection.X);
					Program.Renderer.Camera.AbsoluteSide.Normalize();
					Program.Renderer.Camera.AbsoluteUp = Vector3.Cross(Program.Renderer.Camera.AbsoluteDirection, Program.Renderer.Camera.AbsoluteSide);
					UpdateViewingDistances();
					if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming) {
						// zoom
						const double fadeOutDistance = 600.0; /* the distance with the highest zoom factor is half the fade-out distance */
						const double maxZoomFactor = 7.0; /* the zoom factor at half the fade-out distance */
						const double factor = 256.0 / (fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance * fadeOutDistance);
						double zoom;
						if (t < fadeOutDistance) {
							double tdist4 = fadeOutDistance - t; tdist4 *= tdist4; tdist4 *= tdist4;
							double t4 = t * t; t4 *= t4;
							zoom = 1.0 + factor * zoomMultiplier * (maxZoomFactor - 1.0) * tdist4 * t4;
						} else {
							zoom = 1.0;
						}
						Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle / zoom;
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
					}
				}
			} else {
				// non-fly-by
				{
					// current alignment
					AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.X, Program.Renderer.Camera.AlignmentDirection.Position.X, ref Program.Renderer.Camera.AlignmentSpeed.Position.X, TimeElapsed);
					AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Y, Program.Renderer.Camera.AlignmentDirection.Position.Y, ref Program.Renderer.Camera.AlignmentSpeed.Position.Y, TimeElapsed);
					AdjustAlignment(ref Program.Renderer.Camera.Alignment.Position.Z, Program.Renderer.Camera.AlignmentDirection.Position.Z, ref Program.Renderer.Camera.AlignmentSpeed.Position.Z, TimeElapsed);
					if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On) {
						if (Program.Renderer.Camera.Alignment.Position.Z > 0.75) {
							Program.Renderer.Camera.Alignment.Position.Z = 0.75;
						}
					}
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
				}
				// camera
				Vector3 cF = new Vector3(CameraTrackFollower.WorldPosition);
				Vector3 dF = new Vector3(CameraTrackFollower.WorldDirection);
				Vector3 uF = new Vector3(CameraTrackFollower.WorldUp);
				Vector3 sF = new Vector3(CameraTrackFollower.WorldSide);
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
					Vector3 r = new Vector3(f.WorldPosition - cF + World.CameraTrackFollower.WorldSide * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z);
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
							if (TrainManager.PlayerTrain.Cars[c].CarSections.Length == 0 || !TrainManager.PlayerTrain.Cars[c].CarSections[0].Groups[0].Overlay) {
								double a = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
								double cosa = Math.Cos(-a);
								double sina = Math.Sin(-a);
								d2.Rotate(sF, cosa, sina);
								u2.Rotate(sF, cosa, sina);
							}
						}
					}

					cF += sF * Program.Renderer.Camera.Alignment.Position.X + u2 * Program.Renderer.Camera.Alignment.Position + d2 * Program.Renderer.Camera.Alignment.Position.Z;

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
				double bodyPitch = 0.0;
				double bodyRoll = 0.0;
				double headRoll = Program.Renderer.Camera.Alignment.Roll;
				// rotation
				if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable & (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior | Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)) {
					// with body and head
					bodyPitch += TrainManager.PlayerTrain.DriverBody.Pitch;
					headPitch -= 0.2 * TrainManager.PlayerTrain.DriverBody.Pitch;
					bodyRoll += TrainManager.PlayerTrain.DriverBody.Roll;
					headRoll += 0.2 * TrainManager.PlayerTrain.DriverBody.Roll;
					const double bodyHeight = 0.6;
					const double headHeight = 0.1;
					{
						// body pitch
						double ry = (Math.Cos(-bodyPitch) - 1.0) * bodyHeight;
						double rz = Math.Sin(-bodyPitch) * bodyHeight;
						cF += dF * rz + uF * ry;
						if (bodyPitch != 0.0) {
							double cosa = Math.Cos(-bodyPitch);
							double sina = Math.Sin(-bodyPitch);
							dF.Rotate(sF, cosa, sina);
							uF.Rotate(sF, cosa, sina);
						}
					}
					{
						// body roll
						double rx = Math.Sin(bodyRoll) * bodyHeight;
						double ry = (Math.Cos(bodyRoll) - 1.0) * bodyHeight;
						cF += sF * rx + uF * ry;
						if (bodyRoll != 0.0) {
							double cosa = Math.Cos(-bodyRoll);
							double sina = Math.Sin(-bodyRoll);
							uF.Rotate(dF, cosa, sina);
							sF.Rotate(dF, cosa, sina);
						}
					}
					{
						// head yaw
						double rx = Math.Sin(headYaw) * headHeight;
						double rz = (Math.Cos(headYaw) - 1.0) * headHeight;
						cF += sF * rx + dF * rz;
						if (headYaw != 0.0) {
							double cosa = Math.Cos(headYaw);
							double sina = Math.Sin(headYaw);
							dF.Rotate(uF, cosa, sina);
							sF.Rotate(uF, cosa, sina);
						}
					}
					{
						// head pitch
						double ry = (Math.Cos(-headPitch) - 1.0) * headHeight;
						double rz = Math.Sin(-headPitch) * headHeight;
						cF += dF * rz + uF * ry;
						if (headPitch != 0.0) {
							double cosa = Math.Cos(-headPitch);
							double sina = Math.Sin(-headPitch);
							dF.Rotate(sF, cosa, sina);
							uF.Rotate(sF, cosa, sina);
						}
					}
					{
						// head roll
						double rx = Math.Sin(headRoll) * headHeight;
						double ry = (Math.Cos(headRoll) - 1.0) * headHeight;
						cF += sF * rx + uF * ry;
						if (headRoll != 0.0) {
							double cosa = Math.Cos(-headRoll);
							double sina = Math.Sin(-headRoll);
							uF.Rotate(dF, cosa, sina);
							sF.Rotate(dF, cosa, sina);
						}
					}
				} else {
					// without body or head
					double totalYaw = headYaw;
					double totalPitch = headPitch + bodyPitch;
					double totalRoll = bodyRoll + headRoll;
					if (totalYaw != 0.0) {
						double cosa = Math.Cos(totalYaw);
						double sina = Math.Sin(totalYaw);
						dF.Rotate(uF, cosa, sina);
						sF.Rotate(uF, cosa, sina);
					}
					if (totalPitch != 0.0) {
						double cosa = Math.Cos(-totalPitch);
						double sina = Math.Sin(-totalPitch);
						dF.Rotate(sF, cosa, sina);
						uF.Rotate(sF, cosa, sina);
					}
					if (totalRoll != 0.0) {
						double cosa = Math.Cos(-totalRoll);
						double sina = Math.Sin(-totalRoll);
						uF.Rotate(dF, cosa, sina);
						sF.Rotate(dF, cosa, sina);
					}
				}
				// finish
				Program.Renderer.Camera.AbsolutePosition = cF;
				Program.Renderer.Camera.AbsoluteDirection = dF;
				Program.Renderer.Camera.AbsoluteUp = uF;
				Program.Renderer.Camera.AbsoluteSide = sF;
			}
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed) {
			AdjustAlignment(ref Source, Direction, ref Speed, TimeElapsed, false);
		}
		private static void AdjustAlignment(ref double Source, double Direction, ref double Speed, double TimeElapsed, bool Zoom) {
			if (Direction != 0.0 | Speed != 0.0) {
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
					double x = Source + Speed * TimeElapsed;
					if (!PerformProgressiveAdjustmentForCameraRestriction(ref Source, x, Zoom)) {
						Speed = 0.0;
					}
				}
			}
		}
		private static void ApplyZoom() {
			Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle * Math.Exp(Program.Renderer.Camera.Alignment.Zoom);
			if (Program.Renderer.Camera.VerticalViewingAngle < 0.001) Program.Renderer.Camera.VerticalViewingAngle = 0.001;
			if (Program.Renderer.Camera.VerticalViewingAngle > 1.5) Program.Renderer.Camera.VerticalViewingAngle = 1.5;
			Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
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
			Program.Renderer.UpdateVisibility(CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z, true);
		}

		// ================================
		
		

		// normalize
		internal static void Normalize(ref double x, ref double y) {
			double t = x * x + y * y;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}
	}
}
