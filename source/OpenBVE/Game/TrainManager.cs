using System;
using LibRender2;
using OpenBveApi;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using TrainManager;
using TrainManager.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public class TrainManager : TrainManagerBase
	{
		/// <inheritdoc/>
		public TrainManager(HostInterface host, BaseRenderer renderer, BaseOptions options, FileSystem fileSystem) : base(host, renderer, options, fileSystem)
		{
		}
		
		/// <summary>This method should be called once a frame to update the position, speed and state of all trains within the simulation</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
		internal void UpdateTrains(double TimeElapsed)
		{
			if (Interface.CurrentOptions.GameMode == GameMode.Developer)
			{
				return;
			}
			for (int i = 0; i < Trains.Count; i++) {
				Trains[i].Update(TimeElapsed);
			}

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (ScriptedTrain Train in TFOs) //Must not use var, as otherwise the wrong inferred type
			{
				Train.Update(TimeElapsed);
			}

			// detect collision
			if (!Game.MinimalisticSimulation & Interface.CurrentOptions.Collisions)
			{
				
				for (int i = 0; i < Trains.Count; i++) {
					// with other trains
					if (Trains[i].State == TrainState.Available)
					{
						double a = Trains[i].FrontCarTrackPosition;
						double b = Trains[i].RearCarTrackPosition;
						for (int j = i + 1; j < Trains.Count; j++)
						{
							if (Trains[j].State == TrainState.Available)
							{
								double c = Trains[j].FrontCarTrackPosition;
								double d = Trains[j].RearCarTrackPosition;
								if (a > d & b < c)
								{
									if (a > c)
									{
										// Train [i] driving in the nominal R direction collides with the front of [j]
										if (Trains[j].Cars[0].FrontAxle.Follower.TrackIndex != Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackIndex)
										{
											continue;
										}
										// i > j
										int k = Trains[i].Cars.Length - 1;
										if (Trains[i].Cars[k].CurrentSpeed < Trains[j].Cars[0].CurrentSpeed)
										{
											double v = Trains[j].Cars[0].CurrentSpeed - Trains[i].Cars[k].CurrentSpeed;
											double s = (Trains[i].Cars[k].CurrentSpeed * Trains[i].Cars[k].CurrentMass +
											            Trains[j].Cars[0].CurrentSpeed * Trains[j].Cars[0].CurrentMass) /
											           (Trains[i].Cars[k].CurrentMass + Trains[j].Cars[0].CurrentMass);
											Trains[i].Cars[k].CurrentSpeed = s;
											Trains[j].Cars[0].CurrentSpeed = s;
											double e = 0.5 * (c - b) + 0.0001;
											Trains[i].Cars[k].FrontAxle.Follower.UpdateRelative(e, false, false);
											Trains[i].Cars[k].RearAxle.Follower.UpdateRelative(e, false, false);
											Trains[j].Cars[0].FrontAxle.Follower.UpdateRelative(-e, false, false);

											Trains[j].Cars[0].RearAxle.Follower.UpdateRelative(-e, false, false);
											double f = 2.0 / (Trains[i].Cars[k].CurrentMass + Trains[j].Cars[0].CurrentMass);
											double fi = Trains[j].Cars[0].CurrentMass * f;
											double fj = Trains[i].Cars[k].CurrentMass * f;
											double vi = v * fi;
											double vj = v * fj;
											if (vi > Trains[i].CriticalCollisionSpeedDifference)
											{
												if(Interface.CurrentOptions.Derailments)
													Trains[i].Derail(k, TimeElapsed);
											}
											else if (vj > Trains[j].CriticalCollisionSpeedDifference)
											{
												if(Interface.CurrentOptions.Derailments)
													Trains[j].Derail(i, TimeElapsed);
											}
											else
											{
												if (Trains[i].IsPlayerTrain && Trains[j].Type == TrainType.StaticCars)
												{
													Trains[i].Couple(Trains[j], false);
													Trains[j].Dispose();
													continue;
												}
												if (Trains[j].IsPlayerTrain && Trains[i].Type == TrainType.StaticCars)
												{
													Trains[j].Couple(Trains[i], true);
													Trains[i].Dispose();
													continue;
												}

											}
													
											

											// adjust cars for train i
											for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h + 1].FrontAxle.Position + 0.5 * Trains[i].Cars[h + 1].Length;
												b = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
												    Trains[i].Cars[h].RearAxle.Position - 0.5 * Trains[i].Cars[h].Length;
												d = b - a - Trains[i].Cars[h].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[i].Cars[h].FrontAxle.Follower.UpdateRelative(-d, false, false);
													Trains[i].Cars[h].RearAxle.Follower.UpdateRelative(-d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														f = 2.0 / (Trains[i].Cars[h + 1].CurrentMass + Trains[i].Cars[h].CurrentMass);
														fi = Trains[i].Cars[h + 1].CurrentMass * f;
														fj = Trains[i].Cars[h].CurrentMass * f;
														vi = v * fi;
														vj = v * fj;
														if (vi > Trains[i].CriticalCollisionSpeedDifference)
															Trains[i].Derail(h + 1, TimeElapsed);
														if (vj > Trains[j].CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}

													Trains[i].Cars[h].CurrentSpeed =
														Trains[i].Cars[h + 1].CurrentSpeed;
												}
											}

											// adjust cars for train j
											for (int h = 1; h < Trains[j].Cars.Length; h++)
											{
												a = Trains[j].Cars[h - 1].RearAxle.Follower.TrackPosition -
												    Trains[j].Cars[h - 1].RearAxle.Position - 0.5 * Trains[j].Cars[h - 1].Length;
												b = Trains[j].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h].FrontAxle.Position + 0.5 * Trains[j].Cars[h].Length;
												d = a - b - Trains[j].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[j].Cars[h].FrontAxle.Follower.UpdateRelative(d, false, false);
													Trains[j].Cars[h].RearAxle.Follower.UpdateRelative(d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														f = 2.0 / (Trains[j].Cars[h - 1].CurrentMass + Trains[j].Cars[h].CurrentMass);
														fi = Trains[j].Cars[h - 1].CurrentMass * f;
														fj = Trains[j].Cars[h].CurrentMass * f;
														vi = v * fi;
														vj = v * fj;
														if (vi > Trains[j].CriticalCollisionSpeedDifference)
															Trains[j].Derail(h - 1, TimeElapsed);
														if (vj > Trains[j].CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}

													Trains[j].Cars[h].CurrentSpeed =
														Trains[j].Cars[h - 1].CurrentSpeed;
												}
											}
										}
									}
									else
									{
										// Train [i] driving in the nominal F direction collides with rear of [j]
										if (Trains[i].Cars[0].FrontAxle.Follower.TrackIndex != Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxle.Follower.TrackIndex)
										{
											continue;
										}
										// i < j
										int k = Trains[j].Cars.Length - 1;
										if (Trains[i].Cars[0].CurrentSpeed > Trains[j].Cars[k].CurrentSpeed)
										{
											double v = Trains[i].Cars[0].CurrentSpeed -
											           Trains[j].Cars[k].CurrentSpeed;
											double s = (Trains[i].Cars[0].CurrentSpeed * Trains[i].Cars[0].CurrentMass +
											            Trains[j].Cars[k].CurrentSpeed * Trains[j].Cars[k].CurrentMass) /
											           (Trains[i].Cars[0].CurrentMass + Trains[j].Cars[k].CurrentMass);
											Trains[i].Cars[0].CurrentSpeed = s;
											Trains[j].Cars[k].CurrentSpeed = s;
											double e = 0.5 * (a - d) + 0.0001;
											Trains[i].Cars[0].FrontAxle.Follower.UpdateRelative(-e, false, false);
											Trains[i].Cars[0].RearAxle.Follower.UpdateRelative(-e, false, false);
											Trains[j].Cars[k].FrontAxle.Follower.UpdateRelative(e, false, false);
											Trains[j].Cars[k].RearAxle.Follower.UpdateRelative(e, false, false);
											double f = 2.0 / (Trains[i].Cars[0].CurrentMass + Trains[j].Cars[k].CurrentMass);

											double fi = Trains[j].Cars[k].CurrentMass * f;
											double fj = Trains[i].Cars[0].CurrentMass * f;
											double vi = v * fi;
											double vj = v * fj;
											if (vi > Trains[i].CriticalCollisionSpeedDifference)
											{
												if(Interface.CurrentOptions.Derailments)														
													Trains[j].Derail(0, TimeElapsed);
											}
											else if (vj > Trains[j].CriticalCollisionSpeedDifference)
											{
												if(Interface.CurrentOptions.Derailments)
													Trains[j].Derail(k, TimeElapsed);
											}
											else
											{
												if (Trains[i].IsPlayerTrain && Trains[j].Type == TrainType.StaticCars)
												{
													Trains[i].Couple(Trains[j], true);
													Trains[j].Dispose();
													continue;
												}
												if (Trains[j].IsPlayerTrain && Trains[i].Type == TrainType.StaticCars)
												{
													Trains[j].Couple(Trains[i], false);
													Trains[i].Dispose();
													continue;
												}
											}

											// adjust cars for train i
											for (int h = 1; h < Trains[i].Cars.Length; h++)
											{
												a = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
												    Trains[i].Cars[h - 1].RearAxle.Position - 0.5 * Trains[i].Cars[h - 1].Length;
												b = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
													Trains[i].Cars[h].FrontAxle.Position + 0.5 * Trains[i].Cars[h].Length;
												d = a - b - Trains[i].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[i].Cars[h].FrontAxle.Follower.UpdateRelative(d, false, false);
													Trains[i].Cars[h].RearAxle.Follower.UpdateRelative(d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														f = 2.0 / (Trains[i].Cars[h - 1].CurrentMass + Trains[i].Cars[h].CurrentMass);
														fi = Trains[i].Cars[h - 1].CurrentMass * f;
														fj = Trains[i].Cars[h].CurrentMass * f;
														vi = v * fi;
														vj = v * fj;
														if (vi > Trains[i].CriticalCollisionSpeedDifference)
															Trains[i].Derail(h - 1, TimeElapsed);
														if (vj > Trains[i].CriticalCollisionSpeedDifference)
															Trains[i].Derail(h, TimeElapsed);
													}

													Trains[i].Cars[h].CurrentSpeed =
														Trains[i].Cars[h - 1].CurrentSpeed;
												}
											}

											// adjust cars for train j
											for (int h = Trains[j].Cars.Length - 2; h >= 0; h--)
											{
												a = Trains[j].Cars[h + 1].FrontAxle.Follower.TrackPosition -
													Trains[j].Cars[h + 1].FrontAxle.Position + 0.5 * Trains[j].Cars[h + 1].Length;
												b = Trains[j].Cars[h].RearAxle.Follower.TrackPosition -
												    Trains[j].Cars[h].RearAxle.Position - 0.5 * Trains[j].Cars[h].Length;
												d = b - a - Trains[j].Cars[h].Coupler.MinimumDistanceBetweenCars;
												if (d < 0.0)
												{
													d -= 0.0001;
													Trains[j].Cars[h].FrontAxle.Follower.UpdateRelative(-d, false, false);
													Trains[j].Cars[h].RearAxle.Follower.UpdateRelative(-d, false, false);
													if (Interface.CurrentOptions.Derailments)
													{
														f = 2.0 / (Trains[j].Cars[h + 1].CurrentMass + Trains[j].Cars[h].CurrentMass);
														fi = Trains[j].Cars[h + 1].CurrentMass * f;
														fi = Trains[j].Cars[h].CurrentMass * f;
														vi = v * fi;
														vi = v * fj;
														if (vi > Trains[i].CriticalCollisionSpeedDifference)
															Trains[j].Derail(h + 1, TimeElapsed);
														if (vj > Trains[j].CriticalCollisionSpeedDifference)
															Trains[j].Derail(h, TimeElapsed);
													}

													Trains[j].Cars[h].CurrentSpeed =
														Trains[j].Cars[h + 1].CurrentSpeed;
												}
											}
										}
									}
								}
							}

						}
					}

					// with buffers
					double bPa = Trains[i].Cars[0].FrontAxle.Follower.TrackPosition - Trains[i].Cars[0].FrontAxle.Position +
					             0.5 * Trains[i].Cars[0].Length;
					double bPb = Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Follower.TrackPosition -
					             Trains[i].Cars[Trains[i].Cars.Length - 1].RearAxle.Position - 0.5 * Trains[i].Cars[0].Length;
					for (int j = 0; j < Program.CurrentRoute.BufferTrackPositions.Count; j++)
					{
						if (!Trains[i].IsPlayerTrain && Program.CurrentRoute.BufferTrackPositions[j].PlayerTrainOnly)
						{
							continue;
						}

						if (bPa > Program.CurrentRoute.BufferTrackPositions[j].TrackPosition & bPb < Program.CurrentRoute.BufferTrackPositions[j].TrackPosition)
						{
							bPa += 0.0001;
							bPb -= 0.0001;
							double da = bPa - Program.CurrentRoute.BufferTrackPositions[j].TrackPosition;
							double db = Program.CurrentRoute.BufferTrackPositions[j].TrackPosition - bPb;
							if (da < db)
							{
								if (Trains[i].Cars[0].FrontAxle.Follower.TrackIndex != Program.CurrentRoute.BufferTrackPositions[j].TrackIndex)
								{
									continue;
								}

								// front
								Trains[i].Cars[0].UpdateTrackFollowers(-da, false, false);
								if (Interface.CurrentOptions.Derailments &&
								    Math.Abs(Trains[i].Cars[0].CurrentSpeed) > Trains[i].CriticalCollisionSpeedDifference)
								{
									Trains[i].Derail(0, TimeElapsed);
								}

								Trains[i].Cars[0].CurrentSpeed = 0.0;
								for (int h = 1; h < Trains[i].Cars.Length; h++)
								{
									bPa = Trains[i].Cars[h - 1].RearAxle.Follower.TrackPosition -
									      Trains[i].Cars[h - 1].RearAxle.Position - 0.5 * Trains[i].Cars[h - 1].Length;
									bPb = Trains[i].Cars[h].FrontAxle.Follower.TrackPosition -
										Trains[i].Cars[h].FrontAxle.Position + 0.5 * Trains[i].Cars[h].Length;
									double d = bPa - bPb - Trains[i].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
									if (d < 0.0)
									{
										d -= 0.0001;
										Trains[i].Cars[h].UpdateTrackFollowers(d, false, false);
										if (Interface.CurrentOptions.Derailments &&
										    Math.Abs(Trains[i].Cars[h].CurrentSpeed) >
										    Trains[i].CriticalCollisionSpeedDifference)
										{
											Trains[i].Derail(h, TimeElapsed);
										}

										Trains[i].Cars[h].CurrentSpeed = 0.0;
									}
								}
							}
							else
							{
								// rear
								int c = Trains[i].Cars.Length - 1;
								if (Trains[i].Cars[c].RearAxle.Follower.TrackIndex != Program.CurrentRoute.BufferTrackPositions[j].TrackIndex)
								{
									continue;
								}

								Trains[i].Cars[c].UpdateTrackFollowers(db, false, false);
								if (Interface.CurrentOptions.Derailments &&
								    Math.Abs(Trains[i].Cars[c].CurrentSpeed) > Trains[i].CriticalCollisionSpeedDifference)
								{
									Trains[i].Derail(c, TimeElapsed);
								}

								Trains[i].Cars[c].CurrentSpeed = 0.0;
								for (int h = Trains[i].Cars.Length - 2; h >= 0; h--)
								{
									bPa = Trains[i].Cars[h + 1].FrontAxle.Follower.TrackPosition -
										Trains[i].Cars[h + 1].FrontAxle.Position + 0.5 * Trains[i].Cars[h + 1].Length;
									bPb = Trains[i].Cars[h].RearAxle.Follower.TrackPosition -
									      Trains[i].Cars[h].RearAxle.Position - 0.5 * Trains[i].Cars[h].Length;
									double d = bPb - bPa - Trains[i].Cars[h].Coupler.MinimumDistanceBetweenCars;
									if (d < 0.0)
									{
										d -= 0.0001;
										Trains[i].Cars[h].UpdateTrackFollowers(-d, false, false);
										if (Interface.CurrentOptions.Derailments &&
										    Math.Abs(Trains[i].Cars[h].CurrentSpeed) >
										    Trains[i].CriticalCollisionSpeedDifference)
										{
											Trains[i].Derail(h, TimeElapsed);
										}

										Trains[i].Cars[h].CurrentSpeed = 0.0;
									}
								}
							}
						}
					}

				}
			}

			for (int i = Trains.Count - 1; i > 0; i--)
			{
				switch (Trains[i].State)
				{
					case TrainState.DisposePending:
						Trains[i].State = TrainState.Disposed;
						break;
					case TrainState.Disposed:
						Trains.RemoveAt(i);
						break;
				}
			}

			// compute final angles and positions
			//for (int i = 0; i < Trains.Length; i++) {
			System.Threading.Tasks.Parallel.For(0, Trains.Count, i =>
			{
				if (Trains[i].State < TrainState.DisposePending) 
				{
					for (int j = 0; j < Trains[i].Cars.Length; j++)
					{
						Trains[i].Cars[j].FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Trains[i].Cars[j].RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Trains[i].Cars[j].UpdateTopplingCantAndSpring(TimeElapsed);
						Trains[i].Cars[j].FrontBogie.UpdateTopplingCantAndSpring();
						Trains[i].Cars[j].RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});

			System.Threading.Tasks.Parallel.For(0, TFOs.Length, i =>
			{
				if (TFOs[i].State < TrainState.DisposePending)
				{
					ScriptedTrain t = (ScriptedTrain) TFOs[i];
					foreach (var Car in t.Cars)
					{
						Car.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.FrontBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.FrontAxle.Follower.UpdateWorldCoordinates(true);
						Car.RearBogie.RearAxle.Follower.UpdateWorldCoordinates(true);
						if (TimeElapsed == 0.0 | TimeElapsed > 0.5)
						{
							//Don't update the toppling etc. with excessive or no time
							continue;
						}
						Car.UpdateTopplingCantAndSpring(TimeElapsed);
						Car.FrontBogie.UpdateTopplingCantAndSpring();
						Car.RearBogie.UpdateTopplingCantAndSpring();
					}
				}
			});
		}
	}
}
