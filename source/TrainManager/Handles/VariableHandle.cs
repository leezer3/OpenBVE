using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>A handle with variable properties per notch</summary>
	public class VariableHandle : AbstractHandle
	{
		public VariableHandle(TrainBase train, bool isPower, Tuple<double, string>[] notches = null) : base(train)
		{
			powerHandle = isPower;
			if (notches?.Length > 0)
			{
				MaximumDriverNotch = notches.Length - 1;
				MaximumNotch = notches.Length - 1;
				_notches = notches;
				for (int i = 0; i < _notches.Length; i++)
				{
					Vector2 s = TrainManagerBase.Renderer.Fonts.NormalFont.MeasureString(_notches[i].Item2);
					if (s.X > MaxWidth)
					{
						MaxWidth = s.X;
					}
				}
			}
			else
			{
				MaximumDriverNotch = 100;
				MaximumNotch = 100;
			}
			
		}

		private readonly Tuple<double, string>[] _notches;

		private readonly bool powerHandle;

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
			

			Actual = Math.Max(0, Math.Min(Actual, MaximumNotch));
			Driver = Actual;
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			if (this == baseTrain.Handles.Power && baseTrain.Handles.HandleType == HandleType.SingleHandle && (baseTrain.Handles.Brake.Driver != 0 || baseTrain.Handles.EmergencyBrake.Driver))
			{
				return baseTrain.Handles.Brake.GetNotchDescription(out color);
			}

			if (this == baseTrain.Handles.Power)
			{
				color = Actual == 0 ? MessageColor.Gray : MessageColor.Blue;
			}
			else
			{
				color = Actual == 0 ? MessageColor.Gray : MessageColor.Orange;
			}

			if (_notches == null)
			{
				if (Actual == 0)
				{
					return "N";
				}
				return (powerHandle ? "P" : "B") + Actual + "%";
			}

			return _notches[Actual].Item2;
		}

		/// <summary>Gets the current power modifier for this handle</summary>
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
