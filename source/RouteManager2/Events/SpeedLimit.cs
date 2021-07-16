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

		public LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit) : base(TrackPositionDelta)
		{
			DontTriggerAnymore = false;
			this.PreviousSpeedLimit = PreviousSpeedLimit;
			this.NextSpeedLimit = NextSpeedLimit;
		}

		public override void Trigger(int direction, TrackFollower trackFollower)
		{
			AbstractTrain train = trackFollower.Train;
			EventTriggerType triggerType = trackFollower.TriggerType;

			if (train == null)
			{
				return;
			}

			if (train.RouteLimits == null)
			{
				train.RouteLimits = new double[] { };
			}

			if (direction < 0)
			{
				if (triggerType == EventTriggerType.FrontCarFrontAxle)
				{
					int n = train.RouteLimits.Length;
					if (n > 0)
					{
						Array.Resize(ref train.RouteLimits, n - 1);
						train.CurrentRouteLimit = double.PositiveInfinity;
						for (int i = 0; i < n - 1; i++)
						{
							if (train.RouteLimits[i] < train.CurrentRouteLimit)
							{
								train.CurrentRouteLimit = train.RouteLimits[i];
							}
						}
					}
				}
				else if (triggerType == EventTriggerType.RearCarRearAxle)
				{
					int n = train.RouteLimits.Length;
					Array.Resize(ref train.RouteLimits, n + 1);
					for (int i = n; i > 0; i--)
					{
						train.RouteLimits[i] = train.RouteLimits[i - 1];
					}

					train.RouteLimits[0] = PreviousSpeedLimit;
				}
			}
			else if (direction > 0)
			{
				if (triggerType == EventTriggerType.FrontCarFrontAxle)
				{
					int n = train.RouteLimits.Length;
					Array.Resize(ref train.RouteLimits, n + 1);
					train.RouteLimits[n] = NextSpeedLimit;
					if (NextSpeedLimit < train.CurrentRouteLimit)
					{
						train.CurrentRouteLimit = NextSpeedLimit;
					}
				}
				else if (triggerType == EventTriggerType.RearCarRearAxle)
				{
					int n = train.RouteLimits.Length;
					if (n > 0)
					{
						train.CurrentRouteLimit = double.PositiveInfinity;
						for (int i = 0; i < n - 1; i++)
						{
							train.RouteLimits[i] = train.RouteLimits[i + 1];
							if (train.RouteLimits[i] < train.CurrentRouteLimit)
							{
								train.CurrentRouteLimit = train.RouteLimits[i];
							}
						}
						Array.Resize(ref train.RouteLimits, n - 1);
					}
				}
			}
		}
	}
}
