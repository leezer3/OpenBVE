﻿using System;

namespace TrainManager.Power
{
	/// <summary>An abstract acceleration curve</summary>
	public abstract class AccelerationCurve
	{
		/// <summary>Gets the acceleration output for this curve</summary>
		/// <param name="Speed">The current speed</param>
		/// <returns>The acceleration output</returns>
		public abstract double GetAccelerationOutput(double Speed);

		/// <summary>Gets the maximum possible acceleration output for this curve</summary>
		// ReSharper disable once UnusedMemberInSuper.Global
		public abstract double MaximumAcceleration
		{
			get;
		}
	}
}
