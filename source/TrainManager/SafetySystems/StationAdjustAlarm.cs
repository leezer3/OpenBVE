using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager.MessageTypes;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.SafetySystems
{
	public class StationAdjustAlarm
	{
		/// <summary>Holds the reference to the base train</summary>
		private readonly TrainBase baseTrain;
		/// <summary>The alarm played when the driver should adjust the stop point</summary>
		public CarSound AdjustAlarm;
		/// <summary>Whether the adjust alarm panel lamp is currently lit</summary>
		public bool Lit;

		public StationAdjustAlarm(TrainBase train)
		{
			this.baseTrain = train;
			this.AdjustAlarm = new CarSound();
			this.Lit = false;
		}

		public void Update(double tb, double tf)
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
						TrainManagerBase.currentHost.PlaySound(buffer, 1.0, 1.0, pos, baseTrain.Cars[baseTrain.DriverCar], false);
					}
					if (baseTrain.IsPlayerTrain)
					{
						GeneralMessage message = new GeneralMessage
						{
							MessageOnTimeText = Translations.GetInterfaceString("message_station_correct"),
							Timeout = 5.0,
							MessageColor = MessageColor.Orange,
							Mode = GameMode.Normal
						};
						TrainManagerBase.currentHost.AddMessage(message);
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
