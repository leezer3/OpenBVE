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
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

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
		internal static double RouteAccelerationDueToGravity = 9.80665;
		internal static double RouteInitialAirPressure = 101325.0;
		internal static double RouteInitialAirTemperature = 293.15;
		internal static double RouteInitialElevation = 0.0;
		internal static double RouteSeaLevelAirPressure = 101325.0;
		internal static double RouteSeaLevelAirTemperature = 293.15;
		internal const double CriticalCollisionSpeedDifference = 8.0;
		internal const double BrakePipeLeakRate = 500000.0;
		internal const double MolarMass = 0.0289644;
		internal const double UniversalGasConstant = 8.31447;
		internal const double TemperatureLapseRate = -0.0065;
		internal const double CoefficientOfStiffness = 144117.325646911;

		// athmospheric functions
		internal static void CalculateSeaLevelConstants() {
			RouteSeaLevelAirTemperature = RouteInitialAirTemperature - TemperatureLapseRate * RouteInitialElevation;
			double Exponent = RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
			double Base = 1.0 + TemperatureLapseRate * RouteInitialElevation / RouteSeaLevelAirTemperature;
			if (Base >= 0.0) {
				RouteSeaLevelAirPressure = RouteInitialAirPressure * Math.Pow(Base, Exponent);
				if (RouteSeaLevelAirPressure < 0.001) RouteSeaLevelAirPressure = 0.001;
			} else {
				RouteSeaLevelAirPressure = 0.001;
			}
		}
		internal static double GetAirTemperature(double Elevation) {
			double x = RouteSeaLevelAirTemperature + TemperatureLapseRate * Elevation;
			if (x >= 1.0) {
				return x;
			} else return 1.0;
		}
		internal static double GetAirDensity(double AirPressure, double AirTemperature) {
			double x = AirPressure * MolarMass / (UniversalGasConstant * AirTemperature);
			if (x >= 0.001) {
				return x;
			} else return 0.001;
		}
		internal static double GetAirPressure(double Elevation, double AirTemperature) {
			double Exponent = -RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
			double Base = 1.0 + TemperatureLapseRate * Elevation / RouteSeaLevelAirTemperature;
			if (Base >= 0.0) {
				double x = RouteSeaLevelAirPressure * Math.Pow(Base, Exponent);
				if (x >= 0.001) {
					return x;
				} return 0.001;
			} else return 0.001;
		}
		internal static double GetSpeedOfSound(double AirPressure, double AirTemperature) {
			double AirDensity = GetAirDensity(AirPressure, AirTemperature);
			return Math.Sqrt(CoefficientOfStiffness / AirDensity);
		}

		// game constants
		internal static double[] PrecedingTrainTimeDeltas;
		internal static double PrecedingTrainSpeedLimit;

		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesNoAts;
		internal static string TrainName = "";

		// information
		
		/// <summary>The current plugin debug message to be displayed</summary>
		internal static string InfoDebugString = "";
		/// <summary>The total number of OpenGL triangles in the current frame</summary>
		internal static int InfoTotalTriangles = 0;
		/// <summary>The total number of OpenGL triangle strips in the current frame</summary>
		internal static int InfoTotalTriangleStrip = 0;
		/// <summary>The total number of OpenGL quad strips in the current frame</summary>
		internal static int InfoTotalQuadStrip = 0;
		/// <summary>The total number of OpenGL quads in the current frame</summary>
		internal static int InfoTotalQuads = 0;
		/// <summary>The total number of OpenGL polygons in the current frame</summary>
		internal static int InfoTotalPolygon = 0;
		/// <summary>The total number of static opaque faces in the current frame</summary>
		internal static int InfoStaticOpaqueFaceCount = 0;

		// ================================

		internal static void Reset() {
			// track manager
			TrackManager.CurrentTrack = new Track();
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			RouteComment = "";
			RouteImage = "";
			RouteAccelerationDueToGravity = 9.80665;
			RouteInitialAirPressure = 101325.0;
			RouteInitialAirTemperature = 293.15;
			RouteInitialElevation = 0.0;
			RouteSeaLevelAirPressure = 101325.0;
			RouteSeaLevelAirTemperature = 293.15;
			Stations = new RouteStation[] { };
			CurrentRoute.Sections = new Section[] { };
			BufferTrackPositions = new double[] { };
			MarkerTextures = new Texture[] { };
			PointsOfInterest = new PointOfInterest[] { };
			CurrentRoute.BogusPretrainInstructions = new BogusPretrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			CurrentRoute.PreviousFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.0);
			CurrentRoute.CurrentFog = new Fog(0.0f, 0.0f, Color24.Grey, 0.5);
			CurrentRoute.NextFog = new Fog(0.0f, 0.0f, Color24.Grey, 1.0);
			CurrentRoute.NoFogStart = (float)Backgrounds.BackgroundImageDistance + 200.0f;
			CurrentRoute.NoFogEnd = 2.0f * CurrentRoute.NoFogStart;
			InfoTotalTriangles = 0;
			InfoTotalTriangleStrip = 0;
			InfoTotalQuads = 0;
			InfoTotalQuadStrip = 0;
			InfoTotalPolygon = 0;
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

		internal static RouteStation[] Stations = { };
		

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
				if (Stations[d].Type != StationType.Normal) {
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

		// ================================

		// marker
		internal static Texture[] MarkerTextures = new Texture[] { };
		internal static void AddMarker(Texture Texture) {
			int n = MarkerTextures.Length;
			Array.Resize<Texture>(ref MarkerTextures, n + 1);
			MarkerTextures[n] = Texture;
		}
		internal static void RemoveMarker(Texture Texture) {
			int n = MarkerTextures.Length;
			for (int i = 0; i < n; i++) {
				if (MarkerTextures[i] == Texture) {
					for (int j = i; j < n - 1; j++) {
						MarkerTextures[j] = MarkerTextures[j + 1];
					}
					Array.Resize<Texture>(ref MarkerTextures, n - 1);
					break;
				}
			}
		}

		// ================================

		// points of interest
		internal struct PointOfInterest {
			internal double TrackPosition;
			internal Vector3 TrackOffset;
			internal double TrackYaw;
			internal double TrackPitch;
			internal double TrackRoll;
			internal string Text;
		}
		internal static PointOfInterest[] PointsOfInterest = new PointOfInterest[] { };

		internal static bool ApplyPointOfInterest(int Value, bool Relative) {
			double t = 0.0;
			int j = -1;
			if (Relative) {
				// relative
				if (Value < 0) {
					// previous poi
					t = double.NegativeInfinity;
					for (int i = 0; i < PointsOfInterest.Length; i++) {
						if (PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition) {
							if (PointsOfInterest[i].TrackPosition > t) {
								t = PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				} else if (Value > 0) {
					// next poi
					t = double.PositiveInfinity;
					for (int i = 0; i < PointsOfInterest.Length; i++) {
						if (PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition) {
							if (PointsOfInterest[i].TrackPosition < t) {
								t = PointsOfInterest[i].TrackPosition;
								j = i;
							}
						}
					}
				}
			} else {
				// absolute
				j = Value >= 0 & Value < PointsOfInterest.Length ? Value : -1;
			}
			// process poi
			if (j >= 0) {
				TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, t, true, false);
				Camera.CurrentAlignment.Position = PointsOfInterest[j].TrackOffset;
				Camera.CurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
				Camera.CurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
				Camera.CurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
				Camera.CurrentAlignment.TrackPosition = t;
				World.UpdateAbsoluteCamera(0.0);
				return true;
			} else {
				return false;
			}
		}

	}
}
