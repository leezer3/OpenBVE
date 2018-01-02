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
		internal struct PowerHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal int Safety;
			/// <summary>The actual notch</summary>
			internal int Actual;
			/// <summary>The delayed handle state changes</summary>
			internal HandleChange[] DelayedChanges;
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
		}
		/// <summary>Represents a brake handle</summary>
		internal struct BrakeHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal int Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal int Safety;
			/// <summary>The actual notch</summary>
			internal int Actual;
			/// <summary>The delayed handle state changes</summary>
			internal HandleChange[] DelayedChanges;
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
		}
		/// <summary>Represents an emergency brake handle</summary>
		internal struct EmergencyHandle
		{
			/// <summary>The notch set by the driver</summary>
			internal bool Driver;
			/// <summary>The notch set by the safety system (Train plugin)</summary>
			internal bool Safety;
			/// <summary>The actual notch</summary>
			internal bool Actual;
			internal double ApplicationTime;
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
