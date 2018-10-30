namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>A brake handle</summary>
		internal class BrakeHandle : NotchedHandle
		{
			internal BrakeHandle(int max, int driverMax, EmergencyHandle eb, double[] delayUp, double[] delayDown)
			{
				this.MaximumNotch = max;
				this.MaximumDriverNotch = driverMax;
				this.EmergencyBrake = eb;
				this.DelayUp = delayUp;
				this.DelayDown = delayDown;
				this.DelayedChanges = new HandleChange[] { };
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
	}
}
