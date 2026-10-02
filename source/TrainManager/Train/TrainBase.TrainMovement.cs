using LibRender2.Screens;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using SoundManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;
using SafetySystem = TrainManager.SafetySystems.SafetySystem;

namespace TrainManager.Trains
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public partial class TrainBase : AbstractTrain
	{
		/// <inheritdoc/>
		public override void Reverse(bool flipInterior = false, bool flipDriver = false)
		{
			lock (updateLock)
			{
				double trackPosition = Cars[0].TrackPosition;
				Cars = Cars.Reverse().ToArray();
				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Index = i;
					Cars[i].Reverse(flipInterior);
					// Re-create the coupler with appropriate distances between the cars
					double minDistance = 0, maxDistance = 0;
					if (i < Cars.Length - 1)
					{
						minDistance = Cars[i + 1].Coupler.MinimumDistanceBetweenCars;
						maxDistance = Cars[i + 1].Coupler.MaximumDistanceBetweenCars;
					}

					Cars[i].Coupler = new Coupler(minDistance, maxDistance, Cars[i], i < Cars.Length - 1 ? Cars[i + 1] : null);
					for (int j = 0; j < Cars[i].ParticleSources.Count; j++)
					{
						Cars[i].ParticleSources[j].Offset.Z = -Cars[i].ParticleSources[j].Offset.Z;
					}
				}

				PlaceCars(trackPosition);
				DriverCar = Cars.Length - 1 - DriverCar;
				CameraCar = Cars.Length - 1 - CameraCar;
				UpdateCabObjects();
			}
		}



		/// <inheritdoc/>
		public override double FrontCarTrackPosition
		{
			get
			{
				if (CurrentDirection == TrackDirection.Reverse)
				{
					return Cars[Cars.Length - 1].FrontAxle.Follower.TrackPosition - Cars[Cars.Length - 1].FrontAxle.Position + 0.5 * Cars[Cars.Length -  1].Length;
				}
				return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;
			}
		}
		
		/// <inheritdoc/>
		public override double RearCarTrackPosition
		{
			get
			{
				if (CurrentDirection == TrackDirection.Reverse)
				{
					return Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position + 0.5 * Cars[0].Length;	
				}
				return Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition - Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[Cars.Length - 1].Length;
			}
		}

		public override void Jump(int stationIndex, int trackKey)
		{
			if (IsPlayerTrain)
			{
				for (int i = 0; i < TrainManagerBase.CurrentRoute.Sections.Length; i++)
				{
					TrainManagerBase.CurrentRoute.Sections[i].AccessibilityAnnounced = false;
				}
			}

			SafetySystems.PassAlarm?.Halt();
			int currentTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
			StationState = TrainStopState.Jumping;
			int stopIndex = TrainManagerBase.CurrentRoute.Stations[stationIndex].GetStopIndex(this);
			if (stopIndex >= 0)
			{
				if (IsPlayerTrain)
				{
					Plugin?.BeginJump((InitializationModes) TrainManagerBase.CurrentOptions.TrainStart);
				}

				for (int h = 0; h < Cars.Length; h++)
				{
					Cars[h].CurrentSpeed = 0.0;
					// Change the track followers to the appropriate track
					Cars[h].FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearAxle.Follower.TrackIndex = trackKey;
					Cars[h].FrontBogie.FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].FrontBogie.RearAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearBogie.FrontAxle.Follower.TrackIndex = trackKey;
					Cars[h].RearBogie.RearAxle.Follower.TrackIndex = trackKey;
				}

				double d = TrainManagerBase.CurrentRoute.Stations[stationIndex].Stops[stopIndex].TrackPosition - Cars[0].FrontAxle.Follower.TrackPosition + Cars[0].FrontAxle.Position - 0.5 * Cars[0].Length;
				if (IsPlayerTrain)
				{
					SoundsBase.SuppressSoundEvents = true;
				}

				while (d != 0.0)
				{
					double x;
					if (Math.Abs(d) > 1.0)
					{
						x = Math.Sign(d);
					}
					else
					{
						x = d;
					}

					for (int h = 0; h < Cars.Length; h++)
					{
						Cars[h].Move(x);
					}

					if (Math.Abs(d) >= 1.0)
					{
						d -= x;
					}
					else
					{
						break;
					}
				}

				if (IsPlayerTrain)
				{
					SoundsBase.SuppressSoundEvents = false;
				}

				if (Handles.EmergencyBrake.Driver)
				{
					Handles.Power.ApplyState(0, false);
				}
				else
				{
					Handles.Brake.ApplyState(Handles.Brake.MaximumNotch, false);
					Handles.Power.ApplyState(0, false);
					if (Handles.Brake is AirBrakeHandle)
					{
						Handles.Brake.ApplyState(AirBrakeHandleState.Service);
					}
					
				}

				if (TrainManagerBase.CurrentRoute.Sections.Length > 0)
				{
					TrainManagerBase.CurrentRoute.UpdateAllSections();
				}

				if (IsPlayerTrain)
				{
					if (TrainManagerBase.CurrentRoute.Stations[stationIndex].JumpTime > 0.0)
					{
						// jump time is set, so use that (BVE5)
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].JumpTime;
					}
					else if (TrainManagerBase.CurrentRoute.Stations[stationIndex].ArrivalTime >= 0.0)
					{
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].ArrivalTime;
					}
					else if (TrainManagerBase.CurrentRoute.Stations[stationIndex].DepartureTime >= 0.0)
					{
						TrainManagerBase.CurrentRoute.SecondsSinceMidnight = TrainManagerBase.CurrentRoute.Stations[stationIndex].DepartureTime - TrainManagerBase.CurrentRoute.Stations[stationIndex].StopTime;
					}
				}

				for (int i = 0; i < Cars.Length; i++)
				{
					Cars[i].Doors[0].AnticipatedOpen = TrainManagerBase.CurrentRoute.Stations[stationIndex].OpenLeftDoors;
					Cars[i].Doors[1].AnticipatedOpen = TrainManagerBase.CurrentRoute.Stations[stationIndex].OpenRightDoors;
					Cars[i].ReAdhesionDevice?.Jump();
				}
				if (IsPlayerTrain)
				{
					Plugin?.EndJump();
				}

				StationState = TrainStopState.Pending;
				if (IsPlayerTrain)
				{
					LastStation = stationIndex;
				}

				int newTrackElement = Cars[0].FrontAxle.Follower.LastTrackElement;
				if (newTrackElement < currentTrackElement)
				{
					for (int i = newTrackElement; i < currentTrackElement; i++)
					{
						for (int j = 0; j < TrainManagerBase.currentHost.Tracks[0].Elements[i].Events.Count; j++)
						{
							TrainManagerBase.currentHost.Tracks[0].Elements[i].Events[j].Reset();
						}

					}
				}
				TrainManagerBase.currentHost.ProcessJump(this, stationIndex, 0);
			}
		}

		public override void Couple(AbstractTrain train, bool front)
		{
			TrainBase trainBase = train as TrainBase;
			if (trainBase == null)
			{
				throw new Exception("Attempted to couple to something that isn't a train");
			}

			int oldCars = Cars.Length;
			/*
			 * NOTE: Need to set the speeds to zero for *both* trains on coupling
			 *       The 'old' train will be disposed of immediately, but if not
			 *       set to zero, we can get glitched acceleration and the train enters orbit....
			 */

			if (front)
			{
				CarBase[] newCars = new CarBase[Cars.Length + trainBase.Cars.Length];
				Array.Copy(Cars, 0, newCars, trainBase.Cars.Length, Cars.Length);
				Array.Copy(trainBase.Cars, 0, newCars, 0, trainBase.Cars.Length);
				Cars = newCars;
				// camera / driver car is now down the train
				DriverCar += trainBase.Cars.Length;
				CameraCar += trainBase.Cars.Length;
			}
			else
			{
				Array.Resize(ref Cars, Cars.Length + trainBase.Cars.Length);
				// add new cars to end
				for (int i = 0; i < trainBase.Cars.Length; i++)
				{
					Cars[i + oldCars] = trainBase.Cars[i];
					Cars[i + oldCars].Index = i + oldCars;
					trainBase.Cars[i].CurrentSpeed = 0;
				}
			}

			// set properties
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].baseTrain = this;
				Cars[i].CurrentSpeed = 0;
				if ((int)TrainManagerBase.Renderer.Camera.CurrentMode > 1)
				{
					Cars[i].ChangeCarSection(CarSectionType.Exterior);
				}
				Cars[i].FrontAxle.Follower.Train = this;
				Cars[i].RearAxle.Follower.Train = this;
				Cars[i].FrontBogie.FrontAxle.Follower.Train = this;
				Cars[i].FrontBogie.RearAxle.Follower.Train = this;
				Cars[i].RearBogie.FrontAxle.Follower.Train = this;
				Cars[i].RearBogie.RearAxle.Follower.Train = this;
				if (i < Cars.Length - 1)
				{
					Cars[i].Coupler.ConnectedCar = Cars[i + 1];
				}
				Cars[i].Index = i;
			}

			Cars[oldCars].Coupler.CoupleSound.Play(1.0, 1.0, Cars[oldCars], false);

			Cars[0].BeaconReceiver.Train = this;

			/*
			 * Reset properties for 'old' train to empty cars
			 * 
			 * Due to multi-threading, we can't guarantee
			 * that something isn't trying to access the cars
			 * array until the *next* complete frame.
			 */
			for (int i = 0; i < trainBase.Cars.Length; i++)
			{
				trainBase.Cars[i] = new CarBase(trainBase, i);
			}

			Cars[DriverCar].Sounds.CoupleCab?.Play(1.0, 1.0, Cars[DriverCar], false);

			string message = Translations.GetInterfaceString(HostApplication.OpenBve, front ? new[] { "notification", "couple_front" } : new[] { "notification", "couple_rear" }).Replace("[number]", trainBase.Cars.Length.ToString());
			TrainManagerBase.currentHost.AddMessage(message, MessageDependency.None, GameMode.Normal, MessageColor.White, 5.0, null);
		}
	}
}
