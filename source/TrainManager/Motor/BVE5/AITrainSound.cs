//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using OpenBveApi.Math;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVE5AITrainSounds : AbstractMotorSound
	{

		public readonly BVE5AISoundEntry[] SoundEntries;

		public BVE5AITrainSounds(CarBase car, BVE5AISoundEntry[] entries) : base(car)
		{
			SoundEntries = entries;
			lastSpeed = double.MinValue;
			lastState = BVE5AISoundControl.Invalid;
		}

		private double lastSpeed;
		private BVE5AISoundControl lastState;
		private double timer;

		public override void Update(double TimeElapsed)
		{
			timer += TimeElapsed;
			if (SoundEntries.Length == 0 || timer < 1)
			{
				// no need to update
				return;
			}

			

			BVE5AISoundControl currentState = BVE5AISoundControl.Stationary;
			
			if (Car.CurrentSpeed != 0)
			{
				if (Math.Abs(lastSpeed - Car.CurrentSpeed) < 0.001)
				{
					currentState = BVE5AISoundControl.Rolling;
				}
				else
				{
					currentState = Car.CurrentSpeed > lastSpeed ? BVE5AISoundControl.Acceleration : BVE5AISoundControl.Deceleration;
				}
			}

			// HACK: Assume that acceleration / deceleration must continue for a minium of 1s to be detected (as no fade in/ out)
			timer = 0;
			lastSpeed = Car.CurrentSpeed;
			if (lastState == currentState)
			{
				return;
			}
			lastState = currentState;
			
			for (int i = 0; i < SoundEntries.Length; i++)
			{
				if (SoundEntries[i].Controller == currentState)
				{
					SoundEntries[i].Source = TrainManagerBase.currentHost.PlaySound(SoundEntries[i].Sound, 1.0, 1.0, Vector3.Zero, Car, true) as SoundSource;
				}
				else
				{
					SoundEntries[i].Source?.Stop();
				}
			}

			
		}
	}
}
