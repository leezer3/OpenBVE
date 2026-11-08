//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

using System;
using TrainManager.Car.Systems;

namespace TrainManager.Car
{
	public class Bve5ReadhesionDevice : AbstractReAdhesionDevice
	{
		public double MaximumAccelerationOutput;
		
		private readonly double SlipVelocity; // km/h

		private readonly double SlipAcceleration; // km/h/s

		private readonly double BalanceAcceleration; // km/h/s

		private readonly double HoldingTime; // s

		private int currentStep = 0;

		private double slipSpeed;

		private double holdingTimer;


		public Bve5ReadhesionDevice(CarBase car, double slipVelocity, double slipAcceleration, double balanceAcceleration, double holdingTime) : base(car)
		{
			SlipVelocity = slipVelocity;
			SlipAcceleration = slipAcceleration;
			BalanceAcceleration = balanceAcceleration;
			HoldingTime = holdingTime;
		}

		public override void Update(double timeElapsed)
		{
			// When the train speed is at SlipVelocity above the reference speed
			// The current is cut off (to the value in NoLoadCurrent presumably)
			// The acceleration output is reduced by BalanceAcceleration
			// This is held for a minimum of HoldingTime, or until TrainSpeed is at or below the reference speed
			if ((Car.FrontAxle.CurrentWheelSlip || Car.RearAxle.CurrentWheelSlip) && currentStep == 0)
			{
				currentStep = 1;
				slipSpeed = Car.CurrentSpeed;
			}

			switch (currentStep)
			{
				case 0:
					MaximumAccelerationOutput = Car.TractionModel.TargetAcceleration;
					break;
				case 1:
					MaximumAccelerationOutput = Car.TractionModel.TargetAcceleration;
					if (Math.Abs(Car.CurrentSpeed) - slipSpeed > SlipVelocity)
					{
						// car speed is at or greater than SlipVelocity from speed
						currentStep = 2;
					}
					break;
				case 2:
					if (Car.TractionModel.CurrentAcceleration > 0)
					{
						MaximumAccelerationOutput = Car.TractionModel.TargetAcceleration - BalanceAcceleration;
					}
					else
					{
						MaximumAccelerationOutput = Car.TractionModel.TargetAcceleration + BalanceAcceleration;
					}
					holdingTimer += timeElapsed;

					if (holdingTimer > HoldingTime && Car.CurrentSpeed > slipSpeed)
					{
						currentStep = 0;
					}
					break;
			}
			
		}
	}
}
