using System;

namespace TrainManager.Power
{
	/// <summary>An abstract acceleration curve</summary>
	public abstract class AccelerationCurve
	{
		/// <summary>Gets the acceleration output for this curve</summary>
		/// <param name="speed">The current speed</param>
		/// <param name="loading">A double between 0 (Unloaded) and 1.0 (Loaded) representing the load factor</param>
		/// <returns>The acceleration output</returns>
		public abstract double GetAccelerationOutput(double speed, double loading);

		/// <summary>Gets the maximum possible acceleration output for this curve</summary>
		// ReSharper disable once UnusedMemberInSuper.Global
		public abstract double MaximumAcceleration
		{
			get;
		}
	}

	/// <summary>Represents a BVE2 / BVE4 format Acceleration Curve</summary>
	public class BveAccelerationCurve : AccelerationCurve
	{
		public double StageZeroAcceleration;
		public double StageOneSpeed;
		public double StageOneAcceleration;
		public double StageTwoSpeed;
		public double StageTwoExponent;
		private readonly double Multiplier;

		public override double GetAccelerationOutput(double speed, double loading)
		{
			if (speed <= 0.0)
			{
				return Multiplier * this.StageZeroAcceleration;
			}

			if (speed < this.StageOneSpeed)
			{
				double t = speed / this.StageOneSpeed;
				return Multiplier * (this.StageZeroAcceleration * (1.0 - t) + this.StageOneAcceleration * t);
			}

			if (speed < this.StageTwoSpeed)
			{
				return Multiplier * this.StageOneSpeed * this.StageOneAcceleration / speed;
			}

			return Multiplier * this.StageOneSpeed * this.StageOneAcceleration * Math.Pow(this.StageTwoSpeed, this.StageTwoExponent - 1.0) * Math.Pow(speed, -this.StageTwoExponent);
		}

		public override double MaximumAcceleration => Math.Max(StageZeroAcceleration, StageOneAcceleration);

		public BveAccelerationCurve Clone(double multiplier = 1.0)
		{
			return new BveAccelerationCurve(StageZeroAcceleration, StageOneAcceleration, StageOneSpeed, StageTwoSpeed, StageTwoExponent, multiplier);
		}

		public BveAccelerationCurve(double stageZeroAcceleration, double stageOneAcceleration, double stageOneSpeed, double stageTwoSpeed, double exponent, double multiplier = 1.0)
		{
			StageZeroAcceleration = stageZeroAcceleration;
			StageOneAcceleration = stageOneAcceleration;
			StageOneSpeed = stageOneSpeed;
			StageTwoSpeed = stageTwoSpeed;
			StageTwoExponent = exponent;
			Multiplier = multiplier;
		}

		public BveAccelerationCurve()
		{
			Multiplier = 1.0;
		}
	}

	public class BveDecelerationCurve : AccelerationCurve
	{
		private readonly double MaxDecelerationOutput;

		public override double GetAccelerationOutput(double speed, double loading)
		{
			return this.MaxDecelerationOutput;
		}

		public override double MaximumAcceleration => MaxDecelerationOutput;

		public BveDecelerationCurve(double deceleration)
		{
			this.MaxDecelerationOutput = deceleration;
		}
	}
}
