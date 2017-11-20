using System;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>An abstract acceleration curve</summary>
		internal abstract class AccelerationCurve
		{
			/// <summary>Gets the acceleration output for this curve</summary>
			/// <param name="Speed">The current speed</param>
			/// <param name="Loading">A double between 0 (Unloaded) and 1.0 (Loaded) representing the load factor</param>
			/// <returns>The acceleration output</returns>
			internal abstract double GetAccelerationOutput(double Speed, double Loading);
		}

		/// <summary>Represents a BVE2 / BVE4 format Acceleration Curve</summary>
		internal class BveAccelerationCurve : AccelerationCurve
		{
			internal double StageZeroAcceleration;
			internal double StageOneSpeed;
			internal double StageOneAcceleration;
			internal double StageTwoSpeed;
			internal double StageTwoExponent;
			internal double Multiplier;

			internal override double GetAccelerationOutput(double Speed, double Loading)
			{
				if (Speed <= 0.0)
				{
					return Multiplier * this.StageZeroAcceleration;
				}
				if (Speed < this.StageOneSpeed)
				{
					double t = Speed / this.StageOneSpeed;
					return Multiplier * (this.StageZeroAcceleration * (1.0 - t) + this.StageOneAcceleration * t);
				}
				if (Speed < this.StageTwoSpeed)
				{
					return Multiplier * this.StageOneSpeed * this.StageOneAcceleration / Speed;
				}
				return Multiplier * this.StageOneSpeed * this.StageOneAcceleration * Math.Pow(this.StageTwoSpeed, this.StageTwoExponent - 1.0) * Math.Pow(Speed, -this.StageTwoExponent);
			}

			internal BveAccelerationCurve Clone(double multiplier)
			{
				return new BveAccelerationCurve
				{
					StageZeroAcceleration = this.StageZeroAcceleration,
					StageOneSpeed = this.StageOneSpeed,
					StageOneAcceleration = this.StageOneAcceleration,
					StageTwoSpeed = this.StageTwoSpeed,
					StageTwoExponent = this.StageTwoExponent,
					Multiplier = multiplier
				};
			}
		}

		internal class BveDecelerationCurve : AccelerationCurve
		{
			private double MaxDecelerationOutput;
			internal override double GetAccelerationOutput(double Speed, double Loading)
			{
				return this.MaxDecelerationOutput;
			}

			internal BveDecelerationCurve(double Deceleration)
			{
				this.MaxDecelerationOutput = Deceleration;
			}
		}

		/// <summary>Represents a BVE5 format acceleration curve</summary>
		internal class Bve5AccelerationCurve : AccelerationCurve
		{
			internal Bve5AccelerationCurveEntry[] UnloadedAcceleration;

			internal Bve5AccelerationCurveEntry[] LoadedAcceleration;

			internal override double GetAccelerationOutput(double Speed, double Loading)
			{
				double UnloadedAccelerationFactor = 0.0;
				for (int i = 0; i < this.UnloadedAcceleration.Length; i++)
				{
					if (Speed <= this.UnloadedAcceleration[i].Speed)
					{
						if (i == 0)
						{
							UnloadedAccelerationFactor = this.UnloadedAcceleration[i].Acceleration;
						}
						else
						{
							double speedDiff = (Speed - this.UnloadedAcceleration[i - 1].Speed) / (this.UnloadedAcceleration[i].Speed - this.UnloadedAcceleration[i - 1].Speed);
							UnloadedAccelerationFactor = ((this.UnloadedAcceleration[i].Acceleration - this.UnloadedAcceleration[i -1].Acceleration) * speedDiff) + this.UnloadedAcceleration[i -1].Acceleration;
						}
						break;
					}
				}
				double LoadedAccelerationFactor = 0.0;
				for (int i = 0; i < this.LoadedAcceleration.Length; i++)
				{
					if (Speed <= this.LoadedAcceleration[i].Speed)
					{
						if (i == 0)
						{
							LoadedAccelerationFactor = this.LoadedAcceleration[i].Acceleration;
						}
						else
						{
							double speedDiff = (Speed - this.LoadedAcceleration[i - 1].Speed) / (this.LoadedAcceleration[i].Speed - this.LoadedAcceleration[i - 1].Speed);
							LoadedAccelerationFactor = ((this.LoadedAcceleration[i].Acceleration - this.LoadedAcceleration[i - 1].Acceleration) * speedDiff) + this.LoadedAcceleration[i - 1].Acceleration;
						}
						break;
					}
				}
				return (LoadedAccelerationFactor - UnloadedAccelerationFactor) * Loading + UnloadedAccelerationFactor;
			}

			internal Bve5AccelerationCurve()
			{
				this.UnloadedAcceleration = new Bve5AccelerationCurveEntry[0];
				this.LoadedAcceleration = new Bve5AccelerationCurveEntry[0];
			}
		}

		internal class Bve5AccelerationCurveEntry
		{
			internal double Speed;

			internal double Acceleration;

			internal Bve5AccelerationCurveEntry(double Speed)
			{
				this.Speed = Speed;
				this.Acceleration = 0.0;
			}

		}
	}
}
