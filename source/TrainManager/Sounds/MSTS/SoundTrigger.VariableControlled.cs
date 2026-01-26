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
	/// <summary>A sound trigger controlled by Variable1 increasing past the setpoint</summary>
	/// <remarks>Variable1 represents the current wheel rotation speed</remarks>
	public class Variable1IncPast : SoundTrigger
	{
		private readonly double variableValue;

		public Variable1IncPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double variableValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public Variable1IncPast(SoundBuffer buffer, double variableValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			double value = Math.Abs(car.CurrentSpeed / 1000 / car.DrivingWheels[0].Radius / Math.PI * 5);
			if (car.baseTrain.Handles.Power.Actual > 0 && value > variableValue && Triggered == false)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (value < variableValue)
			{
				Triggered = false;
			}
		}
	}

	/// <summary>A sound trigger controlled by Variable1 increasing past the setpoint</summary>
	/// <remarks>Variable1 represents the current wheel rotation speed</remarks>
	/// Diesel- EngineRPM
	/// Electric- TractiveForce
	/// Steam- CylinderPressure
	public class Variable1DecPast : SoundTrigger
	{
		private readonly double variableValue;

		public Variable1DecPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double variableValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public Variable1DecPast(SoundBuffer buffer, double variableValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			double value = Math.Abs(car.CurrentSpeed / 1000 / car.DrivingWheels[0].Radius / Math.PI * 5);
			if (car.baseTrain.Handles.Power.Actual > 0 && value < variableValue && Triggered == false)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (value > variableValue)
			{
				Triggered = false;
			}
		}
	}

	/// <summary>A sound trigger controlled by Variable2 increasing past the setpoint</summary>
	/// <remarks>Variable2 represents the proportion of power the car's TractionModel is currently generating</remarks>
	/// Diesel- EngineRPM
	/// Electric- TractiveForce
	/// Steam- CylinderPressure
	public class Variable2IncPast : SoundTrigger
	{
		private readonly double variableValue;
		
		public Variable2IncPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double variableValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public Variable2IncPast(SoundBuffer buffer, double variableValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			if (car.TractionModel.CurrentPower >= variableValue && Triggered == false)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (car.TractionModel.CurrentPower < variableValue)
			{
				Triggered = false;
			}
		}
	}

	/// <summary>A sound trigger controlled by Variable2 decreasing past the setpoint</summary>
	/// <remarks>Variable2 represents the proportion of power the car's TractionModel is currently generating</remarks>
	/// Diesel- EngineRPM
	/// Electric- TractiveForce
	/// Steam- CylinderPressure
	public class Variable2DecPast : SoundTrigger
	{
		private readonly double variableValue;

		public Variable2DecPast(SoundBuffer[] buffers, KujuTokenID selectionMethod, double variableValue, bool soundLoops) : base(buffers, selectionMethod, soundLoops)
		{
			this.variableValue = variableValue;
		}
		public Variable2DecPast(SoundBuffer buffer, double variableValue, bool soundLoops) : base(buffer, soundLoops)
		{
			this.variableValue = variableValue;
		}

		public override void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{
			if (car.TractionModel.CurrentPower <= variableValue && Triggered == false)
			{
				soundBuffer = Buffer;
				soundLoops = SoundLoops;
				Triggered = true;
			}

			if (car.TractionModel.CurrentPower > variableValue)
			{
				Triggered = false;
			}
		}
	}
}
