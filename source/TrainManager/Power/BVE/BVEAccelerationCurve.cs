using System;

namespace TrainManager.Power
{
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

		public override double MaximumAcceleration => Math.Max(StageZeroAcceleration, StageOneAcceleration);

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

		public override double MaximumAcceleration => MaxDecelerationOutput;

		public BveDecelerationCurve(double Deceleration)
		{
			this.MaxDecelerationOutput = Deceleration;
		}
	}
}
