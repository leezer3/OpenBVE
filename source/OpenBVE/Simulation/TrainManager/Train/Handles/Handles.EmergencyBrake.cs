namespace OpenBve
{
	public static partial class TrainManager
	{
		/// <summary>Represents an emergency brake handle</summary>
		internal class EmergencyHandle
		{
			internal bool Safety;
			internal bool Actual;
			internal bool Driver;
			internal double ApplicationTime;
			/// <summary>The behaviour of the other handles when the EB handle is activated</summary>
			internal EbHandleBehaviour OtherHandlesBehaviour = EbHandleBehaviour.NoAction;

			internal void Update()
			{
				if (Safety & !Actual)
				{
					double t = Game.SecondsSinceMidnight;
					if (t < ApplicationTime) ApplicationTime = t;
					if (ApplicationTime <= Game.SecondsSinceMidnight)
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
		}
	}
}
