using System;
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve {
	internal static class TrackManager {

		// events
		internal enum EventTriggerType {
			None = 0,
			Camera = 1,
			FrontCarFrontAxle = 2,
			RearCarRearAxle = 3,
			OtherCarFrontAxle = 4,
			OtherCarRearAxle = 5,
			TrainFront = 6
		}
		internal abstract class GeneralEvent {
			internal double TrackPositionDelta;
			internal bool DontTriggerAnymore;
			internal abstract void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex);
		}
		internal static void TryTriggerEvent(GeneralEvent Event, int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
			if (!Event.DontTriggerAnymore) {
				Event.Trigger(Direction, TriggerType, Train, CarIndex);
			}
		}
		// background change
		internal class BackgroundChangeEvent : GeneralEvent {
			internal BackgroundManager.BackgroundHandle PreviousBackground;
			internal BackgroundManager.BackgroundHandle NextBackground;
			internal BackgroundChangeEvent(double TrackPositionDelta, BackgroundManager.BackgroundHandle PreviousBackground, BackgroundManager.BackgroundHandle NextBackground) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousBackground = PreviousBackground;
				this.NextBackground = NextBackground;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.Camera) {
					if (Direction < 0) {
						BackgroundManager.TargetBackground = this.PreviousBackground;
					} else if (Direction > 0) {
						BackgroundManager.TargetBackground = this.NextBackground;
					}
				}
			}
		}
		// fog change
		internal class FogChangeEvent : GeneralEvent {
			internal Game.Fog PreviousFog;
			internal Game.Fog CurrentFog;
			internal Game.Fog NextFog;
			internal FogChangeEvent(double TrackPositionDelta, Game.Fog PreviousFog, Game.Fog CurrentFog, Game.Fog NextFog) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousFog = PreviousFog;
				this.CurrentFog = CurrentFog;
				this.NextFog = NextFog;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.Camera) {
					if (Direction < 0) {
						Game.PreviousFog = this.PreviousFog;
						Game.NextFog = this.CurrentFog;
					} else if (Direction > 0) {
						Game.PreviousFog = this.CurrentFog;
						Game.NextFog = this.NextFog;
					}
				}
			}
		}
		// brightness change
		internal class BrightnessChangeEvent : GeneralEvent {
			internal float CurrentBrightness;
			internal float PreviousBrightness;
			internal double PreviousDistance;
			internal float NextBrightness;
			internal double NextDistance;
			internal BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance, float NextBrightness, double NextDistance) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.CurrentBrightness = CurrentBrightness;
				this.PreviousBrightness = PreviousBrightness;
				this.PreviousDistance = PreviousDistance;
				this.NextBrightness = NextBrightness;
				this.NextDistance = NextDistance;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
					if (Direction < 0) {
						//Train.Cars[CarIndex].Brightness.NextBrightness = Train.Cars[CarIndex].Brightness.PreviousBrightness;
						Train.Cars[CarIndex].Brightness.NextBrightness = this.CurrentBrightness;
						Train.Cars[CarIndex].Brightness.NextTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition;
						Train.Cars[CarIndex].Brightness.PreviousBrightness = this.PreviousBrightness;
						Train.Cars[CarIndex].Brightness.PreviousTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition - this.PreviousDistance;
					} else if (Direction > 0) {
						//Train.Cars[CarIndex].Brightness.PreviousBrightness = Train.Cars[CarIndex].Brightness.NextBrightness;
						Train.Cars[CarIndex].Brightness.PreviousBrightness = this.CurrentBrightness;
						Train.Cars[CarIndex].Brightness.PreviousTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition;
						Train.Cars[CarIndex].Brightness.NextBrightness = this.NextBrightness;
						Train.Cars[CarIndex].Brightness.NextTrackPosition = Train.Cars[CarIndex].FrontAxle.Follower.TrackPosition + this.NextDistance;
					}
				}
			}
		}
		// marker start
		internal class MarkerStartEvent : GeneralEvent
		{
			internal MessageManager.Message Message;
			internal MarkerStartEvent(double trackPositionDelta, MessageManager.Message message) {
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Message = message;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (Train == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle) {
					if (this.Message != null)
					{
						if (Direction < 0)
						{
							this.Message.QueueForRemoval = true;
						}
						else if (Direction > 0)
						{
							if (this.Message.Trains != null && !this.Message.Trains.Contains(new System.IO.DirectoryInfo(Game.RouteInformation.TrainFolder).Name))
							{
								//Our train is NOT in the list of trains which this message triggers for
								return;
							}
							MessageManager.AddMessage(this.Message);
							
						}
					}
					
				}
			}
		}
		// marker end
		internal class MarkerEndEvent : GeneralEvent
		{
			internal MessageManager.Message Message;
			internal MarkerEndEvent(double trackPositionDelta, MessageManager.Message message) {
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Message = message;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (Train == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle) {
					if (this.Message != null)
					{
						if (Direction < 0)
						{
							if (this.Message.Trains != null && !this.Message.Trains.Contains(new System.IO.DirectoryInfo(Game.RouteInformation.TrainFolder).Name))
							{
								//Our train is NOT in the list of trains which this message triggers for
								return;
							}
							MessageManager.AddMessage(this.Message);
						}
						else if (Direction > 0)
						{
							this.Message.QueueForRemoval = true;
						}
					}
				}
			}
		}
		// station pass alarm
		internal class StationPassAlarmEvent : GeneralEvent {
			internal StationPassAlarmEvent(double TrackPositionDelta) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
					if (Direction > 0) {
						int d = Train.DriverCar;
						Sounds.SoundBuffer buffer = Train.Cars[d].Sounds.Halt.Buffer;
						if (buffer != null) {
							OpenBveApi.Math.Vector3 pos = Train.Cars[d].Sounds.Halt.Position;
							if (Train.Specs.PassAlarm == TrainManager.PassAlarmType.Single) {
								Train.Cars[d].Sounds.Halt.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, d, false);
							} else if (Train.Specs.PassAlarm == TrainManager.PassAlarmType.Loop) {
								Train.Cars[d].Sounds.Halt.Source = Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, d, true);
							}
						}
						this.DontTriggerAnymore = true;
					}
				}
			}
		}
		// station start
		internal class StationStartEvent : GeneralEvent {
			internal int StationIndex;
			internal StationStartEvent(double TrackPositionDelta, int StationIndex) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.StationIndex = StationIndex;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.TrainFront) {
					if (Direction < 0) {
						Train.Station = -1;
						Train.StationFrontCar = false;
					} else if (Direction > 0) {
						Train.Station = StationIndex;
						Train.StationFrontCar = true;
						Train.StationState = TrainManager.TrainStopState.Pending;
						Train.LastStation = this.StationIndex;
					}
				} else if (TriggerType == EventTriggerType.RearCarRearAxle) {
					if (Direction < 0) {
						Train.StationRearCar = false;
					} else {
						Train.StationRearCar = true;
					}
				}
			}
		}
		// station end
		internal class StationEndEvent : GeneralEvent {
			internal int StationIndex;
			internal StationEndEvent(double TrackPositionDelta, int StationIndex) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.StationIndex = StationIndex;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
					if (Direction < 0) {
						Train.StationFrontCar = true;
					} else if (Direction > 0) {
						Train.StationFrontCar = false;
						if (Train == TrainManager.PlayerTrain) {
							Timetable.UpdateCustomTimetable(Game.Stations[this.StationIndex].TimetableDaytimeTexture, Game.Stations[this.StationIndex].TimetableNighttimeTexture);
						}
					}
				} else if (TriggerType == EventTriggerType.RearCarRearAxle) {
					if (Direction < 0) {
						Train.Station = this.StationIndex;
						Train.StationRearCar = true;
						Train.LastStation = this.StationIndex;
					} else if (Direction > 0) {
						if (Train.Station == StationIndex) {
							if (Train == TrainManager.PlayerTrain) {
								if (Game.PlayerStopsAtStation(StationIndex) & TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending) {
									string s = Interface.GetInterfaceString("message_station_passed");
									s = s.Replace("[name]", Game.Stations[StationIndex].Name);
									Game.AddMessage(s, Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Orange, Game.SecondsSinceMidnight + 10.0, null);
								} else if (Game.PlayerStopsAtStation(StationIndex) & TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Boarding) {
									string s = Interface.GetInterfaceString("message_station_passed_boarding");
									s = s.Replace("[name]", Game.Stations[StationIndex].Name);
									Game.AddMessage(s, Game.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Red, Game.SecondsSinceMidnight + 10.0, null);
								}
							}
							Train.Station = -1;
							Train.StationRearCar = false;
							Train.StationState = TrainManager.TrainStopState.Pending;
							int d = Train.DriverCar;
							Sounds.StopSound(Train.Cars[d].Sounds.Halt.Source);
						}
					}
				}
			}
		}
		// section change
		internal class SectionChangeEvent : GeneralEvent {
			internal int PreviousSectionIndex;
			internal int NextSectionIndex;
			internal SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousSectionIndex = PreviousSectionIndex;
				this.NextSectionIndex = NextSectionIndex;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (Train != null) {
					if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
						if (Direction < 0) {
							if (this.NextSectionIndex >= 0) {
								Game.Sections[this.NextSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateFrontBackward(Train, true);
						} else if (Direction > 0) {
							UpdateFrontForward(Train, true, true);
						}
					} else if (TriggerType == EventTriggerType.RearCarRearAxle) {
						if (Direction < 0) {
							UpdateRearBackward(Train, true);
						} else if (Direction > 0) {
							if (this.PreviousSectionIndex >= 0) {
								Game.Sections[this.PreviousSectionIndex].TrainReachedStopPoint = false;
							}
							UpdateRearForward(Train, true);
						}
					}
				}
			}
			private void UpdateFrontBackward(TrainManager.Train Train, bool UpdateTrain) {
				// update sections
				if (this.PreviousSectionIndex >= 0) {
					Game.Sections[this.PreviousSectionIndex].Enter(Train);
					Game.UpdateSection(this.PreviousSectionIndex);
				}
				if (this.NextSectionIndex >= 0) {
					Game.Sections[this.NextSectionIndex].Leave(Train);
					Game.UpdateSection(this.NextSectionIndex);
				}
				if (UpdateTrain) {
					// update train
					if (this.PreviousSectionIndex >= 0) {
						if (!Game.Sections[this.PreviousSectionIndex].Invisible) {
							Train.CurrentSectionIndex = this.PreviousSectionIndex;
						}
					} else {
						Train.CurrentSectionLimit = double.PositiveInfinity;
						Train.CurrentSectionIndex = -1;
					}
				}
			}
			private void UpdateFrontForward(TrainManager.Train Train, bool UpdateTrain, bool UpdateSection) {
				if (UpdateTrain) {
					// update train
					if (this.NextSectionIndex >= 0) {
						if (!Game.Sections[this.NextSectionIndex].Invisible) {
							if (Game.Sections[this.NextSectionIndex].CurrentAspect >= 0) {
								Train.CurrentSectionLimit = Game.Sections[this.NextSectionIndex].Aspects[Game.Sections[this.NextSectionIndex].CurrentAspect].Speed;
							} else {
								Train.CurrentSectionLimit = double.PositiveInfinity;
							}
							Train.CurrentSectionIndex = this.NextSectionIndex;
						}
					} else {
						Train.CurrentSectionLimit = double.PositiveInfinity;
						Train.CurrentSectionIndex = -1;
					}
					// messages
					if (this.NextSectionIndex < 0 || !Game.Sections[this.NextSectionIndex].Invisible) {
						if (Train.CurrentSectionLimit == 0.0 && Game.MinimalisticSimulation == false) {
							Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), Game.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
						} else if (Train.Specs.CurrentAverageSpeed > Train.CurrentSectionLimit) {
							Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), Game.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					}
				}
				if (UpdateSection) {
					// update sections
					if (this.NextSectionIndex >= 0) {
						Game.Sections[this.NextSectionIndex].Enter(Train);
						Game.UpdateSection(this.NextSectionIndex);
					}
				}
			}
			private void UpdateRearBackward(TrainManager.Train Train, bool UpdateSection) {
				if (UpdateSection) {
					// update sections
					if (this.PreviousSectionIndex >= 0) {
						Game.Sections[this.PreviousSectionIndex].Enter(Train);
						Game.UpdateSection(this.PreviousSectionIndex);
					}
				}
			}
			private void UpdateRearForward(TrainManager.Train Train, bool UpdateSection) {
				if (UpdateSection) {
					// update sections
					if (this.PreviousSectionIndex >= 0) {
						Game.Sections[this.PreviousSectionIndex].Leave(Train);
						Game.UpdateSection(this.PreviousSectionIndex);
					}
					if (this.NextSectionIndex >= 0) {
						Game.Sections[this.NextSectionIndex].Enter(Train);
						Game.UpdateSection(this.NextSectionIndex);
					}
				}
			}
		}
		// transponder
		internal static class SpecialTransponderTypes {
			/// <summary>Marks the status of ATC.</summary>
			internal const int AtcTrackStatus = -16777215;
			/// <summary>Sets up an ATC speed limit.</summary>
			internal const int AtcSpeedLimit = -16777214;
			/// <summary>Sets up an ATS-P temporary speed limit.</summary>
			internal const int AtsPTemporarySpeedLimit = -16777213;
			/// <summary>Sets up an ATS-P permanent speed limit.</summary>
			internal const int AtsPPermanentSpeedLimit = -16777212;
			/// <summary>For internal use inside the CSV/RW parser only.</summary>
			internal const int InternalAtsPTemporarySpeedLimit = -16777201;
		}
		internal class TransponderEvent : GeneralEvent {
			internal int Type;
			internal int Data;
			internal int SectionIndex;
			internal bool ClipToFirstRedSection;
			internal TransponderEvent(double trackPositionDelta, int type, int data, int sectionIndex, bool clipToFirstRedSection) {
				this.TrackPositionDelta = trackPositionDelta;
				this.DontTriggerAnymore = false;
				this.Type = type;
				this.Data = data;
				this.SectionIndex = sectionIndex;
				this.ClipToFirstRedSection = clipToFirstRedSection;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.TrainFront) {
					int s = this.SectionIndex;
					if (this.ClipToFirstRedSection) {
						if (s >= 0) {
							while (true) {
								if (Game.Sections[s].Exists(Train)) {
									s = this.SectionIndex;
									break;
								}
								int a = Game.Sections[s].CurrentAspect;
								if (a >= 0) {
									if (Game.Sections[s].Aspects[a].Number == 0) {
										break;
									}
								}
								s = Game.Sections[s].PreviousSection;
								if (s < 0) {
									s = this.SectionIndex;
									break;
								}
							}
						}
					}
					if (Train.Plugin != null) {
						Train.Plugin.UpdateBeacon((int)this.Type, s, this.Data);
					}
				}
			}
		}
		// limit change
		internal class LimitChangeEvent : GeneralEvent {
			internal double PreviousSpeedLimit;
			internal double NextSpeedLimit;
			internal LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousSpeedLimit = PreviousSpeedLimit;
				this.NextSpeedLimit = NextSpeedLimit;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (Direction < 0) {
					if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
						int n = Train.RouteLimits.Length;
						if (n > 0) {
							Array.Resize<double>(ref Train.RouteLimits, n - 1);
							Train.CurrentRouteLimit = double.PositiveInfinity;
							for (int i = 0; i < n - 1; i++) {
								if (Train.RouteLimits[i] < Train.CurrentRouteLimit) {
									Train.CurrentRouteLimit = Train.RouteLimits[i];
								}
							}
						}
					} else if (TriggerType == EventTriggerType.RearCarRearAxle) {
						int n = Train.RouteLimits.Length;
						Array.Resize<double>(ref Train.RouteLimits, n + 1);
						for (int i = n; i > 0; i--) {
							Train.RouteLimits[i] = Train.RouteLimits[i - 1];
						}
						Train.RouteLimits[0] = this.PreviousSpeedLimit;
					}
				} else if (Direction > 0) {
					if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
						int n = Train.RouteLimits.Length;
						Array.Resize<double>(ref Train.RouteLimits, n + 1);
						Train.RouteLimits[n] = this.NextSpeedLimit;
						if (this.NextSpeedLimit < Train.CurrentRouteLimit) {
							Train.CurrentRouteLimit = this.NextSpeedLimit;
						}
						if (Train.Specs.CurrentAverageSpeed > this.NextSpeedLimit) {
							Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), Game.MessageDependency.RouteLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
						}
					} else if (TriggerType == EventTriggerType.RearCarRearAxle) {
						int n = Train.RouteLimits.Length;
						if (n > 0) {
							Train.CurrentRouteLimit = double.PositiveInfinity;
							for (int i = 0; i < n - 1; i++) {
								Train.RouteLimits[i] = Train.RouteLimits[i + 1];
								if (Train.RouteLimits[i] < Train.CurrentRouteLimit) {
									Train.CurrentRouteLimit = Train.RouteLimits[i];
								}
							} Array.Resize<double>(ref Train.RouteLimits, n - 1);
						}
					}
				}
			}
		}
		// sound
		internal static bool SuppressSoundEvents = false;
		internal class SoundEvent : GeneralEvent {
			/// <summary>The sound buffer to play.
			/// HACK: Set to a null reference to indicate the train point sound.</summary>
			internal Sounds.SoundBuffer SoundBuffer;
			internal bool PlayerTrainOnly;
			internal bool Once;
			internal bool Dynamic;
			internal Vector3 Position;
			internal double Speed;

			/// <param name="TrackPositionDelta">The delta position of the sound within a track block.</param>
			/// <param name="SoundBuffer">The sound buffer to play. 
			/// HACK: Set to a null reference to indicate the train point sound.</param>
			/// <param name="PlayerTrainOnly">Defines whether this sound is played for the player's train only, or for player and AI trains</param>
			/// <param name="Once">Defines whether this sound repeats looped, or plays once</param>
			/// <param name="Dynamic">Whether this sound is dynamic (Attached to a train)</param>
			/// <param name="Position">The position of the sound relative to it's track location</param>
			/// <param name="Speed">The speed in km/h at which this sound is played at it's original pitch (Set to zero to play at original pitch at all times)</param>
			internal SoundEvent(double TrackPositionDelta, Sounds.SoundBuffer SoundBuffer, bool PlayerTrainOnly, bool Once, bool Dynamic, Vector3 Position, double Speed) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.SoundBuffer = SoundBuffer;
				this.PlayerTrainOnly = PlayerTrainOnly;
				this.Once = Once;
				this.Dynamic = Dynamic;
				this.Position = Position;
				this.Speed = Speed;
			}

			/// <summary>Triggers the playback of a sound</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="CarIndex">The car index which triggered this sound</param>
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (SuppressSoundEvents) return;
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle | TriggerType == EventTriggerType.OtherCarRearAxle | TriggerType == EventTriggerType.RearCarRearAxle) {
					if (!PlayerTrainOnly | Train == TrainManager.PlayerTrain) {
						Vector3 p = this.Position;
						double pitch = 1.0;
						double gain = 1.0;
						Sounds.SoundBuffer buffer = this.SoundBuffer;
						if (buffer == null) {
							// HACK: Represents the train point sound
							if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
								if (Train.Specs.CurrentAverageSpeed <= 0.0) return;
								int bufferIndex = Train.Cars[CarIndex].Sounds.FrontAxleRunIndex;
								if (Train.Cars[CarIndex].Sounds.PointFrontAxle == null || Train.Cars[CarIndex].Sounds.PointFrontAxle.Length == 0)
								{
									//No point sounds defined at all
									return;
								}
								if (bufferIndex > Train.Cars[CarIndex].Sounds.PointFrontAxle.Length -1 || Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Buffer == null)
								{
									//If the switch sound does not exist, return zero
									//Required to handle legacy trains which don't have idx specific run sounds defined
									bufferIndex = 0;
								}
								buffer = Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Buffer;
								p = Train.Cars[CarIndex].Sounds.PointFrontAxle[bufferIndex].Position;
							} else {
								return; // HACK: Don't trigger sound for the rear axles
								//buffer = Train.Cars[CarIndex].Sounds.PointRearAxle.Buffer;
								//p = Train.Cars[CarIndex].Sounds.PointRearAxle.Position;
							}
						}
						if (buffer != null) {
							if (this.Dynamic) {
								double spd = Math.Abs(Train.Specs.CurrentAverageSpeed);
								pitch = spd / this.Speed;
								gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
								if (pitch < 0.2 | gain < 0.2) {
									buffer = null;
								}
							}
							if (buffer != null) {
								Sounds.PlaySound(buffer, pitch, gain, p, Train, CarIndex, false);
							}
						}
						this.DontTriggerAnymore = this.Once;
					}
				}
			}
			internal const int SoundIndexTrainPoint = -2;
		}
		// rail sounds change
		internal class RailSoundsChangeEvent : GeneralEvent {
			internal int PreviousRunIndex;
			internal int PreviousFlangeIndex;
			internal int NextRunIndex;
			internal int NextFlangeIndex;
			internal RailSoundsChangeEvent(double TrackPositionDelta, int PreviousRunIndex, int PreviousFlangeIndex, int NextRunIndex, int NextFlangeIndex) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousRunIndex = PreviousRunIndex;
				this.PreviousFlangeIndex = PreviousFlangeIndex;
				this.NextRunIndex = NextRunIndex;
				this.NextFlangeIndex = NextFlangeIndex;
			}
			/// <summary>Triggers a change in run and flange sounds</summary>
			/// <param name="Direction">The direction of travel- 1 for forwards, and -1 for backwards</param>
			/// <param name="TriggerType">They type of event which triggered this sound</param>
			/// <param name="Train">The root train which triggered this sound</param>
			/// <param name="CarIndex">The car index which triggered this sound</param>
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
					if (Direction < 0) {
						Train.Cars[CarIndex].Sounds.FrontAxleRunIndex = this.PreviousRunIndex;
						Train.Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.PreviousFlangeIndex;
					} else if (Direction > 0) {
						Train.Cars[CarIndex].Sounds.FrontAxleRunIndex = this.NextRunIndex;
						Train.Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.NextFlangeIndex;
					}
				} else if (TriggerType == EventTriggerType.RearCarRearAxle | TriggerType == EventTriggerType.OtherCarRearAxle) {
					if (Direction < 0) {
						Train.Cars[CarIndex].Sounds.RearAxleRunIndex = this.PreviousRunIndex;
						Train.Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.PreviousFlangeIndex;
					} else if (Direction > 0) {
						Train.Cars[CarIndex].Sounds.RearAxleRunIndex = this.NextRunIndex;
						Train.Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.NextFlangeIndex;
					}
				}
			}
		}
		// track end
		internal class TrackEndEvent : GeneralEvent {
			internal TrackEndEvent(double TrackPositionDelta) {
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex) {
				if (TriggerType == EventTriggerType.RearCarRearAxle & Train != TrainManager.PlayerTrain) {
					TrainManager.DisposeTrain(Train);
				} else if (Train == TrainManager.PlayerTrain) {
					Train.Derail(CarIndex, 0.0);
				}
			}
		}

		// ================================

		// track element
		internal struct TrackElement {
			internal double StartingTrackPosition;
			internal double CurveRadius;
			internal double CurveCant;
			internal double CurveCantTangent;
			internal double AdhesionMultiplier;
			internal double CsvRwAccuracyLevel;
			internal double Pitch;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			internal GeneralEvent[] Events;
			internal TrackElement(double StartingTrackPosition) {
				this.StartingTrackPosition = StartingTrackPosition;
				this.Pitch = 0.0;
				this.CurveRadius = 0.0;
				this.CurveCant = 0.0;
				this.CurveCantTangent = 0.0;
				this.AdhesionMultiplier = 1.0;
				this.CsvRwAccuracyLevel = 2.0;
				this.WorldPosition = new Vector3(0.0, 0.0, 0.0);
				this.WorldDirection = new Vector3(0.0, 0.0, 1.0);
				this.WorldUp = new Vector3(0.0, 1.0, 0.0);
				this.WorldSide = new Vector3(1.0, 0.0, 0.0);
				this.Events = new GeneralEvent[] { };
			}
		}

		// track
		internal struct Track {
			internal TrackElement[] Elements;
		}
		internal static Track CurrentTrack;

		// track follower
		internal struct TrackFollower {
			internal int LastTrackElement;
			internal double TrackPosition;
			internal Vector3 WorldPosition;
			internal Vector3 WorldDirection;
			internal Vector3 WorldUp;
			internal Vector3 WorldSide;
			internal double Pitch;
			internal double CurveRadius;
			internal double CurveCant;
			internal double Odometer;
			internal double CantDueToInaccuracy;
			internal double AdhesionMultiplier;
			internal EventTriggerType TriggerType;
			internal TrainManager.Train Train;
			internal int CarIndex;
			internal void UpdateWorldCoordinates(bool AddTrackInaccuracy) {
				UpdateTrackFollower(ref this, this.TrackPosition, true, AddTrackInaccuracy);
			}
		}

		/// <summary>Call this method to update all track followers attached to a car</summary>
		/// <param name="car">The car for which to update, passed via 'ref'</param>
		/// <param name="NewTrackPosition">The track position change</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccurary">Whether to add track innaccuarcy</param>
		internal static void UpdateCarFollowers(ref TrainManager.Car car, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary)
		{
			//Car axles
			UpdateTrackFollower(ref car.FrontAxle.Follower, car.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.RearAxle.Follower, car.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Front bogie axles
			UpdateTrackFollower(ref car.FrontBogie.FrontAxle.Follower, car.FrontBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.FrontBogie.RearAxle.Follower, car.FrontBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			//Rear bogie axles

			UpdateTrackFollower(ref car.RearBogie.FrontAxle.Follower, car.RearBogie.FrontAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
			UpdateTrackFollower(ref car.RearBogie.RearAxle.Follower, car.RearBogie.RearAxle.Follower.TrackPosition + NewTrackPosition, UpdateWorldCoordinates, AddTrackInaccurary);
		}

		/// <summary>Call this method to update a single track follower</summary>
		/// <param name="Follower">The follower to update</param>
		/// <param name="NewTrackPosition">The new track position of the follower</param>
		/// <param name="UpdateWorldCoordinates">Whether to update the world co-ordinates</param>
		/// <param name="AddTrackInaccurary">Whether to add track innacuracy</param>
		internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) {
			if (CurrentTrack.Elements.Length == 0) return;
			int i = Follower.LastTrackElement;
			while (i >= 0 && NewTrackPosition < CurrentTrack.Elements[i].StartingTrackPosition) {
				double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
				double tb = -0.01;
				CheckEvents(ref Follower, i, -1, ta, tb);
				i--;
			}
			if (i >= 0) {
				while (i < CurrentTrack.Elements.Length - 1) {
					if (NewTrackPosition < CurrentTrack.Elements[i + 1].StartingTrackPosition) break;
					double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
					double tb = CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition + 0.01;
					CheckEvents(ref Follower, i, 1, ta, tb);
					i++;
				}
			} else {
				i = 0;
			}
			double da = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
			double db = NewTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;

			// track
			if (UpdateWorldCoordinates) {
				if (db != 0.0) {
					if (CurrentTrack.Elements[i].CurveRadius != 0.0) {
						// curve
						double r = CurrentTrack.Elements[i].CurveRadius;
						double p = CurrentTrack.Elements[i].WorldDirection.Y / Math.Sqrt(CurrentTrack.Elements[i].WorldDirection.X * CurrentTrack.Elements[i].WorldDirection.X + CurrentTrack.Elements[i].WorldDirection.Z * CurrentTrack.Elements[i].WorldDirection.Z);
						double s = db / Math.Sqrt(1.0 + p * p);
						double h = s * p;
						double b = s / Math.Abs(r);
						double f = 2.0 * r * r * (1.0 - Math.Cos(b));
						double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
						double a = 0.5 * (double)Math.Sign(r) * b;
						Vector3 D = new Vector3(CurrentTrack.Elements[i].WorldDirection.X, 0.0, CurrentTrack.Elements[i].WorldDirection.Z);
						World.Normalize(ref D.X, ref D.Y, ref D.Z);
						double cosa = Math.Cos(a);
						double sina = Math.Sin(a);
						World.Rotate(ref D, 0.0, 1.0, 0.0, cosa, sina);
						Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + c * D.X;
						Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + h;
						Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + c * D.Z;
						World.Rotate(ref D, 0.0, 1.0, 0.0, cosa, sina);
						Follower.WorldDirection.X = D.X;
						Follower.WorldDirection.Y = p;
						Follower.WorldDirection.Z = D.Z;
						World.Normalize(ref Follower.WorldDirection.X, ref Follower.WorldDirection.Y, ref Follower.WorldDirection.Z);
						double cos2a = Math.Cos(2.0 * a);
						double sin2a = Math.Sin(2.0 * a);
						Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
						World.Rotate(ref Follower.WorldSide, 0.0, 1.0, 0.0, cos2a, sin2a);
						World.Cross(Follower.WorldDirection, Follower.WorldSide, out Follower.WorldUp);
						
					} else {
						// straight
						Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + db * CurrentTrack.Elements[i].WorldDirection.X;
						Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + db * CurrentTrack.Elements[i].WorldDirection.Y;
						Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + db * CurrentTrack.Elements[i].WorldDirection.Z;
						Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
						Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
						Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
						Follower.CurveRadius = 0.0;
					}
					
					// cant
					if (i < CurrentTrack.Elements.Length - 1) {
						double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
						if (t < 0.0) {
							t = 0.0;
						} else if (t > 1.0) {
							t = 1.0;
						}
						double t2 = t * t;
						double t3 = t2 * t;
						Follower.CurveCant =
							(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentTrack.Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * CurrentTrack.Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * CurrentTrack.Elements[i + 1].CurveCant +
							(t3 - t2) * CurrentTrack.Elements[i + 1].CurveCantTangent;
						Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					} else {
						Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
					}

					
				} else {
					Follower.WorldPosition = CurrentTrack.Elements[i].WorldPosition;
					Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
					Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
					Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
					Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
				}
				
			} else {
				if (db != 0.0) {
					if (CurrentTrack.Elements[i].CurveRadius != 0.0) {
						Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					} else {
						Follower.CurveRadius = 0.0;
					}
					if (i < CurrentTrack.Elements.Length - 1) {
						double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
						if (t < 0.0) {
							t = 0.0;
						} else if (t > 1.0) {
							t = 1.0;
						}
						double t2 = t * t;
						double t3 = t2 * t;
						Follower.CurveCant =
							(2.0 * t3 - 3.0 * t2 + 1.0) * CurrentTrack.Elements[i].CurveCant +
							(t3 - 2.0 * t2 + t) * CurrentTrack.Elements[i].CurveCantTangent +
							(-2.0 * t3 + 3.0 * t2) * CurrentTrack.Elements[i + 1].CurveCant +
							(t3 - t2) * CurrentTrack.Elements[i + 1].CurveCantTangent;
					} else {
						Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
					}
				} else {
					Follower.CurveRadius = CurrentTrack.Elements[i].CurveRadius;
					Follower.CurveCant = CurrentTrack.Elements[i].CurveCant;
				}
			   
			}
			Follower.AdhesionMultiplier = CurrentTrack.Elements[i].AdhesionMultiplier;
			//Pitch added for Plugin Data usage
			//Mutliply this by 1000 to get the original value
			Follower.Pitch = CurrentTrack.Elements[i].Pitch * 1000;
			// inaccuracy
			if (AddTrackInaccurary) {
				double x, y, c;
				if (i < CurrentTrack.Elements.Length - 1) {
					double t = db / (CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition);
					if (t < 0.0) {
						t = 0.0;
					} else if (t > 1.0) {
						t = 1.0;
					}
					double x1, y1, c1;
					double x2, y2, c2;
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x1, out y1, out c1);
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i + 1].CsvRwAccuracyLevel, out x2, out y2, out c2);
					x = (1.0 - t) * x1 + t * x2;
					y = (1.0 - t) * y1 + t * y2;
					c = (1.0 - t) * c1 + t * c2;
				} else {
					GetInaccuracies(NewTrackPosition, CurrentTrack.Elements[i].CsvRwAccuracyLevel, out x, out y, out c);
				}
				Follower.WorldPosition.X += x * Follower.WorldSide.X + y * Follower.WorldUp.X;
				Follower.WorldPosition.Y += x * Follower.WorldSide.Y + y * Follower.WorldUp.Y;
				Follower.WorldPosition.Z += x * Follower.WorldSide.Z + y * Follower.WorldUp.Z;
				Follower.CurveCant += c;
				Follower.CantDueToInaccuracy = c;
			} else {
				Follower.CantDueToInaccuracy = 0.0;
			}
			// events
			CheckEvents(ref Follower, i, Math.Sign(db - da), da, db);
			//Update the odometer
			if (Follower.TrackPosition != NewTrackPosition)
			{
				//HACK: Reset the odometer if we've moved more than 10m this frame
				if (Math.Abs(NewTrackPosition - Follower.TrackPosition) > 10)
				{
					Follower.Odometer = 0;
				}
				else
				{
					Follower.Odometer += NewTrackPosition - Follower.TrackPosition;
				}
			}
			// finish
			Follower.TrackPosition = NewTrackPosition;
			Follower.LastTrackElement = i;
		}
		
		/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
		/// <param name="position">The track position</param>
		/// <param name="inaccuracy">The openBVE innacuaracy value</param>
		/// <param name="x">The X (horizontal) co-ordinate to update</param>
		/// <param name="y">The Y (vertical) co-ordinate to update</param>
		/// <param name="c">???</param>
		private static void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c) {
			if (inaccuracy <= 0.0) {
				x = 0.0;
				y = 0.0;
				c = 0.0;
			} else {
				double z = Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * Math.Sin(0.5843 * z) + 0.82 * Math.Sin(0.2246 * z) + 0.55 * Math.Sin(0.1974 * z);
				x *= 0.0035 * Game.RouteRailGauge * inaccuracy;
				y = 0.18 * Math.Sin(0.5172 * z) + 0.37 * Math.Sin(0.3251 * z) + 0.91 * Math.Sin(0.3773 * z);
				y *= 0.0020 * Game.RouteRailGauge * inaccuracy;
				c = 0.23 * Math.Sin(0.3131 * z) + 0.54 * Math.Sin(0.5807 * z) + 0.81 * Math.Sin(0.3621 * z);
				c *= 0.0025 * Game.RouteRailGauge * inaccuracy;
			}
		}

		// check events
		private static void CheckEvents(ref TrackFollower Follower, int ElementIndex, int Direction, double OldDelta, double NewDelta) {
			if (Follower.TriggerType != EventTriggerType.None) {
				if (Direction < 0) {
					for (int j = CurrentTrack.Elements[ElementIndex].Events.Length - 1; j >= 0; j--) {
						if (OldDelta > CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta <= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
							TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], -1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
						}
					}
				} else if (Direction > 0) {
					for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++) {
						if (OldDelta < CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta >= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
							TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], 1, Follower.TriggerType, Follower.Train, Follower.CarIndex);
						}
					}
				}
			}
		}

	}
}