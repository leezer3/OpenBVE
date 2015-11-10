using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	public static partial class Game {

		// date and time
		internal static double SecondsSinceMidnight = 0.0;
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
		internal static double[] RouteUnitOfLength = new double[] { 1.0 };
		internal const double CoefficientOfGroundFriction = 0.5;
		internal const double CriticalCollisionSpeedDifference = 8.0;
		internal const double BrakePipeLeakRate = 500000.0;
		internal const double MolarMass = 0.0289644;
		internal const double UniversalGasConstant = 8.31447;
		internal const double TemperatureLapseRate = -0.0065;
		internal const double CoefficientOfStiffness = 144117.325646911;

		// atmospheric functions
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
			} else {
				return 1.0;
			}
		}
		internal static double GetAirDensity(double AirPressure, double AirTemperature) {
			double x = AirPressure * MolarMass / (UniversalGasConstant * AirTemperature);
			if (x >= 0.001) {
				return x;
			} else {
				return 0.001;
			}
		}
		internal static double GetAirPressure(double Elevation, double AirTemperature) {
			double Exponent = -RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
			double Base = 1.0 + TemperatureLapseRate * Elevation / RouteSeaLevelAirTemperature;
			if (Base >= 0.0) {
				double x = RouteSeaLevelAirPressure * Math.Pow(Base, Exponent);
				if (x >= 0.001) {
					return x;
				} else {
					return 0.001;
				}
			} else {
				return 0.001;
			}
		}
		internal static double GetSpeedOfSound(double AirPressure, double AirTemperature) {
			double AirDensity = GetAirDensity(AirPressure, AirTemperature);
			return Math.Sqrt(CoefficientOfStiffness / AirDensity);
		}
		internal static double GetSpeedOfSound(double AirDensity) {
			return Math.Sqrt(CoefficientOfStiffness / AirDensity);
		}

		// other trains
		internal static double[] PrecedingTrainTimeDeltas = new double[] { };
		internal static double PrecedingTrainSpeedLimit = double.PositiveInfinity;
		internal static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };

		// startup
		internal enum TrainStartMode {
			ServiceBrakesAts = -1,
			EmergencyBrakesAts = 0,
			EmergencyBrakesNoAts = 1
		}
		internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesAts;
		internal static string TrainName = "";

		// information
		internal static double InfoFrameRate = 1.0;
		internal static string InfoDebugString = "";
		internal static int InfoTotalTriangles = 0;
		internal static int InfoTotalTriangleStrip = 0;
		internal static int InfoTotalQuadStrip = 0;
		internal static int InfoTotalQuads = 0;
		internal static int InfoTotalPolygon = 0;
		internal static int InfoStaticOpaqueFaceCount = 0;

		// ================================

		// interface type
		internal enum InterfaceType { Normal, Pause, Menu }
		internal static InterfaceType CurrentInterface = InterfaceType.Normal;
		internal enum MenuTag { None, Caption, Back, JumpToStation, ExitToMainMenu, Quit }
		internal abstract class MenuEntry {
			internal string Text;
			internal double Highlight;
			internal double Alpha;
		}
		internal class MenuCaption : MenuEntry {
			internal MenuCaption(string Text) {
				this.Text = Text;
				this.Highlight = 0.0;
				this.Alpha = 0.0;
			}
		}
		internal class MenuCommand : MenuEntry {
			internal MenuTag Tag;
			internal int Data;
			internal MenuCommand(string Text, MenuTag Tag, int Data) {
				this.Text = Text;
				this.Highlight = 0.0;
				this.Alpha = 0.0;
				this.Tag = Tag;
				this.Data = Data;
			}
		}
		internal class MenuSubmenu : MenuEntry {
			internal MenuEntry[] Entries;
			internal MenuSubmenu(string Text, MenuEntry[] Entries) {
				this.Text = Text;
				this.Highlight = 0.0;
				this.Alpha = 0.0;
				this.Entries = Entries;
			}
		}
		internal static MenuEntry[] CurrentMenu = new MenuEntry[] { };
		internal static int[] CurrentMenuSelection = new int[] { -1 };
		internal static double[] CurrentMenuOffsets = new double[] { double.NegativeInfinity };
		internal static void CreateMenu(bool QuitOnly) {
			if (QuitOnly) {
				// quit menu only
				CurrentMenu = new MenuEntry[3];
				CurrentMenu[0] = new MenuCaption(Interface.GetInterfaceString("menu_quit_question"));
				CurrentMenu[1] = new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0);
				CurrentMenu[2] = new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0);
				CurrentMenuSelection = new int[] { 1 };
				CurrentMenuOffsets = new double[] { double.NegativeInfinity };
				CurrentMenu[1].Highlight = 1.0;
			} else {
				// full menu
				int n = 0;
				for (int i = 0; i < Stations.Length; i++) {
					if (PlayerStopsAtStation(i) & Stations[i].Stops.Length > 0) {
						n++;
					}
				}
				MenuEntry[] a = new MenuCommand[n];
				n = 0;
				for (int i = 0; i < Stations.Length; i++) {
					if (PlayerStopsAtStation(i) & Stations[i].Stops.Length > 0) {
						a[n] = new MenuCommand(Stations[i].Name, MenuTag.JumpToStation, i);
						n++;
					}
				}
				if (n != 0) {
					CurrentMenu = new MenuEntry[4];
					CurrentMenu[0] = new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.Back, 0);
					CurrentMenu[1] = new MenuSubmenu(Interface.GetInterfaceString("menu_jump"), a);
					CurrentMenu[2] = new MenuSubmenu(Interface.GetInterfaceString("menu_exit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_exit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0)
					                                 });
					CurrentMenu[3] = new MenuSubmenu(Interface.GetInterfaceString("menu_quit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_quit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0)
					                                 });
				} else {
					CurrentMenu = new MenuEntry[3];
					CurrentMenu[0] = new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.Back, 0);
					CurrentMenu[1] = new MenuSubmenu(Interface.GetInterfaceString("menu_exit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_exit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0)
					                                 });
					CurrentMenu[2] = new MenuSubmenu(Interface.GetInterfaceString("menu_quit"), new MenuEntry[] {
					                                 	new MenuCaption(Interface.GetInterfaceString("menu_quit_question")),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0),
					                                 	new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0)
					                                 });
				}
				CurrentMenuSelection = new int[] { 0 };
				CurrentMenuOffsets = new double[] { double.NegativeInfinity };
				CurrentMenu[0].Highlight = 1.0;
			}
		}

		// ================================

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
		private const double ScoreFactorOpenedDoors = -10.0;
		private const double ScoreFactorOverspeed = -1.0;
		private const double ScoreFactorToppling = -10.0;
		private const double ScoreFactorStationLate = -0.333333333333333;
		private const double ScoreFactorStationStop = -50.0;
		private const double ScoreFactorStationDeparture = -1.5;
		private const int ScoreValueDerailment = -1000;
		private const int ScoreValueRedSignal = -100;
		private const int ScoreValueStationPerfectTime = 15;
		private const int ScoreValueStationPerfectStop = 15;
		private const int ScoreValuePassengerDiscomfort = -20;
		internal const int ScoreValueStationArrival = 100;
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
			internal World.Vector2D RendererPosition;
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
		internal static World.Vector2D ScoreMessagesRendererSize = new World.Vector2D(16.0, 16.0);
		internal static string LogRouteName = "";
		internal static string LogTrainName = "";
		internal static DateTime LogDateTime = DateTime.Now;
		private static void AddScore(int Value, ScoreTextToken TextToken, double Duration) {
			AddScore(Value, TextToken, Duration, true);
		}
		private static void AddScore(int Value, ScoreTextToken TextToken, double Duration, bool Count) {
			if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
				int n = ScoreMessages.Length;
				Array.Resize<ScoreMessage>(ref ScoreMessages, n + 1);
				ScoreMessages[n].Value = Value;
				ScoreMessages[n].Text = Interface.GetScoreText(TextToken) + ": " + Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
				ScoreMessages[n].Timeout = SecondsSinceMidnight + Duration;
				ScoreMessages[n].RendererPosition = new World.Vector2D(0.0, 0.0);
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
				ScoreMessages[n].RendererPosition = new World.Vector2D(0.0, 0.0);
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
            /// <summary>A public read-only variable, which returns the current aspcect to external scripts</summary>
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
				for (int i = 0; i < this.Trains.Length; i++) {
					if (this.Trains[i].State == TrainManager.TrainState.Available) {
						return this.Trains[i];
					} else if (AllowBogusTrain & this.Trains[i].State == TrainManager.TrainState.Bogus) {
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
						} else {
							l = Sections[l].PreviousSection;
						}
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

		// ai
		internal abstract class GeneralAI {
			internal abstract void Trigger(TrainManager.Train Train, double TimeElapsed);
		}
		
		// simple human driver
		internal class SimpleHumanDriverAI : GeneralAI {
			// members
			private double TimeLastProcessed;
			private double CurrentInterval;
			private bool BrakeMode;
			private double CurrentSpeedFactor;
			private readonly double PersonalitySpeedFactor;
			private int PowerNotchAtWhichWheelSlipIsObserved;
			private int LastStation;
			// functions
			internal SimpleHumanDriverAI(TrainManager.Train Train) {
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
				this.BrakeMode = false;
				this.PersonalitySpeedFactor = 0.90 + 0.10 * Program.RandomNumberGenerator.NextDouble();
				this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
				this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.MaximumPowerNotch + 1;
				if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Boarding) {
					this.LastStation = Train.Station;
				} else {
					this.LastStation = -1;
				}
			}
			private OpenBveApi.Runtime.AIResponse PerformPlugin(TrainManager.Train Train) {
				OpenBveApi.Runtime.AIResponse response = Train.Plugin.UpdateAI();
				if (response == OpenBveApi.Runtime.AIResponse.Short) {
					CurrentInterval = 0.2 + 0.1 * Program.RandomNumberGenerator.NextDouble();
				} else if (response == OpenBveApi.Runtime.AIResponse.Medium) {
					CurrentInterval = 0.4 + 0.2 * Program.RandomNumberGenerator.NextDouble();
				} else if (response == OpenBveApi.Runtime.AIResponse.Long) {
					CurrentInterval = 0.8 + 0.4 * Program.RandomNumberGenerator.NextDouble();
				}
				return response;
			}
			private void PerformDefault(TrainManager.Train Train) {
				// personality
				double spd = Train.Specs.CurrentAverageSpeed;
				if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Boarding) {
					if (Train.Station != this.LastStation) {
						this.LastStation = Train.Station;
						double time;
						if (Stations[Train.Station].ArrivalTime >= 0.0) {
							time = Stations[Train.Station].ArrivalTime - Train.TimetableDelta;
						} else if (Stations[Train.Station].DepartureTime >= 0.0) {
							time = Stations[Train.Station].DepartureTime - Train.TimetableDelta;
							if (time > SecondsSinceMidnight) {
								time -= Stations[Train.Station].StopTime;
								if (time > SecondsSinceMidnight) {
									time = double.MinValue;
								}
							}
						} else {
							time = double.MinValue;
						}
						if (time != double.MinValue) {
							const double largeThreshold = 30.0;
							const double largeChangeFactor = 0.0025;
							const double smallThreshold = 15.0;
							const double smallChange = 0.05;
							double diff = SecondsSinceMidnight - time;
							if (diff < -largeThreshold) {
								/* The AI is too fast. Decrease the preferred speed. */
								this.CurrentSpeedFactor -= largeChangeFactor * (-diff - largeThreshold);
								if (this.CurrentSpeedFactor < 0.7) {
									this.CurrentSpeedFactor = 0.7;
								}
							} else if (diff > largeThreshold) {
								/* The AI is too slow. Increase the preferred speed. */
								this.CurrentSpeedFactor += largeChangeFactor * (diff - largeThreshold);
								if (this.CurrentSpeedFactor > 1.1) {
									this.CurrentSpeedFactor = 1.1;
								}
							} else if (Math.Abs(diff) < smallThreshold) {
								/* The AI is at about the right speed. Change the preferred speed toward the personality default. */
								if (this.CurrentSpeedFactor < this.PersonalitySpeedFactor) {
									this.CurrentSpeedFactor += smallChange;
									if (this.CurrentSpeedFactor > this.PersonalitySpeedFactor) {
										this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
									}
								} else if (this.CurrentSpeedFactor > this.PersonalitySpeedFactor) {
									this.CurrentSpeedFactor -= smallChange;
									if (this.CurrentSpeedFactor < this.PersonalitySpeedFactor) {
										this.CurrentSpeedFactor = this.PersonalitySpeedFactor;
									}
								}
							}
						}
					}
				}
				// door states
				bool doorsopen = false;
				for (int i = 0; i < Train.Cars.Length; i++) {
					for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
						if (Train.Cars[i].Specs.Doors[j].State != 0.0) {
							doorsopen = true;
							break;
						}
						if (doorsopen) break;
					}
				}
				// do the ai
				Train.Specs.CurrentConstSpeed = false;
				TrainManager.ApplyHoldBrake(Train, false);
				int stopIndex = Train.Station >= 0 ? GetStopIndex(Train.Station, Train.Cars.Length) : -1;
				if (Train.CurrentSectionLimit == 0.0) {
					// passing red signal
					TrainManager.ApplyEmergencyBrake(Train);
					TrainManager.ApplyNotch(Train, -1, true, 1, true);
					CurrentInterval = 0.5;
				} else if (doorsopen | Train.StationState == TrainManager.TrainStopState.Boarding) {
					// door opened or boarding at station
					this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.MaximumPowerNotch + 1;
					if (Train.Station >= 0 && Stations[Train.Station].StationType != StationType.Normal && Train == TrainManager.PlayerTrain) {
						// player's terminal station
						TrainManager.ApplyReverser(Train, 0, false);
						TrainManager.ApplyNotch(Train, -1, true, 1, true);
						TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
						TrainManager.ApplyEmergencyBrake(Train);
						CurrentInterval = 1.0;
					} else {
						CurrentInterval = 1.0;
						TrainManager.ApplyNotch(Train, -1, true, 0, true);
						if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
							if (Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure < 0.3 * Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure) {
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
							} else if (Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure > 0.9 * Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure) {
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							} else {
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
							}
						} else {
							int b;
							if (Math.Abs(spd) < 0.02) {
								b = (int)Math.Ceiling(0.5 * (double)Train.Specs.MaximumBrakeNotch);
								CurrentInterval = 0.3;
							} else {
								b = Train.Specs.MaximumBrakeNotch;
							}
							if (Train.Specs.CurrentBrakeNotch.Driver < b) {
								TrainManager.ApplyNotch(Train, 0, true, 1, true);
							} else if (Train.Specs.CurrentBrakeNotch.Driver > b) {
								TrainManager.ApplyNotch(Train, 0, true, -1, true);
							}
						}
						TrainManager.UnapplyEmergencyBrake(Train);
						if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Completed) {
							// ready for departure - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						} else if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Boarding) {
						} else {
							// not at station - close doors
							if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
								TrainManager.CloseTrainDoors(Train, true, true);
							}
						}
					}
				} else if (Train.Station >= 0 && stopIndex >= 0 && Train.StationDistanceToStopPoint < Stations[Train.Station].Stops[stopIndex].BackwardTolerance && (StopsAtStation(Train.Station, Train) & (Stations[Train.Station].OpenLeftDoors | Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.25 & Train.StationState == TrainManager.TrainStopState.Pending)) {
					// arrived at station - open doors
					if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
						TrainManager.OpenTrainDoors(Train, Stations[Train.Station].OpenLeftDoors, Stations[Train.Station].OpenRightDoors);
					}
					CurrentInterval = 1.0;
				} else if (Train.Station >= 0 && Stations[Train.Station].StationType != StationType.Normal && Train == TrainManager.PlayerTrain && Train.StationDistanceToStopPoint < Stations[Train.Station].Stops[stopIndex].BackwardTolerance && -Train.StationDistanceToStopPoint < Stations[Train.Station].Stops[stopIndex].ForwardTolerance && Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.25) {
					// player's terminal station (not boarding any longer)
					TrainManager.ApplyReverser(Train, 0, false);
					TrainManager.ApplyNotch(Train, -1, true, 1, true);
					TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
					TrainManager.ApplyEmergencyBrake(Train);
					CurrentInterval = 10.0;
				} else {
					// drive
					TrainManager.ApplyReverser(Train, 1, false);
					if (Train.Cars[Train.DriverCar].FrontAxle.CurrentWheelSlip | Train.Cars[Train.DriverCar].RearAxle.CurrentWheelSlip) {
						// react to wheel slip
						if (Train.Specs.CurrentPowerNotch.Driver > 1) {
							this.PowerNotchAtWhichWheelSlipIsObserved = Train.Specs.CurrentPowerNotch.Driver;
							TrainManager.ApplyNotch(Train, -1, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							this.CurrentInterval = 2.5;
							return;
						}
					}
					// initialize
					double acc = Train.Specs.CurrentAverageAcceleration;
					double lim = PrecedingTrainSpeedLimit * 1.2;
					if (Train.CurrentRouteLimit < lim) {
						lim = Train.CurrentRouteLimit;
					}
					if (Train.CurrentSectionLimit < lim) {
						lim = Train.CurrentSectionLimit;
					}
					double powerstart, powerend, brakestart;
					if (double.IsPositiveInfinity(lim)) {
						powerstart = lim;
						powerend = lim;
						brakestart = lim;
					} else {
						lim *= this.CurrentSpeedFactor;
						if (spd < 8.0) {
							powerstart = 0.75 * lim;
							powerend = 0.95 * lim;
						} else {
							powerstart = lim - 2.5;
							powerend = lim - 1.5;
						}
						if (this.BrakeMode) {
							brakestart = powerend;
						} else {
							brakestart = lim + 0.5;
						}
					}
					double dec = 0.0;
					double decelerationCruise;   /* power below this deceleration, cruise above */
					double decelerationStart;    /* brake above this deceleration, cruise below */
					double decelerationStep;     /* the deceleration step per brake notch */
					double BrakeDeceleration = Train.Cars[Train.DriverCar].Specs.BrakeDecelerationAtServiceMaximumPressure;
					for (int i = 0; i < Train.Cars.Length; i++) {
						if (Train.Cars[i].Specs.IsMotorCar) {
							if (Train.Cars[Train.DriverCar].Specs.MotorDeceleration < BrakeDeceleration) {
								BrakeDeceleration = Train.Cars[Train.DriverCar].Specs.MotorDeceleration;
							}
							break;
						}
					}
					if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | Train.Specs.MaximumBrakeNotch <= 0) {
						decelerationCruise = 0.3 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = 0.1 * BrakeDeceleration;
					} else if (Train.Specs.MaximumBrakeNotch <= 2) {
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.4 * BrakeDeceleration;
						decelerationStep = 0.5 * BrakeDeceleration;
					} else {
						decelerationCruise = 0.2 * BrakeDeceleration;
						decelerationStart = 0.5 * BrakeDeceleration;
						decelerationStep = BrakeDeceleration / (double)Train.Specs.MaximumBrakeNotch;
					}
					if (this.CurrentSpeedFactor >= 1.0) {
						decelerationCruise *= 1.25;
						decelerationStart *= 1.25;
						decelerationStep *= 1.25;
					}
					
					if (spd > 0.0 & spd > brakestart) {
						dec = decelerationStep + 0.1 * (spd - brakestart);
					}
					bool reduceDecelerationCruiseAndStart = false;
					// look ahead
					double lookahead = (Train.Station >= 0 ? 150.0 : 50.0) + (spd * spd) / (2.0 * decelerationCruise);
					double tp = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
					double stopDistance = double.MaxValue;
					{
						// next station stop
						int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
						for (int i = te; i < TrackManager.CurrentTrack.Elements.Length; i++) {
							double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
								if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
									TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex) {
										int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
										if (s >= 0) {
											double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
											if (dist > 0.0 & dist < stopDistance) {
												stopDistance = dist;
											}
										}
									}
								}
							}
						}
					}
					{
						// events
						int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
						for (int i = te; i < TrackManager.CurrentTrack.Elements.Length; i++) {
							double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
							if (tp + lookahead <= stp) break;
							for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
								if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent) {
									// speed limit
									TrackManager.LimitChangeEvent e = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (e.NextSpeedLimit < spd) {
										double dist = stp + e.TrackPositionDelta - tp;
										double edec = (spd * spd - e.NextSpeedLimit * e.NextSpeedLimit * this.CurrentSpeedFactor) / (2.0 * dist);
										if (edec > dec) dec = edec;
									}
								} else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.SectionChangeEvent) {
									// section
									TrackManager.SectionChangeEvent e = (TrackManager.SectionChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
									if (stp + e.TrackPositionDelta > tp) {
										if (!Game.Sections[e.NextSectionIndex].Invisible & Game.Sections[e.NextSectionIndex].CurrentAspect >= 0) {
											double elim = Game.Sections[e.NextSectionIndex].Aspects[Game.Sections[e.NextSectionIndex].CurrentAspect].Speed * this.CurrentSpeedFactor;
											if (elim < spd | spd <= 0.0) {
												double dist = stp + e.TrackPositionDelta - tp;
												double edec;
												if (elim == 0.0) {
													double redstopdist;
													if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Completed & dist < 120.0) {
														dist = 1.0;
														redstopdist = 25.0;
													} else if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Pending | stopDistance < dist) {
														redstopdist = 1.0;
													} else if (spd > 9.72222222222222) {
														redstopdist = 55.0;
													} else {
														redstopdist = 35.0;
													}
													if (dist > redstopdist) {
														edec = (spd * spd) / (2.0 * (dist - redstopdist));
													} else {
														edec = BrakeDeceleration;
													}
													if (dist < 100.0) {
														reduceDecelerationCruiseAndStart = true;
													}
												} else {
													if (dist >= 1.0) {
														edec = (spd * spd - elim * elim) / (2.0 * dist);
													} else {
														edec = 0.0;
													}
												}
												if (edec > dec) dec = edec;
											}
										}
									}
								} else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationStartEvent) {
									// station start
									if (Train.Station == -1) {
										TrackManager.StationStartEvent e = (TrackManager.StationStartEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex) {
											int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
											if (s >= 0) {
												double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Stations[e.StationIndex].Stops[s].ForwardTolerance) {
													if (dist < 25.0) {
														reduceDecelerationCruiseAndStart = true;
													} else if (this.CurrentSpeedFactor < 1.0) {
														dist -= 5.0;
													}
												    var edec = spd * spd / (2.0 * dist);
													if (edec > dec) dec = edec;
												}
											}
										}
									}
								} else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent) {
									// station end
									if (Train.Station == -1) {
										TrackManager.StationEndEvent e = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
										if (StopsAtStation(e.StationIndex, Train) & Train.LastStation != e.StationIndex) {
											int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
											if (s >= 0) {
												double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
												if (dist > -Stations[e.StationIndex].Stops[s].ForwardTolerance) {
													if (dist < 25.0) {
														reduceDecelerationCruiseAndStart = true;
													} else if (this.CurrentSpeedFactor < 1.0) {
														dist -= 5.0;
													}
												    var edec = spd * spd / (2.0 * dist);
													if (edec > dec) dec = edec;
												}
											}
										}
									}
								} else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.TrackEndEvent) {
									// track end
									if (Train == TrainManager.PlayerTrain) {
										TrackManager.TrackEndEvent e = (TrackManager.TrackEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
										double dist = stp + e.TrackPositionDelta - tp;
										double edec;
										if (dist >= 15.0) {
											edec = spd * spd / (2.0 * dist);
										} else {
											edec = BrakeDeceleration;
										}
										if (edec > dec) dec = edec;
									}
								}
							}
						}
					}
					// buffers ahead
					if (Train == TrainManager.PlayerTrain) {
						for (int i = 0; i < BufferTrackPositions.Length; i++) {
							double dist = BufferTrackPositions[i] - tp;
							if (dist > 0.0) {
								double edec;
								if (dist >= 10.0) {
									edec = spd * spd / (2.0 * dist);
								} else if (dist >= 5.0) {
									TrainManager.ApplyNotch(Train, -1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								} else {
									TrainManager.ApplyNotch(Train, -1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									TrainManager.ApplyEmergencyBrake(Train);
									this.CurrentInterval = 10.0;
									return;
								}
								if (edec > dec) dec = edec;
							}
						}
					}
					// trains ahead
					for (int i = 0; i < TrainManager.Trains.Length; i++) {
						if (TrainManager.Trains[i] != Train && TrainManager.Trains[i].State == TrainManager.TrainState.Available) {
							double pos =
								TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].RearAxlePosition -
								0.5 * TrainManager.Trains[i].Cars[TrainManager.Trains[i].Cars.Length - 1].Length;
							double dist = pos - tp;
							if (dist > -10.0 & dist < lookahead) {
								const double minDistance = 10.0;
								const double maxDistance = 100.0;
								double edec;
								if (dist > minDistance) {
									double shift = 0.75 * minDistance + 1.0 * spd;
									edec = spd * spd / (2.0 * (dist - shift));
								} else if (dist > 0.5 * minDistance) {
									TrainManager.ApplyNotch(Train, -1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									this.CurrentInterval = 0.1;
									return;
								} else {
									TrainManager.ApplyNotch(Train, -1, true, 1, true);
									TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
									TrainManager.ApplyEmergencyBrake(Train);
									this.CurrentInterval = 1.0;
									return;
								}
								if (dist < maxDistance) {
									reduceDecelerationCruiseAndStart = true;
								}
								if (edec > dec) dec = edec;
							}
						}
					}
					TrainManager.UnapplyEmergencyBrake(Train);
					// current station
					if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Pending) {
						if (StopsAtStation(Train.Station, Train)) {
							int s = GetStopIndex(Train.Station, Train.Cars.Length);
							if (s >= 0) {
								double dist = Stations[Train.Station].Stops[s].TrackPosition - tp;
								if (dist > 0.0) {
									if (dist < 25.0) {
										reduceDecelerationCruiseAndStart = true;
									} else if (this.CurrentSpeedFactor < 1.0) {
										dist -= 5.0;
									}
								    var edec = spd * spd / (2.0 * dist);
									if (edec > dec) dec = edec;
								} else {
									dec = BrakeDeceleration;
								}
							}
						}
					}
					// power / brake
					if (reduceDecelerationCruiseAndStart) {
						decelerationCruise *= 0.3;
						decelerationStart *= 0.3;
					}
					double brakeModeBrakeThreshold = 0.75 * decelerationStart + 0.25 * decelerationCruise;
					if (!BrakeMode & dec > decelerationStart | BrakeMode & dec > brakeModeBrakeThreshold | false) {
						// brake
						BrakeMode = true;
						double decdiff = -acc - dec;
						if (decdiff < -decelerationStep) {
							// brake start
							if (Train.Specs.CurrentPowerNotch.Driver == 0) {
								TrainManager.ApplyNotch(Train, 0, true, 1, true);
								TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
							} else {
								TrainManager.ApplyNotch(Train, -1, true, 0, true);
							}
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						} else if (decdiff > decelerationStep) {
							// brake stop
							TrainManager.ApplyNotch(Train, -1, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.4;
							if (CurrentInterval < 0.3) CurrentInterval = 0.3;
						} else {
							// keep brake
							TrainManager.ApplyNotch(Train, -1, true, 0, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
							CurrentInterval *= 1.2;
							if (CurrentInterval > 1.0) CurrentInterval = 1.0;
						}
						if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0) {
							TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
						}
						if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
							CurrentInterval = 0.1;
						}
					} else if (dec > decelerationCruise) {
						// cut power/brake
						BrakeMode = false;
						TrainManager.ApplyNotch(Train, -1, true, -1, true);
						TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
						if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0) {
							TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
						}
						CurrentInterval *= 0.4;
						if (CurrentInterval < 0.3) CurrentInterval = 0.3;
					} else {
						// power
						BrakeMode = false;
						double acclim;
						if (!double.IsInfinity(lim)) {
							double d = lim - spd;
							if (d > 0.0) {
								acclim = 0.1 / (0.1 * d + 1.0) - 0.12;
							} else {
								acclim = -1.0;
							}
						} else {
							acclim = -1.0;
						}
						if (spd < powerstart) {
							// power start (under-speed)
							if (Train.Specs.CurrentBrakeNotch.Driver == 0) {
								if (Train.Specs.CurrentPowerNotch.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1) {
									TrainManager.ApplyNotch(Train, 1, true, 0, true);
								}
							} else {
								TrainManager.ApplyNotch(Train, 0, true, -1, true);
							}
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							if (double.IsPositiveInfinity(powerstart)) {
								CurrentInterval = 0.3 + 0.1 * Train.Specs.CurrentPowerNotch.Driver;
							} else {
								double p = (double)Train.Specs.CurrentPowerNotch.Driver / (double)Train.Specs.MaximumPowerNotch;
								CurrentInterval = 0.3 + 15.0 * p / (powerstart - spd + 1.0);
							}
							if (CurrentInterval > 1.3) CurrentInterval = 1.3;
						} else if (spd > powerend) {
							// power end (over-speed)
							TrainManager.ApplyNotch(Train, -1, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval *= 0.3;
							if (CurrentInterval < 0.2) CurrentInterval = 0.2;
						} else if (acc < acclim) {
							// power start (under-acceleration)
							if (Train.Specs.CurrentBrakeNotch.Driver == 0) {
								if (Train.Specs.CurrentPowerNotch.Driver < this.PowerNotchAtWhichWheelSlipIsObserved - 1) {
									if (Train.Specs.CurrentPowerNotch.Driver == Train.Specs.CurrentPowerNotch.Actual) {
										TrainManager.ApplyNotch(Train, 1, true, 0, true);
									}
								}
							} else {
								TrainManager.ApplyNotch(Train, 0, true, -1, true);
							}
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							CurrentInterval = 1.3;
						} else {
							// keep power
							TrainManager.ApplyNotch(Train, 0, true, -1, true);
							TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
							if (Train.Specs.CurrentPowerNotch.Driver != 0) {
								Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
							}
							if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0) {
								TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
							}
							CurrentInterval *= 1.1;
							if (CurrentInterval > 1.5) CurrentInterval = 1.5;
						}
					}
				}
			}
			internal override void Trigger(TrainManager.Train Train, double TimeElapsed) {
				if (TimeLastProcessed > SecondsSinceMidnight) {
					TimeLastProcessed = SecondsSinceMidnight;
				} else if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval) {
					TimeLastProcessed = SecondsSinceMidnight;
					if (Train.Plugin != null && Train.Plugin.SupportsAI) {
						if (PerformPlugin(Train) != OpenBveApi.Runtime.AIResponse.None) {
							return;
						}
					}
					PerformDefault(Train);
				}
			}
		}

		// bogus pretrain
		internal struct BogusPretrainInstruction {
			internal double TrackPosition;
			internal double Time;
		}
		internal class BogusPretrainAI : GeneralAI {
			private double TimeLastProcessed;
			private double CurrentInterval;
			internal BogusPretrainAI(TrainManager.Train Train) {
				this.TimeLastProcessed = 0.0;
				this.CurrentInterval = 1.0;
			}
			internal override void Trigger(TrainManager.Train Train, double TimeElapsed) {
				if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval) {
					TimeLastProcessed = SecondsSinceMidnight;
					CurrentInterval = 5.0;
					double ap = double.MaxValue, at = double.MaxValue;
					double bp = double.MinValue, bt = double.MinValue;
					for (int i = 0; i < BogusPretrainInstructions.Length; i++) {
						if (BogusPretrainInstructions[i].Time < SecondsSinceMidnight | at == double.MaxValue) {
							at = BogusPretrainInstructions[i].Time;
							ap = BogusPretrainInstructions[i].TrackPosition;
						}
					}
					for (int i = BogusPretrainInstructions.Length - 1; i >= 0; i--) {
						if (BogusPretrainInstructions[i].Time > at | bt == double.MinValue) {
							bt = BogusPretrainInstructions[i].Time;
							bp = BogusPretrainInstructions[i].TrackPosition;
						}
					}
					if (at != double.MaxValue & bt != double.MinValue & SecondsSinceMidnight <= BogusPretrainInstructions[BogusPretrainInstructions.Length - 1].Time) {
						double r = bt - at;
						if (r > 0.0) {
							r = (SecondsSinceMidnight - at) / r;
							if (r < 0.0) r = 0.0;
							if (r > 1.0) r = 1.0;
						} else {
							r = 0.0;
						}
						double p = ap + r * (bp - ap);
						double d = p - Train.Cars[0].FrontAxle.Follower.TrackPosition;
						for (int j = 0; j < Train.Cars.Length; j++) {
							TrainManager.MoveCar(Train, j, d, 0.1);
						}
					} else {
						TrainManager.DisposeTrain(Train);
					}
				}
			}
		}

		// ================================

		// messages
		internal enum MessageColor {
			None = 0,
			Black = 1,
			Gray = 2,
			White = 3,
			Red = 4,
			Orange = 5,
			Green = 6,
			Blue = 7,
			Magenta = 8
		}
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
			internal World.Vector2D RendererPosition;
			internal double RendererAlpha;
		}
		internal static Message[] Messages = new Message[] { };
		internal static World.Vector2D MessagesRendererSize = new World.Vector2D(16.0, 16.0);
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
				Messages[n].RendererPosition = new World.Vector2D(0.0, 0.0);
				Messages[n].RendererAlpha = 0.0;
			}
		}
		internal static void AddDebugMessage(string text, double duration) {
			Game.AddMessage(text, Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Magenta, Game.SecondsSinceMidnight + duration);
		}
		internal static void UpdateMessages() {
			for (int i = 0; i < Messages.Length; i++) {
				bool remove = SecondsSinceMidnight >= Messages[i].Timeout;
				switch (Messages[i].Depencency) {
					case MessageDependency.RouteLimit:
						{
							double spd = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed);
							double lim = TrainManager.PlayerTrain.CurrentRouteLimit;
							spd = Math.Round(spd * 3.6);
							lim = Math.Round(lim * 3.6);
							remove = spd <= lim;
							string s = Messages[i].InternalText, t;
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
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
							t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[speed]", t);
							t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
							s = s.Replace("[limit]", t);
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
			if (j >= 0) {
				TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, t, true, false);
				World.CameraCurrentAlignment.Position = PointsOfInterest[j].TrackOffset;
				World.CameraCurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
				World.CameraCurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
				World.CameraCurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
				World.CameraCurrentAlignment.TrackPosition = t;
				World.UpdateAbsoluteCamera(0.0);
				if (PointsOfInterest[j].Text != null) {
					double n = 3.0 + 0.5 * Math.Sqrt((double)PointsOfInterest[j].Text.Length);
					Game.AddMessage(PointsOfInterest[j].Text, Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.White, Game.SecondsSinceMidnight + n);
				}
				return true;
			} else {
				return false;
			}
		}


	}
}