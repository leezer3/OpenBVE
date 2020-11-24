namespace TrainManager.Handles
{
	/// <summary>Represents an air-brake handle</summary>
	public class AirBrakeHandle : AbstractHandle
	{
		private AirBrakeHandleState DelayedValue;
		private double DelayedTime;

		public AirBrakeHandle()
		{
			this.MaximumNotch = 3;
			this.MaximumDriverNotch = 3;
		}

		public override void Update()
		{
			if (DelayedValue != AirBrakeHandleState.Invalid)
			{
				if (DelayedTime <= TrainManagerBase.currentHost.InGameTime)
				{
					Actual = (int)DelayedValue;
					DelayedValue = AirBrakeHandleState.Invalid;
				}
			}
			else
			{
				if (Safety == (int)AirBrakeHandleState.Release & Actual != (int)AirBrakeHandleState.Release)
				{
					DelayedValue = AirBrakeHandleState.Release;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int)AirBrakeHandleState.Service & Actual != (int)AirBrakeHandleState.Service)
				{
					DelayedValue = AirBrakeHandleState.Service;
					DelayedTime = TrainManagerBase.currentHost.InGameTime;
				}
				else if (Safety == (int)AirBrakeHandleState.Lap)
				{
					Actual = (int)AirBrakeHandleState.Lap;
				}
			}
		}
	}
}
