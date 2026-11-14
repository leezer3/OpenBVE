// Simplified BSD License (BSD-2-Clause)
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

using OpenBveApi.Hosts;
using OpenBveApi.Trains;
using System;

namespace TrainManager.Car
{
	public class BVE5Axle : AbstractAxle
	{
		private readonly double wheelslipAcceleration;

		private readonly double aerodynamicDragCoefficient = 1.1;

		private readonly double coefficientOfRollingResistance = 0.0025;

		private readonly double coefficientOfFriction = 0.35;

		public BVE5Axle(HostInterface currentHost, AbstractTrain Train, AbstractCar Car, double wheelslip) : base(currentHost, Train, Car)
		{
			wheelslipAcceleration = wheelslip;
		}

		public override double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			double f = FrontalArea * aerodynamicDragCoefficient * AirDensity / (2.0 * Math.Max(1.0, baseCar.CurrentMass));
			double a = AccelerationDueToGravity * coefficientOfRollingResistance + f * Speed * Speed;
			return a;
		}

		public override double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			return wheelslipAcceleration * Follower.AdhesionMultiplier;
		}

		public override double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return coefficientOfFriction * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
