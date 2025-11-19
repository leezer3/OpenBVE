using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>SafetySystem providing the default overspeed message</summary>
	public class OverspeedMessage : AbstractSafetySystem
	{
		/// <summary>Stores the previous route speed limit</summary>
		private double previousRouteLimit;

		private bool currentlyOverspeed;

		public OverspeedMessage(CarBase car) : base(car)
		{
		}

		public void Update()
		{
			if (!baseCar.baseTrain.IsPlayerTrain)
			{
				return;
			}
			if (baseCar.CurrentSpeed > baseCar.baseTrain.CurrentRouteLimit)
			{
				if (!currentlyOverspeed || previousRouteLimit != baseCar.baseTrain.CurrentRouteLimit || TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
				{
					/*
					 * HACK: If the limit has changed, or we are in arcade mode, notify the player
					 *       This conforms to the original behaviour, but doesn't need to raise the message from the event.
					 */
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","route_overspeed"}), MessageDependency.RouteLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
				}
				currentlyOverspeed = true;
			}
			else
			{
				currentlyOverspeed = false;
			}

			if (TrainManagerBase.CurrentOptions.Accessibility)
			{
				if (previousRouteLimit != baseCar.baseTrain.CurrentRouteLimit)
				{
					//Show for 10s and announce the current speed limit if screen reader present
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","route_newlimit"}), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);
				}
			}

			previousRouteLimit = baseCar.baseTrain.CurrentRouteLimit;
		}
	}
}
