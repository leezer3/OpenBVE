using OpenBveApi.Colors;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public class Train
		{
			/// <summary>The plugin used by this train.</summary>
			internal PluginManager.Plugin Plugin;
			internal int TrainIndex;
			internal TrainState State;
			internal Car[] Cars;
			internal Coupler[] Couplers;
			internal int DriverCar;
			internal TrainSpecs Specs;
			internal TrainPassengers Passengers;
			internal int LastStation;
			internal int Station;
			internal bool StationFrontCar;
			internal bool StationRearCar;
			internal TrainStopState StationState;
			internal double StationArrivalTime;
			internal double StationDepartureTime;
			internal bool StationDepartureSoundPlayed;
			internal bool StationAdjust;
			internal double StationDistanceToStopPoint;
			internal double[] RouteLimits;
			internal double CurrentRouteLimit;
			internal double CurrentSectionLimit;
			internal int CurrentSectionIndex;
			internal double TimetableDelta;
			internal Game.GeneralAI AI;
			internal double InternalTimerTimeElapsed;
			internal bool Derailed;
			internal StopSkipMode NextStopSkipped = StopSkipMode.None;

			internal void Initialize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Initialize();
				}
				UpdateAtmosphericConstants(this);
				Update(0.0);
			}

			/// <summary>Disposes of the train</summary>
			internal void Dispose()
			{
				State = TrainState.Disposed;
				for (int i = 0; i < Cars.Length; i++)
				{
					int s = Cars[i].CurrentCarSection;
					if (s >= 0)
					{
						for (int j = 0; j < Cars[i].CarSections[s].Elements.Length; j++)
						{
							Renderer.HideObject(Cars[i].CarSections[s].Elements[j].ObjectIndex);
						}
					}
					s = Cars[i].FrontBogie.CurrentCarSection;
					if (s >= 0)
					{
						for (int j = 0; j < Cars[i].FrontBogie.CarSections[s].Elements.Length; j++)
						{
							Renderer.HideObject(Cars[i].FrontBogie.CarSections[s].Elements[j].ObjectIndex);
						}
					}
					s = Cars[i].RearBogie.CurrentCarSection;
					if (s >= 0)
					{
						for (int j = 0; j < Cars[i].RearBogie.CarSections[s].Elements.Length; j++)
						{
							Renderer.HideObject(Cars[i].RearBogie.CarSections[s].Elements[j].ObjectIndex);
						}
					}
				}
				Sounds.StopAllSounds(this);

				for (int i = 0; i < Game.Sections.Length; i++)
				{
					Game.Sections[i].Leave(this);
				}
				if (Game.Sections.Length != 0)
				{
					Game.UpdateSection(Game.Sections.Length - 1);
				}
			}

			/// <summary>Call this method to update the train</summary>
			/// <param name="TimeElapsed">The elapsed time this frame</param>
			internal void Update(double TimeElapsed)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					bool forceIntroduction = this == PlayerTrain && !Game.MinimalisticSimulation;
					double time = 0.0;
					if (!forceIntroduction)
					{
						for (int i = 0; i < Game.Stations.Length; i++)
						{
							if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass)
							{
								if (Game.Stations[i].ArrivalTime >= 0.0)
								{
									time = Game.Stations[i].ArrivalTime;
								}
								else if (Game.Stations[i].DepartureTime >= 0.0)
								{
									time = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
								}
								break;
							}
						}
						time -= TimetableDelta;
					}
					if (Game.SecondsSinceMidnight >= time | forceIntroduction)
					{
						bool introduce = true;
						if (!forceIntroduction)
						{
							if (CurrentSectionIndex >= 0)
							{
								if (!Game.Sections[CurrentSectionIndex].IsFree())
								{
									introduce = false;
								}
							}
						}
						if (introduce)
						{
							// train is introduced
							State = TrainState.Available;
							for (int j = 0; j < Cars.Length; j++)
							{
								if (Cars[j].CarSections.Length != 0)
								{
									Cars[j].ChangeCarSection(j <= DriverCar | this != PlayerTrain ? 0 : -1);
								}
								Cars[j].FrontBogie.ChangeSection(this != PlayerTrain ? 0 : -1);
								Cars[j].RearBogie.ChangeSection(this != PlayerTrain ? 0 : -1);
								if (Cars[j].Specs.IsMotorCar)
								{
									if (Cars[j].Sounds.Loop.Buffer != null)
									{
										OpenBveApi.Math.Vector3 pos = Cars[j].Sounds.Loop.Position;
										Cars[j].Sounds.Loop.Source = Sounds.PlaySound(Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, pos, this, j, true);
									}
								}
							}
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdateTrainPhysicsAndControls(this, TimeElapsed);
					if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
					{
						if (Specs.CurrentAverageSpeed > CurrentRouteLimit)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), MessageManager.MessageDependency.RouteLimit, Interface.GameMode.Arcade, MessageColor.Orange, double.PositiveInfinity, null);
						}
						if (CurrentSectionLimit == 0.0)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (Specs.CurrentAverageSpeed > CurrentSectionLimit)
						{
							Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					}
					if (AI != null)
					{
						AI.Trigger(this, TimeElapsed);
					}
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					if (AI != null)
					{
						AI.Trigger(this, TimeElapsed);
					}
				}
			}

			/// <summary>Updates the safety system for this train</summary>
			internal void UpdateSafetySystem()
			{
				Game.UpdatePluginSections(this);
				if (Plugin != null)
				{
					Plugin.LastSection = CurrentSectionIndex;
					Plugin.UpdatePlugin();
				}
			}

			/// <summary>Updates the objects for all cars in this train</summary>
			/// <param name="TimeElapsed">The time elapsed</param>
			/// <param name="ForceUpdate">Whether this is a forced update</param>
			internal void UpdateObjects(double TimeElapsed, bool ForceUpdate)
			{
				if (!Game.MinimalisticSimulation)
				{
					for (int i = 0; i < Cars.Length; i++)
					{
						Cars[i].UpdateObjects(TimeElapsed, ForceUpdate, true);
						Cars[i].FrontBogie.UpdateObjects(TimeElapsed, ForceUpdate);
						Cars[i].RearBogie.UpdateObjects(TimeElapsed, ForceUpdate);
					}
				}
			}

			internal void UpdateCabObjects()
			{
				//BUG: ?? This probably ought to be the driver car index, not zero ??
				Cars[0].UpdateObjects(0.0, true, false);
			}

			/// <summary>Synchronizes the entire train after a period of infrequent updates</summary>
			internal void Synchronize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Syncronize();
				}
			}

			/// <summary>Call this method to derail a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			internal void Derail(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Derailed = true;
				this.Derailed = true;
				if (Program.GenerateDebugLogging)
				{
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " derailed. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <summary>Call this method to topple a car</summary>
			/// <param name="CarIndex">The car index to derail</param>
			/// <param name="ElapsedTime">The elapsed time for this frame (Used for logging)</param>
			internal void Topple(int CarIndex, double ElapsedTime)
			{
				this.Cars[CarIndex].Topples = true;
				if (Program.GenerateDebugLogging)
				{
					Program.AppendToLogFile("Train " + TrainIndex + ", Car " + CarIndex + " toppled. Current simulation time: " + Game.SecondsSinceMidnight + " Current frame time: " + ElapsedTime);
				}
			}

			/// <summary>Initializes a train with the default (empty) set of car sounds</summary>
			internal void InitializeCarSounds()
			{
				// initialize
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
					Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
					Cars[i].Sounds.Adjust = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Air = TrainManager.CarSound.Empty;
					Cars[i].Sounds.AirHigh = TrainManager.CarSound.Empty;
					Cars[i].Sounds.AirZero = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Brake = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleApply = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleMin = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleMax = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BrakeHandleRelease = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
					Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpEnd = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpLoop = TrainManager.CarSound.Empty;
					Cars[i].Sounds.CpStart = TrainManager.CarSound.Empty;
					Cars[i].Sounds.DoorCloseL = TrainManager.CarSound.Empty;
					Cars[i].Sounds.DoorCloseR = TrainManager.CarSound.Empty;
					Cars[i].Sounds.DoorOpenL = TrainManager.CarSound.Empty;
					Cars[i].Sounds.DoorOpenR = TrainManager.CarSound.Empty;
					Cars[i].Sounds.EmrBrake = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
					Cars[i].Sounds.FlangeVolume = new double[] { };
					Cars[i].Sounds.Halt = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Horns = new TrainManager.Horn[]
					{
						new TrainManager.Horn(),
						new TrainManager.Horn(),
						new TrainManager.Horn()
					};
					Cars[i].Sounds.RequestStop = new CarSound[]
					{
						//Stop
						CarSound.Empty,
						//Pass
						CarSound.Empty,
						//Ignored
						CarSound.Empty
					};
					Cars[i].Sounds.Loop = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
					Cars[i].Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
					Cars[i].Sounds.PilotLampOn = TrainManager.CarSound.Empty;
					Cars[i].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
					Cars[i].Sounds.PointFrontAxle = new TrainManager.CarSound[] { };
					Cars[i].Sounds.PointRearAxle = new TrainManager.CarSound[] { };
					Cars[i].Sounds.ReverserOn = TrainManager.CarSound.Empty;
					Cars[i].Sounds.ReverserOff = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Rub = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
					Cars[i].Sounds.RunVolume = new double[] { };
					Cars[i].Sounds.SpringL = TrainManager.CarSound.Empty;
					Cars[i].Sounds.SpringR = TrainManager.CarSound.Empty;
					Cars[i].Sounds.Plugin = new TrainManager.CarSound[] { };
				}
			}
		}
	}
}
