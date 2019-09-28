using SoundManager;

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
			/// <summary>The sound played when the emergency brake is applied</summary>
			internal CarSound ApplicationSound;
			/// <summary>The sound played when the emergency brake is released</summary>
			/*
			 * NOTE:	This sound is deliberately not initialised by default.
			 *			If uninitialised, the sim will fall back to the previous behaviour
			 *			of using the Brake release sound when EB is released.
			 */
			internal CarSound ReleaseSound;
			/// <summary>The behaviour of the other handles when the EB handle is activated</summary>
			internal EbHandleBehaviour OtherHandlesBehaviour = EbHandleBehaviour.NoAction;

			internal EmergencyHandle()
			{
				ApplicationSound = new CarSound();
			}

			internal void Update()
			{
				if (Safety & !Actual)
				{
					double t = Program.CurrentRoute.SecondsSinceMidnight;
					if (t < ApplicationTime) ApplicationTime = t;
					if (ApplicationTime <= Program.CurrentRoute.SecondsSinceMidnight)
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
