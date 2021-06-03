using System.Reflection;
using OpenBveApi.Runtime;
using TrainManager.Car;

namespace TrainManager.SafetySystems
{
	/// <summary>An AI to control the legacy Win32 UKSpt plugin</summary>
	internal class UKSptAI : PluginAI
	{
		/// <summary>Control variable used to determine next AI step</summary>
		private int currentStep;
		/// <summary>Variable controlling whether the door startup hack has been performed</summary>
		private bool doorStart;

		internal UKSptAI(Plugin plugin)
		{
			Plugin = plugin;
			currentStep = 0;
			nextPluginAction = 0;
		}

		internal override void Perform(AIData data)
		{
			if (Plugin.Panel[5] == 2)
			{
				//Engine is currently not running, so run startup sequence
				switch (currentStep)
				{
					case 0:
						data.Handles.Reverser = 1;
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 1:
						data.Handles.Reverser = 0;
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 2:
						if (Plugin.Sound[2] == 0)
						{
							data.Response = AIResponse.Medium;
							currentStep++;
						}

						return;
					case 3:
						Plugin.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 4:
						Plugin.KeyUp(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 5:
						Plugin.KeyDown(VirtualKeys.D);
						data.Response = AIResponse.Long;
						currentStep++;
						return;
					case 6:
						currentStep++;
						return;
					case 7:
						Plugin.KeyUp(VirtualKeys.D);
						data.Response = AIResponse.Medium;
						currentStep = 100;
						return;
				}
			}
			else if (Plugin.Panel[5] == 3)
			{
				//Engine either stalled on start, or has been stopped by user
				currentStep = 5;
			}

			if (!doorStart)
			{
				TrainDoorState state = Plugin.Train.GetDoorsState(true, true);
				if (Plugin.Train.GetDoorsState(true, true) == (TrainDoorState.Closed | TrainDoorState.AllClosed) && Plugin.Train.CurrentSpeed == 0)
				{
					/*
					 * HACK: Work around the fact that the guard AI isn't designed to start somewhere with no open doors
					 */
					data.Response = AIResponse.Medium;
					Plugin.DoorChange(DoorStates.None, DoorStates.Left);
					Plugin.DoorChange(DoorStates.Left, DoorStates.None);
					doorStart = true;
					return;
				}
			}

			if (Plugin.Panel[13] == 1)
			{
				//DRA is enabled, so toggle off
				Plugin.KeyDown(VirtualKeys.S);
				data.Response = AIResponse.Medium;
				currentStep = 99;
				return;
			}

			if (currentStep == 99)
			{
				Plugin.KeyUp(VirtualKeys.S);
				data.Response = AIResponse.Medium;
				currentStep++;
				return;
			}

			if (Plugin.Sound[2] == 0)
			{
				//AWS horn active, so wait a sec before triggering cancel
				switch (currentStep)
				{
					case 100:
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
					case 101:
						Plugin.KeyDown(VirtualKeys.A1);
						data.Response = AIResponse.Medium;
						currentStep++;
						return;
				}
			}

			if (currentStep == 101)
			{
				//Raise the AWS horn cancel key
				Plugin.KeyUp(VirtualKeys.A1);
				data.Response = AIResponse.Medium;
				currentStep = 100;
				return;
			}

			if (TrainManagerBase.currentHost.InGameTime > nextPluginAction)
			{
				//If nothing else has happened recently, hit the AWS reset key to ensure vigilance doesn't trigger
				Plugin.KeyDown(VirtualKeys.A1);
				Plugin.KeyUp(VirtualKeys.A1);
				data.Response = AIResponse.Short;
				nextPluginAction = TrainManagerBase.currentHost.InGameTime + 20.0;
				return;
			}

		}
	}
}
