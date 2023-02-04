﻿using System;
using OpenBveApi.Interface;
using TrainManager.Car;
using TrainManager.Motor;
using TrainManager.Power;

namespace TrainManager.TractionModels
{
	/// <summary>An abstract traction model</summary>
	public abstract class AbstractTractionModel
	{
		/// <summary>Holds a reference to the base car</summary>
		internal readonly CarBase Car;
		/// <summary>The current deceleration due to brake effects</summary>
		public double DecelerationDueToBrake;
		/// <summary>The current decelerationdue to traction effects</summary>
		/// <remarks>Regenerative braking etc.</remarks>
		public double DecelerationDueToTraction;
		/// <summary>The current total acceleration output supplied by the car from all sources</summary>
		/// <remarks>Is positive for power and negative for brake, regardless of the train's direction</remarks>
		public double Acceleration;
		/// <summary>The acceleration generated by the motor</summary>
		/// <remarks>Is positive for power and negative for brake, regardless of the train's direction</remarks>
		public double MotorAcceleration;
		/// <summary>The acceleration curves</summary>
		public AccelerationCurve[] AccelerationCurves;
		/// <summary>The maximum acceleration possible</summary>
		public double MaximumAcceleration;
		/// <summary>The motor sounds</summary>
		public AbstractMotorSound Sounds;
		/// <summary>The current mass of the traction model</summary>
		/// <remarks>May be modified by fuel consumption etc.</remarks>
		public virtual double Mass => 0;

		/// <summary>Creates a new abstract traction model</summary>
		protected AbstractTractionModel(CarBase car)
		{
			Car = car;
			AccelerationCurves = new AccelerationCurve[] { };
		}
		/// <summary>Base acceleration figure due to rolling on an incline</summary>
		internal double PowerRollingCouplerAcceleration
		{
			get
			{
				double a = Car.FrontAxle.Follower.WorldDirection.Y;
				double b = Car.RearAxle.Follower.WorldDirection.Y;
				return -0.5 * (a + b) * TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity;
			}
		
		}
		/// <summary>Base acceleration figure due to friction</summary>
		internal double FrictionBrakeAcceleration
		{
			get
			{
				double v = Math.Abs(Car.CurrentSpeed);
				double t = Car.Index == 0 & Car.CurrentSpeed >= 0.0 || Car.Index == Car.baseTrain.NumberOfCars - 1 & Car.CurrentSpeed <= 0.0 ? Car.Specs.ExposedFrontalArea : Car.Specs.UnexposedFrontalArea;
				double a = Car.FrontAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(Car.FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				double b = Car.RearAxle.GetResistance(v, t, TrainManagerBase.CurrentRoute.Atmosphere.GetAirDensity(Car.FrontAxle.Follower.WorldPosition.Y), TrainManagerBase.CurrentRoute.Atmosphere.AccelerationDueToGravity);
				return 0.5 * (a + b);
			}
			
		}

		/// <summary>Updates the traction model</summary>
		/// <param name="TimeElapsed">The frame time elapsed</param>
		/// <param name="Speed">The updated speed value</param>
		public virtual void Update(double TimeElapsed, out double Speed)
		{
			Speed = 0;
		}

		/// <summary>Handles key down events</summary>
		/// <param name="command">The command pressed</param>
		public virtual void HandleKeyDown(Translations.Command command)
		{

		}

		/// <summary>Handles key up events</summary>
		/// <param name="command">The command pressed</param>
		public virtual void HandleKeyUp(Translations.Command command)
		{

		}

		/// <summary>Renders any overlay graphics / text generated by the traction model</summary>
		/// <param name="yOffset">The Y-offset for the rendering to start at</param>
		public virtual void RenderOverlay(ref double yOffset)
		{

		}
	}
}
