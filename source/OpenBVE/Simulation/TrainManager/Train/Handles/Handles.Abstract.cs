using System;

namespace OpenBve
{
	/*
	 * The abstract handle types
	 *
	 * A cab handle should implement one of these
	 */
	public static partial class TrainManager
	{
		/// <summary>Represents an abstract handle with a set number of notches</summary>
		internal abstract class NotchedHandle
		{
			/// <summary>The maximum notch this handle can be increased to</summary>
			internal int MaximumNotch;
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal int Safety;
			/// <summary>The actual notch</summary>
			internal int Actual;
			/// <summary>The number of steps this handle must be reduced by for a change to take effect</summary>
			internal int ReduceSteps;
			/// <summary>The delayed handle state changes</summary>
			internal HandleChange[] DelayedChanges;
			/// <summary>The list of delay values for each notch increase</summary>
			internal double[] DelayUp;
			/// <summary>The list of delay values for each notch decrease</summary>
			internal double[] DelayDown;
			/// <summary>Adds a delayed handle state change</summary>
			/// <param name="Value">The value to add or subtract</param>
			/// <param name="Delay">The delay in seconds</param>
			internal void AddChange(int Value, double Delay)
			{
				int n = DelayedChanges.Length;
				Array.Resize<HandleChange>(ref DelayedChanges, n + 1);
				DelayedChanges[n].Value = Value;
				DelayedChanges[n].Time = Game.SecondsSinceMidnight + Delay;
			}
			/// <summary>Removes a specified number of delayed changes</summary>
			/// <param name="Count">The number of changes to remove</param>
			internal void RemoveChanges(int Count)
			{
				int n = DelayedChanges.Length;
				for (int i = 0; i < n - Count; i++)
				{
					DelayedChanges[i] = DelayedChanges[i + Count];
				}
				Array.Resize<HandleChange>(ref DelayedChanges, n - Count);
			}

			internal abstract void Update();

			/// <summary>Gets the delay value for this handle</summary>
			/// <param name="Increase">Whether this is an increase or a decrease</param>
			/// <returns>The delay value to apply</returns>
			internal double GetDelay(bool Increase)
			{
				if (Increase)
				{
					if (DelayUp == null || DelayUp.Length == 0)
					{
						return 0.0;
					}
					if (Actual < DelayUp.Length)
					{
						return DelayUp[Actual];
					}

					return DelayUp[DelayUp.Length - 1];
				}

				if (DelayDown == null || DelayDown.Length == 0)
				{
					return 0.0;
				}
				if (Actual < DelayDown.Length)
				{
					return DelayDown[Actual];
				}

				return DelayUp[DelayUp.Length - 1];
			}
		}
	}
}
