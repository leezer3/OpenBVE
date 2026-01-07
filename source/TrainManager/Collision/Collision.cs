using OpenBveApi.Trains;
using RouteManager2.Tracks;
using System;
using System.Collections.Generic;

namespace TrainManager.Trains
{
	public partial class TrainBase
	{
		public void DetectTrainCollision(double timeElapsed, int trainIndex, List<TrainBase> Trains)
		{
			if (Cars.Length == 0 || State != TrainState.Available)
			{
				return;
			}

			double a = FrontCarTrackPosition;
			double b = RearCarTrackPosition;
			for (int j = trainIndex + 1; j < Trains.Count; j++)
			{
				if (Trains[j].State != TrainState.Available)
				{
					continue;
				}
				double c = Trains[j].FrontCarTrackPosition;
				double d = Trains[j].RearCarTrackPosition;
				if (!(a > d & b < c))
				{
					// not currently in collision
					continue;
				}
				if (a > c)
				{
					// Train [i] driving in the nominal R direction collides with the front of [j]
					if (Trains[j].Cars[0].FrontAxle.Follower.TrackIndex != Cars[Cars.Length - 1].RearAxle.Follower.TrackIndex)
					{
						continue;
					}
					// i > j
					int k = Cars.Length - 1;
					if (Cars[k].CurrentSpeed < Trains[j].Cars[0].CurrentSpeed)
					{
						double v = Trains[j].Cars[0].CurrentSpeed - Cars[k].CurrentSpeed;
						double s = (Cars[k].CurrentSpeed * Cars[k].CurrentMass +
						            Trains[j].Cars[0].CurrentSpeed * Trains[j].Cars[0].CurrentMass) /
						           (Cars[k].CurrentMass + Trains[j].Cars[0].CurrentMass);
						Cars[k].CurrentSpeed = s;
						Trains[j].Cars[0].CurrentSpeed = s;
						double e = 0.5 * (c - b) + 0.0001;
							
						Cars[k].MoveDueToCollision(e);
						Trains[j].Cars[0].MoveDueToCollision(-e);

						double f = 2.0 / (Cars[k].CurrentMass + Trains[j].Cars[0].CurrentMass);
						double fi = Trains[j].Cars[0].CurrentMass * f;
						double fj = Cars[k].CurrentMass * f;
						double vi = v * fi;
						double vj = v * fj;
						if (vi > CriticalCollisionSpeedDifference && TrainManagerBase.CurrentOptions.Derailments)
						{
							Derail(k, timeElapsed);
						}
						else if (vj > Trains[j].CriticalCollisionSpeedDifference && TrainManagerBase.CurrentOptions.Derailments)
						{
							Trains[j].Derail(0, timeElapsed);
						}
						else
						{
							if (IsPlayerTrain && Trains[j].Type == TrainType.StaticCars)
							{
								Couple(Trains[j], false);
								Trains[j].Dispose();
								continue;
							}
							if (Trains[j].IsPlayerTrain && Type == TrainType.StaticCars)
							{
								Trains[j].Couple(this, true);
								Dispose();
								continue;
							}
						}
									
						// adjust cars for train i
						for (int h = Cars.Length - 2; h >= 0; h--)
						{
							a = Cars[h + 1].FrontAxle.Follower.TrackPosition - Cars[h + 1].FrontAxle.Position + 0.5 * Cars[h + 1].Length;
							b = Cars[h].RearAxle.Follower.TrackPosition - Cars[h].RearAxle.Position - 0.5 * Cars[h].Length;
							d = b - a - Cars[h].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Cars[h].MoveDueToCollision(-d);
								if (TrainManagerBase.CurrentOptions.Derailments)
								{
									f = 2.0 / (Cars[h + 1].CurrentMass + Cars[h].CurrentMass);
									fi = Cars[h + 1].CurrentMass * f;
									fj = Cars[h].CurrentMass * f;
									vi = v * fi;
									vj = v * fj;
									if (vi > CriticalCollisionSpeedDifference)
										Derail(h + 1, timeElapsed);
									if (vj > Trains[j].CriticalCollisionSpeedDifference)
										Derail(h, timeElapsed);
								}

								Cars[h].CurrentSpeed = Cars[h + 1].CurrentSpeed;
							}
						}

						// adjust cars for train j
						for (int h = 1; h < Trains[j].Cars.Length; h++)
						{
							a = Trains[j].Cars[h - 1].RearAxle.Follower.TrackPosition - Trains[j].Cars[h - 1].RearAxle.Position - 0.5 * Trains[j].Cars[h - 1].Length;
							b = Trains[j].Cars[h].FrontAxle.Follower.TrackPosition -
								Trains[j].Cars[h].FrontAxle.Position + 0.5 * Trains[j].Cars[h].Length;
							d = a - b - Trains[j].Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Trains[j].Cars[h].MoveDueToCollision(d);
								if (TrainManagerBase.CurrentOptions.Derailments)
								{
									f = 2.0 / (Trains[j].Cars[h - 1].CurrentMass + Trains[j].Cars[h].CurrentMass);
									fi = Trains[j].Cars[h - 1].CurrentMass * f;
									fj = Trains[j].Cars[h].CurrentMass * f;
									vi = v * fi;
									vj = v * fj;
									if (vi > Trains[j].CriticalCollisionSpeedDifference)
										Trains[j].Derail(h - 1, timeElapsed);
									if (vj > Trains[j].CriticalCollisionSpeedDifference)
										Trains[j].Derail(h, timeElapsed);
								}

								Trains[j].Cars[h].CurrentSpeed = Trains[j].Cars[h - 1].CurrentSpeed;
							}
						}
					}
				}
				else
				{
					// Train [i] driving in the nominal F direction collides with rear of [j]
					if (Cars[0].FrontAxle.Follower.TrackIndex != Trains[j].Cars[Trains[j].Cars.Length - 1].RearAxle.Follower.TrackIndex)
					{
						continue;
					}
					// i < j
					int k = Trains[j].Cars.Length - 1;
					if (Cars[0].CurrentSpeed > Trains[j].Cars[k].CurrentSpeed)
					{
						double v = Cars[0].CurrentSpeed - Trains[j].Cars[k].CurrentSpeed;
						double s = (Cars[0].CurrentSpeed * Cars[0].CurrentMass +
						            Trains[j].Cars[k].CurrentSpeed * Trains[j].Cars[k].CurrentMass) /
						           (Cars[0].CurrentMass + Trains[j].Cars[k].CurrentMass);
						Cars[0].CurrentSpeed = s;
						Trains[j].Cars[k].CurrentSpeed = s;
						double e = 0.5 * (a - d) + 0.0001;
							
						Cars[0].MoveDueToCollision(-e);
						Trains[j].Cars[k].MoveDueToCollision(e);

						double f = 2.0 / (Cars[0].CurrentMass + Trains[j].Cars[k].CurrentMass);
						double fi = Trains[j].Cars[k].CurrentMass * f;
						double fj = Cars[0].CurrentMass * f;
						double vi = v * fi;
						double vj = v * fj;
						if (vi > CriticalCollisionSpeedDifference && TrainManagerBase.CurrentOptions.Derailments)
						{
							Trains[j].Derail(0, timeElapsed);
						}
						else if (vj > Trains[j].CriticalCollisionSpeedDifference && TrainManagerBase.CurrentOptions.Derailments)
						{
							Trains[j].Derail(k, timeElapsed);
						}
						else
						{
							if (IsPlayerTrain && Trains[j].Type == TrainType.StaticCars)
							{
								Couple(Trains[j], true);
								Trains[j].Dispose();
								continue;
							}
							if (Trains[j].IsPlayerTrain && Type == TrainType.StaticCars)
							{
								Trains[j].Couple(this, false);
								Dispose();
								continue;
							}
						}

						// adjust cars for train i
						for (int h = 1; h < Cars.Length; h++)
						{
							a = Cars[h - 1].RearAxle.Follower.TrackPosition - Cars[h - 1].RearAxle.Position - 0.5 * Cars[h - 1].Length;
							b = Cars[h].FrontAxle.Follower.TrackPosition - Cars[h].FrontAxle.Position + 0.5 * Cars[h].Length;
							d = a - b - Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Cars[h].MoveDueToCollision(d);
								if (TrainManagerBase.CurrentOptions.Derailments)
								{
									f = 2.0 / (Cars[h - 1].CurrentMass + Cars[h].CurrentMass);
									fi = Cars[h - 1].CurrentMass * f;
									fj = Cars[h].CurrentMass * f;
									vi = v * fi;
									vj = v * fj;
									if (vi > CriticalCollisionSpeedDifference)
										Derail(h - 1, timeElapsed);
									if (vj > CriticalCollisionSpeedDifference)
										Derail(h, timeElapsed);
								}

								Cars[h].CurrentSpeed = Cars[h - 1].CurrentSpeed;
							}
						}

						// adjust cars for train j
						for (int h = Trains[j].Cars.Length - 2; h >= 0; h--)
						{
							a = Trains[j].Cars[h + 1].FrontAxle.Follower.TrackPosition - Trains[j].Cars[h + 1].FrontAxle.Position + 0.5 * Trains[j].Cars[h + 1].Length;
							b = Trains[j].Cars[h].RearAxle.Follower.TrackPosition - Trains[j].Cars[h].RearAxle.Position - 0.5 * Trains[j].Cars[h].Length;
							d = b - a - Trains[j].Cars[h].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Trains[j].Cars[h].MoveDueToCollision(-d);
								if (TrainManagerBase.CurrentOptions.Derailments)
								{
									f = 2.0 / (Trains[j].Cars[h + 1].CurrentMass + Trains[j].Cars[h].CurrentMass);
									fi = Trains[j].Cars[h + 1].CurrentMass * f;
									fj = Trains[j].Cars[h].CurrentMass * f;
									vi = v * fi;
									vj = v * fj;
									if (vi > CriticalCollisionSpeedDifference)
										Trains[j].Derail(h + 1, timeElapsed);
									if (vj > Trains[j].CriticalCollisionSpeedDifference)
										Trains[j].Derail(h, timeElapsed);
								}

								Trains[j].Cars[h].CurrentSpeed = Trains[j].Cars[h + 1].CurrentSpeed;
							}
						}
					}
				}
			}
		}

		public void DetectBufferCollision(double timeElapsed, List<BufferStop> bufferTrackPositions)
		{
			if (Cars.Length == 0)
			{
				return;
			}
			double bPa = Cars[0].FrontAxle.Follower.TrackPosition - Cars[0].FrontAxle.Position +
						 0.5 * Cars[0].Length;
			double bPb = Cars[Cars.Length - 1].RearAxle.Follower.TrackPosition -
						 Cars[Cars.Length - 1].RearAxle.Position - 0.5 * Cars[0].Length;
			for (int j = 0; j < bufferTrackPositions.Count; j++)
			{
				if (!IsPlayerTrain && bufferTrackPositions[j].PlayerTrainOnly)
				{
					continue;
				}

				if (bPa > bufferTrackPositions[j].TrackPosition & bPb < bufferTrackPositions[j].TrackPosition)
				{
					bPa += 0.0001;
					bPb -= 0.0001;
					double da = bPa - bufferTrackPositions[j].TrackPosition;
					double db = bufferTrackPositions[j].TrackPosition - bPb;
					if (da < db)
					{
						if (Cars[0].FrontAxle.Follower.TrackIndex != bufferTrackPositions[j].TrackIndex)
						{
							continue;
						}

						// front
						Cars[0].UpdateTrackFollowers(-da, false, false);
						if (TrainManagerBase.CurrentOptions.Derailments && Math.Abs(Cars[0].CurrentSpeed) > CriticalCollisionSpeedDifference)
						{
							Derail(0, timeElapsed);
						}

						Cars[0].CurrentSpeed = 0.0;
						for (int h = 1; h < Cars.Length; h++)
						{
							bPa = Cars[h - 1].RearAxle.Follower.TrackPosition - Cars[h - 1].RearAxle.Position - 0.5 * Cars[h - 1].Length;
							bPb = Cars[h].FrontAxle.Follower.TrackPosition - Cars[h].FrontAxle.Position + 0.5 * Cars[h].Length;
							double d = bPa - bPb - Cars[h - 1].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Cars[h].UpdateTrackFollowers(d, false, false);
								if (TrainManagerBase.CurrentOptions.Derailments && Math.Abs(Cars[h].CurrentSpeed) > CriticalCollisionSpeedDifference)
								{
									Derail(h, timeElapsed);
								}

								Cars[h].CurrentSpeed = 0.0;
							}
						}
					}
					else
					{
						// rear
						int c = Cars.Length - 1;
						if (Cars[c].RearAxle.Follower.TrackIndex != bufferTrackPositions[j].TrackIndex)
						{
							continue;
						}

						Cars[c].UpdateTrackFollowers(db, false, false);
						if (TrainManagerBase.CurrentOptions.Derailments && Math.Abs(Cars[c].CurrentSpeed) > CriticalCollisionSpeedDifference)
						{
							Derail(c, timeElapsed);
						}

						Cars[c].CurrentSpeed = 0.0;
						for (int h = Cars.Length - 2; h >= 0; h--)
						{
							bPa = Cars[h + 1].FrontAxle.Follower.TrackPosition - Cars[h + 1].FrontAxle.Position + 0.5 * Cars[h + 1].Length;
							bPb = Cars[h].RearAxle.Follower.TrackPosition - Cars[h].RearAxle.Position - 0.5 * Cars[h].Length;
							double d = bPb - bPa - Cars[h].Coupler.MinimumDistanceBetweenCars;
							if (d < 0.0)
							{
								d -= 0.0001;
								Cars[h].UpdateTrackFollowers(-d, false, false);
								if (TrainManagerBase.CurrentOptions.Derailments && Math.Abs(Cars[h].CurrentSpeed) > CriticalCollisionSpeedDifference)
								{
									Derail(h, timeElapsed);
								}

								Cars[h].CurrentSpeed = 0.0;
							}
						}
					}
				}
			}
		}
	}
}
