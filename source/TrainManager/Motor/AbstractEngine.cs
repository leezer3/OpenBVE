//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using TrainManager.Car;
using TrainManager.Power;

namespace TrainManager.Motor
{
	/// <summary>An abstract engine</summary>
    public abstract class TractionModel
    {
		/// <summary>Holds a reference to the base car</summary>
	    internal readonly CarBase BaseCar;
		/// <summary>The fuel supply</summary>
	    public FuelTank FuelTank;
	    /// <summary>Whether the engine is running</summary>
	    public bool IsRunning;
		/// <summary>The components of the engine</summary>
	    public Dictionary<EngineComponent, AbstractComponent> Components;
		/// <summary>Whether the engine provides power (acceleration) to the train</summary>
	    public readonly bool ProvidesPower;
		/// <summary>The acceleration curves</summary>
	    public AccelerationCurve[] AccelerationCurves;
	    /// <summary>The motor sounds</summary>
	    public AbstractMotorSound MotorSounds;
		/// <summary>The maximum possible acceleration provided</summary>
	    public double MaximumPossibleAcceleration;
	    /// <summary>The maximum possible motor acceleration</summary>
	    /// <remarks>This ignores wheelspin etc.</remarks>
	    public double MaximumCurrentAcceleration;
		/// <summary>The total acceleration generated by the motor</summary>
		/// <remarks>Is positive for power and negative for brake, regardless of the train's direction, and takes into account wheelspin etc.</remarks>
		public double CurrentAcceleration;


		/// <summary>Creates a new TractionModel</summary>
		protected TractionModel(CarBase car, AccelerationCurve[] accelerationCurves, bool providesPower)
	    {
		    BaseCar = car;
			AccelerationCurves = accelerationCurves;
			Components = new Dictionary<EngineComponent, AbstractComponent>();
			ProvidesPower = providesPower;
	    }

	    protected TractionModel(CarBase car)
	    {
		    BaseCar = car;
			AccelerationCurves = new AccelerationCurve[0];
			Components = new Dictionary<EngineComponent, AbstractComponent>();
		}
		
		/// <summary>Called once a frame to update the engine</summary>
		/// <param name="timeElapsed"></param>
	    public abstract void Update(double timeElapsed);

	    /// <summary>Gets the current power level</summary>
	    public virtual double CurrentPower => 0;

	    /// <summary>The target acceleration</summary>
	    /// <remarks>Figure achievable before wheelslip etc. is applied</remarks>
	    public virtual double TargetAcceleration => 0;
    }
}
