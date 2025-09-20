using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
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
					AdjustAlarm.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					if (baseTrain.IsPlayerTrain)
					{
						TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve,  new [] {"message","station_correct"}), MessageDependency.None, GameMode.Normal, MessageColor.Orange, 5.0, null);
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
