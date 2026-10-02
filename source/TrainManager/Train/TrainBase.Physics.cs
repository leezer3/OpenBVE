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
		public override void Update(double timeElapsed)
		{
			lock (updateLock)
			{
				if (State == TrainState.Pending)
				{
					// pending train
					bool forceIntroduction = !IsPlayerTrain && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation;
					double time = 0.0;
					if (!forceIntroduction)
					{
						for (int i = 0; i < TrainManagerBase.CurrentRoute.Stations.Length; i++)
						{
							if (TrainManagerBase.CurrentRoute.Stations[i].StopMode == StationStopMode.AllStop | TrainManagerBase.CurrentRoute.Stations[i].StopMode == StationStopMode.PlayerPass)
							{
								if (TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime >= 0.0)
								{
									time = TrainManagerBase.CurrentRoute.Stations[i].ArrivalTime;
								}
								else if (TrainManagerBase.CurrentRoute.Stations[i].DepartureTime >= 0.0)
								{
									time = TrainManagerBase.CurrentRoute.Stations[i].DepartureTime - TrainManagerBase.CurrentRoute.Stations[i].StopTime;
								}

								break;
							}
						}

						time -= TimetableDelta;
					}

					if (TrainManagerBase.CurrentRoute.SecondsSinceMidnight >= time | forceIntroduction)
					{
						bool introduce = true;
						if (!forceIntroduction)
						{
							if (CurrentSectionIndex >= 0 && TrainManagerBase.CurrentRoute.Sections.Length > CurrentSectionIndex)
							{
								if (!TrainManagerBase.CurrentRoute.Sections[CurrentSectionIndex].IsFree())
								{
									introduce = false;
								}
							}
						}

						if (this == TrainManagerBase.PlayerTrain && TrainManagerBase.currentHost.SimulationState != SimulationState.Loading)
						{
							/* Loading has finished, but we still have an AI train in the current section
							 * This may be caused by an iffy RunInterval value, or simply by having no sections							 *
							 *
							 * We must introduce the player's train as otherwise the cab and loop sounds are missing
							 * NOTE: In this case, the signalling cannot prevent the player from colliding with
							 * the AI train
							 */

							introduce = true;
						}

						if (introduce)
						{
							// train is introduced
							State = TrainState.Available;
							for (int j = 0; j < Cars.Length; j++)
							{
								if (j == DriverCar && IsPlayerTrain && TrainManagerBase.CurrentOptions.InitialViewpoint == 0)
								{
									Cars[j].ChangeCarSection(CarSectionType.Interior);
								}
								else
								{
									/*
									 * HACK: Load in exterior mode first to ensure everything is cached
									 * before switching immediately to not visible
									 * https://github.com/leezer3/OpenBVE/issues/226
									 * Stuff like the R142A really needs to downsize the textures supplied,
									 * but we have no control over external factors....
									 */
									Cars[j].ChangeCarSection(CarSectionType.Exterior);
									if (IsPlayerTrain && TrainManagerBase.CurrentOptions.InitialViewpoint == 0)
									{
										Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
									}
								}

								if (Cars[j].TractionModel !=  null && Cars[j].TractionModel.ProvidesPower && Cars[j].Sounds.Loop != null)
								{
									Cars[j].Sounds.Loop.Play(Cars[j], true);
								}
							}
						}
					}
				}
				else if (State == TrainState.Available)
				{
					// available train
					UpdatePhysicsAndControls(timeElapsed);
					if (TrainManagerBase.CurrentOptions.Accessibility)
					{
						Section nextSection = TrainManagerBase.CurrentRoute.NextSection(FrontCarTrackPosition);
						if (nextSection != null)
						{
							//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
							//Aspect announce to be triggered via a separate keybind
							double tPos = nextSection.TrackPosition - FrontCarTrackPosition;
							if (!nextSection.AccessibilityAnnounced && tPos < 500)
							{
								string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "route_nextsection" }).Replace("[distance]", $"{tPos:0.0}") + "m";
								TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
								nextSection.AccessibilityAnnounced = true;
							}
						}

						RouteStation nextStation = TrainManagerBase.CurrentRoute.NextStation(FrontCarTrackPosition);
						if (nextStation != null)
						{
							//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
							//Aspect announce to be triggered via a separate keybind
							double tPos = nextStation.DefaultTrackPosition - FrontCarTrackPosition;
							if (!nextStation.AccessibilityAnnounced && tPos < 500)
							{
								string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "route_nextstation" }).Replace("[distance]", $"{tPos:0.0}m").Replace("[name]", nextStation.Name);
								TrainManagerBase.currentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
								nextStation.AccessibilityAnnounced = true;
							}
						}
					}

					if (TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
					{
						if (CurrentSectionLimit == 0.0)
						{
							TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_stop" }), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						}
						else if (CurrentSpeed > CurrentSectionLimit)
						{
							TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_overspeed" }), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					}

					AI?.Trigger(timeElapsed);
				}
				else if (State == TrainState.Bogus)
				{
					// bogus train
					AI?.Trigger(timeElapsed);
				}

				//Trigger point sounds if appropriate
				for (int i = 0; i < Cars.Length; i++)
				{
					CarSound c = null;
					if (Cars[i].FrontAxle.PointSoundTriggered)
					{
						Cars[i].FrontAxle.PointSoundTriggered = false;
						int bufferIndex = Cars[i].FrontAxle.RunIndex;
						if (bufferIndex > Cars[i].FrontAxle.PointSounds.Length - 1)
						{
							//If the switch sound does not exist, return zero
							//Required to handle legacy trains which don't have idx specific run sounds defined
							bufferIndex = 0;
						}

						if (Cars[i].FrontAxle.PointSounds == null || Cars[i].FrontAxle.PointSounds.Length == 0)
						{
							//No point sounds defined at all
							continue;
						}

						c = (CarSound)Cars[i].FrontAxle.PointSounds[bufferIndex];
						if (c.Buffer == null)
						{
							c = (CarSound)Cars[i].FrontAxle.PointSounds[0];
						}
					}

					if (c != null)
					{
						double spd = Math.Abs(CurrentSpeed);
						double pitch = spd / 12.5;
						double gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
						if (pitch > 0.2 && gain > 0.2)
						{
							c.Play(pitch, gain, Cars[i], false);
						}
					}
				}
			}
		}

		/// <summary>Updates the physics and controls for this train</summary>
		/// <param name="timeElapsed">The time elapsed</param>
		private void UpdatePhysicsAndControls(double timeElapsed)
		{
			if (timeElapsed == 0.0 || timeElapsed > 1000)
			{
				//HACK: The physics engine really does not like update times above 1000ms
				//This works around a bug experienced when jumping to a station on a steep hill
				//causing exessive acceleration
				return;
			}

			// Car level initial processing
			for (int i = 0; i < Cars.Length; i++)
			{
				// move cars
				
				Cars[i].Move((double.IsNaN(Cars[i].CurrentSpeed) ? 0 : Cars[i].CurrentSpeed) * timeElapsed);
				if (State >= TrainState.DisposePending)
				{
					return;
				}
				// update cargo and related score
				Cars[i].Cargo.Update(Specs.CurrentAverageAcceleration, timeElapsed);
			}

			// update station and doors
			UpdateStation(timeElapsed);
			UpdateDoors(timeElapsed);
			// delayed handles
			if (Plugin == null)
			{
				Handles.Power.ApplySafetyState(Handles.Power.Driver);
				Handles.Brake.ApplySafetyState(Handles.Brake.Driver);
				Handles.EmergencyBrake.Safety = Handles.EmergencyBrake.Driver;
			}

			Handles.Power.Update(timeElapsed);
			Handles.Brake.Update(timeElapsed);
			if (Handles.HasLocoBrake)
			{
				Handles.LocoBrake.Update(timeElapsed);
			}
			Handles.EmergencyBrake.Update();
			Handles.HoldBrake.Actual = Handles.HoldBrake.Driver;
			for(int i = 0; i < Cars[DriverCar].SafetySystems.Count; i++)
			{
				SafetySystem system = Cars[DriverCar].SafetySystems.ElementAt(i).Key;
				Cars[DriverCar].SafetySystems[system].Update(timeElapsed);
			}
			// update speeds
			UpdateSpeeds(timeElapsed);
			// Update Run and Motor sounds
			for (int i = 0; i < Cars.Length; i++)
			{
				Cars[i].Run.Update(timeElapsed);
				for (int j = 0; j < Cars[i].Sounds.ControlledSounds.Count; j++)
				{
					Cars[i].Sounds.ControlledSounds[j].Update(timeElapsed);
				}
			}

			// safety system
			if (TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation | !IsPlayerTrain)
			{
				UpdateSafetySystem();
			}

			{
				// breaker sound
				bool breaker;
				if (Cars[DriverCar].CarBrake is AutomaticAirBrake)
				{
					breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == (int) AirBrakeHandleState.Release & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
				}
				else
				{
					breaker = Handles.Reverser.Actual != 0 & Handles.Power.Safety >= 1 & Handles.Brake.Safety == 0 & !Handles.EmergencyBrake.Safety & !Handles.HoldBrake.Actual;
				}
				Cars[DriverCar].Breaker?.Update(breaker);
			}
			// signals
			if (CurrentSectionLimit == 0.0)
			{
				if (Handles.EmergencyBrake.Driver & CurrentSpeed > -0.03 & CurrentSpeed < 0.03)
				{
					CurrentSectionLimit = 6.94444444444444;
					if (IsPlayerTrain)
					{
						string s = Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_proceed"});
						double a = (3.6 * CurrentSectionLimit) * TrainManagerBase.CurrentOptions.SpeedConversionFactor;
						s = s.Replace("[speed]", a.ToString("0", CultureInfo.InvariantCulture));
						s = s.Replace("[unit]", TrainManagerBase.CurrentOptions.UnitOfSpeed);
						TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, 5.0, null);
					}
				}
			}

			// infrequent updates
			InternalTimerTimeElapsed += timeElapsed;
			if (InternalTimerTimeElapsed > 10.0)
			{
				InternalTimerTimeElapsed -= 10.0;
				Synchronize();
			}
		}
	}
}
