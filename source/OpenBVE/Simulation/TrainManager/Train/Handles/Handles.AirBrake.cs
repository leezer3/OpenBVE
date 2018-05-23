namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents an air-brake handle</summary>
		internal class AirBrakeHandle
		{
			/// <summary>The position set by the driver</summary>
			internal AirBrakeHandleState Driver;
			/// <summary>The position set by the safety system (Train plugin)</summary>
			internal AirBrakeHandleState Safety;
			/// <summary>The actual position</summary>
			internal AirBrakeHandleState Actual;
			private AirBrakeHandleState DelayedValue;
			private double DelayedTime;

			internal void Update()
			{
				if (DelayedValue != AirBrakeHandleState.Invalid)
				{
					if (DelayedTime <= Game.SecondsSinceMidnight)
					{
						Actual = DelayedValue;
						DelayedValue = AirBrakeHandleState.Invalid;
					}
				}
				else
				{
					if (Safety == AirBrakeHandleState.Release & Actual != AirBrakeHandleState.Release)
					{
						DelayedValue = AirBrakeHandleState.Release;
						DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Safety == AirBrakeHandleState.Service & Actual != AirBrakeHandleState.Service)
					{
						DelayedValue = AirBrakeHandleState.Service;
						DelayedTime = Game.SecondsSinceMidnight;
					}
					else if (Safety == AirBrakeHandleState.Lap)
					{
						Actual = AirBrakeHandleState.Lap;
					}
				}
			}
		}
    }
}
