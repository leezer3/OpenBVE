#pragma warning disable 0660 // Defines == or != but does not override Object.Equals
#pragma warning disable 0661 // Defines == or != but does not override Object.GetHashCode

using System;
using OpenBveApi;
using OpenBveApi.Math;
using Vector2 = OpenBveApi.Math.Vector2;
using OpenBveApi.Objects;
using OpenBveApi.Runtime;
using OpenBveShared;
using TrackManager;

namespace OpenBve {
	internal static partial class World {
		// driver body
		internal struct DriverBody {
			internal double SlowX;
			internal double FastX;
			internal double Roll;
			internal Damping RollDamping;
			internal double SlowY;
			internal double FastY;
			internal double Pitch;
			internal Damping PitchDamping;

			internal void Update(double TimeElapsed) {
			if (Camera.CameraRestriction == CameraRestrictionMode.NotAvailable) {
				{
					// pitch
					double targetY = TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
					const double accelerationSlow = 0.25;
					const double accelerationFast = 2.0;
					if (SlowY < targetY) {
						SlowY += accelerationSlow * TimeElapsed;
						if (SlowY > targetY) {
							SlowY = targetY;
						}
					} else if (SlowY > targetY) {
						SlowY -= accelerationSlow * TimeElapsed;
						if (SlowY < targetY) {
							SlowY = targetY;
						}
					}
					if (FastY < targetY) {
						FastY += accelerationFast * TimeElapsed;
						if (FastY > targetY) {
							FastY = targetY;
						}
					} else if (FastY > targetY) {
						FastY -= accelerationFast * TimeElapsed;
						if (FastY < targetY) {
							FastY = targetY;
						}
					}
					double diffY = FastY - SlowY;
					diffY = (double)Math.Sign(diffY) * diffY * diffY;
					Pitch = 0.5 * Math.Atan(0.1 * diffY);
					if (Pitch > 0.1) {
						Pitch = 0.1;
					}
					if (PitchDamping == null) {
						PitchDamping = new Damping(6.0, 0.3);
					}
					PitchDamping.Update(TimeElapsed, ref Pitch, true);
				}
				{
					// roll
					int c = TrainManager.PlayerTrain.DriverCar;
					double frontRadius = TrainManager.PlayerTrain.Cars[c].FrontAxle.Follower.CurveRadius;
					double rearRadius = TrainManager.PlayerTrain.Cars[c].RearAxle.Follower.CurveRadius;
					double radius;
					if (frontRadius != 0.0 & rearRadius != 0.0) {
						if (frontRadius != -rearRadius) {
							radius = 2.0 * frontRadius * rearRadius / (frontRadius + rearRadius);
						} else {
							radius = 0.0;
						}
					} else if (frontRadius != 0.0) {
						radius = 2.0 * frontRadius;
					} else if (rearRadius != 0.0) {
						radius = 2.0 * rearRadius;
					} else {
						radius = 0.0;
					}
					double targetX;
					if (radius != 0.0) {
						double speed = TrainManager.PlayerTrain.Cars[c].Specs.CurrentSpeed;
						targetX = speed * speed / radius;
					} else {
						targetX = 0.0;
					}
					const double accelerationSlow = 1.0;
					const double accelerationFast = 10.0;
					if (SlowX < targetX) {
						SlowX += accelerationSlow * TimeElapsed;
						if (SlowX > targetX) {
							SlowX = targetX;
						}
					} else if (SlowX > targetX) {
						SlowX -= accelerationSlow * TimeElapsed;
						if (SlowX < targetX) {
							SlowX = targetX;
						}
					}
					if (FastX < targetX) {
						FastX += accelerationFast * TimeElapsed;
						if (FastX > targetX) {
							FastX = targetX;
						}
					} else if (FastX > targetX) {
						FastX -= accelerationFast * TimeElapsed;
						if (FastX < targetX) {
							FastX = targetX;
						}
					}
					double diffX = SlowX - FastX;
					diffX = (double)Math.Sign(diffX) * diffX * diffX;
					Roll = 0.5 * Math.Atan(0.3 * diffX);
					if (RollDamping == null) {
						RollDamping = new Damping(6.0, 0.3);
					}
					RollDamping.Update(TimeElapsed, ref Roll, true);
				}
			}
		}
		}
		internal static DriverBody CurrentDriverBody;
		
		
		// mouse grab
		internal static bool MouseGrabEnabled = false;
		internal static bool MouseGrabIgnoreOnce = false;
		internal static Vector2 MouseGrabTarget = Vector2.Null;
		internal static void UpdateMouseGrab(double TimeElapsed) {
			if (MouseGrabEnabled) {
				double factor;
				if (Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) {
					factor = 1.0;
				} else {
					factor = 3.0;
				}

				Camera.CameraAlignmentDirection.Yaw += factor * MouseGrabTarget.X;
				Camera.CameraAlignmentDirection.Pitch -= factor * MouseGrabTarget.Y;
				MouseGrabTarget = Vector2.Null;
			}
		}
		
		// relative camera
		
		internal static TrackFollower CameraTrackFollower;
		internal static bool CameraAtWorldEnd;
		
		
		
		
		/// <summary>The index of the car which the camera is currently anchored to</summary>
		internal static int CameraCar;
		
		// camera memory
		internal static CameraAlignment CameraSavedExterior;
		internal static CameraAlignment CameraSavedTrack;

		// camera restriction
		internal static Vector3 CameraRestrictionBottomLeft = new Vector3(-1.0, -1.0, 1.0);
		internal static Vector3 CameraRestrictionTopRight = new Vector3(1.0, 1.0, 1.0);

		// absolute camera
		

		// camera restriction
		internal static void InitializeCameraRestriction() {
			if ((Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) & Camera.CameraRestriction == CameraRestrictionMode.On) {
				Camera.CameraAlignmentSpeed = new CameraAlignment();
				UpdateAbsoluteCamera(0.0);
				if (!PerformCameraRestrictionTest()) {
					Camera.CameraCurrentAlignment = new CameraAlignment();
					OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle;
					OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
					UpdateAbsoluteCamera(0.0);
					UpdateViewingDistances();
					if (!PerformCameraRestrictionTest()) {
						Camera.CameraCurrentAlignment.Position.Z = 0.8;
						UpdateAbsoluteCamera(0.0);
						PerformProgressiveAdjustmentForCameraRestriction(ref Camera.CameraCurrentAlignment.Position.Z, 0.0, true);
						if (!PerformCameraRestrictionTest()) {
							Camera.CameraCurrentAlignment.Position.X = 0.5 * (CameraRestrictionBottomLeft.X + CameraRestrictionTopRight.X);
							Camera.CameraCurrentAlignment.Position.Y = 0.5 * (CameraRestrictionBottomLeft.Y + CameraRestrictionTopRight.Y);
							Camera.CameraCurrentAlignment.Position.Z = 0.0;
							UpdateAbsoluteCamera(0.0);
							if (PerformCameraRestrictionTest()) {
								PerformProgressiveAdjustmentForCameraRestriction(ref Camera.CameraCurrentAlignment.Position.X, 0.0, true);
								PerformProgressiveAdjustmentForCameraRestriction(ref Camera.CameraCurrentAlignment.Position.Y, 0.0, true);
							} else {
								Camera.CameraCurrentAlignment.Position.Z = 0.8;
								UpdateAbsoluteCamera(0.0);
								PerformProgressiveAdjustmentForCameraRestriction(ref Camera.CameraCurrentAlignment.Position.Z, 0.0, true);
								if (!PerformCameraRestrictionTest()) {
									Camera.CameraCurrentAlignment = new CameraAlignment();
								}
							}
						}
					}
					UpdateAbsoluteCamera(0.0);
				}
			}
		}
		internal static bool PerformProgressiveAdjustmentForCameraRestriction(ref double Source, double Target, bool Zoom) {
			if ((Camera.CameraView != CameraViewMode.Interior & Camera.CameraView != CameraViewMode.InteriorLookAhead) | Camera.CameraRestriction != CameraRestrictionMode.On) {
				Source = Target;
				return true;
			}
			double best = Source;
			const int Precision = 8;
			double a = Source;
			double b = Target;
			Source = Target;
			if (Zoom) ApplyZoom();
			if (PerformCameraRestrictionTest()) {
				return true;
			}
			double x = 0.5 * (a + b);
			bool q = true;
			for (int i = 0; i < Precision; i++) {
				//Do not remove, this is updated via the ref & causes the panel zoom to bug out
				Source = x;
				if (Zoom) ApplyZoom();
				q = PerformCameraRestrictionTest();
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

		internal static bool PerformCameraRestrictionTest()
		{
			if (Camera.CameraRestriction == CameraRestrictionMode.On) {
				Vector3[] p = new Vector3[] { CameraRestrictionBottomLeft, CameraRestrictionTopRight };
				Vector2[] r = new Vector2[2];
				for (int j = 0; j < 2; j++) {
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
					r[j].X = ex / (ez * Math.Tan(0.5 * OpenBveShared.World.HorizontalViewingAngle));
					r[j].Y = ey / (ez * Math.Tan(0.5 * OpenBveShared.World.VerticalViewingAngle));
				}
				return r[0].X <= -1.0025 & r[1].X >= 1.0025 & r[0].Y <= -1.0025 & r[1].Y >= 1.0025;
			}
			return true;
		}

		// update absolute camera
		internal static void UpdateAbsoluteCamera(double TimeElapsed) {
			// zoom
			double zm = Camera.CameraCurrentAlignment.Zoom;
			AdjustAlignment(ref Camera.CameraCurrentAlignment.Zoom, Camera.CameraAlignmentDirection.Zoom, ref Camera.CameraAlignmentSpeed.Zoom, TimeElapsed, true);
			if (zm != Camera.CameraCurrentAlignment.Zoom) {
				ApplyZoom();
			}
			if (Camera.CameraView == CameraViewMode.FlyBy | Camera.CameraView == CameraViewMode.FlyByZooming) {
				// fly-by
				AdjustAlignment(ref Camera.CameraCurrentAlignment.Position.X, Camera.CameraAlignmentDirection.Position.X, ref Camera.CameraAlignmentSpeed.Position.X, TimeElapsed);
				AdjustAlignment(ref Camera.CameraCurrentAlignment.Position.Y, Camera.CameraAlignmentDirection.Position.Y, ref Camera.CameraAlignmentSpeed.Position.Y, TimeElapsed);
				double tr = Camera.CameraCurrentAlignment.TrackPosition;
				AdjustAlignment(ref Camera.CameraCurrentAlignment.TrackPosition, Camera.CameraAlignmentDirection.TrackPosition, ref Camera.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
				if (tr != Camera.CameraCurrentAlignment.TrackPosition) {
					World.CameraTrackFollower.Update(TrackManager.CurrentTrack, Camera.CameraCurrentAlignment.TrackPosition, true, false);
					UpdateViewingDistances();
				}
				// camera
				double px = World.CameraTrackFollower.WorldPosition.X;
				double py = World.CameraTrackFollower.WorldPosition.Y;
				double pz = World.CameraTrackFollower.WorldPosition.Z;
				// position to focus on
				double tx, ty, tz;
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
							double dx = x - px;
							double dy = y - py;
							double dz = z - pz;
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
							tx = (1.0 - t) * x1 + t * x2;
							ty = (1.0 - t) * y1 + t * y2;
							tz = (1.0 - t) * z1 + t * z2;
							zoomMultiplier = 1.0 - 2.0 * t;
						} else {
							tx = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.X + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.X);
							ty = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Y + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Y) + heightFactor * bestTrain.Cars[0].Height;
							tz = 0.5 * (bestTrain.Cars[0].FrontAxle.Follower.WorldPosition.Z + bestTrain.Cars[0].RearAxle.Follower.WorldPosition.Z);
							zoomMultiplier = 1.0;
						}
					} else {
						tx = 0.0;
						ty = 0.0;
						tz = 0.0;
						zoomMultiplier = 1.0;
					}
				}
				// camera
				{
					double dx = World.CameraTrackFollower.WorldDirection.X;
					double dy = World.CameraTrackFollower.WorldDirection.Y;
					double dz = World.CameraTrackFollower.WorldDirection.Z;
					double ux = World.CameraTrackFollower.WorldUp.X;
					double uy = World.CameraTrackFollower.WorldUp.Y;
					double uz = World.CameraTrackFollower.WorldUp.Z;
					double sx = World.CameraTrackFollower.WorldSide.X;
					double sy = World.CameraTrackFollower.WorldSide.Y;
					double sz = World.CameraTrackFollower.WorldSide.Z;
					double ox = Camera.CameraCurrentAlignment.Position.X;
					double oy = Camera.CameraCurrentAlignment.Position.Y;
					double oz = Camera.CameraCurrentAlignment.Position.Z;
					double cx = px + sx * ox + ux * oy + dx * oz;
					double cy = py + sy * ox + uy * oy + dy * oz;
					double cz = pz + sz * ox + uz * oy + dz * oz;
					Camera.AbsoluteCameraPosition = new Vector3(cx, cy, cz);
					dx = tx - cx;
					dy = ty - cy;
					dz = tz - cz;
					double t = Math.Sqrt(dx * dx + dy * dy + dz * dz);
					double ti = 1.0 / t;
					dx *= ti;
					dy *= ti;
					dz *= ti;
					Camera.AbsoluteCameraDirection = new Vector3(dx, dy, dz);
					Camera.AbsoluteCameraSide = new Vector3(dz, 0.0, -dx);
					Camera.AbsoluteCameraSide.Normalize();
					Camera.AbsoluteCameraUp = Vector3.Cross(new Vector3(dx, dy, dz), Camera.AbsoluteCameraSide);
					UpdateViewingDistances();
					if (Camera.CameraView == CameraViewMode.FlyByZooming) {
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
						OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle / zoom;
						OpenBveShared.Renderer.UpdateViewport(OpenBveShared.Renderer.ViewPortChangeMode.NoChange);
					}
				}
			} else {
				// non-fly-by
				{
					// current alignment
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Position.X, Camera.CameraAlignmentDirection.Position.X, ref Camera.CameraAlignmentSpeed.Position.X, TimeElapsed);
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Position.Y, Camera.CameraAlignmentDirection.Position.Y, ref Camera.CameraAlignmentSpeed.Position.Y, TimeElapsed);
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Position.Z, Camera.CameraAlignmentDirection.Position.Z, ref Camera.CameraAlignmentSpeed.Position.Z, TimeElapsed);
					if ((Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) & Camera.CameraRestriction == CameraRestrictionMode.On) {
						if (Camera.CameraCurrentAlignment.Position.Z > 0.75) {
							Camera.CameraCurrentAlignment.Position.Z = 0.75;
						}
					}
					bool q = Camera.CameraAlignmentSpeed.Yaw != 0.0 | Camera.CameraAlignmentSpeed.Pitch != 0.0 | Camera.CameraAlignmentSpeed.Roll != 0.0;
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Yaw, Camera.CameraAlignmentDirection.Yaw, ref Camera.CameraAlignmentSpeed.Yaw, TimeElapsed);
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Pitch, Camera.CameraAlignmentDirection.Pitch, ref Camera.CameraAlignmentSpeed.Pitch, TimeElapsed);
					AdjustAlignment(ref Camera.CameraCurrentAlignment.Roll, Camera.CameraAlignmentDirection.Roll, ref Camera.CameraAlignmentSpeed.Roll, TimeElapsed);
					double tr = Camera.CameraCurrentAlignment.TrackPosition;
					AdjustAlignment(ref Camera.CameraCurrentAlignment.TrackPosition, Camera.CameraAlignmentDirection.TrackPosition, ref Camera.CameraAlignmentSpeed.TrackPosition, TimeElapsed);
					if (tr != Camera.CameraCurrentAlignment.TrackPosition) {
						World.CameraTrackFollower.Update(TrackManager.CurrentTrack, Camera.CameraCurrentAlignment.TrackPosition, true, false);
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
				if (Camera.CameraView == CameraViewMode.InteriorLookAhead) {
					// look-ahead
					double d = 20.0;
					if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed > 0.0) {
						d += 3.0 * (Math.Sqrt(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed * TrainManager.PlayerTrain.Specs.CurrentAverageSpeed + 1.0) - 1.0);
					}
					d -= TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Position;
					TrackFollower f = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower;
					f.TriggerType = EventTriggerType.None;
					f.Update(TrackManager.CurrentTrack, f.TrackPosition + d, true, false);
					double rx = f.WorldPosition.X - cF.X + World.CameraTrackFollower.WorldSide.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.X * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					double ry = f.WorldPosition.Y - cF.Y + World.CameraTrackFollower.WorldSide.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.Y * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					double rz = f.WorldPosition.Z - cF.Z + World.CameraTrackFollower.WorldSide.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.X + World.CameraTrackFollower.WorldUp.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Y + World.CameraTrackFollower.WorldDirection.Z * TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Driver.Z;
					World.Normalize(ref rx, ref ry, ref rz);
					double t = dF.Z * (sF.Y * uF.X - sF.X * uF.Y) + dF.Y * (-sF.Z * uF.X + sF.X * uF.Z) + dF.X * (sF.Z * uF.Y - sF.Y * uF.Z);
					if (t != 0.0) {
						t = 1.0 / t;

						double tx = (rz * (-dF.Y * uF.X + dF.X * uF.Y) + ry * (dF.Z * uF.X - dF.X * uF.Z) + rx * (-dF.Z * uF.Y + dF.Y * uF.Z)) * t;
						double ty = (rz * (dF.Y * sF.X - dF.X * sF.Y) + ry * (-dF.Z * sF.X + dF.X * sF.Z) + rx * (dF.Z * sF.Y - dF.Y * sF.Z)) * t;
						double tz = (rz * (sF.Y * uF.X - sF.X * uF.Y) + ry * (-sF.Z * uF.X + sF.X * uF.Z) + rx * (sF.Z * uF.Y - sF.Y * uF.Z)) * t;
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
					if ((Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
						int c = TrainManager.PlayerTrain.DriverCar;
						if (c >= 0) {
							if (TrainManager.PlayerTrain.Cars[c].CarSections.Length == 0 || !TrainManager.PlayerTrain.Cars[c].CarSections[0].Overlay) {
								double a = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
								double cosa = Math.Cos(-a);
								double sina = Math.Sin(-a);
								d2.Rotate(sF, cosa, sina);
								u2.Rotate(sF, cosa, sina);
							}
						}
					}
					cF.X += sF.X * Camera.CameraCurrentAlignment.Position.X + u2.X * Camera.CameraCurrentAlignment.Position.Y + d2.X * Camera.CameraCurrentAlignment.Position.Z;
					cF.Y += sF.Y * Camera.CameraCurrentAlignment.Position.X + u2.Y * Camera.CameraCurrentAlignment.Position.Y + d2.Y * Camera.CameraCurrentAlignment.Position.Z;
					cF.Z += sF.Z * Camera.CameraCurrentAlignment.Position.X + u2.Z * Camera.CameraCurrentAlignment.Position.Y + d2.Z * Camera.CameraCurrentAlignment.Position.Z;
				}
				// yaw, pitch, roll
				double headYaw = Camera.CameraCurrentAlignment.Yaw + lookaheadYaw;
				if ((Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headYaw += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverYaw;
					}
				}
				double headPitch = Camera.CameraCurrentAlignment.Pitch + lookaheadPitch;
				if ((Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead) & TrainManager.PlayerTrain != null) {
					if (TrainManager.PlayerTrain.DriverCar >= 0) {
						headPitch += TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DriverPitch;
					}
				}
				double bodyPitch = 0.0;
				double bodyRoll = 0.0;
				double headRoll = Camera.CameraCurrentAlignment.Roll;
				// rotation
				if (Camera.CameraRestriction == CameraRestrictionMode.NotAvailable & (Camera.CameraView == CameraViewMode.Interior | Camera.CameraView == CameraViewMode.InteriorLookAhead)) {
					// with body and head
					bodyPitch += CurrentDriverBody.Pitch;
					headPitch -= 0.2 * CurrentDriverBody.Pitch;
					bodyRoll += CurrentDriverBody.Roll;
					headRoll += 0.2 * CurrentDriverBody.Roll;
					const double bodyHeight = 0.6;
					const double headHeight = 0.1;
					{
						// body pitch
						double ry = (Math.Cos(-bodyPitch) - 1.0) * bodyHeight;
						double rz = Math.Sin(-bodyPitch) * bodyHeight;
						cF.X += dF.X * rz + uF.X * ry;
						cF.Y += dF.Y * rz + uF.Y * ry;
						cF.Z += dF.Z * rz + uF.Z * ry;
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
						cF.X += sF.X * rx + uF.X * ry;
						cF.Y += sF.Y * rx + uF.Y * ry;
						cF.Z += sF.Z * rx + uF.Z * ry;
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
						cF.X += sF.X * rx + dF.X * rz;
						cF.Y += sF.Y * rx + dF.Y * rz;
						cF.Z += sF.Z * rx + dF.Z * rz;
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
						cF.X += dF.X * rz + uF.X * ry;
						cF.Y += dF.Y * rz + uF.Y * ry;
						cF.Z += dF.Z * rz + uF.Z * ry;
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
						cF.X += sF.X * rx + uF.X * ry;
						cF.Y += sF.Y * rx + uF.Y * ry;
						cF.Z += sF.Z * rx + uF.Z * ry;
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
				Camera.AbsoluteCameraPosition = cF;
				Camera.AbsoluteCameraDirection = dF;
				Camera.AbsoluteCameraUp = uF;
				Camera.AbsoluteCameraSide = sF;
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
			OpenBveShared.World.VerticalViewingAngle = OpenBveShared.World.OriginalVerticalViewingAngle * Math.Exp(Camera.CameraCurrentAlignment.Zoom);
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
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + Camera.CameraCurrentAlignment.Position.Z, true);
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
		internal static void Normalize(ref double x, ref double y, ref double z) {
			double t = x * x + y * y + z * z;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
				z *= t;
			}
		}
	}
}
