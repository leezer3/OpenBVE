using System;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace RouteManager2.Events
{
	/// <summary>Is called when the speed limit upon the track change</summary>
	public class LimitChangeEvent : GeneralEvent
	{
		public readonly double PreviousSpeedLimit;
		public readonly double NextSpeedLimit;

		public LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit)
		{
			this.TrackPositionDelta = TrackPositionDelta;
			DontTriggerAnymore = false;
			this.PreviousSpeedLimit = PreviousSpeedLimit;
			this.NextSpeedLimit = NextSpeedLimit;
		}

		public override void Trigger(int Direction, EventTriggerType TriggerType, AbstractTrain Train, AbstractCar Car)
		{
			if (Train == null)
			{
				return;
			}

			if (Train.RouteLimits == null)
			{
				Train.RouteLimits = new double[] { };
			}

			if (Direction < 0)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					int n = Train.RouteLimits.Length;
					if (n > 0)
					{
						Array.Resize(ref Train.RouteLimits, n - 1);
						Train.CurrentRouteLimit = double.PositiveInfinity;
						for (int i = 0; i < n - 1; i++)
						{
							if (Train.RouteLimits[i] < Train.CurrentRouteLimit)
							{
								Train.CurrentRouteLimit = Train.RouteLimits[i];
							}
						}
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle)
				{
					int n = Train.RouteLimits.Length;
					Array.Resize(ref Train.RouteLimits, n + 1);
					for (int i = n; i > 0; i--)
					{
						Train.RouteLimits[i] = Train.RouteLimits[i - 1];
					}

					Train.RouteLimits[0] = PreviousSpeedLimit;
				}
			}
			else if (Direction > 0)
			{
				if (TriggerType == EventTriggerType.FrontCarFrontAxle)
				{
					int n = Train.RouteLimits.Length;
					Array.Resize(ref Train.RouteLimits, n + 1);
					Train.RouteLimits[n] = NextSpeedLimit;
					if (NextSpeedLimit < Train.CurrentRouteLimit)
					{
						Train.CurrentRouteLimit = NextSpeedLimit;
					}
				}
				else if (TriggerType == EventTriggerType.RearCarRearAxle)
				{
					int n = Train.RouteLimits.Length;
					if (n > 0)
					{
						Train.CurrentRouteLimit = double.PositiveInfinity;
						for (int i = 0; i < n - 1; i++)
						{
							Train.RouteLimits[i] = Train.RouteLimits[i + 1];
							if (Train.RouteLimits[i] < Train.CurrentRouteLimit)
							{
								Train.CurrentRouteLimit = Train.RouteLimits[i];
							}
						}
						Array.Resize(ref Train.RouteLimits, n - 1);
					}
				}
			}
		}
	}
}
