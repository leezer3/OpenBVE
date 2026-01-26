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
using System.Linq;
using OpenBveApi;
using OpenBveApi.Motor;
using TrainManager.Car;
using TrainManager.Power;
using TrainManager.Trains;

namespace TrainManager.Motor
{
    public class DieselEngine : TractionModel
    {
		/// <summary>The RPM at maximum power</summary>
	    public readonly double MaxRPM;
		/// <summary>The RPM at minimum power</summary>
		public readonly double MinRPM;
		/// <summary>The RPM at idle power</summary>
		public readonly double IdleRPM;
		/// <summary>The rate at which RPM changes up</summary>
		public readonly double RPMChangeUpRate;
		/// <summary>The rate at which RPM changes down</summary>
		public readonly double RPMChangeDownRate;
		/// <summary>The fuel used at idle</summary>
		public readonly double IdleFuelUse;
		/// <summary>The fuel used at max power</summary>
		public readonly double MaxPowerFuelUse;

		public readonly double MaxTractiveEffortSpeed;
	    /// <summary>Gets or sets the current engine RPM</summary>
		public double CurrentRPM
		{
			get => currentRPM;
			private set => currentRPM = value;
		}


		private double currentRPM;
		private double targetRPM;
		private readonly double perNotchRPM;


		public DieselEngine(CarBase car, AccelerationCurve[] accelerationCurves, double idleRPM, double minRPM, double maxRPM, double rpmChangeUpRate, double rpmChangeDownRate, double idleFuelUse = 0, double maxFuelUse = 0, double maxTractiveEffortSpeed = 0) : base (car, accelerationCurves, true)
		{
			MinRPM = minRPM;
			MaxRPM = maxRPM;
			IdleRPM = idleRPM;
			perNotchRPM = (MaxRPM - MinRPM) / car.baseTrain.Handles.Power.MaximumDriverNotch;
			RPMChangeUpRate = rpmChangeUpRate;
			RPMChangeDownRate = rpmChangeDownRate;
			IdleFuelUse = idleFuelUse;
			MaxPowerFuelUse = maxFuelUse;
			MaxTractiveEffortSpeed = maxTractiveEffortSpeed;
		}

		public override void Update(double timeElapsed)
		{
			if (IsRunning)
			{
				if (BaseCar.baseTrain.AI is TrackFollowingObjectAI)
				{
					// HACK: TFO AI doesn't update power properly
					targetRPM = BaseCar.CurrentSpeed == 0 ? IdleRPM : MaxRPM;
				}
				else
				{
					if (BaseCar.baseTrain.Handles.Power.Actual == 0)
					{
						targetRPM = IdleRPM;
					}
					else
					{
						targetRPM = MinRPM + BaseCar.baseTrain.Handles.Power.Actual * perNotchRPM;
					}
				}
			}
			else
			{
				targetRPM = 0;
			}
			MotorSounds?.Update(timeElapsed);

			if (targetRPM > currentRPM)
			{
				CurrentRPM += RPMChangeUpRate * timeElapsed;
				CurrentRPM = Math.Min(CurrentRPM, targetRPM);

			}
			else if(targetRPM < currentRPM) 
			{
				CurrentRPM -= RPMChangeDownRate * timeElapsed;
				CurrentRPM = Math.Max(CurrentRPM, targetRPM);
			}

			Message = @"Current RPM " + Math.Round(CurrentRPM);

			if(Components.TryGetTypedValue(EngineComponent.Gearbox, out Gearbox gearbox))
			{
				Message += " Current Gear " + gearbox.CurrentGear;
			}

			if (FuelTank != null)
			{
				if (currentRPM <= IdleRPM)
				{
					FuelTank.CurrentLevel -= IdleFuelUse * timeElapsed;
					Message += " Fuel Use " + Math.Round(IdleFuelUse * timeElapsed, 5);
				}
				else
				{
					FuelTank.CurrentLevel -= (MaxPowerFuelUse - IdleFuelUse) / BaseCar.baseTrain.Handles.Power.MaximumDriverNotch * BaseCar.baseTrain.Handles.Power.Actual * timeElapsed;
					Message += " Fuel Use " + Math.Round((MaxPowerFuelUse - IdleFuelUse) / BaseCar.baseTrain.Handles.Power.MaximumDriverNotch * BaseCar.baseTrain.Handles.Power.Actual * timeElapsed, 5);
				}
			}

			for (int i = 0; i < Components.Count; i++)
			{
				Components.ElementAt(i).Value.Update(timeElapsed);
			}

			MaximumPossibleAcceleration = AccelerationCurves[0].MaximumAcceleration;
		}

		public override double CurrentPower => Math.Max(0, currentRPM - MinRPM) / (MaxRPM - MinRPM);

		public override double TargetAcceleration
		{
			get
			{
				if (Components.TryGetTypedValue(EngineComponent.Gearbox, out Gearbox gearbox))
				{
					if (gearbox.OperationMode > GearboxOperation.Manual)
					{
						if (BaseCar.CurrentSpeed == 0)
						{
							if (BaseCar.baseTrain.Handles.Power.Actual == 0)
							{
								if (gearbox.CurrentGear > 0)
								{
									gearbox.GearDown();
								}
							}
							else
							{
								if (gearbox.CurrentGear == 0)
								{
									gearbox.GearUp();
								}
							}
						}
						else
						{
							if (BaseCar.CurrentSpeed < gearbox.PreviousMaximumGearSpeed)
							{
								// slow enough for previous gear
								gearbox.GearDown();
							}
							else if (BaseCar.CurrentSpeed >= gearbox.MaximumGearSpeed)
							{
								// gear change up
								gearbox.GearUp();
							}
						}
					}
					if (gearbox.CurrentGear == 0)
					{
						return 0;
					}

					return AccelerationCurves[gearbox.CurrentGear - 1].GetAccelerationOutput(BaseCar.CurrentSpeed);

				}
				return AccelerationCurves[0].GetAccelerationOutput(BaseCar.CurrentSpeed);

			}
		}
    }
}
