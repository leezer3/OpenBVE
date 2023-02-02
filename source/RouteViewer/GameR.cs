// ╔═════════════════════════════════════════════════════════════╗
// ║ Game.cs for the Route Viewer                                ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using System.Collections.Generic;
using OpenBveApi.Colors;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using RouteManager2;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Stations;
using TrainManager.Trains;

namespace RouteViewer {
	internal static class Game {

		// date and time
		internal static double SecondsSinceMidnight = 0.0;

		// ================================

		internal static void Reset() {
			Program.Renderer.Reset();
			// track manager
			Program.CurrentRoute.Tracks = new Dictionary<int, Track>();
			Track t = new Track
			{
				Elements = new TrackElement[0]
			};
			Program.CurrentRoute.Tracks.Add(0, t);
			// train manager
			Program.TrainManager.Trains = new TrainBase[] { };
			// game
			Interface.LogMessages.Clear();
			Program.CurrentHost.MissingFiles.Clear();
			Program.CurrentRoute.Comment = "";
			Program.CurrentRoute.Image = "";
			Program.CurrentRoute.Atmosphere = new Atmosphere();
			Program.CurrentRoute.LightDefinitions = new LightDefinition[] { };
			Program.CurrentRoute.Stations = new RouteStation[] { };
			Program.CurrentRoute.Sections = new Section[] { };
			Program.CurrentRoute.BufferTrackPositions = new double[] { };
			Program.CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			Program.CurrentRoute.BogusPreTrainInstructions = new BogusPreTrainInstruction[] { };
			Interface.CurrentOptions.TrainName = "";
			Interface.CurrentOptions.TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			Program.CurrentRoute.NoFogStart = (float)Math.Max(1.33333333333333 * Interface.CurrentOptions.ViewingDistance, 800.0);
			Program.CurrentRoute.NoFogEnd = (float)Math.Max(2.66666666666667 * Interface.CurrentOptions.ViewingDistance, 1600.0);
			Program.CurrentRoute.PreviousFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.0);
			Program.CurrentRoute.CurrentFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 0.5);
			Program.CurrentRoute.NextFog = new Fog(Program.CurrentRoute.NoFogStart, Program.CurrentRoute.NoFogEnd, Color24.Grey, 1.0);
			Program.Renderer.InfoTotalTriangles = 0;
			Program.Renderer.InfoTotalTriangleStrip = 0;
			Program.Renderer.InfoTotalQuads = 0;
			Program.Renderer.InfoTotalQuadStrip = 0;
			Program.Renderer.InfoTotalPolygon = 0;
			Program.Renderer.VisibleObjects.quadTree.Clear();
			// object manager
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new WorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
			// renderer / sound
			Program.Sounds.StopAllSounds();
			GC.Collect();
		}

		// ================================

		internal static bool ApplyPointOfInterest(int Value, bool Relative) {
			double t = 0.0;
			int j = -1;
			if (Relative) {
				// relative
				if (Value < 0) {
					// previous poi
					t = double.NegativeInfinity;
					for (int i = 0; i < Program.CurrentRoute.PointsOfInterest.Length; i++) {
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition < Program.Renderer.CameraTrackFollower.TrackPosition) {
							if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition > t) {
								t = Program.CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				} else if (Value > 0) {
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < Program.CurrentRoute.PointsOfInterest.Length; i++) {
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition > Program.Renderer.CameraTrackFollower.TrackPosition) {
							if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition < t) {
								t = Program.CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			} else {
				// absolute
				j = Value >= 0 & Value < Program.CurrentRoute.PointsOfInterest.Length ? Value : -1;
			}
			// process poi
			if (j >= 0) {
				Program.Renderer.CameraTrackFollower.UpdateAbsolute(t, true, false);
				Program.Renderer.Camera.Alignment.Position = Program.CurrentRoute.PointsOfInterest[j].TrackOffset;
				Program.Renderer.Camera.Alignment.Yaw = Program.CurrentRoute.PointsOfInterest[j].TrackYaw;
				Program.Renderer.Camera.Alignment.Pitch = Program.CurrentRoute.PointsOfInterest[j].TrackPitch;
				Program.Renderer.Camera.Alignment.Roll = Program.CurrentRoute.PointsOfInterest[j].TrackRoll;
				Program.Renderer.Camera.Alignment.TrackPosition = t;
				World.UpdateAbsoluteCamera(0.0);
				return true;
			} else {
				return false;
			}
		}

	}
}
