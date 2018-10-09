namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>A locomotive brake handle</summary>
		internal class LocoBrakeHandle : NotchedHandle
		{
			internal LocoBrakeHandle(int max, EmergencyHandle eb, double[] delayUp, double[] delayDown)
			{
				this.MaximumNotch = max;
				this.EmergencyBrake = eb;
				this.DelayUp = delayUp;
				this.DelayDown = delayDown;
			}

			/// <summary>Provides a reference to the associated EB handle</summary>
			private readonly EmergencyHandle EmergencyBrake;

			internal override void Update()
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
					if (DelayedChanges[0].Time <= Game.SecondsSinceMidnight)
					{
						Actual = DelayedChanges[0].Value;
						RemoveChanges(1);
					}
				}
			}
		}

		internal class LocoAirBrakeHandle : AbstractHandle
		{
			private AirBrakeHandleState DelayedValue;
			private double DelayedTime;

			internal LocoAirBrakeHandle()
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
