using System;
using OpenBveApi.Colors;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class TrackManager
	{
		/// <summary>Is called when the speed limit upon the track change</summary>
		internal class LimitChangeEvent : GeneralEvent
		{
			internal readonly double PreviousSpeedLimit;
			internal readonly double NextSpeedLimit;
			internal LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit)
			{
				this.TrackPositionDelta = TrackPositionDelta;
				this.DontTriggerAnymore = false;
				this.PreviousSpeedLimit = PreviousSpeedLimit;
				this.NextSpeedLimit = NextSpeedLimit;
			}
			internal override void Trigger(int Direction, EventTriggerType TriggerType, TrainManager.Train Train, int CarIndex)
			{
				if (Train == null)
				{
					return;
				}
				if (Train.RouteLimits == null)
				{
					Train.RouteLimits = new double[] {};
				}
				if (Direction < 0)
				{
					if (TriggerType == EventTriggerType.FrontCarFrontAxle)
					{
						int n = Train.RouteLimits.Length;
						if (n > 0)
						{
							Array.Resize<double>(ref Train.RouteLimits, n - 1);
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
						Array.Resize<double>(ref Train.RouteLimits, n + 1);
						for (int i = n; i > 0; i--)
						{
							Train.RouteLimits[i] = Train.RouteLimits[i - 1];
						}
						Train.RouteLimits[0] = this.PreviousSpeedLimit;
					}
				}
				else if (Direction > 0)
				{
					if (TriggerType == EventTriggerType.FrontCarFrontAxle)
					{
						int n = Train.RouteLimits.Length;
						Array.Resize<double>(ref Train.RouteLimits, n + 1);
						Train.RouteLimits[n] = this.NextSpeedLimit;
						if (this.NextSpeedLimit < Train.CurrentRouteLimit)
						{
							Train.CurrentRouteLimit = this.NextSpeedLimit;
						}
						if (Train.Specs.CurrentAverageSpeed > this.NextSpeedLimit)
						{
							Game.AddMessage(Translations.GetInterfaceString("message_route_overspeed"), MessageManager.MessageDependency.RouteLimit, Interface.GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
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
							Array.Resize<double>(ref Train.RouteLimits, n - 1);
						}
					}
				}
			}
		}
	}
}
