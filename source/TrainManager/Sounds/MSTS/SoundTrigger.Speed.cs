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
using OpenBve.Formats.MsTs;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	/// <summary>A sound trigger that activates when the train's speed increases past the set value</summary>
	public class SpeedIncPast : SoundTrigger
	{
		private readonly double speedValue;

		
		public SpeedIncPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double speedValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.speedValue = speedValue;
		}

		public SpeedIncPast(CarBase car, SoundBuffer buffer, double speedValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.speedValue = speedValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			double speed = Math.Abs(car.CurrentSpeed);
			if (speed >= speedValue)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (speed < speedValue)
			{
				Triggered = false;
			}
		}
	}

	/// <summary>A sound trigger that activates when the train's speed decreases past the set value</summary>
	public class SpeedDecPast : SoundTrigger
	{
		private readonly double speedValue;
		
		public SpeedDecPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double speedValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.speedValue = speedValue;
		}

		public SpeedDecPast(SoundBuffer buffer, double speedValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.speedValue = speedValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			double speed = Math.Abs(car.CurrentSpeed);
			if (speed <= speedValue && Triggered == false)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (speed > speedValue)
			{
				Triggered = false;
			}
		}
	}
}
