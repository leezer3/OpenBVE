using System;

namespace TrainManager.Power
{
	/// <summary>An abstract acceleration curve</summary>
	public abstract class AccelerationCurve
	{
		/// <summary>Gets the acceleration output for this curve</summary>
		/// <param name="Speed">The current speed</param>
		/// <param name="Loading">A double between 0 (Unloaded) and 1.0 (Loaded) representing the load factor</param>
		/// <returns>The acceleration output</returns>
		public abstract double GetAccelerationOutput(double Speed, double Loading);
	}

	/// <summary>Represents a BVE2 / BVE4 format Acceleration Curve</summary>
	public class BveAccelerationCurve : AccelerationCurve
	{
		public double StageZeroAcceleration;
		public double StageOneSpeed;
		public double StageOneAcceleration;
		public double StageTwoSpeed;
		public double StageTwoExponent;
		public double Multiplier;

		public override double GetAccelerationOutput(double Speed, double Loading)
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

		public BveAccelerationCurve Clone(double multiplier)
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

	public class BveDecelerationCurve : AccelerationCurve
	{
		private readonly double MaxDecelerationOutput;

		public override double GetAccelerationOutput(double Speed, double Loading)
		{
			return this.MaxDecelerationOutput;
		}

		public BveDecelerationCurve(double Deceleration)
		{
			this.MaxDecelerationOutput = Deceleration;
		}
	}
}
