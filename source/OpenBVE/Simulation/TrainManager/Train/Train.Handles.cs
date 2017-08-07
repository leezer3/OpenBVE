using System;

namespace OpenBve
{
	public static partial class TrainManager
	{
		internal struct HandleChange
		{
			internal int Value;
			internal double Time;
		}

		/// <summary>Represents a power handle</summary>
		internal class PowerHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal int Safety;
			/// <summary>The actual notch</summary>
			internal int Actual;

			internal double DelayPowerUp;

			internal double DelayPowerDown;

			internal int PowerNotchReduceSteps;
			/// <summary>The delayed handle state changes</summary>
			internal HandleChange[] DelayedChanges;
			/// <summary>Adds a delayed handle state change</summary>
			/// <param name="Train">The train to add the delayed state change to</param>
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

			internal void Update()
			{
				if (DelayedChanges.Length == 0)
				{
					if (Safety < Actual)
					{
						if (PowerNotchReduceSteps <= 1)
						{
							AddChange(Actual - 1, DelayPowerDown);
						}
						else if (Safety + PowerNotchReduceSteps <= Actual | Safety == 0)
						{
							AddChange(Safety, DelayPowerDown);
						}
					}
					else if (Safety > Actual)
					{
						AddChange(Actual + 1, DelayPowerUp);
					}
				}
				else
				{
					int m = DelayedChanges.Length - 1;
					if (Safety < DelayedChanges[m].Value)
					{
						AddChange(Safety, DelayPowerDown);
					}
					else if (Safety > DelayedChanges[m].Value)
					{
						AddChange(Safety, DelayPowerUp);
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
		/// <summary>Represents a brake handle</summary>
		internal class BrakeHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal int Safety;
			/// <summary>The actual notch</summary>
			internal int Actual;
			/// <summary>The delayed handle state changes</summary>
			internal HandleChange[] DelayedChanges;

			internal double DelayBrakeUp;

			internal double DelayBrakeDown;

			internal int MaximumBrakeNotch;

			private Train Train;
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

			internal void Update()
			{
				int sec = Train.EmergencyBrake.SafetySystemApplied ? MaximumBrakeNotch : Safety;
				if (DelayedChanges.Length == 0)
				{
					if (sec < Actual)
					{
						AddChange(Actual - 1, DelayBrakeDown);
					}
					else if (sec > Actual)
					{
						AddChange(Actual + 1, DelayBrakeUp);
					}
				}
				else
				{
					int m = DelayedChanges.Length - 1;
					if (sec < DelayedChanges[m].Value)
					{
						AddChange(sec, DelayBrakeDown);
					}
					else if (sec > DelayedChanges[m].Value)
					{
						AddChange(sec, DelayBrakeUp);
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

			internal BrakeHandle(Train train)
			{
				//A reference to the base train is required to check EB status
				this.Train = train;
			}
		}
		/// <summary>Represnts a reverser handle</summary>
		internal struct ReverserHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The actual notch</summary>
			internal int Actual;
		}
		/// <summary>Represents a hold-brake handle</summary>
		internal struct HoldBrakeHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal bool Driver;
			/// <summary>The actual notch</summary>
			internal bool Actual;
		}

		/// <summary>Represents an air-brake handle</summary>
		internal struct AirBrakeHandle
		{
			/// <summary>The position set by the driver</summary>
			internal AirBrakeHandleState Driver;
			/// <summary>The position set by the safety system (Train plugin)</summary>
			internal AirBrakeHandleState Safety;
			/// <summary>The actual position</summary>
			internal AirBrakeHandleState Actual;
			internal AirBrakeHandleState DelayedValue;
			internal double DelayedTime;

			internal void Update()
			{
				// air brake handle
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

		/// <summary>Possible states of an air-brake handle</summary>
		internal enum AirBrakeHandleState
		{
			Invalid = -1,
			Release = 0,
			Lap = 1,
			Service = 2,
		}
	}
}
