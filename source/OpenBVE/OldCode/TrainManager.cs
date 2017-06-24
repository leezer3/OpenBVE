using System;
using System.Windows.Forms;
using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		// cars
		
		internal struct AccelerationCurve
		{
			internal double StageZeroAcceleration;
			internal double StageOneSpeed;
			internal double StageOneAcceleration;
			internal double StageTwoSpeed;
			internal double StageTwoExponent;
		}
		internal enum CarBrakeType
		{
			ElectromagneticStraightAirBrake = 0,
			ElectricCommandBrake = 1,
			AutomaticAirBrake = 2
		}
		internal enum EletropneumaticBrakeType
		{
			None = 0,
			ClosingElectromagneticValve = 1,
			DelayFillingControl = 2
		}


		internal struct CarHoldBrake
		{
			internal double CurrentAccelerationOutput;
			internal double NextUpdateTime;
			internal double UpdateInterval;
		}
		
		
		
		internal struct CarBrightness
		{
			internal float PreviousBrightness;
			internal double PreviousTrackPosition;
			internal float NextBrightness;
			internal double NextTrackPosition;
		}

		internal struct CarSound
		{
			internal Sounds.SoundBuffer Buffer;
			internal Sounds.SoundSource Source;
			internal Vector3 Position;
			private CarSound(Sounds.SoundBuffer buffer, Sounds.SoundSource source, Vector3 position)
			{
				this.Buffer = buffer;
				this.Source = source;
				this.Position = position;
			}
			internal static readonly CarSound Empty = new CarSound(null, null, new Vector3(0.0, 0.0, 0.0));
		}
		internal struct MotorSoundTableEntry
		{
			internal Sounds.SoundBuffer Buffer;
			internal int SoundIndex;
			internal float Pitch;
			internal float Gain;
		}
		internal struct MotorSoundTable
		{
			internal MotorSoundTableEntry[] Entries;
			internal Sounds.SoundBuffer Buffer;
			internal Sounds.SoundSource Source;
		}
		internal struct MotorSound
		{
			internal MotorSoundTable[] Tables;
			internal Vector3 Position;
			internal double SpeedConversionFactor;
			internal int CurrentAccelerationDirection;
			internal const int MotorP1 = 0;
			internal const int MotorP2 = 1;
			internal const int MotorB1 = 2;
			internal const int MotorB2 = 3;
		}



		// train
		
		// train specs
		internal enum PassAlarmType
		{
			None = 0,
			Single = 1,
			Loop = 2
		}
		internal struct TrainAirBrake
		{
			internal AirBrakeHandle Handle;
		}
		internal enum DoorMode
		{
			AutomaticManualOverride = 0,
			Automatic = 1,
			Manual = 2
		}

		[Flags]
		internal enum DefaultSafetySystems
		{
			AtsSn = 1,
			AtsP = 2,
			Atc = 4,
			Eb = 8
		}
		
		// trains
		/// <summary>The list of trains available in the simulation.</summary>
		internal static Train[] Trains = new Train[] { };
		/// <summary>A reference to the train of the Trains element that corresponds to the player's train.</summary>
		internal static Train PlayerTrain = null;

    	
		

		// update camera
		internal static void UpdateCamera(Train Train, int Car)
		{
			int i = Car;
			int j = Train.DriverCar;
			double dx = Train.Cars[i].FrontAxle.Follower.WorldPosition.X - Train.Cars[i].RearAxle.Follower.WorldPosition.X;
			double dy = Train.Cars[i].FrontAxle.Follower.WorldPosition.Y - Train.Cars[i].RearAxle.Follower.WorldPosition.Y;
			double dz = Train.Cars[i].FrontAxle.Follower.WorldPosition.Z - Train.Cars[i].RearAxle.Follower.WorldPosition.Z;
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double ux = Train.Cars[i].Up.X;
			double uy = Train.Cars[i].Up.Y;
			double uz = Train.Cars[i].Up.Z;
			double sx = dz * uy - dy * uz;
			double sy = dx * uz - dz * ux;
			double sz = dy * ux - dx * uy;
			double rx = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.X + Train.Cars[i].RearAxle.Follower.WorldPosition.X);
			double ry = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.Y + Train.Cars[i].RearAxle.Follower.WorldPosition.Y);
			double rz = 0.5 * (Train.Cars[i].FrontAxle.Follower.WorldPosition.Z + Train.Cars[i].RearAxle.Follower.WorldPosition.Z);
			double cx = rx + sx * Train.Cars[j].DriverPosition.X + ux * Train.Cars[j].DriverPosition.Y + dx * Train.Cars[j].DriverPosition.Z;
			double cy = ry + sy * Train.Cars[j].DriverPosition.X + uy * Train.Cars[j].DriverPosition.Y + dy * Train.Cars[j].DriverPosition.Z;
			double cz = rz + sz * Train.Cars[j].DriverPosition.X + uz * Train.Cars[j].DriverPosition.Y + dz * Train.Cars[j].DriverPosition.Z;
			World.CameraTrackFollower.WorldPosition = new Vector3(cx, cy, cz);
			World.CameraTrackFollower.WorldDirection = new Vector3(dx, dy, dz);
			World.CameraTrackFollower.WorldUp = new Vector3(ux, uy, uz);
			World.CameraTrackFollower.WorldSide = new Vector3(sx, sy, sz);
			double f = (Train.Cars[i].DriverPosition.Z - Train.Cars[i].RearAxle.Position) / (Train.Cars[i].FrontAxle.Position - Train.Cars[i].RearAxle.Position);
			double tp = (1.0 - f) * Train.Cars[i].RearAxle.Follower.TrackPosition + f * Train.Cars[i].FrontAxle.Follower.TrackPosition;
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, tp, false, false);
		}
		




		// update train objects
		internal static void UpdateTrainObjects(double TimeElapsed, bool ForceUpdate)
		{
			for (int i = 0; i < Trains.Length; i++)
			{
				Trains[i].UpdateObjects(TimeElapsed, ForceUpdate);
			}
		}

		
		
	


		// update train
		internal static void UpdateTrain(Train Train, double TimeElapsed)
		{
			if (Train.State == TrainState.Pending)
			{
				// pending train
				bool forceIntroduction = Train == PlayerTrain && !Game.MinimalisticSimulation;
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
					time -= Train.TimetableDelta;
				}
				if (Game.SecondsSinceMidnight >= time | forceIntroduction)
				{
					bool introduce = true;
					if (!forceIntroduction)
					{
						if (Train.CurrentSectionIndex >= 0)
						{
							if (!Game.Sections[Train.CurrentSectionIndex].IsFree())
							{
								introduce = false;
							}
						}
					}
					if (introduce)
					{
						// train is introduced
						Train.State = TrainState.Available;
						for (int j = 0; j < Train.Cars.Length; j++)
						{
							if (Train.Cars[j].CarSections.Length != 0)
							{
                                Train.Cars[j].ChangeCarSection(j <= Train.DriverCar | Train != PlayerTrain ? 0 : -1);
                                Train.Cars[j].FrontBogie.ChangeCarSection(Train != PlayerTrain ? 0 : -1);
							    Train.Cars[j].RearBogie.ChangeCarSection(Train != PlayerTrain ? 0 : -1);
                            }
							if (Train.Cars[j].Specs.IsMotorCar)
							{
								if (Train.Cars[j].Sounds.Loop.Buffer != null)
								{
									OpenBveApi.Math.Vector3 pos = Train.Cars[j].Sounds.Loop.Position;
									Train.Cars[j].Sounds.Loop.Source = Sounds.PlaySound(Train.Cars[j].Sounds.Loop.Buffer, 1.0, 1.0, pos, Train, j, true);
								}
							}
						}
					}
				}
			}
			else if (Train.State == TrainState.Available)
			{
				// available train
				UpdateTrainPhysicsAndControls(Train, TimeElapsed);
				if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade)
				{
					if (Train.Specs.CurrentAverageSpeed > Train.CurrentRouteLimit)
					{
						Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), MessageManager.MessageDependency.RouteLimit, Interface.GameMode.Arcade, MessageColor.Orange, double.PositiveInfinity, null);
					}
					if (Train.CurrentSectionLimit == 0.0)
					{
						Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
					}
					else if (Train.Specs.CurrentAverageSpeed > Train.CurrentSectionLimit)
					{
						Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), MessageManager.MessageDependency.SectionLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
					}
				}
				if (Train.AI != null)
				{
					Train.AI.Trigger(Train, TimeElapsed);
				}
			}
			else if (Train.State == TrainState.Bogus)
			{
				// bogus train
				if (Train.AI != null)
				{
					Train.AI.Trigger(Train, TimeElapsed);
				}
			}
		}

		/// <summary>This method should be called once a frame to update the position, speed and state of all trains within the simulation</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal static void UpdateTrains(double TimeElapsed)
		{
			for (int i = 0; i < Trains.Length; i++) {
				UpdateTrain(Trains[i], TimeElapsed);
			}
			// detect collision
			if (!Game.MinimalisticSimulation & Interface.CurrentOptions.Collisions)
			{
				
				//for (int i = 0; i < Trains.Length; i++) {
				System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
				{
					// with other trains
					if (Trains[i].State == TrainState.Available)
					{
						double a = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxle.Position +
								   0.5*Trains[i].Cars[0].Length;
						double b = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								   Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Position - 0.5*Trains[i].Cars[0].Length;
						for (int j = i + 1; j < Trains.Length; j++)
						{
							if (Trains[j].State == TrainState.Available)
							{
								double c = Trains[j].Cars[0].FrontAxle.Follower.TrackPosition -
										   Trains[j].Cars[0].FrontAxle.Position + 0.5*Trains[j].Cars[0].Length;
								double d = Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition -
										   Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxle.Position -
										   0.5*Trains[j].Cars[0].Length;
								if (a > d & b < c)
								{
									if (a > c)
									{
										// i > j
										int k = Trains[i].Cars.Length - 1;
										if (Trains[i].Cars[k].Specs.CurrentSpeed < Trains[j].Cars[0].Specs.CurrentSpeed)
										{
											double v = Trains[j].Cars[0].Specs.CurrentSpeed -
													   Trains[i].Cars[k].Specs.CurrentSpeed;
											double s = (Trains[i].Cars[k].Specs.CurrentSpeed*Trains[i].Cars[k].Specs.MassCurrent +
														Trains[j].Cars[0].Specs.CurrentSpeed*Trains[j].Cars[0].Specs.MassCurrent)/
													   (Trains[i].Cars[k].Specs.MassCurrent + Trains[j].Cars[0].Specs.MassCurrent);
											Trains[i].Cars[k].Specs.CurrentSpeed = s;
											Trains[j].Cars[0].Specs.CurrentSpeed = s;
											double e = 0.5*(c - b) + 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[k].FrontAxle.Follower,Trains[i].Cars[k].FrontAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[k].RearAxle.Follower,Trains[i].Cars[k].RearAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[0].FrontAxle.Follower,Trains[j].Cars[0].FrontAxle.Follower.TrackPosition - e, false, false);

											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[0].RearAxle.Follower,Trains[j].Cars[0].RearAxle.Follower.TrackPosition - e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/
														   (Trains[i].Cars[k].Specs.MassCurrent +
															Trains[j].Cars[0].Specs.MassCurrent);
												double fi = Trains[j].Cars[0].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[k].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Derail(k, TimeElapsed);
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Derail(i, TimeElapsed);
											}
											// adjust cars for train i
											for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h + 1].FrontAxle.Position + 0.5*Trains[i].Cars[h + 1].Length;
												b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h].RearAxle.Position - 0.5*Trains[i].Cars[h].Length;
												d = b - a - Trains[i].Couplers[h].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition - d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition - d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[i].Cars[h + 1].Specs.MassCurrent +
																	Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h + 1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}
													Trains[i].Cars[h].Specs.CurrentSpeed =
														Trains[i].Cars[h + 1].Specs.CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = 1; h < Trains[j].Cars.Length; h++)
											{
												a = Trains[j].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h - 1].RearAxle.Position - 0.5*Trains[j].Cars[h - 1].Length;
												b = Trains[j].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h].FrontAxle.Position + 0.5*Trains[j].Cars[h].Length;
												d = a - b - Trains[j].Couplers[h - 1].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].FrontAxle.Follower,Trains[j].Cars[h].FrontAxle.Follower.TrackPosition + d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].RearAxle.Follower,Trains[j].Cars[h].RearAxle.Follower.TrackPosition + d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[j].Cars[h - 1].Specs.MassCurrent +
																	Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h -1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}
													Trains[j].Cars[h].Specs.CurrentSpeed =
														Trains[j].Cars[h - 1].Specs.CurrentSpeed;
												}
											}
										}
									}
									else
									{
										// i < j
										int k = Trains[j].Cars.Length - 1;
										if (Trains[i].Cars[0].Specs.CurrentSpeed > Trains[j].Cars[k].Specs.CurrentSpeed)
										{
											double v = Trains[i].Cars[0].Specs.CurrentSpeed -
													   Trains[j].Cars[k].Specs.CurrentSpeed;
											double s = (Trains[i].Cars[0].Specs.CurrentSpeed*Trains[i].Cars[0].Specs.MassCurrent +
														Trains[j].Cars[k].Specs.CurrentSpeed*Trains[j].Cars[k].Specs.MassCurrent)/
													   (Trains[i].Cars[0].Specs.MassCurrent + Trains[j].Cars[k].Specs.MassCurrent);
											Trains[i].Cars[0].Specs.CurrentSpeed = s;
											Trains[j].Cars[k].Specs.CurrentSpeed = s;
											double e = 0.5*(a - d) + 0.0001;
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].FrontAxle.Follower,Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[i].Cars[0].RearAxle.Follower,Trains[i].Cars[0].RearAxle.Follower.TrackPosition - e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[k].FrontAxle.Follower,Trains[j].Cars[k].FrontAxle.Follower.TrackPosition + e, false, false);
											TrackManager.UpdateTrackFollower(ref Trains[j].Cars[k].RearAxle.Follower,Trains[j].Cars[k].RearAxle.Follower.TrackPosition + e, false, false);
											if (Interface.CurrentOptions.Derailments)
											{
												double f = 2.0/
														   (Trains[i].Cars[0].Specs.MassCurrent +
															Trains[j].Cars[k].Specs.MassCurrent);
												double fi = Trains[j].Cars[k].Specs.MassCurrent*f;
												double fj = Trains[i].Cars[0].Specs.MassCurrent*f;
												double vi = v*fi;
												double vj = v*fj;
												if (vi > Game.CriticalCollisionSpeedDifference)
													Trains[i].Derail(0, TimeElapsed);
												if (vj > Game.CriticalCollisionSpeedDifference)
													Trains[j].Derail(k, TimeElapsed);
											}
											// adjust cars for train i
											for (int h = 1; h < Trains[i].Cars.Length; h++)
											{
												a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
													Trains[i].Cars[h - 1].RearAxle.Position - 0.5*Trains[i].Cars[h - 1].Length;
												b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h].FrontAxle.Position + 0.5*Trains[i].Cars[h].Length;
												d = a - b - Trains[i].Couplers[h - 1].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].FrontAxle.Follower,Trains[i].Cars[h].FrontAxle.Follower.TrackPosition + d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[i].Cars[h].RearAxle.Follower,Trains[i].Cars[h].RearAxle.Follower.TrackPosition + d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[i].Cars[h - 1].Specs.MassCurrent +
																	Trains[i].Cars[h].Specs.MassCurrent);
														double fi = Trains[i].Cars[h - 1].Specs.MassCurrent*f;
														double fj = Trains[i].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h -1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}
													Trains[i].Cars[h].Specs.CurrentSpeed =
														Trains[i].Cars[h - 1].Specs.CurrentSpeed;
												}
											}
											// adjust cars for train j
											for (int h = Trains[j].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[j].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h + 1].FrontAxle.Position + 0.5*Trains[j].Cars[h + 1].Length;
												b = Trains[j].Cars[h].RearAxle.Follower.TrackPosition -
													Trains[j].Cars[h].RearAxle.Position - 0.5*Trains[j].Cars[h].Length;
												d = b - a - Trains[j].Couplers[h].MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].FrontAxle.Follower,Trains[j].Cars[h].FrontAxle.Follower.TrackPosition - d, false, false);
													TrackManager.UpdateTrackFollower(ref Trains[j].Cars[h].RearAxle.Follower,Trains[j].Cars[h].RearAxle.Follower.TrackPosition - d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														double f = 2.0/
																   (Trains[j].Cars[h + 1].Specs.MassCurrent +
																	Trains[j].Cars[h].Specs.MassCurrent);
														double fi = Trains[j].Cars[h + 1].Specs.MassCurrent*f;
														double fj = Trains[j].Cars[h].Specs.MassCurrent*f;
														double vi = v*fi;
														double vj = v*fj;
														if (vi > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h + 1, TimeElapsed);
														if (vj > Game.CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}
													Trains[j].Cars[h].Specs.CurrentSpeed =
														Trains[j].Cars[h + 1].Specs.CurrentSpeed;
												}
											}
										}
									}
								}
							}

						}
					}
					// with buffers
					if (i == PlayerTrain.TrainIndex)
					{
						double a = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxle.Position +
								   0.5*Trains[i].Cars[0].Length;
						double b = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
								   Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Position - 0.5*Trains[i].Cars[0].Length;
						for (int j = 0; j < Game.BufferTrackPositions.Length; j++)
						{
							if (a > Game.BufferTrackPositions[j] & b < Game.BufferTrackPositions[j])
							{
								a += 0.0001;
								b -= 0.0001;
								double da = a - Game.BufferTrackPositions[j];
								double db = Game.BufferTrackPositions[j] - b;
								if (da < db)
								{
									// front
									TrackManager.UpdateCarFollowers(ref Trains[i].Cars[0], -da, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[0].Specs.CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Derail(0, TimeElapsed);
									}
									Trains[i].Cars[0].Specs.CurrentSpeed = 0.0;
									for (int h = 1; h < Trains[i].Cars.Length; h++)
									{
										a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h - 1].RearAxle.Position - 0.5*Trains[i].Cars[h - 1].Length;
										b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h].FrontAxle.Position + 0.5*Trains[i].Cars[h].Length;
										double d = a - b - Trains[i].Couplers[h - 1].MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											TrackManager.UpdateCarFollowers(ref Trains[i].Cars[h], d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].Specs.CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Derail(h, TimeElapsed);
											}
											Trains[i].Cars[h].Specs.CurrentSpeed = 0.0;
										}
									}
								}
								else
								{
									// rear
									int c = Trains[i].Cars.Length - 1;
									TrackManager.UpdateCarFollowers(ref Trains[i].Cars[c], db, false, false);
									if (Interface.CurrentOptions.Derailments &&
										Math.Abs(Trains[i].Cars[c].Specs.CurrentSpeed) > Game.CriticalCollisionSpeedDifference)
									{
										Trains[i].Derail(c, TimeElapsed);
									}
									Trains[i].Cars[c].Specs.CurrentSpeed = 0.0;
									for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
									{
										a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
											Trains[i].Cars[h + 1].FrontAxle.Position + 0.5*Trains[i].Cars[h + 1].Length;
										b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
											Trains[i].Cars[h].RearAxle.Position - 0.5*Trains[i].Cars[h].Length;
										double d = b - a - Trains[i].Couplers[h].MinimumDistanceBetweenCars;
										if (d < 0.0)
										{
											d -= 0.0001;
											TrackManager.UpdateCarFollowers(ref Trains[i].Cars[h], -d, false, false);
											if (Interface.CurrentOptions.Derailments &&
												Math.Abs(Trains[i].Cars[h].Specs.CurrentSpeed) >
												Game.CriticalCollisionSpeedDifference)
											{
												Trains[i].Derail(h, TimeElapsed);
											}
											Trains[i].Cars[h].Specs.CurrentSpeed = 0.0;
										}
									}
								}
							}
						}
					}
				});
			}
			// compute final angles and positions
			//for (int i = 0; i < Trains.Length; i++) {
			System.Threading.Tasks.Parallel.For(0, Trains.Length, i =>
			{
				if (Trains[i].State != TrainState.Disposed & Trains[i].State != TrainManager.TrainState.Bogus)
				{
					for (int j = 0; j < Trains[i].Cars.Length; j++)
					{
						Trains[i].Cars[j].FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].UpdateTopplingCantAndSpring(TimeElapsed);
                        Trains[i].Cars[j].FrontBogie.UpdateTopplingCantAndSpring(TimeElapsed);
					    Trains[i].Cars[j].RearBogie.UpdateTopplingCantAndSpring(TimeElapsed);
					}
				}
			});
		}

		// apply notch
		internal static void ApplyNotch(Train Train, int PowerValue, bool PowerRelative, int BrakeValue, bool BrakeRelative)
		{
			// determine notch
			int p = PowerRelative ? PowerValue + Train.Specs.CurrentPowerNotch.Driver : PowerValue;
			if (p < 0)
			{
				p = 0;
			}
			else if (p > Train.Specs.MaximumPowerNotch)
			{
				p = Train.Specs.MaximumPowerNotch;
			}
			int b = BrakeRelative ? BrakeValue + Train.Specs.CurrentBrakeNotch.Driver : BrakeValue;
			if (b < 0)
			{
				b = 0;
			}
			else if (b > Train.Specs.MaximumBrakeNotch)
			{
				b = Train.Specs.MaximumBrakeNotch;
			}
			// power sound
			if (p < Train.Specs.CurrentPowerNotch.Driver)
			{
				if (p > 0)
				{
					// down (not min)
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerDown.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerDown.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
				else
				{
					// min
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerMin.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerMin.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			else if (p > Train.Specs.CurrentPowerNotch.Driver)
			{
				if (p < Train.Specs.MaximumPowerNotch)
				{
					// up (not max)
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerUp.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerUp.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
				else
				{
					// max
					Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.MasterControllerMax.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.MasterControllerMax.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			// brake sound
			if (b < Train.Specs.CurrentBrakeNotch.Driver)
			{
				// brake release
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.Brake.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.Brake.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
				if (b > 0)
				{
					// brake release (not min)
					buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
				else
				{
					// brake min
					buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin.Position;
						Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
					}
				}
			}
			else if (b > Train.Specs.CurrentBrakeNotch.Driver)
			{
				// brake
				Sounds.SoundBuffer buffer = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Buffer;
				if (buffer != null)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply.Position;
					Sounds.PlaySound(buffer, 1.0, 1.0, pos, Train, Train.DriverCar, false);
				}
			}
			// apply notch
			if (Train.Specs.SingleHandle)
			{
				if (b != 0) p = 0;
			}
			Train.Specs.CurrentPowerNotch.Driver = p;
			Train.Specs.CurrentBrakeNotch.Driver = b;
			Game.AddBlackBoxEntry(Game.BlackBoxEventToken.None);
			// plugin
			if (Train.Plugin != null)
			{
				Train.Plugin.UpdatePower();
				Train.Plugin.UpdateBrake();
			}
		}	

		

		// update speeds
		private static void UpdateSpeeds(Train Train, double TimeElapsed)
		{
			if (Game.MinimalisticSimulation & Train == PlayerTrain)
			{
				// hold the position of the player's train during startup
				for (int i = 0; i < Train.Cars.Length; i++)
				{
					Train.Cars[i].Specs.CurrentSpeed = 0.0;
					Train.Cars[i].Specs.CurrentAccelerationOutput = 0.0;
				}
				return;
			}
			// update brake system
			double[] DecelerationDueToBrake, DecelerationDueToMotor;
			UpdateBrakeSystem(Train, TimeElapsed, out DecelerationDueToBrake, out DecelerationDueToMotor);
			// calculate new car speeds
			double[] NewSpeeds = new double[Train.Cars.Length];
			for (int i = 0; i < Train.Cars.Length; i++)
			{
			    NewSpeeds[i] = Train.Cars[i].GetSpeed(TimeElapsed, DecelerationDueToMotor[i], DecelerationDueToBrake[i]);
			}
			// calculate center of mass position
			double[] CenterOfCarPositions = new double[Train.Cars.Length];
			double CenterOfMassPosition = 0.0;
			double TrainMass = 0.0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				double pr = Train.Cars[i].RearAxle.Follower.TrackPosition - Train.Cars[i].RearAxle.Position;
				double pf = Train.Cars[i].FrontAxle.Follower.TrackPosition - Train.Cars[i].FrontAxle.Position;
				CenterOfCarPositions[i] = 0.5 * (pr + pf);
				CenterOfMassPosition += CenterOfCarPositions[i] * Train.Cars[i].Specs.MassCurrent;
				TrainMass += Train.Cars[i].Specs.MassCurrent;
			}
			if (TrainMass != 0.0)
			{
				CenterOfMassPosition /= TrainMass;
			}
			{ // coupler
				// determine closest cars
				int p = -1; // primary car index
				int s = -1; // secondary car index
				{
					double PrimaryDistance = double.MaxValue;
					for (int i = 0; i < Train.Cars.Length; i++)
					{
						double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
						if (d < PrimaryDistance)
						{
							PrimaryDistance = d;
							p = i;
						}
					}
					double SecondDistance = double.MaxValue;
					for (int i = p - 1; i <= p + 1; i++)
					{
						if (i >= 0 & i < Train.Cars.Length & i != p)
						{
							double d = Math.Abs(CenterOfCarPositions[i] - CenterOfMassPosition);
							if (d < SecondDistance)
							{
								SecondDistance = d;
								s = i;
							}
						}
					}
					if (s >= 0 && PrimaryDistance <= 0.25 * (PrimaryDistance + SecondDistance))
					{
						s = -1;
					}
				}
				// coupler
				bool[] CouplerCollision = new bool[Train.Couplers.Length];
				int cf, cr;
				if (s >= 0)
				{
					// use two cars as center of mass
					if (p > s)
					{
						int t = p; p = s; s = t;
					}
					double min = Train.Couplers[p].MinimumDistanceBetweenCars;
					double max = Train.Couplers[p].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[p] - CenterOfCarPositions[s] - 0.5 * (Train.Cars[p].Length + Train.Cars[s].Length);
					if (d < min)
					{
						double t = (min - d) / (Train.Cars[p].Specs.MassCurrent + Train.Cars[s].Specs.MassCurrent);
						double tp = t * Train.Cars[s].Specs.MassCurrent;
						double ts = t * Train.Cars[p].Specs.MassCurrent;
						TrackManager.UpdateCarFollowers(ref Train.Cars[p], tp, false, false);
						TrackManager.UpdateCarFollowers(ref Train.Cars[s], -ts, false, false);
						CenterOfCarPositions[p] += tp;
						CenterOfCarPositions[s] -= ts;
						CouplerCollision[p] = true;
					}
					else if (d > max & !Train.Cars[p].Derailed & !Train.Cars[s].Derailed)
					{
						double t = (d - max) / (Train.Cars[p].Specs.MassCurrent + Train.Cars[s].Specs.MassCurrent);
						double tp = t * Train.Cars[s].Specs.MassCurrent;
						double ts = t * Train.Cars[p].Specs.MassCurrent;

						TrackManager.UpdateCarFollowers(ref Train.Cars[p], -tp, false, false);
						TrackManager.UpdateCarFollowers(ref Train.Cars[s], ts, false, false);
						CenterOfCarPositions[p] -= tp;
						CenterOfCarPositions[s] += ts;
						CouplerCollision[p] = true;
					}
					cf = p;
					cr = s;
				}
				else
				{
					// use one car as center of mass
					cf = p;
					cr = p;
				}
				// front cars
				for (int i = cf - 1; i >= 0; i--)
				{
					double min = Train.Couplers[i].MinimumDistanceBetweenCars;
					double max = Train.Couplers[i].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[i] - CenterOfCarPositions[i + 1] - 0.5 * (Train.Cars[i].Length + Train.Cars[i + 1].Length);
					if (d < min)
					{
						double t = min - d + 0.0001;
						TrackManager.UpdateCarFollowers(ref Train.Cars[i], t, false, false);
						CenterOfCarPositions[i] += t;
						CouplerCollision[i] = true;
					}
					else if (d > max & !Train.Cars[i].Derailed & !Train.Cars[i + 1].Derailed)
					{
						double t = d - max + 0.0001;
						TrackManager.UpdateCarFollowers(ref Train.Cars[i], -t, false, false);
						CenterOfCarPositions[i] -= t;
						CouplerCollision[i] = true;
					}
				}
				// rear cars
				for (int i = cr + 1; i < Train.Cars.Length; i++)
				{
					double min = Train.Couplers[i - 1].MinimumDistanceBetweenCars;
					double max = Train.Couplers[i - 1].MaximumDistanceBetweenCars;
					double d = CenterOfCarPositions[i - 1] - CenterOfCarPositions[i] - 0.5 * (Train.Cars[i].Length + Train.Cars[i - 1].Length);
					if (d < min)
					{
						double t = min - d + 0.0001;
						TrackManager.UpdateCarFollowers(ref Train.Cars[i], -t, false, false);
						CenterOfCarPositions[i] -= t;
						CouplerCollision[i - 1] = true;
					}
					else if (d > max & !Train.Cars[i].Derailed & !Train.Cars[i - 1].Derailed)
					{
						double t = d - max + 0.0001;
						TrackManager.UpdateCarFollowers(ref Train.Cars[i], t, false, false);

						CenterOfCarPositions[i] += t;
						CouplerCollision[i - 1] = true;
					}
				}
				// update speeds
				for (int i = 0; i < Train.Couplers.Length; i++)
				{
					if (CouplerCollision[i])
					{
						int j;
						for (j = i + 1; j < Train.Couplers.Length; j++)
						{
							if (!CouplerCollision[j])
							{
								break;
							}
						}
						double v = 0.0;
						double m = 0.0;
						for (int k = i; k <= j; k++)
						{
							v += NewSpeeds[k] * Train.Cars[k].Specs.MassCurrent;
							m += Train.Cars[k].Specs.MassCurrent;
						}
						if (m != 0.0)
						{
							v /= m;
						}
						for (int k = i; k <= j; k++)
						{
							if (Interface.CurrentOptions.Derailments && Math.Abs(v - NewSpeeds[k]) > 0.5 * Game.CriticalCollisionSpeedDifference)
							{
								Train.Derail(k, TimeElapsed);
							}
							NewSpeeds[k] = v;
						}
						i = j - 1;
					}
				}
			}
			// update average data
			Train.Specs.CurrentAverageSpeed = 0.0;
			Train.Specs.CurrentAverageAcceleration = 0.0;
			Train.Specs.CurrentAverageJerk = 0.0;
			double invtime = TimeElapsed != 0.0 ? 1.0 / TimeElapsed : 1.0;
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.Cars[i].Specs.CurrentAcceleration = (NewSpeeds[i] - Train.Cars[i].Specs.CurrentSpeed) * invtime;
				Train.Cars[i].Specs.CurrentSpeed = NewSpeeds[i];
				Train.Specs.CurrentAverageSpeed += NewSpeeds[i];
				Train.Specs.CurrentAverageAcceleration += Train.Cars[i].Specs.CurrentAcceleration;
			}
			double invcarlen = 1.0 / (double)Train.Cars.Length;
			Train.Specs.CurrentAverageSpeed *= invcarlen;
			Train.Specs.CurrentAverageAcceleration *= invcarlen;
		}


		// update train physics and controls
		private static void UpdateTrainPhysicsAndControls(Train Train, double TimeElapsed)
		{
			if (TimeElapsed == 0.0 || TimeElapsed > 1000)
			{
				//HACK: The physics engine really does not like update times above 1000ms
				//This works around a bug experienced when jumping to a station on a steep hill
				//causing exessive acceleration
				return;
			}
			// move cars
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				Train.MoveCar(i, Train.Cars[i].Specs.CurrentSpeed * TimeElapsed, TimeElapsed);
				if (Train.State == TrainState.Disposed)
				{
					return;
				}
			}
			// update station and doors
			UpdateTrainStation(Train, TimeElapsed);
			UpdateTrainDoors(Train, TimeElapsed);
			// delayed handles
			{
				// power notch
				if (Train.Specs.CurrentPowerNotch.DelayedChanges.Length == 0)
				{
					if (Train.Specs.CurrentPowerNotch.Safety < Train.Specs.CurrentPowerNotch.Actual)
					{
						if (Train.Specs.PowerNotchReduceSteps <= 1)
						{
							Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Actual - 1, Train.Specs.DelayPowerDown);
						}
						else if (Train.Specs.CurrentPowerNotch.Safety + Train.Specs.PowerNotchReduceSteps <= Train.Specs.CurrentPowerNotch.Actual | Train.Specs.CurrentPowerNotch.Safety == 0)
						{
							Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerDown);
						}
					}
					else if (Train.Specs.CurrentPowerNotch.Safety > Train.Specs.CurrentPowerNotch.Actual)
					{
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Actual + 1, Train.Specs.DelayPowerUp);
					}
				}
				else
				{
					int m = Train.Specs.CurrentPowerNotch.DelayedChanges.Length - 1;
					if (Train.Specs.CurrentPowerNotch.Safety < Train.Specs.CurrentPowerNotch.DelayedChanges[m].Value)
					{
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerDown);
					}
					else if (Train.Specs.CurrentPowerNotch.Safety > Train.Specs.CurrentPowerNotch.DelayedChanges[m].Value)
					{
						Train.Specs.CurrentPowerNotch.AddChange(Train, Train.Specs.CurrentPowerNotch.Safety, Train.Specs.DelayPowerUp);
					}
				}
				if (Train.Specs.CurrentPowerNotch.DelayedChanges.Length >= 1)
				{
					if (Train.Specs.CurrentPowerNotch.DelayedChanges[0].Time <= Game.SecondsSinceMidnight)
					{
						Train.Specs.CurrentPowerNotch.Actual = Train.Specs.CurrentPowerNotch.DelayedChanges[0].Value;
						Train.Specs.CurrentPowerNotch.RemoveChanges(1);
					}
				}
			}
			{
				// brake notch
				int sec = Train.Specs.CurrentEmergencyBrake.Safety ? Train.Specs.MaximumBrakeNotch : Train.Specs.CurrentBrakeNotch.Safety;
				if (Train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0)
				{
					if (sec < Train.Specs.CurrentBrakeNotch.Actual)
					{
						Train.Specs.CurrentBrakeNotch.AddChange(Train, Train.Specs.CurrentBrakeNotch.Actual - 1, Train.Specs.DelayBrakeDown);
					}
					else if (sec > Train.Specs.CurrentBrakeNotch.Actual)
					{
						Train.Specs.CurrentBrakeNotch.AddChange(Train, Train.Specs.CurrentBrakeNotch.Actual + 1, Train.Specs.DelayBrakeUp);
					}
				}
				else
				{
					int m = Train.Specs.CurrentBrakeNotch.DelayedChanges.Length - 1;
					if (sec < Train.Specs.CurrentBrakeNotch.DelayedChanges[m].Value)
					{
						Train.Specs.CurrentBrakeNotch.AddChange(Train, sec, Train.Specs.DelayBrakeDown);
					}
					else if (sec > Train.Specs.CurrentBrakeNotch.DelayedChanges[m].Value)
					{
						Train.Specs.CurrentBrakeNotch.AddChange(Train, sec, Train.Specs.DelayBrakeUp);
					}
				}
				if (Train.Specs.CurrentBrakeNotch.DelayedChanges.Length >= 1)
				{
					if (Train.Specs.CurrentBrakeNotch.DelayedChanges[0].Time <= Game.SecondsSinceMidnight)
					{
						Train.Specs.CurrentBrakeNotch.Actual = Train.Specs.CurrentBrakeNotch.DelayedChanges[0].Value;
						Train.Specs.CurrentBrakeNotch.RemoveChanges(1);
					}
				}
			}
			{
				// air brake handle
				if (Train.Specs.AirBrake.Handle.DelayedValue != AirBrakeHandleState.Invalid)
				{
					if (Train.Specs.AirBrake.Handle.DelayedTime <= Game.SecondsSinceMidnight)
					{
						Train.Specs.AirBrake.Handle.Actual = Train.Specs.AirBrake.Handle.DelayedValue;
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Invalid;
					}
				}
				else
				{
					if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Release & Train.Specs.AirBrake.Handle.Actual != AirBrakeHandleState.Release)
					{
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Release;
						Train.Specs.AirBrake.Handle.DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Service & Train.Specs.AirBrake.Handle.Actual != AirBrakeHandleState.Service)
					{
						Train.Specs.AirBrake.Handle.DelayedValue = AirBrakeHandleState.Service;
						Train.Specs.AirBrake.Handle.DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Lap)
					{
						Train.Specs.AirBrake.Handle.Actual = AirBrakeHandleState.Lap;
					}
				}
			}
			{
				// emergency brake
				if (Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentEmergencyBrake.Actual)
				{
					double t = Game.SecondsSinceMidnight;
					if (t < Train.Specs.CurrentEmergencyBrake.ApplicationTime) Train.Specs.CurrentEmergencyBrake.ApplicationTime = t;
					if (Train.Specs.CurrentEmergencyBrake.ApplicationTime <= Game.SecondsSinceMidnight)
					{
						Train.Specs.CurrentEmergencyBrake.Actual = true;
						Train.Specs.CurrentEmergencyBrake.ApplicationTime = double.MaxValue;
					}
				}
				else if (!Train.Specs.CurrentEmergencyBrake.Safety)
				{
					Train.Specs.CurrentEmergencyBrake.Actual = false;
				}
			}
			Train.Specs.CurrentHoldBrake.Actual = Train.Specs.CurrentHoldBrake.Driver;
			// update speeds
			UpdateSpeeds(Train, TimeElapsed);
			// run sound
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				const double factor = 0.04; // 90 km/h -> m/s -> 1/x
				double speed = Math.Abs(Train.Cars[i].Specs.CurrentSpeed);
				if (Train.Cars[i].Derailed)
				{
					speed = 0.0;
				}
				double pitch = speed * factor;
				double basegain;
				if (Train.Cars[i].Specs.CurrentSpeed == 0.0)
				{
					if (i != 0)
					{
						Train.Cars[i].Sounds.RunNextReasynchronizationPosition = Train.Cars[0].FrontAxle.Follower.TrackPosition;
					}
				}
				else if (Train.Cars[i].Sounds.RunNextReasynchronizationPosition == double.MaxValue & Train.Cars[i].FrontAxle.currentRunIdx >= 0)
				{
					double distance = Math.Abs(Train.Cars[i].FrontAxle.Follower.TrackPosition - World.CameraTrackFollower.TrackPosition);
					const double minDistance = 150.0;
					const double maxDistance = 750.0;
					if (distance > minDistance)
					{
						if (Train.Cars[i].FrontAxle.currentRunIdx < Train.Cars[i].Sounds.Run.Length)
						{
							Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.Run[Train.Cars[i].FrontAxle.currentRunIdx].Buffer;
							if (buffer != null)
							{
								double duration = Sounds.GetDuration(buffer);
								if (duration > 0.0)
								{
									double offset = distance > maxDistance ? 25.0 : 300.0;
									Train.Cars[i].Sounds.RunNextReasynchronizationPosition = duration * Math.Ceiling((Train.Cars[0].FrontAxle.Follower.TrackPosition + offset) / duration);
								}
							}
						}
					}
				}
				if (Train.Cars[i].FrontAxle.Follower.TrackPosition >= Train.Cars[i].Sounds.RunNextReasynchronizationPosition)
				{
					Train.Cars[i].Sounds.RunNextReasynchronizationPosition = double.MaxValue;
					basegain = 0.0;
				}
				else
				{
					basegain = speed < 2.77777777777778 ? 0.36 * speed : 1.0;
				}
				for (int j = 0; j < Train.Cars[i].Sounds.Run.Length; j++)
				{
					if (j == Train.Cars[i].FrontAxle.currentRunIdx | j == Train.Cars[i].RearAxle.currentRunIdx)
					{
						Train.Cars[i].Sounds.RunVolume[j] += 3.0 * TimeElapsed;
						if (Train.Cars[i].Sounds.RunVolume[j] > 1.0) Train.Cars[i].Sounds.RunVolume[j] = 1.0;
					}
					else
					{
						Train.Cars[i].Sounds.RunVolume[j] -= 3.0 * TimeElapsed;
						if (Train.Cars[i].Sounds.RunVolume[j] < 0.0) Train.Cars[i].Sounds.RunVolume[j] = 0.0;
					}
					double gain = basegain * Train.Cars[i].Sounds.RunVolume[j];
					if (Sounds.IsPlaying(Train.Cars[i].Sounds.Run[j].Source))
					{
						if (pitch > 0.01 & gain > 0.001)
						{
							Train.Cars[i].Sounds.Run[j].Source.Pitch = pitch;
							Train.Cars[i].Sounds.Run[j].Source.Volume = gain;
						}
						else
						{
							Sounds.StopSound(Train.Cars[i].Sounds.Run[j].Source);
						}
					}
					else if (pitch > 0.02 & gain > 0.01)
					{
						Sounds.SoundBuffer buffer = Train.Cars[i].Sounds.Run[j].Buffer;
						if (buffer != null)
						{
							OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.Run[j].Position;
							Train.Cars[i].Sounds.Run[j].Source = Sounds.PlaySound(buffer, pitch, gain, pos, Train, i, true);
						}
					}
				}
			}
			// motor sound
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (Train.Cars[i].Specs.IsMotorCar)
				{
					OpenBveApi.Math.Vector3 pos = Train.Cars[i].Sounds.Motor.Position;
					double speed = Math.Abs(Train.Cars[i].Specs.CurrentPerceivedSpeed);
					int idx = (int)Math.Round(speed * Train.Cars[i].Sounds.Motor.SpeedConversionFactor);
					int odir = Train.Cars[i].Sounds.Motor.CurrentAccelerationDirection;
					int ndir = Math.Sign(Train.Cars[i].Specs.CurrentAccelerationOutput);
					for (int h = 0; h < 2; h++)
					{
						int j = h == 0 ? TrainManager.MotorSound.MotorP1 : TrainManager.MotorSound.MotorP2;
						int k = h == 0 ? TrainManager.MotorSound.MotorB1 : TrainManager.MotorSound.MotorB2;
						if (odir > 0 & ndir <= 0)
						{
							if (j < Train.Cars[i].Sounds.Motor.Tables.Length)
							{
								Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
								Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
								Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
							}
						}
						else if (odir < 0 & ndir >= 0)
						{
							if (k < Train.Cars[i].Sounds.Motor.Tables.Length)
							{
								Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[k].Source);
								Train.Cars[i].Sounds.Motor.Tables[k].Source = null;
								Train.Cars[i].Sounds.Motor.Tables[k].Buffer = null;
							}
						}
						if (ndir != 0)
						{
							if (ndir < 0) j = k;
							if (j < Train.Cars[i].Sounds.Motor.Tables.Length)
							{
								int idx2 = idx;
								if (idx2 >= Train.Cars[i].Sounds.Motor.Tables[j].Entries.Length)
								{
									idx2 = Train.Cars[i].Sounds.Motor.Tables[j].Entries.Length - 1;
								}
								if (idx2 >= 0)
								{
									Sounds.SoundBuffer obuf = Train.Cars[i].Sounds.Motor.Tables[j].Buffer;
									Sounds.SoundBuffer nbuf = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Buffer;
									double pitch = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Pitch;
									double gain = Train.Cars[i].Sounds.Motor.Tables[j].Entries[idx2].Gain;
									if (ndir == 1)
									{
										// power
										double max = Train.Cars[i].Specs.AccelerationCurveMaximum;
										if (max != 0.0)
										{
											double cur = Train.Cars[i].Specs.CurrentAccelerationOutput;
											if (cur < 0.0) cur = 0.0;
											gain *= Math.Pow(cur / max, 0.25);
										}
									}
									else if (ndir == -1)
									{
										// brake
										double max = Train.Cars[i].Specs.BrakeDecelerationAtServiceMaximumPressure;
										if (max != 0.0)
										{
											double cur = -Train.Cars[i].Specs.CurrentAccelerationOutput;
											if (cur < 0.0) cur = 0.0;
											gain *= Math.Pow(cur / max, 0.25);
										}
									}
									if (obuf != nbuf)
									{
										Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
										if (nbuf != null)
										{
											Train.Cars[i].Sounds.Motor.Tables[j].Source = Sounds.PlaySound(nbuf, pitch, gain, pos, Train, i, true);
											Train.Cars[i].Sounds.Motor.Tables[j].Buffer = nbuf;
										}
										else
										{
											Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
											Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
										}
									}
									else if (nbuf != null)
									{
										if (Train.Cars[i].Sounds.Motor.Tables[j].Source != null)
										{
											Train.Cars[i].Sounds.Motor.Tables[j].Source.Pitch = pitch;
											Train.Cars[i].Sounds.Motor.Tables[j].Source.Volume = gain;
										}
									}
									else
									{
										Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
										Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
										Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
									}
								}
								else
								{
									Sounds.StopSound(Train.Cars[i].Sounds.Motor.Tables[j].Source);
									Train.Cars[i].Sounds.Motor.Tables[j].Source = null;
									Train.Cars[i].Sounds.Motor.Tables[j].Buffer = null;
								}
							}
						}
					}
					Train.Cars[i].Sounds.Motor.CurrentAccelerationDirection = ndir;
				}
			}
			// safety system
			if (!Game.MinimalisticSimulation | Train != PlayerTrain)
			{
				Train.UpdateSafetySystem();
			}
			{
				// breaker sound
				bool breaker;
				if (Train.Cars[Train.DriverCar].Specs.BrakeType == CarBrakeType.AutomaticAirBrake)
				{
					breaker = Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Safety >= 1 & Train.Specs.AirBrake.Handle.Safety == AirBrakeHandleState.Release & !Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentHoldBrake.Actual;
				}
				else
				{
					breaker = Train.Specs.CurrentReverser.Actual != 0 & Train.Specs.CurrentPowerNotch.Safety >= 1 & Train.Specs.CurrentBrakeNotch.Safety == 0 & !Train.Specs.CurrentEmergencyBrake.Safety & !Train.Specs.CurrentHoldBrake.Actual;
				}
				if (breaker & !Train.Cars[Train.DriverCar].Sounds.BreakerResumed)
				{
					// resume
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResume.Buffer != null)
					{
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResume.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResume.Position, Train, Train.DriverCar, false);
					}
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
					{
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Train, Train.DriverCar, false);
					}
					Train.Cars[Train.DriverCar].Sounds.BreakerResumed = true;
				}
				else if (!breaker & Train.Cars[Train.DriverCar].Sounds.BreakerResumed)
				{
					// interrupt
					if (Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer != null)
					{
						Sounds.PlaySound(Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Buffer, 1.0, 1.0, Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt.Position, Train, Train.DriverCar, false);
					}
					Train.Cars[Train.DriverCar].Sounds.BreakerResumed = false;
				}
			}
			// passengers
			UpdateTrainPassengers(Train, TimeElapsed);
			// signals
			if (Train.CurrentSectionLimit == 0.0)
			{
				if (Train.Specs.CurrentEmergencyBrake.Driver & Train.Specs.CurrentAverageSpeed > -0.03 & Train.Specs.CurrentAverageSpeed < 0.03)
				{
					Train.CurrentSectionLimit = 6.94444444444444;
					if (Train == PlayerTrain)
					{
						string s = Interface.GetInterfaceString("message_signal_proceed");
						double a = (3.6 * Train.CurrentSectionLimit) * Game.SpeedConversionFactor;
						s = s.Replace("[speed]", a.ToString("0", System.Globalization.CultureInfo.InvariantCulture));
						s = s.Replace("[unit]", Game.UnitOfSpeed);
						Game.AddMessage(s, MessageManager.MessageDependency.None, Interface.GameMode.Normal, MessageColor.Red, Game.SecondsSinceMidnight + 5.0, null);
					}
				}
			}
			// infrequent updates
			Train.InternalTimerTimeElapsed += TimeElapsed;
			if (Train.InternalTimerTimeElapsed > 10.0)
			{
				Train.InternalTimerTimeElapsed -= 10.0;
				Train.Synchronize();
				Train.UpdateAtmosphericConstants();
			}
		}

	}
}
