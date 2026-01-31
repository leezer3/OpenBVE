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

using SoundManager;
using TrainManager.Car;

namespace TrainManager.BrakeSystems
{
	public class HandBrake
	{
		/// <summary>Whether the handbrake is currently applied</summary>
		public bool Applied;
		/// <summary>The brake force of the handbrake</summary>
		private readonly double Force;
		/// <summary>The sound played on application</summary>
		public CarSound ApplicationSound;
		/// <summary>The sound played on release</summary>
		public CarSound ReleaseSound;

		/// <summary>Gets the deceleration provided by the handbrake</summary>
		public double Deceleration => Car.CurrentMass / Force;

		private readonly CarBase Car;

		public HandBrake(CarBase car, double force)
		{
			Car = car;
			Force = force;
		}

		public void Set(bool newStatus)
		{
			if (Applied != newStatus)
			{
				if (newStatus)
				{
					ApplicationSound?.Play(Car, false);
				}
				else
				{
					ReleaseSound?.Play(Car, false);
				}
			}
		}
	}
}
