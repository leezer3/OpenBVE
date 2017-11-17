using System;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Represents damping that may be applied to the rotation of an animated object</summary>
		internal class Damping
		{
			private readonly double NaturalFrequency;
			private readonly double NaturalTime;
			private readonly double DampingRatio;
			private readonly double NaturalDampingFrequency;
			internal double OriginalAngle;
			internal double OriginalDerivative;
			internal double TargetAngle;
			internal double CurrentAngle;
			internal double CurrentValue;
			internal double CurrentTimeDelta;
			internal Damping(double NaturalFrequency, double DampingRatio)
			{
				if (NaturalFrequency < 0.0)
				{
					throw new ArgumentException("NaturalFrequency must be non-negative in the constructor of the Damping class.");
				}
				if (DampingRatio < 0.0)
				{
					throw new ArgumentException("DampingRatio must be non-negative in the constructor of the Damping class.");
				}
				this.NaturalFrequency = NaturalFrequency;
				this.NaturalTime = NaturalFrequency != 0.0 ? 1.0 / NaturalFrequency : 0.0;
				this.DampingRatio = DampingRatio;
				if (DampingRatio < 1.0)
				{
					this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(1.0 - DampingRatio * DampingRatio);
				}
				else if (DampingRatio == 1.0)
				{
					this.NaturalDampingFrequency = NaturalFrequency;
				}
				else
				{
					this.NaturalDampingFrequency = NaturalFrequency * Math.Sqrt(DampingRatio * DampingRatio - 1.0);
				}
				this.OriginalAngle = 0.0;
				this.OriginalDerivative = 0.0;
				this.TargetAngle = 0.0;
				this.CurrentAngle = 0.0;
				this.CurrentValue = 1.0;
				this.CurrentTimeDelta = 0.0;
			}

			internal Damping Clone()
			{
				return (Damping)this.MemberwiseClone();
			}

			internal void Update(double TimeElapsed, ref double Angle, bool Enable)
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
						newDerivative = OriginalDerivative * Math.Cos(NaturalFrequency * CurrentTimeDelta) -
						                NaturalFrequency * Math.Sin(NaturalFrequency * CurrentTimeDelta);
					}
					else if (DampingRatio < 1.0)
					{
						newDerivative = Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
						                (NaturalDampingFrequency * OriginalDerivative *
						                 Math.Cos(NaturalDampingFrequency * CurrentTimeDelta) -
						                 (NaturalDampingFrequency * NaturalDampingFrequency + DampingRatio * NaturalFrequency *
						                  (DampingRatio * NaturalFrequency + OriginalDerivative)) *
						                 Math.Sin(NaturalDampingFrequency * CurrentTimeDelta)) / NaturalDampingFrequency;
					}
					else if (DampingRatio == 1.0)
					{
						newDerivative = Math.Exp(-NaturalFrequency * CurrentTimeDelta) *
						                (OriginalDerivative - NaturalFrequency * (NaturalFrequency + OriginalDerivative) *
						                 CurrentTimeDelta);
					}
					else
					{
						newDerivative = Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
						                (NaturalDampingFrequency * OriginalDerivative *
						                 Math.Cosh(NaturalDampingFrequency * CurrentTimeDelta) +
						                 (NaturalDampingFrequency * NaturalDampingFrequency - DampingRatio * NaturalFrequency *
						                  (DampingRatio * NaturalFrequency + OriginalDerivative)) *
						                 Math.Sinh(NaturalDampingFrequency * CurrentTimeDelta)) / NaturalDampingFrequency;
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
						newValue = Math.Cos(NaturalFrequency * CurrentTimeDelta) + OriginalDerivative *
						           Math.Sin(NaturalFrequency * CurrentTimeDelta) / NaturalFrequency;
					}
					else if (DampingRatio < 1.0)
					{
						double n = (OriginalDerivative + NaturalFrequency * DampingRatio) / NaturalDampingFrequency;
						newValue = Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
						           (Math.Cos(NaturalDampingFrequency * CurrentTimeDelta) +
						            n * Math.Sin(NaturalDampingFrequency * CurrentTimeDelta));
					}
					else if (DampingRatio == 1.0)
					{
						newValue = Math.Exp(-NaturalFrequency * CurrentTimeDelta) *
						           (1.0 + (OriginalDerivative + NaturalFrequency) * CurrentTimeDelta);
					}
					else
					{
						double n = (OriginalDerivative + NaturalFrequency * DampingRatio) / NaturalDampingFrequency;
						newValue = Math.Exp(-DampingRatio * NaturalFrequency * CurrentTimeDelta) *
						           (Math.Cosh(NaturalDampingFrequency * CurrentTimeDelta) +
						            n * Math.Sinh(NaturalDampingFrequency * CurrentTimeDelta));
					}
					CurrentValue = newValue;
					CurrentAngle = TargetAngle * (1.0 - newValue) + OriginalAngle * newValue;
					CurrentTimeDelta += TimeElapsed;
					Angle = CurrentAngle;
				}
			}
		}
	}
}
