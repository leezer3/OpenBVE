namespace TrainManager.Handles
{
	/// <summary>A power handle</summary>
	public class PowerHandle : NotchedHandle
	{
		public PowerHandle(int max, int driverMax, double[] delayUp, double[] delayDown)
		{
			this.MaximumNotch = max;
			this.MaximumDriverNotch = driverMax;
			this.DelayUp = delayUp;
			this.DelayDown = delayDown;
		}

		public override void Update()
		{
			if (DelayedChanges.Length == 0)
			{
				if (Safety < Actual)
				{
					if (ReduceSteps <= 1)
					{
						AddChange(Actual - 1, GetDelay(false));
					}
					else if (Safety + ReduceSteps <= Actual | Safety == 0)
					{
						AddChange(Safety, GetDelay(false));
					}
				}
				else if (Safety > Actual)
				{
					AddChange(Actual + 1, GetDelay(true));
				}
			}
			else
			{
				int m = DelayedChanges.Length - 1;
				if (Safety < DelayedChanges[m].Value)
				{
					AddChange(Safety, GetDelay(false));
				}
				else if (Safety > DelayedChanges[m].Value)
				{
					AddChange(Safety, GetDelay(true));
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
}
