namespace TrainManager.Handles
{
	/// <summary>A locomotive brake handle</summary>
	public class LocoBrakeHandle : NotchedHandle
	{
		public LocoBrakeHandle(int max, EmergencyHandle eb, double[] delayUp, double[] delayDown)
		{
			this.MaximumNotch = max;
			this.EmergencyBrake = eb;
			this.DelayUp = delayUp;
			this.DelayDown = delayDown;
		}

		/// <summary>Provides a reference to the associated EB handle</summary>
		private readonly EmergencyHandle EmergencyBrake;

		public override void Update()
		{
			int sec = EmergencyBrake.Safety ? MaximumNotch : Safety;
			if (DelayedChanges.Length == 0)
			{
				if (sec < Actual)
				{
					AddChange(Actual - 1, GetDelay(false));
				}
				else if (sec > Actual)
				{
					AddChange(Actual + 1, GetDelay(true));
				}
			}
			else
			{
				int m = DelayedChanges.Length - 1;
				if (sec < DelayedChanges[m].Value)
				{
					AddChange(sec, GetDelay(false));
				}
				else if (sec > DelayedChanges[m].Value)
				{
					AddChange(sec, GetDelay(true));
				}
			}

			if (DelayedChanges.Length >= 1)
			{
				if (DelayedChanges[0].Time <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = DelayedChanges[0].Value;
					RemoveChanges(1);
				}
			}
		}
	}

	/// <summary>A locomotive air brake handle</summary>
	public class LocoAirBrakeHandle : AbstractHandle
	{
		private AirBrakeHandleState DelayedValue;
		private double DelayedTime;

		public LocoAirBrakeHandle()
		{
			this.MaximumNotch = 3;
		}

		public override void Update()
		{
			if (DelayedValue != AirBrakeHandleState.Invalid)
			{
				if (DelayedTime <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = (int) DelayedValue;
					DelayedValue = AirBrakeHandleState.Invalid;
				}
			}
			else
			{
				if (Safety == (int) AirBrakeHandleState.Release & Actual != (int) AirBrakeHandleState.Release)
				{
					DelayedValue = AirBrakeHandleState.Release;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int) AirBrakeHandleState.Service & Actual != (int) AirBrakeHandleState.Service)
				{
					DelayedValue = AirBrakeHandleState.Service;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int) AirBrakeHandleState.Lap)
				{
					Actual = (int) AirBrakeHandleState.Lap;
				}
			}
		}
	}
}
