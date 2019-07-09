// ╔═════════════════════════════════════════════════════════════╗
// ║ Game.cs for the Route Viewer                                ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;
using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBve.RouteManager;
using OpenBve.SignalManager;
using OpenBveApi.Objects;
using OpenBveApi.Routes;

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
			TrackManager.Tracks = new Track[] { new Track() };
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			RouteComment = "";
			RouteImage = "";
			Atmosphere.AccelerationDueToGravity = 9.80665;
			Atmosphere.InitialAirPressure = 101325.0;
			Atmosphere.InitialAirTemperature = 293.15;
			CurrentRoute.InitialElevation = 0.0;
			Atmosphere.SeaLevelAirPressure = 101325.0;
			Atmosphere.SeaLevelAirTemperature = 293.15;
			CurrentRoute.Stations = new RouteStation[] { };
			CurrentRoute.Sections = new Section[] { };
			BufferTrackPositions = new double[] { };
			LibRender.Renderer.MarkerTextures = new Texture[] { };
			CurrentRoute.PointsOfInterest = new PointOfInterest[] { };
			CurrentRoute.BogusPretrainInstructions = new BogusPretrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			CurrentRoute.PreviousFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.0);
			CurrentRoute.CurrentFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.5);
			CurrentRoute.NextFog = new Fog(0.0f, 0.0f, Color24.Grey, 1.0);
			CurrentRoute.NoFogStart = (float)Backgrounds.BackgroundImageDistance + 200.0f;
			CurrentRoute.NoFogEnd = 2.0f * CurrentRoute.NoFogStart;
			LibRender.Renderer.InfoTotalTriangles = 0;
			LibRender.Renderer.InfoTotalTriangleStrip = 0;
			LibRender.Renderer.InfoTotalQuads = 0;
			LibRender.Renderer.InfoTotalQuadStrip = 0;
			LibRender.Renderer.InfoTotalPolygon = 0;
			// object manager
			ObjectManager.Objects = new StaticObject[16];
			ObjectManager.ObjectsUsed = 0;
			ObjectManager.ObjectsSortedByStart = new int[] { };
			ObjectManager.ObjectsSortedByEnd = new int[] { };
			ObjectManager.ObjectsSortedByStartPointer = 0;
			ObjectManager.ObjectsSortedByEndPointer = 0;
			ObjectManager.LastUpdatedTrackPosition = 0.0;
			ObjectManager.AnimatedWorldObjects = new ObjectManager.AnimatedWorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
			// renderer / sound
			Renderer.Reset();
			Program.Sounds.StopAllSounds();
			GC.Collect();
		}

		// ================================

		
		

		// ================================

		// sections
		
		internal static void UpdateAllSections() {
			if (CurrentRoute.Sections.Length != 0) {
				UpdateSection(CurrentRoute.Sections.Length - 1);
			}
		}
		internal static void UpdateSection(int SectionIndex) {
			// preparations
			int zeroaspect;
			bool settored = false;
			if (CurrentRoute.Sections[SectionIndex].Type == SectionType.ValueBased) {
				// value-based
				zeroaspect = int.MaxValue;
				for (int i = 0; i < CurrentRoute.Sections[SectionIndex].Aspects.Length; i++) {
					if (CurrentRoute.Sections[SectionIndex].Aspects[i].Number < zeroaspect) {
						zeroaspect = CurrentRoute.Sections[SectionIndex].Aspects[i].Number;
					}
				} 
				if (zeroaspect == int.MaxValue) {
					zeroaspect = -1;
				}
			} else {
				// index-based
				zeroaspect = 0;
			}
			// hold station departure signal at red
			int d = CurrentRoute.Sections[SectionIndex].StationIndex;
			if (d >= 0) {
				// look for train in previous blocks
				//int l = Sections[SectionIndex].PreviousSection;
				if (CurrentRoute.Stations[d].Type != StationType.Normal) {
					settored = true;
				}
			}
			// train in block
			if (CurrentRoute.Sections[SectionIndex].Trains.Length != 0) {
				settored = true;
			}
			// free sections
			int newaspect = -1;
			if (settored) {
				CurrentRoute.Sections[SectionIndex].FreeSections = 0;
				newaspect = zeroaspect;
			} else {
				int n = CurrentRoute.Sections[SectionIndex].NextSection;
				if (n >= 0) {
					if (CurrentRoute.Sections[n].FreeSections == -1) {
						CurrentRoute.Sections[SectionIndex].FreeSections = -1;
					} else {
						CurrentRoute.Sections[SectionIndex].FreeSections = CurrentRoute.Sections[n].FreeSections + 1;
					}
				} else {
					CurrentRoute.Sections[SectionIndex].FreeSections = -1;
				}
			}
			// change aspect
			if (newaspect == -1) {
				if (CurrentRoute.Sections[SectionIndex].Type == SectionType.ValueBased) {
					// value-based
					int n = CurrentRoute.Sections[SectionIndex].NextSection;
					int a = CurrentRoute.Sections[SectionIndex].Aspects[CurrentRoute.Sections[SectionIndex].Aspects.Length - 1].Number;
					if (n >= 0 && CurrentRoute.Sections[n].CurrentAspect >= 0) {
						a = CurrentRoute.Sections[n].Aspects[CurrentRoute.Sections[n].CurrentAspect].Number;
					}
					for (int i = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1; i >= 0; i--) {
						if (CurrentRoute.Sections[SectionIndex].Aspects[i].Number > a) {
							newaspect = i;
						}
					} if (newaspect == -1) {
						newaspect = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1;
					}
				} else {
					// index-based
					if (CurrentRoute.Sections[SectionIndex].FreeSections >= 0 & CurrentRoute.Sections[SectionIndex].FreeSections < CurrentRoute.Sections[SectionIndex].Aspects.Length) {
						newaspect = CurrentRoute.Sections[SectionIndex].FreeSections;
					} else {
						newaspect = CurrentRoute.Sections[SectionIndex].Aspects.Length - 1;
					}
				}
			}
			CurrentRoute.Sections[SectionIndex].CurrentAspect = newaspect;
			// update previous section
			if (CurrentRoute.Sections[SectionIndex].PreviousSection >= 0) {
				UpdateSection(CurrentRoute.Sections[SectionIndex].PreviousSection);
			}
		}

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
					for (int i = 0; i < CurrentRoute.PointsOfInterest.Length; i++) {
						if (CurrentRoute.PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition) {
							if (CurrentRoute.PointsOfInterest[i].TrackPosition > t) {
								t = CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				} else if (Value > 0) {
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < CurrentRoute.PointsOfInterest.Length; i++) {
						if (CurrentRoute.PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition) {
							if (CurrentRoute.PointsOfInterest[i].TrackPosition < t) {
								t = CurrentRoute.PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			} else {
				// absolute
				j = Value >= 0 & Value < CurrentRoute.PointsOfInterest.Length ? Value : -1;
			}
			// process poi
			if (j >= 0) {
				World.CameraTrackFollower.UpdateAbsolute(t, true, false);
				Camera.CurrentAlignment.Position = CurrentRoute.PointsOfInterest[j].TrackOffset;
				Camera.CurrentAlignment.Yaw = CurrentRoute.PointsOfInterest[j].TrackYaw;
				Camera.CurrentAlignment.Pitch = CurrentRoute.PointsOfInterest[j].TrackPitch;
				Camera.CurrentAlignment.Roll = CurrentRoute.PointsOfInterest[j].TrackRoll;
				Camera.CurrentAlignment.TrackPosition = t;
				World.UpdateAbsoluteCamera(0.0);
				return true;
			} else {
				return false;
			}
		}

	}
}
