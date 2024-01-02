using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager.Car;
using TrainManager.Trains;

namespace TrainManager.SafetySystems
{
	public class OverspeedDevice : AbstractSafetySystem
	{
		/// <summary>Holds a reference to the base train</summary>
		private readonly TrainBase baseTrain;
		/// <summary>Stores the previous route speed limit</summary>
		private double previousRouteLimit;
		/// <summary>Stores whether we are currently overspeed</summary>
		private bool currentlyOverspeed;
		/// <summary>The required speed for the overspeed device to release</summary>
		private readonly double RequiredSpeed;

		public OverspeedDevice(TrainBase train) : base (train.Cars[train.DriverCar], InterventionMode.None, false, 0, 0)
		{
			// called from the train.dat constructor- no intervention
			baseTrain = train;
		}

		public OverspeedDevice(CarBase car, InterventionMode mode, bool loopingAlarm, double interventionTime, double requiredStopTime, double requiredSpeed) : base(car, mode, loopingAlarm, interventionTime, requiredStopTime)
		{
			// called from XML
			RequiredSpeed = requiredSpeed;
		}

		public override void Update(double TimeElapsed)
		{
			if (baseTrain.CurrentSpeed > baseTrain.CurrentRouteLimit)
			{
				if (!currentlyOverspeed || previousRouteLimit != baseTrain.CurrentRouteLimit || TrainManagerBase.CurrentOptions.GameMode == GameMode.Arcade)
				{
					/*
					 * HACK: If the limit has changed, or we are in arcade mode, notify the player
					 *       This conforms to the original behaviour, but doesn't need to raise the message from the event.
					 */
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_route_overspeed"), MessageDependency.RouteLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
				}
				currentlyOverspeed = true;
				
			}
			else
			{
				currentlyOverspeed = false;
			}

			if (TrainManagerBase.CurrentOptions.Accessibility)
			{
				if (previousRouteLimit != baseTrain.CurrentRouteLimit)
				{
					//Show for 10s and announce the current speed limit if screen reader present
					TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString("message_route_newlimit"), MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, TrainManagerBase.currentHost.InGameTime + 10.0, null);
				}
			}

			previousRouteLimit = baseTrain.CurrentRouteLimit;

			// Deal with any interventions etc. after the original message code
			if (currentlyOverspeed)
			{
				if (Type != InterventionMode.None)
				{
					Timer += TimeElapsed;
					if (Timer > InterventionTime)
					{
						Triggered = true;
					}
				}
			}
			else
			{
				if (Triggered == false)
				{
					Timer = 0;
				}
			}

			if (Triggered)
			{
				switch (Type)
				{
					case InterventionMode.None:
						return;
					case InterventionMode.CutsPower:
						baseTrain.Handles.Power.ApplySafetyState(0);
						break;
					case InterventionMode.ApplyBrake:
						baseTrain.Handles.Brake.ApplySafetyState(baseTrain.Handles.Brake.MaximumNotch);
						break;
					case InterventionMode.ApplyEmergencyBrake:
						baseTrain.Handles.EmergencyBrake.Safety = true;
						break;
				}

				if (currentlyOverspeed == false)
				{
					if(baseTrain.CurrentSpeed)
				}
			}

		}
	}
}
