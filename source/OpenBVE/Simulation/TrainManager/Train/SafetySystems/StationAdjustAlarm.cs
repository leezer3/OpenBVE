using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using SoundManager;

namespace OpenBve.SafetySystems
{
	class StationAdjustAlarm
	{
		/// <summary>Holds the reference to the base train</summary>
		private readonly TrainManager.Train baseTrain;
		/// <summary>The alarm played when the driver should adjust the stop point</summary>
		internal CarSound AdjustAlarm;
		/// <summary>Whether the adjust alarm panel lamp is currently lit</summary>
		internal bool Lit;

		internal StationAdjustAlarm(TrainManager.Train train)
		{
			this.baseTrain = train;
			this.AdjustAlarm = new CarSound();
			this.Lit = false;
		}

		internal void Update(double tb, double tf)
		{
			if (baseTrain.CurrentSpeed > -0.277777777777778 & baseTrain.CurrentSpeed < 0.277777777777778)
			{
				// correct stop position
				if (!Lit & (baseTrain.StationDistanceToStopPoint > tb | baseTrain.StationDistanceToStopPoint < -tf))
				{
					SoundBuffer buffer = AdjustAlarm.Buffer;
					if (buffer != null)
					{
						OpenBveApi.Math.Vector3 pos = AdjustAlarm.Position;
						Program.Sounds.PlaySound(buffer, 1.0, 1.0, pos, baseTrain.Cars[baseTrain.DriverCar], false);
					}
					if (baseTrain.IsPlayerTrain)
					{
						Game.AddMessage(Translations.GetInterfaceString("message_station_correct"), MessageDependency.None, GameMode.Normal, MessageColor.Orange, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
					}
					Lit = true;
				}
			}
			else
			{
				Lit = false;
			}
		}
	}
}
