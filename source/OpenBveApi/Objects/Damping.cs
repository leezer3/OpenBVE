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
		/// <summary>The derivative at the starting angle</summary>
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
		/// <param name="naturalFrequency">The natural frequency</param>
		/// <param name="dampingRatio">The damping ratio to be applied</param>
		public Damping(double naturalFrequency, double dampingRatio)
		{
			if (naturalFrequency < 0.0)
			{
				throw new ArgumentException("NaturalFrequency must be non-negative in the constructor of the Damping class.");
			}
			if (dampingRatio < 0.0)
			{
				throw new ArgumentException("DampingRatio must be non-negative in the constructor of the Damping class.");
			}

			NaturalFrequency = naturalFrequency;
			NaturalTime = naturalFrequency != 0.0 ? 1.0 / naturalFrequency : 0.0;
			DampingRatio = dampingRatio;
			if (dampingRatio < 1.0)
			{
				NaturalDampingFrequency = naturalFrequency * System.Math.Sqrt(1.0 - dampingRatio * dampingRatio);
			}
			else if (dampingRatio == 1.0)
			{
				NaturalDampingFrequency = naturalFrequency;
			}
			else
			{
				NaturalDampingFrequency = naturalFrequency * System.Math.Sqrt(dampingRatio * dampingRatio - 1.0);
			}

			OriginalAngle = 0.0;
			OriginalDerivative = 0.0;
			TargetAngle = 0.0;
			CurrentAngle = 0.0;
			CurrentValue = 1.0;
			CurrentTimeDelta = 0.0;
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
					CurrentTimeDelta %= NaturalTime;
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
