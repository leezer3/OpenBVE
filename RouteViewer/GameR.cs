// ╔═════════════════════════════════════════════════════════════╗
// ║ Game.cs for the Route Viewer                                ║
// ╠═════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.       ║
// ║ The file from the openBVE main program cannot be used here. ║
// ╚═════════════════════════════════════════════════════════════╝

using System;

namespace OpenBve {
	internal static class Game {

		// random numbers
		internal static Random Generator = new Random();

		// game mode
		internal enum GameMode {
			Arcade = 0,
			Normal = 1,
			Expert = 2
		}
		internal static GameMode CurrentMode = GameMode.Normal;

		// date and time
		internal static double SecondsSinceMidnight = 0.0;
		internal static double StartupTime = 0.0;
		internal static bool MinimalisticSimulation = false;
		internal static double[] RouteUnitOfLength = new double[] { 1.0 };

		// fog
		internal struct Fog {
			internal float Start;
			internal float End;
			internal World.ColorRGB Color;
			internal double TrackPosition;
			internal Fog(float Start, float End, World.ColorRGB Color, double TrackPosition) {
				this.Start = Start;
				this.End = End;
				this.Color = Color;
				this.TrackPosition = TrackPosition;
			}
		}
		internal static Fog PreviousFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.0);
		internal static Fog CurrentFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.5);
		internal static Fog NextFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 1.0);
		internal static float NoFogStart = 800.0f;
		internal static float NoFogEnd = 1600.0f;

		// route constants
		internal static string RouteComment = "";
		internal static string RouteImage = "";
		internal static double RouteAccelerationDueToGravity = 9.80665;
		internal static double RouteRailGauge = 1.435;
		internal static double RouteInitialAirPressure = 101325.0;
		internal static double RouteInitialAirTemperature = 293.15;
		internal static double RouteInitialElevation = 0.0;
		internal static double RouteSeaLevelAirPressure = 101325.0;
		internal static double RouteSeaLevelAirTemperature = 293.15;
		internal const double CoefficientOfGroundFriction = 0.2;
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

		// startup
		internal enum TrainStartMode {
			ServiceBrakesAts = -1,
			EmergencyBrakesAts = 0,
			EmergencyBrakesNoAts = 1
		}
		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesNoAts;
		internal static string TrainName = "";

		// information
		internal static double InfoFrameRate = 1.0;
		internal static string InfoDebugString = "";
		internal static int InfoTotalTriangles = 0;
		internal static int InfoTotalTriangleStrip = 0;
		internal static int InfoTotalQuadStrip = 0;
		internal static int InfoTotalQuads = 0;
		internal static int InfoTotalPolygon = 0;

		// ================================

		internal static void Reset() {
			// track manager
			TrackManager.CurrentTrack = new TrackManager.Track();
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			RouteComment = "";
			RouteImage = "";
			RouteAccelerationDueToGravity = 9.80665;
			RouteRailGauge = 1.435;
			RouteInitialAirPressure = 101325.0;
			RouteInitialAirTemperature = 293.15;
			RouteInitialElevation = 0.0;
			RouteSeaLevelAirPressure = 101325.0;
			RouteSeaLevelAirTemperature = 293.15;
			Stations = new Station[] { };
			Sections = new Section[] { };
			BufferTrackPositions = new double[] { };
			MarkerTextures = new int[] { };
			PointsOfInterest = new PointOfInterest[] { };
			BogusPretrainInstructions = new BogusPretrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			PreviousFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.0);
			CurrentFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.5);
			NextFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 1.0);
			NoFogStart = (float)World.BackgroundImageDistance + 200.0f;
			NoFogEnd = 2.0f * NoFogStart;
			InfoTotalTriangles = 0;
			InfoTotalTriangleStrip = 0;
			InfoTotalQuads = 0;
			InfoTotalQuadStrip = 0;
			InfoTotalPolygon = 0;
			// object manager
			ObjectManager.Objects = new ObjectManager.StaticObject[16];
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
			SoundManager.StopAllSounds(true);
			GC.Collect();
		}

		// ================================

		// stations
		internal struct StationStop {
			internal double TrackPosition;
			internal double ForwardTolerance;
			internal double BackwardTolerance;
			internal int Cars;
		}
		internal enum SafetySystem {
			Any = -1,
			Ats = 0,
			Atc = 1
		}
		internal enum StationStopMode {
			AllStop = 0,
			AllPass = 1,
			PlayerStop = 2,
			PlayerPass = 3
		}
		internal enum StationType {
			Normal = 0,
			ChangeEnds = 1,
			Terminal = 2
		}
		internal struct Station {
			internal string Name;
			internal double ArrivalTime;
			internal int ArrivalSoundIndex;
			internal double DepartureTime;
			internal int DepartureSoundIndex;
			internal double StopTime;
			internal World.Vector3D SoundOrigin;
			internal StationStopMode StopMode;
			internal StationType StationType;
			internal bool ForceStopSignal;
			internal bool OpenLeftDoors;
			internal bool OpenRightDoors;
			internal SafetySystem SafetySystem;
			internal StationStop[] Stops;
			internal double PassengerRatio;
			internal int TimetableDaytimeTexture;
			internal int TimetableNighttimeTexture;
			internal double DefaultTrackPosition;
		}
		internal static Station[] Stations = new Station[] { };
		internal static int GetStopIndex(int StationIndex, int Cars) {
			int j = -1;
			for (int i = Stations[StationIndex].Stops.Length - 1; i >= 0; i--) {
				if (Cars <= Stations[StationIndex].Stops[i].Cars | Stations[StationIndex].Stops[i].Cars == 0) {
					j = i;
				}
			}
			if (j == -1) {
				return Stations[StationIndex].Stops.Length - 1;
			} else return j;
		}

		// ================================

		// sections
		internal enum SectionType { ValueBased, IndexBased }
		internal struct SectionAspect {
			internal int Number;
			internal double Speed;
			internal SectionAspect(int Number, double Speed) {
				this.Number = Number;
				this.Speed = Speed;
			}
		}
		internal struct Section {
			internal int PreviousSection;
			internal int NextSection;
			internal TrainManager.Train[] Trains;
			internal bool TrainReachedStopPoint;
			internal int StationIndex;
			internal bool Invisible;
			internal int[] SignalIndices;
			internal double TrackPosition;
			internal SectionType Type;
			internal SectionAspect[] Aspects;
			internal int CurrentAspect;
			internal int FreeSections;
			internal void Enter(TrainManager.Train Train) {
				int n = this.Trains.Length;
				for (int i = 0; i < n; i++) {
					if (this.Trains[i] == Train) return;
				}
				Array.Resize<TrainManager.Train>(ref this.Trains, n + 1);
				this.Trains[n] = Train;
			}
			internal void Leave(TrainManager.Train Train) {
				int n = this.Trains.Length;
				for (int i = 0; i < n; i++) {
					if (this.Trains[i] == Train) {
						for (int j = i; j < n - 1; j++) {
							this.Trains[j] = this.Trains[j + 1];
						}
						Array.Resize<TrainManager.Train>(ref this.Trains, n - 1);
						return;
					}
				}
			}
			internal bool Exists(TrainManager.Train Train) {
				for (int i = 0; i < this.Trains.Length; i++) {
					if (this.Trains[i] == Train) return true;
				} return false;
			}
		}
		internal static Section[] Sections = new Section[] { };
		internal static void UpdateAllSections() {
			if (Sections.Length != 0) {
				UpdateSection(Sections.Length - 1);
			}
		}
		internal static void UpdateSection(int SectionIndex) {
			// preparations
			int zeroaspect;
			bool settored = false;
			if (Sections[SectionIndex].Type == SectionType.ValueBased) {
				// value-based
				zeroaspect = int.MaxValue;
				for (int i = 0; i < Sections[SectionIndex].Aspects.Length; i++) {
					if (Sections[SectionIndex].Aspects[i].Number < zeroaspect) {
						zeroaspect = Sections[SectionIndex].Aspects[i].Number;
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
			int d = Sections[SectionIndex].StationIndex;
			if (d >= 0) {
				// look for train in previous blocks
				int l = Sections[SectionIndex].PreviousSection;
				if (Stations[d].StationType != StationType.Normal) {
					settored = true;
				}
			}
			// train in block
			if (Sections[SectionIndex].Trains.Length != 0) {
				settored = true;
			}
			// free sections
			int newaspect = -1;
			if (settored) {
				Sections[SectionIndex].FreeSections = 0;
				newaspect = zeroaspect;
			} else {
				int n = Sections[SectionIndex].NextSection;
				if (n >= 0) {
					if (Sections[n].FreeSections == -1) {
						Sections[SectionIndex].FreeSections = -1;
					} else {
						Sections[SectionIndex].FreeSections = Sections[n].FreeSections + 1;
					}
				} else {
					Sections[SectionIndex].FreeSections = -1;
				}
			}
			// change aspect
			if (newaspect == -1) {
				if (Sections[SectionIndex].Type == SectionType.ValueBased) {
					// value-based
					int n = Sections[SectionIndex].NextSection;
					int a = Sections[SectionIndex].Aspects[Sections[SectionIndex].Aspects.Length - 1].Number;
					if (n >= 0 && Sections[n].CurrentAspect >= 0) {
						a = Sections[n].Aspects[Sections[n].CurrentAspect].Number;
					}
					for (int i = Sections[SectionIndex].Aspects.Length - 1; i >= 0; i--) {
						if (Sections[SectionIndex].Aspects[i].Number > a) {
							newaspect = i;
						}
					} if (newaspect == -1) {
						newaspect = Sections[SectionIndex].Aspects.Length - 1;
					}
				} else {
					// index-based
					if (Sections[SectionIndex].FreeSections >= 0 & Sections[SectionIndex].FreeSections < Sections[SectionIndex].Aspects.Length) {
						newaspect = Sections[SectionIndex].FreeSections;
					} else {
						newaspect = Sections[SectionIndex].Aspects.Length - 1;
					}
				}
			}
			Sections[SectionIndex].CurrentAspect = newaspect;
			// update previous section
			if (Sections[SectionIndex].PreviousSection >= 0) {
				UpdateSection(Sections[SectionIndex].PreviousSection);
			}
		}

		// buffers
		internal static double[] BufferTrackPositions = new double[] { };

		// ================================

		// marker
		internal static int[] MarkerTextures = new int[] { };
		internal static void AddMarker(int TextureIndex) {
			int n = MarkerTextures.Length;
			Array.Resize<int>(ref MarkerTextures, n + 1);
			MarkerTextures[n] = TextureIndex;
		}
		internal static void RemoveMarker(int TextureIndex) {
			int n = MarkerTextures.Length;
			for (int i = 0; i < n; i++) {
				if (MarkerTextures[i] == TextureIndex) {
					for (int j = i; j < n - 1; j++) {
						MarkerTextures[j] = MarkerTextures[j + 1];
					}
					Array.Resize<int>(ref MarkerTextures, n - 1);
					break;
				}
			}
		}

		// ================================

		// bogus pretrain
		internal struct BogusPretrainInstruction {
			internal double TrackPosition;
			internal double Time;
		}
		internal static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };

		// ================================

		// points of interest
		internal struct PointOfInterest {
			internal double TrackPosition;
			internal World.Vector3D TrackOffset;
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
				World.CameraCurrentAlignment.Position = PointsOfInterest[j].TrackOffset;
				World.CameraCurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
				World.CameraCurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
				World.CameraCurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
				World.CameraCurrentAlignment.TrackPosition = t;
				World.UpdateAbsoluteCamera(0.0);
				return true;
			} else {
				return false;
			}
		}

	}
}