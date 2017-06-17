using System;
using OpenBveApi.Math;

namespace OpenBve
{
	/*
	 * Contains the base definition of a train
	 */
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

			/// <summary>Call this method to move a car</summary>
			/// <param name="CarIndex">The car index to move</param>
			/// <param name="Delta">The length to move</param>
			/// <param name="TimeElapsed">The elapsed time</param>
			internal void MoveCar(int CarIndex, double Delta, double TimeElapsed)
			{
				if (State != TrainState.Disposed)
				{
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontAxle.Follower, Cars[CarIndex].FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontBogie.FrontAxle.Follower, Cars[CarIndex].FrontBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
					TrackManager.UpdateTrackFollower(ref Cars[CarIndex].FrontBogie.RearAxle.Follower, Cars[CarIndex].FrontBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
					if (State != TrainState.Disposed)
					{
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearAxle.Follower, Cars[CarIndex].RearAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearBogie.FrontAxle.Follower, Cars[CarIndex].RearBogie.FrontAxle.Follower.TrackPosition + Delta, true, true);
						TrackManager.UpdateTrackFollower(ref Cars[CarIndex].RearBogie.RearAxle.Follower, Cars[CarIndex].RearBogie.RearAxle.Follower.TrackPosition + Delta, true, true);
						if (State != TrainState.Disposed)
						{
							TrackManager.UpdateTrackFollower(ref Cars[CarIndex].BeaconReceiver, Cars[CarIndex].BeaconReceiver.TrackPosition + Delta, true, true);
						}
					}
				}
			}

			/// <summary>Call this method to place the cars of a train</summary>
			/// <param name="TrackPosition">The track position to start from</param>
			internal void PlaceCars(double TrackPosition)
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					//Front axle track position
					Cars[i].FrontAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].FrontAxle.Position;
					//Bogie for front axle
					Cars[i].FrontBogie.FrontAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.FrontAxle.Position;
					Cars[i].FrontBogie.RearAxle.Follower.TrackPosition = Cars[i].FrontAxle.Follower.TrackPosition - 0.5 * Cars[i].FrontBogie.Length + Cars[i].FrontBogie.RearAxlePosition;
					//Rear axle track position
					Cars[i].RearAxle.Follower.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].RearAxle.Position;
					//Bogie for rear axle
					Cars[i].RearBogie.FrontAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.FrontAxle.Position;
					Cars[i].RearBogie.RearAxle.Follower.TrackPosition = Cars[i].RearAxle.Follower.TrackPosition - 0.5 * Cars[i].RearBogie.Length + Cars[i].RearBogie.RearAxlePosition;
					//Beacon reciever (AWS, ATC etc.)
					Cars[i].BeaconReceiver.TrackPosition = TrackPosition - 0.5 * Cars[i].Length + Cars[i].BeaconReceiverPosition;
					TrackPosition -= Cars[i].Length;
					if (i < Cars.Length - 1)
					{
						TrackPosition -= 0.5 * (Couplers[i].MinimumDistanceBetweenCars + Couplers[i].MaximumDistanceBetweenCars);
					}
				}
			}

			internal void LoadExterior(string exteriorFolder, System.Text.Encoding textEncoding)
			{
				ObjectManager.UnifiedObject[] CarObjects;
				ObjectManager.UnifiedObject[] BogieObjects;
				ExtensionsCfgParser.ParseExtensionsConfig(exteriorFolder, textEncoding, out CarObjects, out BogieObjects, this);
				System.Threading.Thread.Sleep(1);
				//Stores the current array index of the bogie object to add
				//Required as there are two bogies per car, and we're using a simple linear array....
				int currentBogieObject = 0;
				for (int i = 0; i < Cars.Length; i++)
				{
					if (CarObjects[i] == null)
					{
						// load default exterior object
						string file = OpenBveApi.Path.CombineFile(Program.FileSystem.GetDataFolder("Compatibility"), "exterior.csv");
						ObjectManager.StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
						if (so == null)
						{
							CarObjects[i] = null;
						}
						else
						{
							double sx = Cars[i].Width;
							double sy = Cars[i].Height;
							double sz = Cars[i].Length;
							CsvB3dObjectParser.ApplyScale(so, sx, sy, sz);
							CarObjects[i] = so;
						}
					}
					if (CarObjects[i] != null)
					{
						// add object
						int j = Cars[i].CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].CarSections, j + 1);
						if (CarObjects[i] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)CarObjects[i];
							Cars[i].CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].CarSections[j].Elements[0] = new ObjectManager.AnimatedObject
							{
								States = new ObjectManager.AnimatedObjectState[1]
							};
							Cars[i].CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
							Cars[i].CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}

					//Load bogie objects
					if (BogieObjects[currentBogieObject] != null)
					{
						int j = Cars[i].FrontBogie.CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].FrontBogie.CarSections, j + 1);
						if (BogieObjects[currentBogieObject] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)BogieObjects[currentBogieObject];
							Cars[i].FrontBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].FrontBogie.CarSections[j].Elements[0] = new ObjectManager.AnimatedObject();
							Cars[i].FrontBogie.CarSections[j].Elements[0].States = new ObjectManager.AnimatedObjectState[1];
							Cars[i].FrontBogie.CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].FrontBogie.CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].FrontBogie.CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].FrontBogie.CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (BogieObjects[currentBogieObject] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)BogieObjects[currentBogieObject];
							Cars[i].FrontBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].FrontBogie.CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].FrontBogie.CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}
					currentBogieObject++;
					//Can't think of a better way to do this than two functions......
					if (BogieObjects[currentBogieObject] != null)
					{
						int j = Cars[i].RearBogie.CarSections.Length;
						Array.Resize<TrainManager.CarSection>(ref Cars[i].RearBogie.CarSections, j + 1);
						if (BogieObjects[currentBogieObject] is ObjectManager.StaticObject)
						{
							ObjectManager.StaticObject s = (ObjectManager.StaticObject)BogieObjects[currentBogieObject];
							Cars[i].RearBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[1];
							Cars[i].RearBogie.CarSections[j].Elements[0] = new ObjectManager.AnimatedObject
							{
								States = new ObjectManager.AnimatedObjectState[1]
							};
							Cars[i].RearBogie.CarSections[j].Elements[0].States[0].Position = new Vector3(0.0, 0.0, 0.0);
							Cars[i].RearBogie.CarSections[j].Elements[0].States[0].Object = s;
							Cars[i].RearBogie.CarSections[j].Elements[0].CurrentState = 0;
							Cars[i].RearBogie.CarSections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
						}
						else if (BogieObjects[currentBogieObject] is ObjectManager.AnimatedObjectCollection)
						{
							ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)BogieObjects[currentBogieObject];
							Cars[i].RearBogie.CarSections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
							for (int h = 0; h < a.Objects.Length; h++)
							{
								Cars[i].RearBogie.CarSections[j].Elements[h] = a.Objects[h];
								Cars[i].RearBogie.CarSections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
							}
						}
					}
					currentBogieObject++;
				}
			}

			/// <summary>Updates the atmospheric constants applying to this train (Effects brake pressure speeds etc.)</summary>
			internal void UpdateAtmosphericConstants()
			{
				double h = 0.0;
				for (int i = 0; i < Cars.Length; i++)
				{
					h += Cars[i].FrontAxle.Follower.WorldPosition.Y + Cars[i].RearAxle.Follower.WorldPosition.Y;
				}
				Specs.CurrentElevation = Game.RouteInitialElevation + h / (2.0 * (double)Cars.Length);
				Specs.CurrentAirTemperature = Game.GetAirTemperature(Specs.CurrentElevation);
				Specs.CurrentAirPressure = Game.GetAirPressure(Specs.CurrentElevation, Specs.CurrentAirTemperature);
				Specs.CurrentAirDensity = Game.GetAirDensity(Specs.CurrentAirPressure, Specs.CurrentAirTemperature);
			}

			/// <summary>If this train is currently non-visible (minimalistic simulation) used to update the position of all cars infrequently</summary>
			internal void Synchronize()
			{
				for (int i = 0; i < Cars.Length; i++)
				{
					double s = 0.5 * (Cars[i].FrontAxle.Follower.TrackPosition + Cars[i].RearAxle.Follower.TrackPosition);
					double d = 0.5 * (Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].RearAxle.Follower.TrackPosition);
					TrackManager.UpdateTrackFollower(ref Cars[i].FrontAxle.Follower, s + d, false, false);
					TrackManager.UpdateTrackFollower(ref Cars[i].RearAxle.Follower, s - d, false, false);
					double b = Cars[i].FrontAxle.Follower.TrackPosition - Cars[i].FrontAxle.Position + Cars[i].BeaconReceiverPosition;
					TrackManager.UpdateTrackFollower(ref Cars[i].BeaconReceiver, b, false, false);
				}
			}

			/// <summary>Updates this train's safety system plugin</summary>
			internal void UpdateSafetySystem()
			{
				Game.UpdatePluginSections(this);
				if (Plugin != null)
				{
					Plugin.LastSection = CurrentSectionIndex;
					Plugin.UpdatePlugin();
				}
			}
		}
	}
}
