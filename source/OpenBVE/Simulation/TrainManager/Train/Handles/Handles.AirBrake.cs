namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents an air-brake handle</summary>
		internal class AirBrakeHandle : AbstractHandle
		{
			private AirBrakeHandleState DelayedValue;
			private double DelayedTime;

			internal AirBrakeHandle()
			{
				this.MaximumNotch = 3;
			}

			internal override void Update()
			{
				if (DelayedValue != AirBrakeHandleState.Invalid)
				{
					if (DelayedTime <= Game.SecondsSinceMidnight)
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
						DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Safety == (int)AirBrakeHandleState.Service & Actual != (int)AirBrakeHandleState.Service)
					{
						DelayedValue = AirBrakeHandleState.Service;
						DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Safety == (int)AirBrakeHandleState.Lap)
					{
						Actual = (int)AirBrakeHandleState.Lap;
					}
				}
			}
		}
    }
}
