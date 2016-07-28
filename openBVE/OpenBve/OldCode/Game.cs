using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Runtime;

namespace OpenBve {
	internal static partial class Game {

		// date and time
        /// <summary>The current in game time, expressed as the number of seconds since midnight on the first day</summary>
		internal static double SecondsSinceMidnight = 0.0;
        /// <summary>The time at which the current game started, expressed as the number of seconds since midnight on the first day</summary>
		internal static double StartupTime = 0.0;
		internal static bool MinimalisticSimulation = false;

		// fog
		internal struct Fog {
			internal float Start;
			internal float End;
			internal Color24 Color;
			internal double TrackPosition;
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
			Messages = new Message[] { };
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
			internal Sounds.SoundBuffer ArrivalSoundBuffer;
			internal double DepartureTime;
			internal Sounds.SoundBuffer DepartureSoundBuffer;
			internal double StopTime;
			internal Vector3 SoundOrigin;
			internal StationStopMode StopMode;
			internal StationType StationType;
			internal bool ForceStopSignal;
			internal bool OpenLeftDoors;
			internal bool OpenRightDoors;
			internal SafetySystem SafetySystem;
			internal StationStop[] Stops;
			internal double PassengerRatio;
			internal Textures.Texture TimetableDaytimeTexture;
			internal Textures.Texture TimetableNighttimeTexture;
			internal double DefaultTrackPosition;
			//TODO: Temp variable added for BVE5 parser, remove ASAP
			internal string Key;
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
		/// <summary>Indicates whether the player's train stops at a station.</summary>
		internal static bool PlayerStopsAtStation(int StationIndex) {
			return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerStop;
		}
		/// <summary>Indicates whether a train stops at a station.</summary>
		internal static bool StopsAtStation(int StationIndex, TrainManager.Train Train) {
			if (Train == TrainManager.PlayerTrain) {
				return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerStop;
			} else {
				return Stations[StationIndex].StopMode == StationStopMode.AllStop | Stations[StationIndex].StopMode == StationStopMode.PlayerPass;
			}
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

	    public struct Section {
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
            /// <summary>A public read-only variable, which returns the current aspect to external scripts</summary>
            public int currentAspect{ get { return CurrentAspect; }}
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
					if (this.Trains[i] == Train)
						return true;
				}
				return false;
			}
			/// <summary>Checks whether the section is free, disregarding the specified train.</summary>
			/// <param name="train">The train to disregard.</param>
			/// <returns>Whether the section is free, disregarding the specified train.</returns>
			internal bool IsFree(TrainManager.Train train) {
				for (int i = 0; i < this.Trains.Length; i++) {
					if (this.Trains[i] != train & (this.Trains[i].State == TrainManager.TrainState.Available | this.Trains[i].State == TrainManager.TrainState.Bogus)) {
						return false;
					}
				}
				return true;
			}
			internal bool IsFree() {
				for (int i = 0; i < this.Trains.Length; i++) {
					if (this.Trains[i].State == TrainManager.TrainState.Available | this.Trains[i].State == TrainManager.TrainState.Bogus) {
						return false;
					}
				}
				return true;
			}
			internal TrainManager.Train GetFirstTrain(bool AllowBogusTrain) {
				for (int i = 0; i < this.Trains.Length; i++)
				{
				    if (this.Trains[i].State == TrainManager.TrainState.Available) {
						return this.Trains[i];
					}
				    if (AllowBogusTrain & this.Trains[i].State == TrainManager.TrainState.Bogus) {
				        return this.Trains[i];
				    }
				}
			    return null;
			}
		}
		public static Section[] Sections = new Section[] { };
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
				zeroaspect = 0;
				for (int i = 1; i < Sections[SectionIndex].Aspects.Length; i++) {
					if (Sections[SectionIndex].Aspects[i].Number < Sections[SectionIndex].Aspects[zeroaspect].Number) {
						zeroaspect = i;
					}
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
				TrainManager.Train train = null;
				while (true) {
					if (l >= 0) {
						train = Sections[l].GetFirstTrain(false);
						if (train != null) {
							break;
						}
					    l = Sections[l].PreviousSection;
					} else {
						break;
					}
				}
				if (train == null) {
					double b = -double.MaxValue;
					for (int i = 0; i < TrainManager.Trains.Length; i++) {
						if (TrainManager.Trains[i].State == TrainManager.TrainState.Available) {
							if (TrainManager.Trains[i].TimetableDelta > b) {
								b = TrainManager.Trains[i].TimetableDelta;
								train = TrainManager.Trains[i];
							}
						}
					}
				}
				// set to red where applicable
				if (train != null) {
					if (!Sections[SectionIndex].TrainReachedStopPoint) {
						if (train.Station == d) {
							int c = GetStopIndex(d, train.Cars.Length);
							if (c >= 0) {
								double p0 = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxlePosition + 0.5 * train.Cars[0].Length;
								double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;
								if (p0 >= p1) {
									Sections[SectionIndex].TrainReachedStopPoint = true;
								}
							} else {
								Sections[SectionIndex].TrainReachedStopPoint = true;
							}
						}
					}
					double t = -15.0;
					if (Stations[d].DepartureTime >= 0.0) {
						t = Stations[d].DepartureTime - 15.0;
					} else if (Stations[d].ArrivalTime >= 0.0) {
						t = Stations[d].ArrivalTime;
					}
					if (train == TrainManager.PlayerTrain & Stations[d].StationType != StationType.Normal & Stations[d].DepartureTime < 0.0) {
						settored = true;
					} else if (t >= 0.0 & SecondsSinceMidnight < t - train.TimetableDelta) {
						settored = true;
					} else if (!Sections[SectionIndex].TrainReachedStopPoint) {
						settored = true;
					}
				} else if (Stations[d].StationType != StationType.Normal) {
					settored = true;
				}
			}
			// train in block
			if (!Sections[SectionIndex].IsFree()) {
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
					}
					if (newaspect == -1) {
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
			// apply new aspect
			Sections[SectionIndex].CurrentAspect = newaspect;
			// update previous section
			if (Sections[SectionIndex].PreviousSection >= 0) {
				UpdateSection(Sections[SectionIndex].PreviousSection);
			}
		}
		
		// get plugin signal
		/// <summary>Gets the signal data for a plugin.</summary>
		/// <param name="train">The train.</param>
		/// <param name="section">The absolute section index, referencing Game.Sections[].</param>
		/// <returns>The signal data.</returns>
		internal static OpenBveApi.Runtime.SignalData GetPluginSignal(TrainManager.Train train, int section) {
			if (Sections[section].Exists(train)) {
				int aspect;
				if (Sections[section].IsFree(train)) {
					if (Sections[section].Type == SectionType.IndexBased) {
						if (section + 1 < Sections.Length) {
							int value = Sections[section + 1].FreeSections;
							if (value == -1) {
								value = Sections[section].Aspects.Length - 1;
							} else {
								value++;
								if (value >= Sections[section].Aspects.Length) {
									value = Sections[section].Aspects.Length - 1;
								}
								if (value < 0) {
									value = 0;
								}
							}
							aspect = Sections[section].Aspects[value].Number;
						} else {
							aspect = Sections[section].Aspects[Sections[section].Aspects.Length - 1].Number;
						}
					} else {
						aspect = Sections[section].Aspects[Sections[section].Aspects.Length - 1].Number;
						if (section < Sections.Length - 1) {
							int value = Sections[section + 1].Aspects[Sections[section + 1].CurrentAspect].Number;
							for (int i = 0; i < Sections[section].Aspects.Length; i++) {
								if (Sections[section].Aspects[i].Number > value) {
									aspect = Sections[section].Aspects[i].Number;
									break;
								}
							}
						}
					}
				} else {
					aspect = Sections[section].Aspects[Sections[section].CurrentAspect].Number;
				}
				double position = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxlePosition + 0.5 * train.Cars[0].Length;
				double distance = Sections[section].TrackPosition - position;
				return new OpenBveApi.Runtime.SignalData(aspect, distance);
			} else {
				int aspect = Sections[section].Aspects[Sections[section].CurrentAspect].Number;
				double position = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxlePosition + 0.5 * train.Cars[0].Length;
				double distance = Sections[section].TrackPosition - position;
				return new OpenBveApi.Runtime.SignalData(aspect, distance);
			}
		}
		
		// update plugin sections
		/// <summary>Updates the plugin to inform about sections.</summary>
		/// <param name="train">The train.</param>
		internal static void UpdatePluginSections(TrainManager.Train train) {
			if (train.Plugin != null) {
				OpenBveApi.Runtime.SignalData[] data = new OpenBveApi.Runtime.SignalData[16];
				int count = 0;
				int start = train.CurrentSectionIndex >= 0 ? train.CurrentSectionIndex : 0;
				for (int i = start; i < Sections.Length; i++) {
					OpenBveApi.Runtime.SignalData signal = GetPluginSignal(train, i);
					if (data.Length == count) {
						Array.Resize<OpenBveApi.Runtime.SignalData>(ref data, data.Length << 1);
					}
					data[count] = signal;
					count++;
					if (signal.Aspect == 0 | count == 16) {
						break;
					}
				}
				Array.Resize<OpenBveApi.Runtime.SignalData>(ref data, count);
				train.Plugin.UpdateSignals(data);
			}
		}

		// buffers
		internal static double[] BufferTrackPositions = new double[] { };

		// ================================


		internal enum MessageDependency {
			None = 0,
			RouteLimit = 1,
			SectionLimit = 2,
			Station = 3
		}
		internal struct Message {
			internal string InternalText;
			internal string DisplayText;
			internal MessageDependency Depencency;
			internal double Timeout;
			internal MessageColor Color;
			internal Vector2 RendererPosition;
			internal double RendererAlpha;
		}
		internal static Message[] Messages = new Message[] { };
		internal static Vector2 MessagesRendererSize = new Vector2(16.0, 16.0);
        /// <summary>Adds a message to the in-game interface render queue</summary>
        /// <param name="Text">The text of the message</param>
        /// <param name="Depencency"></param>
        /// <param name="Mode"></param>
        /// <param name="Color">The color of the message text</param>
        /// <param name="Timeout">The time this message will display for</param>
		internal static void AddMessage(string Text, MessageDependency Depencency, Interface.GameMode Mode, MessageColor Color, double Timeout) {
			if (Interface.CurrentOptions.GameMode <= Mode) {
				if (Depencency == MessageDependency.RouteLimit | Depencency == MessageDependency.SectionLimit) {
					for (int i = 0; i < Messages.Length; i++) {
						if (Messages[i].Depencency == Depencency) return;
					}
				}
				int n = Messages.Length;
				Array.Resize<Message>(ref Messages, n + 1);
				Messages[n].InternalText = Text;
				Messages[n].DisplayText = "";
				Messages[n].Depencency = Depencency;
				Messages[n].Timeout = Timeout;
				Messages[n].Color = Color;
				Messages[n].RendererPosition = new Vector2(0.0, 0.0);
				Messages[n].RendererAlpha = 0.0;
			}
		}
		internal static void AddDebugMessage(string text, double duration) {
			Game.AddMessage(text, Game.MessageDependency.None, Interface.GameMode.Expert, MessageColor.Magenta, Game.SecondsSinceMidnight + duration);
		}

	    internal static double SpeedConversionFactor = 0.0;
	    internal static string UnitOfSpeed = "km/h";

		internal static void UpdateMessages() {
			for (int i = 0; i < Messages.Length; i++) {
				bool remove = SecondsSinceMidnight >= Messages[i].Timeout;
				switch (Messages[i].Depencency) {
					case MessageDependency.RouteLimit:
						{
							double spd = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed);
							double lim = TrainManager.PlayerTrain.CurrentRouteLimit;
                            //Get the speed and limit in km/h
							spd = Math.Round(spd * 3.6);
							lim = Math.Round(lim * 3.6);
							remove = spd <= lim;
							string s = Messages[i].InternalText, t;
                            if (SpeedConversionFactor != 0.0)
                            {
                                spd = Math.Round(spd * SpeedConversionFactor);
                                lim = Math.Round(lim * SpeedConversionFactor);
                            }
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
                            s = s.Replace("[unit]", UnitOfSpeed);
							Messages[i].DisplayText = s;
						} break;
					case MessageDependency.SectionLimit:
						{
							double spd = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed);
							double lim = TrainManager.PlayerTrain.CurrentSectionLimit;
							spd = Math.Round(spd * 3.6);
							lim = Math.Round(lim * 3.6);
							remove = spd <= lim;
							string s = Messages[i].InternalText, t;
                            if (SpeedConversionFactor != 0.0)
                            {
                                spd = Math.Round(spd * SpeedConversionFactor);
                                lim = Math.Round(lim * SpeedConversionFactor);
                            }
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
                            s = s.Replace("[unit]", UnitOfSpeed);
							Messages[i].DisplayText = s;
						} break;
					case MessageDependency.Station:
						{
							int j = TrainManager.PlayerTrain.Station;
							if (j >= 0 & TrainManager.PlayerTrain.StationState != TrainManager.TrainStopState.Completed) {
								double d = TrainManager.PlayerTrain.StationDepartureTime - SecondsSinceMidnight + 1.0;
								if (d < 0.0) d = 0.0;
								string s = Messages[i].InternalText;
								TimeSpan a = TimeSpan.FromSeconds(d);
								System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
								string t = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
								s = s.Replace("[time]", t);
								s = s.Replace("[name]", Stations[j].Name);
								Messages[i].DisplayText = s;
								if (d > 0.0) remove = false;
							} else {
								remove = true;
							}
						} break;
					default:
						Messages[i].DisplayText = Messages[i].InternalText;
						break;
				}
				if (remove) {
					if (Messages[i].Timeout == double.PositiveInfinity) {
						Messages[i].Timeout = SecondsSinceMidnight - 1.0;
					}
					if (SecondsSinceMidnight >= Messages[i].Timeout & Messages[i].RendererAlpha == 0.0) {
						for (int j = i; j < Messages.Length - 1; j++) {
							Messages[j] = Messages[j + 1];
						}
						i--;
						Array.Resize<Message>(ref Messages, Messages.Length - 1);
					}
				}
			}
		}

		// ================================

		// marker
		internal static Textures.Texture[] MarkerTextures = new Textures.Texture[] { };
		internal static void AddMarker(Textures.Texture Texture) {
			int n = MarkerTextures.Length;
			Array.Resize<Textures.Texture>(ref MarkerTextures, n + 1);
			MarkerTextures[n] = Texture;
		}
		internal static void RemoveMarker(Textures.Texture Texture) {
			int n = MarkerTextures.Length;
			for (int i = 0; i < n; i++) {
				if (MarkerTextures[i] == Texture) {
					for (int j = i; j < n - 1; j++) {
						MarkerTextures[j] = MarkerTextures[j + 1];
					}
					n--;
					Array.Resize<Textures.Texture>(ref MarkerTextures, n);
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
		    if (j < 0) return false;
		    TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, t, true, false);
		    World.CameraCurrentAlignment.Position = PointsOfInterest[j].TrackOffset;
		    World.CameraCurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
		    World.CameraCurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
		    World.CameraCurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
		    World.CameraCurrentAlignment.TrackPosition = t;
		    World.UpdateAbsoluteCamera(0.0);
		    if (PointsOfInterest[j].Text != null) {
		        double n = 3.0 + 0.5 * Math.Sqrt((double)PointsOfInterest[j].Text.Length);
		        Game.AddMessage(PointsOfInterest[j].Text, Game.MessageDependency.None, Interface.GameMode.Expert, MessageColor.White, Game.SecondsSinceMidnight + n);
		    }
		    return true;
		}


	}
}