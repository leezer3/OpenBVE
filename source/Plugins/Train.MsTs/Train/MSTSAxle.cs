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

using System;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace Train.MsTs
{
	/// <summary>Derived class for axles on MSTS cars</summary>
	public class MSTSAxle : AbstractAxle
	{
		/// <summary>The friction properties</summary>
		private readonly Friction FrictionProperties;
		/// <summary>The adhesion properties</summary>
		private readonly Adhesion AdhesionProperties;

		internal MSTSAxle(HostInterface currentHost, AbstractTrain train, AbstractCar car, Friction friction, Adhesion adhesion) : base(currentHost, train, car)
		{
			FrictionProperties = friction;
			AdhesionProperties = adhesion;
		}

		public override double GetResistance(double Speed, double FrontalArea, double AirDensity, double AccelerationDueToGravity)
		{
			return FrictionProperties.GetResistanceValue(Speed) / Math.Max(1.0, baseCar.CurrentMass);
		}

		public override double CriticalWheelSlipAccelerationForElectricMotor(double AccelerationDueToGravity)
		{
			return AdhesionProperties.GetWheelslipValue();
		}

		public override double CriticalWheelSlipAccelerationForFrictionBrake(double AccelerationDueToGravity)
		{
			// TODO: This is the BVE formula
			double NormalForceAcceleration = Follower.WorldUp.Y * AccelerationDueToGravity;
			return 0.35 * Follower.AdhesionMultiplier * NormalForceAcceleration;
		}
	}
}
