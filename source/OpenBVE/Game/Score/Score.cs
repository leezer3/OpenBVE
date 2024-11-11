﻿using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.Trains;

namespace OpenBve
{
	internal static partial class Game
	{
		/// <summary>The current in-game score</summary>
		internal static Score CurrentScore = new Score();

		/// <summary>The score class</summary>
		internal partial class Score
		{
			/// <summary>The current total score</summary>
			internal int CurrentValue;
			/// <summary>The maxium available score</summary>
			internal int Maximum;
			/// <summary>The number of times the doors have been opened incorrectly</summary>
			internal double OpenedDoorsCounter;
			/// <summary>The number of times the speed limit has been exceeded</summary>
			internal double OverspeedCounter;
			/// <summary>The number of times the train has toppled due to excessive speed in curves</summary>
			internal double TopplingCounter;
			/// <summary>The number of times a signal has been passed at danger</summary>
			internal bool RedSignal;
			/// <summary>The number of times the train has been derailed</summary>
			internal bool Derailed;
			internal int ArrivalStation;
			internal int DepartureStation;
			internal double PassengerTimer;

			/// <summary>This method should be called once a frame to update the player's score</summary>
			/// <param name="timeElapsed">The time elapsed since this function was last called</param>
			internal void Update(double timeElapsed)
			{
				if (TrainManager.PlayerTrain == null)
				{
					return;
				}
				// doors
				{
					bool leftopen = false;
					bool rightopen = false;
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						for (int k = 0; k < TrainManager.PlayerTrain.Cars[j].Doors.Length; k++)
						{
							if (TrainManager.PlayerTrain.Cars[j].Doors[k].State != 0.0)
							{
								if (TrainManager.PlayerTrain.Cars[j].Doors[k].Direction == -1)
								{
									leftopen = true;
								}
								else if (TrainManager.PlayerTrain.Cars[j].Doors[k].Direction == 1)
								{
									rightopen = true;
								}
							}
						}
					}
					bool bad;
					if (leftopen | rightopen)
					{
						bad = true;
						int j = TrainManager.PlayerTrain.Station;
						if (j >= 0)
						{
							int p = Program.CurrentRoute.Stations[j].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
							if (p >= 0)
							{
								if (Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) < 0.1)
								{
									if (leftopen == Program.CurrentRoute.Stations[j].OpenLeftDoors & rightopen == Program.CurrentRoute.Stations[j].OpenRightDoors)
									{
										bad = false;
									}
								}
							}
						}
					}
					else
					{
						bad = false;
					}
					if (bad)
					{
						OpenedDoorsCounter += (Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) + 0.25) * timeElapsed;
					}
					else if (OpenedDoorsCounter != 0.0)
					{
						int x = (int)Math.Ceiling(FactorOpenedDoors * OpenedDoorsCounter);
						CurrentValue += x;
						if (x != 0)
						{
							AddScore(x, ScoreTextToken.DoorsOpened, 5.0);
						}
						OpenedDoorsCounter = 0.0;
					}
				}
				// overspeed
				double nr = TrainManager.PlayerTrain.CurrentRouteLimit;
				double ns = TrainManager.PlayerTrain.CurrentSectionLimit;
				double n = nr < ns ? nr : ns;
				double a = Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) - 0.277777777777778;
				if (a > n)
				{
					OverspeedCounter += (a - n) * timeElapsed;
				}
				else if (OverspeedCounter != 0.0)
				{
					int x = (int)Math.Ceiling(FactorOverspeed * OverspeedCounter);
					CurrentValue += x;
					if (x != 0)
					{
						AddScore(x, ScoreTextToken.Overspeed, 5.0);
					}
					OverspeedCounter = 0.0;
				}
				// toppling
				{
					bool q = false;
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						if (TrainManager.PlayerTrain.Cars[j].Topples)
						{
							q = true;
							break;
						}
					}
					if (q)
					{
						TopplingCounter += timeElapsed;
					}
					else if (TopplingCounter != 0.0)
					{
						int x = (int)Math.Ceiling(FactorToppling * TopplingCounter);
						CurrentValue += x;
						if (x != 0)
						{
							AddScore(x, ScoreTextToken.Toppling, 5.0);
						}
						TopplingCounter = 0.0;
					}
				}
				// derailment
				if (!Derailed)
				{
					bool q = false;
					for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
					{
						if (TrainManager.PlayerTrain.Cars[j].Derailed)
						{
							q = true;
							break;
						}
					}
					if (q)
					{
						int x = ValueDerailment;
						if (CurrentValue > 0) x -= CurrentValue;
						CurrentValue += x;
						if (x != 0)
						{
							AddScore(x, ScoreTextToken.Derailed, 5.0);
						}
						Derailed = true;
					}
				}
				// red signal
				{
					if (TrainManager.PlayerTrain.CurrentSectionLimit == 0.0)
					{
						if (!RedSignal)
						{
							int x = ValueRedSignal;
							CurrentValue += x;
							AddScore(x, ScoreTextToken.PassedRedSignal, 5.0);
							RedSignal = true;
						}
					}
					else
					{
						RedSignal = false;
					}
				}
				// arrival
				{
					int j = TrainManager.PlayerTrain.Station;
					if (j >= 0 & j < Program.CurrentRoute.Stations.Length)
					{
						if (j >= ArrivalStation & TrainManager.PlayerTrain.StationState == TrainStopState.Boarding)
						{
							if (j == 0 || Program.CurrentRoute.Stations[j - 1].Type != StationType.ChangeEnds && Program.CurrentRoute.Stations[j - 1].Type != StationType.Jump)
							{
								// arrival
								int xa = ValueStationArrival;
								CurrentValue += xa;
								AddScore(xa, ScoreTextToken.ArrivedAtStation, 10.0);
								// early/late
								int xb;
								if (Program.CurrentRoute.Stations[j].ArrivalTime >= 0)
								{
									double d = Program.CurrentRoute.SecondsSinceMidnight - Program.CurrentRoute.Stations[j].ArrivalTime;
									if (d >= -5.0 & d <= 0.0)
									{
										xb = ValueStationPerfectTime;
										CurrentValue += xb;
										AddScore(xb, ScoreTextToken.PerfectTimeBonus, 10.0);
									}
									else if (d > 0.0)
									{
										xb = (int)Math.Ceiling(FactorStationLate * (d - 1.0));
										CurrentValue += xb;
										if (xb != 0)
										{
											AddScore(xb, ScoreTextToken.Late, 10.0);
										}
									}
									else
									{
										xb = 0;
									}
								}
								else
								{
									xb = 0;
								}
								// position
								int xc;
								int p = Program.CurrentRoute.Stations[j].GetStopIndex(TrainManager.PlayerTrain.NumberOfCars);
								if (p >= 0)
								{
									double d = TrainManager.PlayerTrain.StationDistanceToStopPoint;
									double r;
									if (d >= 0)
									{
										double t = Program.CurrentRoute.Stations[j].Stops[p].BackwardTolerance;
										r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
									}
									else
									{
										double t = Program.CurrentRoute.Stations[j].Stops[p].ForwardTolerance;
										r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
									}
									if (r < 0.01)
									{
										xc = ValueStationPerfectStop;
										CurrentValue += xc;
										AddScore(xc, ScoreTextToken.PerfectStopBonus, 10.0);
									}
									else
									{
										if (r > 1.0) r = 1.0;
										r = (r - 0.01) * 1.01010101010101;
										xc = (int)Math.Ceiling(FactorStationStop * r);
										CurrentValue += xc;
										if (xc != 0)
										{
											AddScore(xc, ScoreTextToken.Stop, 10.0);
										}
									}
								}
								else
								{
									xc = 0;
								}
								// sum
								if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
								{
									int xs = xa + xb + xc;
									AddScore("", 10.0);
									AddScore(xs, ScoreTextToken.Total, 10.0, false);
									AddScore(" ", 10.0);
								}
								// evaluation
								if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
								{
									if (Program.CurrentRoute.Stations[j].Type == StationType.Terminal)
									{
										double y = CurrentValue / (double)Maximum;
										if (y < 0.0) y = 0.0;
										if (y > 1.0) y = 1.0;
										int k = (int)Math.Floor(y * Translations.RatingsCount);
										if (k >= Translations.RatingsCount) k = Translations.RatingsCount - 1;
										System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
										AddScore(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"score","rating"}), 20.0);
										AddScore(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"rating" , k.ToString(Culture)}) + " (" + (100.0 * y).ToString("0.00", Culture) + "%)", 20.0);
									}
								}
							}
							// finalize
							DepartureStation = j;
							ArrivalStation = j + 1;
						}
					}
				}
				// departure
				{
					int j = TrainManager.PlayerTrain.Station;
					if (j >= 0 & j < Program.CurrentRoute.Stations.Length & j == DepartureStation)
					{
						bool q;
						if (Program.CurrentRoute.Stations[j].OpenLeftDoors | Program.CurrentRoute.Stations[j].OpenRightDoors)
						{
							q = TrainManager.PlayerTrain.StationState == TrainStopState.Completed;
						}
						else
						{
							q = TrainManager.PlayerTrain.StationState != TrainStopState.Pending & (TrainManager.PlayerTrain.CurrentSpeed < -1.5 | TrainManager.PlayerTrain.CurrentSpeed > 1.5);
						}
						if (q)
						{
							double r = TrainManager.PlayerTrain.StationDepartureTime - Program.CurrentRoute.SecondsSinceMidnight;
							if (r > 0.0)
							{
								int x = (int)Math.Ceiling(FactorStationDeparture * r);
								CurrentValue += x;
								if (x != 0)
								{
									AddScore(x, ScoreTextToken.PrematureDeparture, 5.0);
								}
							}
							DepartureStation = -1;
						}
					}
				}
				// passengers
				bool fallenOver = false;
				for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
				{
					/*
					 * NOTE: This should currently break on the first car.
					 * When other cargo types are implemented, it may not
					 */

					if (TrainManager.PlayerTrain.Cars[i].Cargo.Damaged)
					{
						fallenOver = true;
						break;
					}
				}
				if (fallenOver & PassengerTimer == 0.0)
				{
					int x = ValuePassengerDiscomfort;
					CurrentValue += x;
					AddScore(x, ScoreTextToken.PassengerDiscomfort, 5.0);
					PassengerTimer = 5.0;
				}
				else
				{
					PassengerTimer -= timeElapsed;
					if (PassengerTimer <= 0.0)
					{
						PassengerTimer = fallenOver ? 5.0 : 0.0;
					}
				}
			}

			/// <summary>Is called by the update function to add a new score event to the log</summary>
			/// <param name="scoreValue">The value of the score event</param>
			/// <param name="textToken">The token type which caused the score event</param>
			/// <param name="scoreDuration">The duration of the score event (e.g. overspeed)</param>
			/// <param name="countedEvent">Whether this should be counted as a unique event (NOTE: Scheduled stops are the only case which are not)</param>
			private void AddScore(int scoreValue, ScoreTextToken textToken, double scoreDuration, bool countedEvent = true)
			{
				if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
				{
					int n = ScoreMessages.Length;
					Array.Resize(ref ScoreMessages, n + 1);
					ScoreMessages[n].Value = scoreValue;
					ScoreMessages[n].Text = Interface.GetScoreText(textToken) + ": " + scoreValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
					ScoreMessages[n].Timeout = Program.CurrentRoute.SecondsSinceMidnight + scoreDuration;
					ScoreMessages[n].RendererPosition = new Vector2(0.0, 0.0);
					ScoreMessages[n].RendererAlpha = 0.0;
					if (scoreValue < 0.0)
					{
						ScoreMessages[n].Color = MessageColor.Red;
					}
					else if (scoreValue > 0.0)
					{
						ScoreMessages[n].Color = MessageColor.Green;
					}
					else
					{
						ScoreMessages[n].Color = MessageColor.White;
					}
				}
				if (scoreValue != 0 & countedEvent)
				{
					if (ScoreLogCount == ScoreLogs.Length)
					{
						Array.Resize(ref ScoreLogs, ScoreLogs.Length << 1);
					}
					ScoreLogs[ScoreLogCount].Value = scoreValue;
					ScoreLogs[ScoreLogCount].TextToken = textToken;
					ScoreLogs[ScoreLogCount].Position = TrainManager.PlayerTrain.Cars[0].TrackPosition;
					ScoreLogs[ScoreLogCount].Time = Program.CurrentRoute.SecondsSinceMidnight;
					ScoreLogCount++;
				}
			}

			/// <summary>Is called by the update function to add a new score event to the log</summary>
			/// <param name="scoreText">The log text for this score event</param>
			/// <param name="scoreDuration">The duration of the score event (e.g. overspeed)</param>
			private void AddScore(string scoreText, double scoreDuration)
			{
				if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
				{
					int n = ScoreMessages.Length;
					Array.Resize(ref ScoreMessages, n + 1);
					ScoreMessages[n].Value = 0;
					ScoreMessages[n].Text = scoreText.Length != 0 ? scoreText : "══════════";
					ScoreMessages[n].Timeout = Program.CurrentRoute.SecondsSinceMidnight + scoreDuration;
					ScoreMessages[n].RendererPosition = new Vector2(0.0, 0.0);
					ScoreMessages[n].RendererAlpha = 0.0;
					ScoreMessages[n].Color = MessageColor.White;
				}
			}
		}

		/// <summary>Updates all score messages displayed by the renderer</summary>
		internal static void UpdateScoreMessages()
		{
			if (Interface.CurrentOptions.GameMode == GameMode.Arcade)
			{
				for (int i = 0; i < ScoreMessages.Length; i++)
				{
					if (Program.CurrentRoute.SecondsSinceMidnight >= ScoreMessages[i].Timeout & ScoreMessages[i].RendererAlpha == 0.0)
					{
						for (int j = i; j < ScoreMessages.Length - 1; j++)
						{
							ScoreMessages[j] = ScoreMessages[j + 1];
						}
						Array.Resize(ref ScoreMessages, ScoreMessages.Length - 1);
						i--;
					}
				}
			}
		}
	}
}
