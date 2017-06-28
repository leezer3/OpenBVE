using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	internal static partial class Game {

		// date and time
        /// <summary>The current in game time, expressed as the number of seconds since midnight on the first day</summary>
		internal static double SecondsSinceMidnight = 0.0;
        /// <summary>The time at which the current game started, expressed as the number of seconds since midnight on the first day</summary>
		internal static double StartupTime = 0.0;
		/// <summary>Whether the game is in minimal simulation mode: 
		/// This is used when the game is fast-forwarding on start or station jump.
		/// Train and time movements are processed, but no graphical processing is done
		/// </summary>
		internal static bool MinimalisticSimulation = false;

		/// <summary>Defines a region of fog</summary>
		internal struct Fog {
			/// <summary>The offset at which the fog starts</summary>
			internal float Start;
			/// <summary>The offset at which the fog ends</summary>
			internal float End;
			/// <summary>The color of the fog</summary>
			internal Color24 Color;
			/// <summary>The track position at which the fog is placed</summary>
			internal double TrackPosition;
			/// <summary>Creates a new region of fog</summary>
			internal Fog(float Start, float End, Color24 Color, double TrackPosition) {
				this.Start = Start;
				this.End = End;
				this.Color = Color;
				this.TrackPosition = TrackPosition;
			}
		}
		internal static float NoFogStart = 800.0f; // must not be 600 or below
		internal static float NoFogEnd = 1600.0f;
		internal static Fog PreviousFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.0);
		internal static Fog CurrentFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.5);
		internal static Fog NextFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 1.0);
		
		

		// other trains
		internal static double[] PrecedingTrainTimeDeltas = new double[] { };
		internal static double PrecedingTrainSpeedLimit = double.PositiveInfinity;
		internal static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };

		/// <summary>The startup state of the train</summary>
		internal enum TrainStartMode {
            /// <summary>The train will start with the service brakes applied, and the safety-system plugin initialised</summary>
			ServiceBrakesAts = -1,
            /// <summary>The train will start with the EB brakes applied, and the safety-system plugin initialised</summary>
			EmergencyBrakesAts = 0,
            /// <summary>The train will start with the EB brakes applied, and the safety-system plugin inactive</summary>
			EmergencyBrakesNoAts = 1
		}
        /// <summary>The default mode for the train's safety system to start in</summary>
		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesAts;
        /// <summary>The name of the current train</summary>
		internal static string TrainName = "";

		// ================================

        /// <summary>Call this function to reset the game</summary>
        /// <param name="ResetLogs">Whether the logs should be reset</param>
		internal static void Reset(bool ResetLogs) {
			// track manager
			TrackManager.CurrentTrack = new TrackManager.Track();
			// train manager
			TrainManager.Trains = new TrainManager.Train[] { };
			// game
			Interface.ClearMessages();
			CurrentInterface = InterfaceType.Normal;
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
			//Messages = new Message[] { };
			MarkerTextures = new Textures.Texture[] { };
			PointsOfInterest = new PointOfInterest[] { };
			PrecedingTrainTimeDeltas = new double[] { };
			PrecedingTrainSpeedLimit = double.PositiveInfinity;
			BogusPretrainInstructions = new BogusPretrainInstruction[] { };
			TrainName = "";
			TrainStart = TrainStartMode.EmergencyBrakesNoAts;
			NoFogStart = (float)Math.Max(1.33333333333333 * Interface.CurrentOptions.ViewingDistance, 800.0);
			NoFogEnd = (float)Math.Max(2.66666666666667 * Interface.CurrentOptions.ViewingDistance, 1600.0);
			PreviousFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.0);
			CurrentFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 0.5);
			NextFog = new Fog(NoFogStart, NoFogEnd, new Color24(128, 128, 128), 1.0);
			InfoTotalTriangles = 0;
			InfoTotalTriangleStrip = 0;
			InfoTotalQuads = 0;
			InfoTotalQuadStrip = 0;
			InfoTotalPolygon = 0;
			InfoStaticOpaqueFaceCount = 0;
			if (ResetLogs) {
				LogRouteName = "";
				LogTrainName = "";
				LogDateTime = DateTime.Now;
				CurrentScore = new Score();
				ScoreMessages = new ScoreMessage[] { };
				ScoreLogs = new ScoreLog[64];
				ScoreLogCount = 0;
				BlackBoxEntries = new BlackBoxEntry[256];
				BlackBoxEntryCount = 0;
				BlackBoxNextUpdate = 0.0;
			}
			// renderer
			Renderer.Reset();
		}

		// ================================

		// score
		internal struct Score {
			internal int Value;
			internal int Maximum;
			internal double OpenedDoorsCounter;
			internal double OverspeedCounter;
			internal double TopplingCounter;
			internal bool RedSignal;
			internal bool Derailed;
			internal int ArrivalStation;
			internal int DepartureStation;
			internal double PassengerTimer;
		}
		internal static Score CurrentScore = new Score();
        /// <summary>The default number of points lost when the doors are opened unexpectedly</summary>
		private const double ScoreFactorOpenedDoors = -10.0;
        /// <summary>The default number of points lost per second when running overspeed</summary>
		private const double ScoreFactorOverspeed = -1.0;
        /// <summary>The default number of points lost when toppling the train through overspeed</summary>
		private const double ScoreFactorToppling = -10.0;
        /// <summary>The default number of points lost per second late</summary>
		private const double ScoreFactorStationLate = -0.333333333333333;
        /// <summary>The default number of points lost when missing a station's defined stop point</summary>
		private const double ScoreFactorStationStop = -50.0;
        /// <summary>The default number of points lost when departing unexpectedly from a station</summary>
		private const double ScoreFactorStationDeparture = -1.5;
        /// <summary>The default number of points lost when the train is derailed</summary>
		private const int ScoreValueDerailment = -1000;
        /// <summary>The default number of points lost when a red signal is passed</summary>
		private const int ScoreValueRedSignal = -100;
        /// <summary>The default number of points gained when arriving at a station on time</summary>
		private const int ScoreValueStationPerfectTime = 15;
        /// <summary>The default number of points gained when stopping within tolerance of a station's defined stop point</summary>
		private const int ScoreValueStationPerfectStop = 15;
        /// <summary>The default number of points lost when the passengers are experiencing discomfort (Excessive speed through curves etc)</summary>
		private const int ScoreValuePassengerDiscomfort = -20;
        /// <summary>The default number of points gained when stopping at a scheduled station</summary>
		internal const int ScoreValueStationArrival = 100;

        /// <summary>This method should be called once a frame to update the player's score</summary>
        /// <param name="TimeElapsed">The time elapsed since this function was last called</param>
		internal static void UpdateScore(double TimeElapsed) {
			// doors
			{
				bool leftopen = false;
				bool rightopen = false;
				for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
					for (int k = 0; k < TrainManager.PlayerTrain.Cars[j].Specs.Doors.Length; k++) {
						if (TrainManager.PlayerTrain.Cars[j].Specs.Doors[k].State != 0.0) {
							if (TrainManager.PlayerTrain.Cars[j].Specs.Doors[k].Direction == -1) {
								leftopen = true;
							} else if (TrainManager.PlayerTrain.Cars[j].Specs.Doors[k].Direction == 1) {
								rightopen = true;
							}
						}
					}
				}
				bool bad;
				if (leftopen | rightopen) {
					bad = true;
					int j = TrainManager.PlayerTrain.Station;
					if (j >= 0) {
						int p = Game.GetStopIndex(j, TrainManager.PlayerTrain.Cars.Length);
						if (p >= 0) {
							if (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) < 0.1) {
								if (leftopen == Stations[j].OpenLeftDoors & rightopen == Stations[j].OpenRightDoors) {
									bad = false;
								}
							}
						}
					}
				} else {
					bad = false;
				}
				if (bad) {
					CurrentScore.OpenedDoorsCounter += (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) + 0.25) * TimeElapsed;
				} else if (CurrentScore.OpenedDoorsCounter != 0.0) {
					int x = (int)Math.Ceiling(ScoreFactorOpenedDoors * CurrentScore.OpenedDoorsCounter);
					CurrentScore.Value += x;
					if (x != 0) {
						AddScore(x, ScoreTextToken.DoorsOpened, 5.0);
					}
					CurrentScore.OpenedDoorsCounter = 0.0;
				}
			}
			// overspeed
			double nr = TrainManager.PlayerTrain.CurrentRouteLimit;
			double ns = TrainManager.PlayerTrain.CurrentSectionLimit;
			double n = nr < ns ? nr : ns;
			double a = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) - 0.277777777777778;
			if (a > n) {
				CurrentScore.OverspeedCounter += (a - n) * TimeElapsed;
			} else if (CurrentScore.OverspeedCounter != 0.0) {
				int x = (int)Math.Ceiling(ScoreFactorOverspeed * CurrentScore.OverspeedCounter);
				CurrentScore.Value += x;
				if (x != 0) {
					AddScore(x, ScoreTextToken.Overspeed, 5.0);
				}
				CurrentScore.OverspeedCounter = 0.0;
			}
			// toppling
			{
				bool q = false;
				for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
					if (TrainManager.PlayerTrain.Cars[j].Topples) {
						q = true;
						break;
					}
				}
				if (q) {
					CurrentScore.TopplingCounter += TimeElapsed;
				} else if (CurrentScore.TopplingCounter != 0.0) {
					int x = (int)Math.Ceiling(ScoreFactorToppling * CurrentScore.TopplingCounter);
					CurrentScore.Value += x;
					if (x != 0) {
						AddScore(x, ScoreTextToken.Toppling, 5.0);
					}
					CurrentScore.TopplingCounter = 0.0;
				}
			}
			// derailment
			if (!CurrentScore.Derailed) {
				bool q = false;
				for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
					if (TrainManager.PlayerTrain.Cars[j].Derailed) {
						q = true;
						break;
					}
				}
				if (q) {
					int x = ScoreValueDerailment;
					if (CurrentScore.Value > 0) x -= CurrentScore.Value;
					CurrentScore.Value += x;
					if (x != 0) {
						AddScore(x, ScoreTextToken.Derailed, 5.0);
					}
					CurrentScore.Derailed = true;
				}
			}
			// red signal
			{
				if (TrainManager.PlayerTrain.CurrentSectionLimit == 0.0) {
					if (!CurrentScore.RedSignal) {
						int x = ScoreValueRedSignal;
						CurrentScore.Value += x;
						if (x != 0) {
							AddScore(x, ScoreTextToken.PassedRedSignal, 5.0);
						}
						CurrentScore.RedSignal = true;
					}
				} else {
					CurrentScore.RedSignal = false;
				}
			}
			// arrival
			{
				int j = TrainManager.PlayerTrain.Station;
				if (j >= 0 & j < Stations.Length) {
					if (j >= CurrentScore.ArrivalStation & TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Boarding) {
						if (j == 0 || Stations[j - 1].StationType != StationType.ChangeEnds) {
							// arrival
							int xa = ScoreValueStationArrival;
							CurrentScore.Value += xa;
							if (xa != 0) {
								AddScore(xa, ScoreTextToken.ArrivedAtStation, 10.0);
							}
							// early/late
							int xb;
							if (Stations[j].ArrivalTime >= 0) {
								double d = SecondsSinceMidnight - Stations[j].ArrivalTime;
								if (d >= -5.0 & d <= 0.0) {
									xb = ScoreValueStationPerfectTime;
									CurrentScore.Value += xb;
									AddScore(xb, ScoreTextToken.PerfectTimeBonus, 10.0);
								} else if (d > 0.0) {
									xb = (int)Math.Ceiling(ScoreFactorStationLate * (d - 1.0));
									CurrentScore.Value += xb;
									if (xb != 0) {
										AddScore(xb, ScoreTextToken.Late, 10.0);
									}
								} else {
									xb = 0;
								}
							} else {
								xb = 0;
							}
							// position
							int xc;
							int p = Game.GetStopIndex(j, TrainManager.PlayerTrain.Cars.Length);
							if (p >= 0) {
								double d = TrainManager.PlayerTrain.StationDistanceToStopPoint;
								double r;
								if (d >= 0) {
									double t = Stations[j].Stops[p].BackwardTolerance;
									r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
								} else {
									double t = Stations[j].Stops[p].ForwardTolerance;
									r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
								}
								if (r < 0.01) {
									xc = ScoreValueStationPerfectStop;
									CurrentScore.Value += xc;
									AddScore(xc, ScoreTextToken.PerfectStopBonus, 10.0);
								} else {
									if (r > 1.0) r = 1.0;
									r = (r - 0.01) * 1.01010101010101;
									xc = (int)Math.Ceiling(ScoreFactorStationStop * r);
									CurrentScore.Value += xc;
									if (xc != 0) {
										AddScore(xc, ScoreTextToken.Stop, 10.0);
									}
								}
							} else {
								xc = 0;
							}
							// sum
							if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
								int xs = xa + xb + xc;
								AddScore("", 10.0);
								AddScore(xs, ScoreTextToken.Total, 10.0, false);
								AddScore(" ", 10.0);
							}
							// evaluation
							if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
								if (Stations[j].StationType == StationType.Terminal) {
									double y = (double)CurrentScore.Value / (double)CurrentScore.Maximum;
									if (y < 0.0) y = 0.0;
									if (y > 1.0) y = 1.0;
									int k = (int)Math.Floor(y * (double)Interface.RatingsCount);
									if (k >= Interface.RatingsCount) k = Interface.RatingsCount - 1;
									System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
									AddScore(Interface.GetInterfaceString("score_rating"), 20.0);
									AddScore(Interface.GetInterfaceString("rating_" + k.ToString(Culture)) + " (" + (100.0 * y).ToString("0.00", Culture) + "%)", 20.0);
								}
							}
						}
						// finalize
						CurrentScore.DepartureStation = j;
						CurrentScore.ArrivalStation = j + 1;
					}
				}
			}
			// departure
			{
				int j = TrainManager.PlayerTrain.Station;
				if (j >= 0 & j < Stations.Length & j == CurrentScore.DepartureStation) {
					bool q;
					if (Stations[j].OpenLeftDoors | Stations[j].OpenRightDoors) {
						q = TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Completed;
					} else {
						q = TrainManager.PlayerTrain.StationState != TrainManager.TrainStopState.Pending & (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed < -1.5 | TrainManager.PlayerTrain.Specs.CurrentAverageSpeed > 1.5);
					}
					if (q) {
						double r = TrainManager.PlayerTrain.StationDepartureTime - SecondsSinceMidnight;
						if (r > 0.0) {
							int x = (int)Math.Ceiling(ScoreFactorStationDeparture * r);
							CurrentScore.Value += x;
							if (x != 0) {
								AddScore(x, ScoreTextToken.PrematureDeparture, 5.0);
							}
						}
						CurrentScore.DepartureStation = -1;
					}
				}
			}
			// passengers
			if (TrainManager.PlayerTrain.Passengers.FallenOver & CurrentScore.PassengerTimer == 0.0) {
				int x = ScoreValuePassengerDiscomfort;
				CurrentScore.Value += x;
				AddScore(x, ScoreTextToken.PassengerDiscomfort, 5.0);
				CurrentScore.PassengerTimer = 5.0;
			} else {
				CurrentScore.PassengerTimer -= TimeElapsed;
				if (CurrentScore.PassengerTimer <= 0.0) {
					if (TrainManager.PlayerTrain.Passengers.FallenOver) {
						CurrentScore.PassengerTimer = 5.0;
					} else {
						CurrentScore.PassengerTimer = 0.0;
					}
				}
			}
		}

		// score messages and logs
		internal enum ScoreTextToken : short {
			Invalid = 0,
			Overspeed = 1,
			PassedRedSignal = 2,
			Toppling = 3,
			Derailed = 4,
			PassengerDiscomfort = 5,
			DoorsOpened = 6,
			ArrivedAtStation = 7,
			PerfectTimeBonus = 8,
			Late = 9,
			PerfectStopBonus = 10,
			Stop = 11,
			PrematureDeparture = 12,
			Total = 13
		}
		internal struct ScoreMessage {
			internal int Value;
			internal string Text;
			internal double Timeout;
			internal MessageColor Color;
			internal Vector2 RendererPosition;
			internal double RendererAlpha;
		}
		internal struct ScoreLog {
			internal int Value;
			internal ScoreTextToken TextToken;
			internal double Position;
			internal double Time;
		}
		internal static ScoreLog[] ScoreLogs = new ScoreLog[64];
		internal static int ScoreLogCount = 0;
		internal static ScoreMessage[] ScoreMessages = new ScoreMessage[] { };
		internal static Vector2 ScoreMessagesRendererSize = new Vector2(16.0, 16.0);
		internal static string LogRouteName = "";
		internal static string LogTrainName = "";
		internal static DateTime LogDateTime = DateTime.Now;

	    private static void AddScore(int Value, ScoreTextToken TextToken, double Duration, bool Count = true) {
			if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
				int n = ScoreMessages.Length;
				Array.Resize<ScoreMessage>(ref ScoreMessages, n + 1);
				ScoreMessages[n].Value = Value;
				ScoreMessages[n].Text = Interface.GetScoreText(TextToken) + ": " + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
				ScoreMessages[n].Timeout = SecondsSinceMidnight + Duration;
				ScoreMessages[n].RendererPosition = new Vector2(0.0, 0.0);
				ScoreMessages[n].RendererAlpha = 0.0;
				if (Value < 0.0) {
					ScoreMessages[n].Color = MessageColor.Red;
				} else if (Value > 0.0) {
					ScoreMessages[n].Color = MessageColor.Green;
				} else {
					ScoreMessages[n].Color = MessageColor.White;
				}
			}
			if (Value != 0 & Count) {
				if (ScoreLogCount == ScoreLogs.Length) {
					Array.Resize<ScoreLog>(ref ScoreLogs, ScoreLogs.Length << 1);
				}
				ScoreLogs[ScoreLogCount].Value = Value;
				ScoreLogs[ScoreLogCount].TextToken = TextToken;
				ScoreLogs[ScoreLogCount].Position = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition;
				ScoreLogs[ScoreLogCount].Time = SecondsSinceMidnight;
				ScoreLogCount++;
			}
		}
		private static void AddScore(string Text, double Duration) {
			if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
				int n = ScoreMessages.Length;
				Array.Resize<ScoreMessage>(ref ScoreMessages, n + 1);
				ScoreMessages[n].Value = 0;
				ScoreMessages[n].Text = Text.Length != 0 ? Text : "══════════";
				ScoreMessages[n].Timeout = SecondsSinceMidnight + Duration;
				ScoreMessages[n].RendererPosition = new Vector2(0.0, 0.0);
				ScoreMessages[n].RendererAlpha = 0.0;
				ScoreMessages[n].Color = MessageColor.White;
			}
		}
		internal static void UpdateScoreMessages(double TimeElapsed) {
			if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
				for (int i = 0; i < ScoreMessages.Length; i++) {
					if (SecondsSinceMidnight >= ScoreMessages[i].Timeout & ScoreMessages[i].RendererAlpha == 0.0) {
						for (int j = i; j < ScoreMessages.Length - 1; j++) {
							ScoreMessages[j] = ScoreMessages[j + 1];
						} Array.Resize<ScoreMessage>(ref ScoreMessages, ScoreMessages.Length - 1);
						i--;
					}
				}
			}
		}

		// ================================

		// black box
		internal enum BlackBoxEventToken : short {
			None = 0
		}
		internal enum BlackBoxPower : short {
			PowerNull = 0
		}
		internal enum BlackBoxBrake : short {
			BrakeNull = 0,
			Emergency = -1,
			HoldBrake = -2,
			Release = -3,
			Lap = -4,
			Service = -5
		}
		internal struct BlackBoxEntry {
			internal double Time;
			internal double Position;
			internal float Speed;
			internal float Acceleration;
			internal short ReverserDriver;
			internal short ReverserSafety;
			internal BlackBoxPower PowerDriver;
			internal BlackBoxPower PowerSafety;
			internal BlackBoxBrake BrakeDriver;
			internal BlackBoxBrake BrakeSafety;
			internal BlackBoxEventToken EventToken;
		}
		internal static BlackBoxEntry[] BlackBoxEntries = new BlackBoxEntry[256];
		internal static int BlackBoxEntryCount = 0;
		private static double BlackBoxNextUpdate = 0.0;
		internal static void UpdateBlackBox() {
			if (SecondsSinceMidnight >= BlackBoxNextUpdate) {
				AddBlackBoxEntry(BlackBoxEventToken.None);
				BlackBoxNextUpdate = SecondsSinceMidnight + 1.0;
			}
		}
		internal static void AddBlackBoxEntry(BlackBoxEventToken EventToken) {
			if (Interface.CurrentOptions.BlackBox) {
				if (BlackBoxEntryCount >= BlackBoxEntries.Length) {
					Array.Resize<BlackBoxEntry>(ref BlackBoxEntries, BlackBoxEntries.Length << 1);
				}
				int d = TrainManager.PlayerTrain.DriverCar;
				BlackBoxEntries[BlackBoxEntryCount].Time = SecondsSinceMidnight;
				BlackBoxEntries[BlackBoxEntryCount].Position = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition;
				BlackBoxEntries[BlackBoxEntryCount].Speed = (float)TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
				BlackBoxEntries[BlackBoxEntryCount].Acceleration = (float)TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration;
				BlackBoxEntries[BlackBoxEntryCount].ReverserDriver = (short)TrainManager.PlayerTrain.Specs.CurrentReverser.Driver;
				BlackBoxEntries[BlackBoxEntryCount].ReverserSafety = (short)TrainManager.PlayerTrain.Specs.CurrentReverser.Actual;
				BlackBoxEntries[BlackBoxEntryCount].PowerDriver = (BlackBoxPower)TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
				BlackBoxEntries[BlackBoxEntryCount].PowerSafety = (BlackBoxPower)TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety;
				if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					switch (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver) {
							case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Release; break;
							case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Lap; break;
							case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Service; break;
							default: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency; break;
					}
				} else {
					BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = (BlackBoxBrake)TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
				}
				if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Emergency;
				} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual) {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.HoldBrake;
				} else if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					switch (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Safety) {
							case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Release; break;
							case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Lap; break;
							case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Service; break;
							default: BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = BlackBoxBrake.Emergency; break;
					}
				} else {
					BlackBoxEntries[BlackBoxEntryCount].BrakeSafety = (BlackBoxBrake)TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety;
				}
				BlackBoxEntries[BlackBoxEntryCount].EventToken = EventToken;
				BlackBoxEntryCount++;
			}
		}


		// buffers
		internal static double[] BufferTrackPositions = new double[] { };


	}
}