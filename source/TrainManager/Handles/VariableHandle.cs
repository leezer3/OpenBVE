using System;
using OpenBveApi.Colors;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	public class VariableHandle : AbstractHandle
	{
		public VariableHandle(TrainBase train, Tuple<double, string>[] notches = null) : base(train)
		{
			if (notches?.Length > 0)
			{
				MaximumDriverNotch = notches.Length - 1;
				MaximumNotch = notches.Length - 1;
				_notches = notches;
			}
			else
			{
				MaximumDriverNotch = 100;
				MaximumNotch = 100;
			}
			
		}

		private readonly Tuple<double, string>[] _notches;

		private double timer;

		private bool isIncreasing;

		
		public override void Update(double timeElapsed)
		{
			if (_notches != null)
			{
				return;
			}
			timer += timeElapsed;
			if (ContinuousMovement && timer > 0.1)
			{
				if (isIncreasing)
				{
					Actual++;
				}
				else
				{
					Actual--;
				}

				Actual = Math.Min(100, Math.Max(0, Actual));
				timer = 0;
			}
		}

		public override void ApplySafetyState(int newState)
		{
		}

		public override void ApplyState(int newState, bool relativeChange, bool isOverMaxDriverNotch = false)
		{
			if (_notches != null)
			{
				if (relativeChange)
				{
					Actual += newState;
				}
				else
				{
					Actual = newState;
				}
			}
			else
			{
				if (relativeChange)
				{
					isIncreasing = newState > 0;
				}
			}
			

			Actual = Math.Min(Actual, MaximumNotch);
			Driver = Actual;
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = Actual == 0 ? MessageColor.Gray : MessageColor.Blue;

			if (_notches == null)
			{
				if (Actual == 0)
				{
					return "N";
				}
				return "P" + Actual + "%";
			}

			return _notches[Actual].Item2;
		}

		public double GetPowerModifier
		{
			get
			{
				if (_notches == null)
				{
					return (double)Actual / MaximumDriverNotch;
				}

				return _notches[Actual].Item1;
			}
		}
	}
}
