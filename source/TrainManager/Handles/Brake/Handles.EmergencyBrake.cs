using System;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents an emergency brake handle</summary>
	public class EmergencyHandle
	{
		public bool Safety;
		public bool Actual;
		public bool Driver;
		internal double ApplicationTime;
		/// <summary>The sound played when the emergency brake is applied</summary>
		public CarSound ApplicationSound;
		/// <summary>The sound played when the emergency brake is released</summary>
		/*
		 * NOTE:	This sound is deliberately not initialised by default.
		 *			If uninitialised, the sim will fall back to the previous behaviour
		 *			of using the Brake release sound when EB is released.
		 */
		public CarSound ReleaseSound;
		/// <summary>The behaviour of the other handles when the EB handle is activated</summary>
		public EbHandleBehaviour OtherHandlesBehaviour = EbHandleBehaviour.NoAction;

		private readonly TrainBase baseTrain;

		public EmergencyHandle(TrainBase train)
		{
			ApplicationSound = new CarSound();
			ApplicationTime = double.MaxValue;
			baseTrain = train;
		}

		public void Update()
		{
			if (Safety & !Actual)
			{
				if (TrainManagerBase.currentHost.InGameTime < ApplicationTime) ApplicationTime = TrainManagerBase.currentHost.InGameTime;
				if (ApplicationTime <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = true;
					ApplicationTime = double.MaxValue;
				}
			}
			else if (!Safety)
			{
				Actual = false;
			}
		}

		public void Apply()
		{
			// sound
			if (!Driver)
			{
				baseTrain.Handles.Brake.Max.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				ApplicationSound.Play(baseTrain.Cars[baseTrain.DriverCar], false);
			}

			// apply
			baseTrain.Handles.Brake.ApplyState(baseTrain.Handles.Brake.MaximumNotch, true);
			baseTrain.Handles.Power.ApplyState(0, baseTrain.Handles.HandleType != HandleType.SingleHandle);
			baseTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
			Driver = true;
			baseTrain.Handles.HoldBrake.Driver = false;
			baseTrain.Specs.CurrentConstSpeed = false;
			if (Driver)
			{
				switch (OtherHandlesBehaviour)
				{
					case EbHandleBehaviour.PowerNeutral:
						if (baseTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							baseTrain.Handles.Power.ApplyState(0, false);
						}
						break;
					case EbHandleBehaviour.ReverserNeutral:
						baseTrain.Handles.Reverser.ApplyState(ReverserPosition.Neutral);
						break;
					case EbHandleBehaviour.PowerReverserNeutral:
						if (baseTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							baseTrain.Handles.Power.ApplyState(0, false);
						}
						baseTrain.Handles.Reverser.ApplyState(ReverserPosition.Neutral);
						break;
				}
			}

			// plugin
			if (baseTrain.Plugin != null)
			{
				baseTrain.Plugin.UpdatePower();
				baseTrain.Plugin.UpdateBrake();
			}
			
			if (!TrainManagerBase.CurrentOptions.Accessibility) return;
			if (Driver)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.QuickReferences.HandleEmergency, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, 10.0, null);	
			}
		}

		public void Release()
		{
			if (Driver)
			{
				// sound
				if (ReleaseSound != null)
				{
					ReleaseSound.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
				else
				{
					baseTrain.Handles.Brake.Decrease.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
				// apply
					
				if (baseTrain.Handles.Brake is AirBrakeHandle)
				{
					baseTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
				}
				else
				{
					baseTrain.Handles.Brake.ApplyState(baseTrain.Handles.Brake.MaximumNotch, true);
					baseTrain.Handles.Power.ApplyState(0, baseTrain.Handles.HandleType != HandleType.SingleHandle);
				}
				Driver = false;
				// plugin
				if (baseTrain.Plugin == null) return;
				baseTrain.Plugin.UpdatePower();
				baseTrain.Plugin.UpdateBrake();
			}
		}
	}
}
