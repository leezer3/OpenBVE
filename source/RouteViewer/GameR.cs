// ╔═════════════════════════════════════════════════════════════╗
// ║ Game.cs for the Route Viewer                                ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
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

namespace OpenBve {
	internal static class Game {

		// random numbers
		internal static readonly Random Generator = new Random();

		// date and time
		internal static double SecondsSinceMidnight = 0.0;
		internal static double StartupTime = 0.0;
		internal static bool MinimalisticSimulation = false;
		internal static double[] RouteUnitOfLength = new double[] { 1.0 };

		

		// route constants
		internal static string RouteComment = "";
		internal static string RouteImage = "";
		
		// game constants
		internal static double[] PrecedingTrainTimeDeltas;
		internal static double PrecedingTrainSpeedLimit;

		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesNoAts;
		internal static string TrainName = "";

		// information
		
		/// <summary>The current plugin debug message to be displayed</summary>
		internal static string InfoDebugString = "";
		

		// ================================

		internal static void Reset() {
			// track manager
			Program.CurrentRoute.Tracks = new Track[] { new Track() };
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			RouteComment = "";
			RouteImage = "";
			Program.CurrentRoute.Atmosphere.AccelerationDueToGravity = 9.80665;
			Program.CurrentRoute.Atmosphere.InitialAirPressure = 101325.0;
			Program.CurrentRoute.Atmosphere.InitialAirTemperature = 293.15;
			Program.CurrentRoute.Atmosphere.InitialElevation = 0.0;
			Program.CurrentRoute.Atmosphere.SeaLevelAirPressure = 101325.0;
			Program.CurrentRoute.Atmosphere.SeaLevelAirTemperature = 293.15;
			Program.CurrentRoute.Stations = new RouteStation[] { };
			Program.CurrentRoute.Sections = new Section[] { };
			BufferTrackPositions = new double[] { };
			Program.Renderer.Marker.MarkerTextures = new Texture[] { };
			Program.CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			Program.CurrentRoute.BogusPreTrainInstructions = new BogusPreTrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			Program.CurrentRoute.PreviousFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.0);
			Program.CurrentRoute.CurrentFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.5);
			Program.CurrentRoute.NextFog = new Fog(0.0f, 0.0f, Color24.Grey, 1.0);
			Program.CurrentRoute.NoFogStart = (float)BackgroundHandle.BackgroundImageDistance + 200.0f;
			Program.CurrentRoute.NoFogEnd = 2.0f * Program.CurrentRoute.NoFogStart;
			Program.Renderer.InfoTotalTriangles = 0;
			Program.Renderer.InfoTotalTriangleStrip = 0;
			Program.Renderer.InfoTotalQuads = 0;
			Program.Renderer.InfoTotalQuadStrip = 0;
			Program.Renderer.InfoTotalPolygon = 0;
			// object manager
			Program.Renderer.InitializeVisibility();
			ObjectManager.AnimatedWorldObjects = new AnimatedWorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
			// renderer / sound
			Program.Renderer.Reset();
			Program.Sounds.StopAllSounds();
			GC.Collect();
		}

		// ================================

		
		

		// ================================

		// buffers
		internal static double[] BufferTrackPositions = new double[] { };

		internal static bool ApplyPointOfInterest(int Value, bool Relative) {
			double t = 0.0;
			int j = -1;
			if (Relative) {
				// relative
				if (Value < 0) {
					// previous poi
					t = double.NegativeInfinity;
					for (int i = 0; i < Program.CurrentRoute.PointsOfInterest.Length; i++) {
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition) {
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
						if (Program.CurrentRoute.PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition) {
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
				World.CameraTrackFollower.UpdateAbsolute(t, true, false);
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
