using System;

namespace OpenBveApi.Objects
{
	/// <summary>Rotation damping be applied to an animated object</summary>
	public class Damping
	{
		/// <summary>The natural frequency of undamped oscillations</summary>
		public readonly double NaturalFrequency;
		/// <summary>The time for one undamped oscillation</summary>
		public readonly double NaturalTime;
		/// <summary>The ratio to be applied</summary>
		public readonly double DampingRatio;
		/// <summary>The natural frequency when damped</summary>
		public readonly double NaturalDampingFrequency;
		/// <summary>The starting angle</summary>
		public double OriginalAngle;
		/// <summary>The derivitive at the starting angle</summary>
		public double OriginalDerivative;
		/// <summary>The target angle</summary>
		public double TargetAngle;
		/// <summary>The current angle</summary>
		public double CurrentAngle;
		/// <summary>The current daming value</summary>
		public double CurrentValue;
		/// <summary>The current delta time value</summary>
		public double CurrentTimeDelta;

		/// <summary>Constructs a new damping instance</summary>
		/// <param name="NaturalFrequency">The natural frequency</param>
		/// <param name="DampingRatio">The damping ratio to be applied</param>
		public Damping(double NaturalFrequency, double DampingRatio)
		{
			if (NaturalFrequency < 0.0)
			{
				throw new ArgumentException("NaturalFrequency must be non-negative in the constructor of the Damping class.");
			}
			else if (DampingRatio < 0.0)
			{
				throw new ArgumentException("DampingRatio must be non-negative in the constructor of the Damping class.");
			}
			else
			{
				this.NaturalFrequency = NaturalFrequency;
				this.NaturalTime = NaturalFrequency != 0.0 ? 1.0 / NaturalFrequency : 0.0;
				this.DampingRatio = DampingRatio;
				if (DampingRatio < 1.0)
				{
					this.NaturalDampingFrequency = NaturalFrequency * System.Math.Sqrt(1.0 - DampingRatio * DampingRatio);
				}
				else if (DampingRatio == 1.0)
				{
					this.NaturalDampingFrequency = NaturalFrequency;
				}
				else
				{
					this.NaturalDampingFrequency = NaturalFrequency * System.Math.Sqrt(DampingRatio * DampingRatio - 1.0);
				}

				this.OriginalAngle = 0.0;
				this.OriginalDerivative = 0.0;
				this.TargetAngle = 0.0;
				this.CurrentAngle = 0.0;
				this.CurrentValue = 1.0;
				this.CurrentTimeDelta = 0.0;
			}
		}

		/// <summary>Clones a damping instance</summary>
		/// <returns>The new damping instance</returns>
		public Damping Clone()
		{
			return (Damping) this.MemberwiseClone();
		}

		/// <summary>Updates this damping instance</summary>
		/// <param name="TimeElapsed">The time elapsed in ms</param>
		/// <param name="Angle">The new angle</param>
		/// <param name="Enable">Whether damping is to be applied</param>
		public void Update(double TimeElapsed, ref double Angle, bool Enable)
		{
			if (Enable == false)
			{
				CurrentValue = 1.0;
				CurrentAngle = Angle;
				OriginalAngle = Angle;
				TargetAngle = Angle;
				return;
			}

			if (TimeElapsed < 0.0)
			{
				TimeElapsed = 0.0;
			}
			else if (TimeElapsed > 1.0)
			{
				TimeElapsed = 1.0;
			}

			if (CurrentTimeDelta > NaturalTime)
			{
				// update
				double newDerivative;
				if (NaturalFrequency == 0.0)
				{
					newDerivative = 0.0;
				}
				else if (DampingRatio == 0.0)
				{
					newDerivative = OriginalDerivative * System.Math.Cos(NaturalFrequency * CurrentTimeDelta) -
					                NaturalFrequency * System.Math.Sin(NaturalFrequency * CurrentTimeDelta);
				}
				else if (DampingRatio < 1.0)
				{
					newDerivative = System.Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
					                (NaturalDampingFrequency * OriginalDerivative *
					                 System.Math.Cos(NaturalDampingFrequency * CurrentTimeDelta) -
					                 (NaturalDampingFrequency * NaturalDampingFrequency + DampingRatio * NaturalFrequency *
					                  (DampingRatio * NaturalFrequency + OriginalDerivative)) *
					                 System.Math.Sin(NaturalDampingFrequency * CurrentTimeDelta)) / NaturalDampingFrequency;
				}
				else if (DampingRatio == 1.0)
				{
					newDerivative = System.Math.Exp(-NaturalFrequency * CurrentTimeDelta) *
					                (OriginalDerivative - NaturalFrequency * (NaturalFrequency + OriginalDerivative) *
					                 CurrentTimeDelta);
				}
				else
				{
					newDerivative = System.Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
					                (NaturalDampingFrequency * OriginalDerivative *
					                 System.Math.Cosh(NaturalDampingFrequency * CurrentTimeDelta) +
					                 (NaturalDampingFrequency * NaturalDampingFrequency - DampingRatio * NaturalFrequency *
					                  (DampingRatio * NaturalFrequency + OriginalDerivative)) *
					                 System.Math.Sinh(NaturalDampingFrequency * CurrentTimeDelta)) / NaturalDampingFrequency;
				}

				double a = TargetAngle - OriginalAngle;
				OriginalAngle = CurrentAngle;
				TargetAngle = Angle;
				double b = TargetAngle - OriginalAngle;
				double r = b == 0.0 ? 1.0 : a / b;
				OriginalDerivative = newDerivative * r;
				if (NaturalTime > 0.0)
				{
					CurrentTimeDelta = CurrentTimeDelta % NaturalTime;
				}
			}

			{
				// perform
				double newValue;
				if (NaturalFrequency == 0.0)
				{
					newValue = 1.0;
				}
				else if (DampingRatio == 0.0)
				{
					newValue = System.Math.Cos(NaturalFrequency * CurrentTimeDelta) + OriginalDerivative *
					           System.Math.Sin(NaturalFrequency * CurrentTimeDelta) / NaturalFrequency;
				}
				else if (DampingRatio < 1.0)
				{
					double n = (OriginalDerivative + NaturalFrequency * DampingRatio) / NaturalDampingFrequency;
					newValue = System.Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
					           (System.Math.Cos(NaturalDampingFrequency * CurrentTimeDelta) +
					            n * System.Math.Sin(NaturalDampingFrequency * CurrentTimeDelta));
				}
				else if (DampingRatio == 1.0)
				{
					newValue = System.Math.Exp(-NaturalFrequency * CurrentTimeDelta) *
					           (1.0 + (OriginalDerivative + NaturalFrequency) * CurrentTimeDelta);
				}
				else
				{
					double n = (OriginalDerivative + NaturalFrequency * DampingRatio) / NaturalDampingFrequency;
					newValue = System.Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
					           (System.Math.Cosh(NaturalDampingFrequency * CurrentTimeDelta) +
					            n * System.Math.Sinh(NaturalDampingFrequency * CurrentTimeDelta));
				}

				CurrentValue = newValue;
				CurrentAngle = TargetAngle * (1.0 - newValue) + OriginalAngle * newValue;
				CurrentTimeDelta += TimeElapsed;
				Angle = CurrentAngle;
			}

		}
	}
}
