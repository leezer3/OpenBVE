using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train
		{
			/// <inheritdoc/>
			public override void EnterStation(int stationIndex, int direction)
			{
				if (direction < 0)
				{
					if (Handles.Reverser.Actual == ReverserPosition.Forwards && Handles.Power.Driver != 0 && !Game.MinimalisticSimulation && stationIndex == Station)
					{
						//Our reverser and power are in F, but we are rolling backwards
						//Leave the station index alone, and we won't trigger again when we actually move forwards
						return;
					}
					Station = -1;
				}
				else if (direction > 0)
				{
					if (Station == stationIndex || NextStopSkipped != StopSkipMode.None)
					{
						return;
					}
					Station = stationIndex;
					if (StationState != TrainStopState.Jumping)
					{
						StationState = TrainStopState.Pending;
					}
					LastStation = stationIndex;
				}
			}

			/// <inheritdoc/>
			public override void LeaveStation(int stationIndex, int direction)
			{
				if (direction < 0)
				{
					Station = stationIndex;
					if (NextStopSkipped != StopSkipMode.None)
					{
						LastStation = stationIndex;
					}
					NextStopSkipped = StopSkipMode.None;
				}
				else if (direction > 0)
				{
					if (Station == stationIndex)
					{
						if (this.IsPlayerTrain)
						{
							if (Program.CurrentRoute.Stations[stationIndex].PlayerStops() & TrainManager.PlayerTrain.StationState == TrainStopState.Pending)
							{
								string s = Translations.GetInterfaceString("message_station_passed");
								s = s.Replace("[name]", Program.CurrentRoute.Stations[stationIndex].Name);
								Game.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Orange, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
							}
							else if (Program.CurrentRoute.Stations[stationIndex].PlayerStops() & TrainManager.PlayerTrain.StationState == TrainStopState.Boarding)
							{
								string s = Translations.GetInterfaceString("message_station_passed_boarding");
								s = s.Replace("[name]", Program.CurrentRoute.Stations[stationIndex].Name);
								Game.AddMessage(s, MessageDependency.None, GameMode.Normal, MessageColor.Red, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
							}
						}
						Station = -1;
						if (StationState != TrainStopState.Jumping)
						{
							StationState = TrainStopState.Pending;
						}

						SafetySystems.PassAlarm.Halt();
					}
				}
			}
		}
	}
}
