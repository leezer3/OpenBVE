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


using OpenBveApi.Colors;
using TrainManager.Trains;

namespace TrainManager.Handles
{
	/// <summary>Represents a regulator handle</summary>
	/// <remarks>A regulator handle is a simple percentage, as opposed to notch based power handle</remarks>
	public class Regulator : AbstractHandle
	{
		public Regulator(TrainBase Train) : base(Train)
		{
			MaximumNotch = 100;
			MaximumDriverNotch = 100;
		}

		public override void Update(double timeElapsed)
		{
			// no support for delayed changes
			if (baseTrain.Plugin != null)
			{
				Actual = Safety;
			}
			else
			{
				Actual = Driver;
			}
			
			// Spring return
			if (SpringType != SpringType.Unsprung && SpringTime > 0)
			{
				if (TrainManagerBase.currentHost.InGameTime > SpringTimer)
				{
					ApplyState(-1, true);
				}
			}
		}

		public override void ApplyState(int newState, bool relativeChange, bool isOverMaxDriverNotch = false)
		{
			// determine notch
			int p = relativeChange ? newState + Driver : newState;
			if (p < 0)
			{
				p = 0;
			}
			else if (p > MaximumNotch)
			{
				p = MaximumNotch;
			}
			if (!isOverMaxDriverNotch && p > MaximumDriverNotch)
			{
				p = MaximumDriverNotch;
			}

			// power sound
			if (p < Driver)
			{
				if (p > 0)
				{
					// down (not min)
					if (Driver - p > 2 | ContinuousMovement && DecreaseFast.Buffer != null)
					{
						DecreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						Decrease.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}
				else
				{
					// min
					Min.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}
			else if (p > Driver)
			{
				if (p < MaximumDriverNotch)
				{
					// up (not max)
					if (Driver - p > 2 | ContinuousMovement && IncreaseFast.Buffer != null)
					{
						IncreaseFast.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
					else
					{
						Increase.Play(baseTrain.Cars[baseTrain.DriverCar], false);
					}
				}
				else
				{
					// max
					Max.Play(baseTrain.Cars[baseTrain.DriverCar], false);
				}
			}

			Driver = p;
			TrainManagerBase.currentHost.AddBlackBoxEntry();
			// plugin
			if (baseTrain.Plugin != null)
			{
				baseTrain.Plugin.UpdatePower();
				baseTrain.Plugin.UpdateBrake();
			}
		}

		public override string GetNotchDescription(out MessageColor color)
		{
			color = MessageColor.Gray;
			if (Actual > 0)
			{
				color = MessageColor.Blue;
			}
			return Driver + "%";
		}

		public override string GetDebugDescription(HandleState state)
		{
			switch (state)
			{
				case HandleState.Actual:
					if (Actual == 0)
					{
						return "N";
					}

					return Actual + "%";
				case HandleState.Driver:
					if (Driver == 0)
					{
						return "N";
					}

					return Driver + "%";
				case HandleState.Safety:
					if (Safety == 0)
					{
						return "N";
					}

					return Safety + "%";
			}

			return string.Empty;
		}
	}
}

