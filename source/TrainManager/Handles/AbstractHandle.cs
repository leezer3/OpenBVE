using System;
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>The basic abstract handle</summary>
	public abstract class AbstractHandle
	{
		/// <summary>The notch set by the driver</summary>
		public int Driver;

		/// <summary>The notch set by the safety sytem</summary>
		public int Safety;

		/// <summary>The actual notch, as used by the physics system etc.</summary>
		public int Actual;

		/// <summary>The maximum notch this handle may be advanced to</summary>
		public int MaximumNotch;

		/// <summary>The maximum notch the driver may advance this handle to</summary>
		public int MaximumDriverNotch;

		public HandleChange[] DelayedChanges;

		/// <summary>Whether the current handle motion is a continuous motion</summary>
		public bool ContinuousMovement;

		/// <summary>The sound played when the handle position is increased</summary>
		public CarSound Increase;

		/// <summary>The sound played when the handle position is increased in a fast motion</summary>
		public CarSound IncreaseFast;

		/// <summary>The sound played when the handle position is decreased</summary>
		public CarSound Decrease;

		/// <summary>The sound played when the handle position is decreased in a fast motion</summary>
		public CarSound DecreaseFast;

		/// <summary>The sound played when the handle is moved to the minimum position</summary>
		public CarSound Min;

		/// <summary>The sound played when the handles is moved to the maximum position</summary>
		public CarSound Max;

		/// <summary>Contains the notch descriptions to be displayed on the in-game UI</summary>
		public string[] NotchDescriptions;

		/// <summary>The max width used in px for the description string</summary>
		public double MaxWidth = 48;

		internal readonly TrainBase baseTrain;

		public abstract void Update();

		public virtual void ApplyState(int newState, bool relativeChange, bool isOverMaxDriverNotch = false)
		{

		}

		public virtual void ApplyState(AirBrakeHandleState newState)
		{

		}

		protected AbstractHandle(TrainBase Train)
		{
			baseTrain = Train;
			Increase = new CarSound();
			IncreaseFast = new CarSound();
			Decrease = new CarSound();
			DecreaseFast = new CarSound();
			Min = new CarSound();
			Max = new CarSound();
		}
	}

	/// <summary>Represents an abstract handle with a set number of notches</summary>
	public abstract class NotchedHandle : AbstractHandle
	{

		/// <summary>The number of steps this handle must be reduced by for a change to take effect</summary>
		public int ReduceSteps;

		/// <summary>The list of delay values for each notch increase</summary>
		internal double[] DelayUp;

		/// <summary>The list of delay values for each notch decrease</summary>
		internal double[] DelayDown;

		internal NotchedHandle(TrainBase train) : base(train)
		{

		}

		/// <summary>Adds a delayed handle state change</summary>
		/// <param name="Value">The value to add or subtract</param>
		/// <param name="Delay">The delay in seconds</param>
		internal void AddChange(int Value, double Delay)
		{
			int n = DelayedChanges.Length;
			Array.Resize(ref DelayedChanges, n + 1);
			DelayedChanges[n].Value = Value;
			DelayedChanges[n].Time = TrainManagerBase.currentHost.InGameTime + Delay;
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

			Array.Resize(ref DelayedChanges, n - Count);
		}


		/// <summary>Gets the delay value for this handle</summary>
		/// <param name="ShouldIncrease">Whether this is an increase or a decrease</param>
		/// <returns>The delay value to apply</returns>
		internal double GetDelay(bool ShouldIncrease)
		{
			if (ShouldIncrease)
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

			return DelayDown[DelayDown.Length - 1];
		}

	}
}
