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

using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Trains;
using System;
using System.Collections.Generic;
using OpenBveApi;
using TrainManager.Car.Systems;
using TrainManager.Motor;
using OpenBveApi.Hosts;
using TrainManager.SafetySystems;
// ReSharper disable SwitchStatementMissingSomeEnumCasesNoDefault

namespace Train.MsTs
{
	/// <summary>Animation class handling CVF elements with keyframe mappings</summary>
	internal class CvfAnimation : AnimationScript
	{
		internal readonly HostInterface CurrentHost;

		internal readonly PanelSubject Subject;

		internal readonly FrameMapping[] FrameMapping;

		internal readonly int Digit;

		internal readonly double UnitConversionFactor;

		private double lastResult;

		internal CvfAnimation(HostInterface host, PanelSubject subject)
		{
			CurrentHost = host;
			Subject = subject;
			switch (subject)
			{
				case PanelSubject.Aspect_Display:
					Minimum = 0;
					Maximum = 7;
					break;
				default:
					Minimum = 0;
					Maximum = double.MaxValue;
					break;
			}
			Digit = -1;
		}

		internal CvfAnimation(HostInterface host, PanelSubject subject, FrameMapping[] frameMapping)
		{
			CurrentHost = host;
			Subject = subject;
			FrameMapping = frameMapping;
			Minimum = 0;
			Maximum = FrameMapping.Length;
			Digit = -1;
		}

		internal CvfAnimation(HostInterface host, PanelSubject subject, Units unit, int digit)
		{
			CurrentHost = host;
			Subject = subject;
			switch (unit)
			{
				case Units.Miles_Per_Hour:
					UnitConversionFactor = 2.2369362920544;
					break;
				case Units.Kilometers_Per_Hour:
					UnitConversionFactor = 3.6;
					break;
				case Units.PSI:
					UnitConversionFactor = 0.000145038;
					break;
			}
			Digit = digit;
		}

		internal CvfAnimation(HostInterface host, PanelSubject subject, Units unit, FrameMapping[] frameMapping)
		{
			CurrentHost = host;
			Subject = subject;
			switch (unit)
			{
				case Units.Miles_Per_Hour:
					UnitConversionFactor = 2.2369362920544;
					break;
				case Units.Kilometers_Per_Hour:
					UnitConversionFactor = 3.6;
					break;
			}

			Digit = -1;
			FrameMapping = frameMapping;
		}

		internal CvfAnimation(HostInterface host, PanelSubject subject, CabComponentStyle style, int digit)
		{
			CurrentHost = host;
			Subject = subject;
			Digit = digit;
			switch (style)
			{
				case CabComponentStyle.TwelveHour:
					UnitConversionFactor = 1;
					break;
				case CabComponentStyle.TwentyFourHour:
					UnitConversionFactor = 0;
					break;
			}
		}

		public double ExecuteScript(AbstractTrain train, int carIndex, Vector3 position, double trackPosition, int sectionIndex, bool isPartOfTrain, double timeElapsed, int currentState)
		{
			dynamic dynamicTrain = train;
			switch (Subject)
			{
				case PanelSubject.CP_Handle:
				case PanelSubject.CPH_Display:
					// NOTE: 0.5 mapping == N
					double mapping = 0.5;
					if (dynamicTrain.Handles.Brake.Actual > 0)
					{
						mapping += (double)dynamicTrain.Handles.Brake.Actual / dynamicTrain.Handles.Brake.MaximumNotch * 0.5;
					}
					else
					{
						mapping -= (double)dynamicTrain.Handles.Power.Actual / dynamicTrain.Handles.Power.MaximumNotch * 0.5;
					}
					MapResult(mapping);
					break;
				case PanelSubject.Throttle_Display:
				case PanelSubject.Throttle:
					MapResult((double)dynamicTrain.Handles.Power.Actual / dynamicTrain.Handles.Power.MaximumNotch);
					break;
				case PanelSubject.Train_Brake:
					MapResult((double)dynamicTrain.Handles.Brake.Actual / dynamicTrain.Handles.Brake.MaximumNotch);
					break;
				case PanelSubject.Friction_Braking:
					// NOTE: Assumed at the minute this goes out at speed zero
					bool isBraking = Math.Abs(train.CurrentSpeed) > 0 && (dynamicTrain.Handles.Brake.Actual > 0 || (dynamicTrain.Handles.HasLocoBrake && dynamicTrain.Handles.LocoBrake.Actual > 0));
					MapResult(isBraking ? 1 : 0);
					break;
				case PanelSubject.Direction_Display:
				case PanelSubject.Direction:
					lastResult = (int)dynamicTrain.Handles.Reverser.Actual + 1;
					break;
				case PanelSubject.Speedlim_Display:
					double speedLim = Math.Min(train.CurrentRouteLimit, train.CurrentSectionLimit) * UnitConversionFactor;
					if (Digit == -1)
					{
						// color
						for (int i = 0; i < FrameMapping.Length; i++)
						{
							if (FrameMapping[i].MappingValue <= speedLim)
							{
								lastResult = FrameMapping[i].FrameKey;
								break;
							}
						}
					}
					else
					{
						// digit
						if (double.IsPositiveInfinity(speedLim))
						{
							lastResult = -1; // cheat to hide
						}
						else
						{
							lastResult = (int)(speedLim / (int)Math.Pow(10, Digit) % 10);
						}
					}
					break;
				case PanelSubject.Speedometer:
					double currentSpeed = Math.Abs(train.CurrentSpeed) * UnitConversionFactor;
					MapDigitalResult(currentSpeed);
					break;
				case PanelSubject.Aspect_Display:
					lastResult = train.CurrentSignalAspect;
					break;
				case PanelSubject.Overspeed:
					double currentLimit = Math.Min(train.CurrentRouteLimit, train.CurrentSectionLimit);
					lastResult = Math.Abs(train.CurrentSpeed) > currentLimit ? 1 : 0;
					break;
				case PanelSubject.Front_Hlight:
					lastResult = dynamicTrain.SafetySystems.Headlights.CurrentState;
					break;
				case PanelSubject.Pantograph:
				case PanelSubject.Panto_Display:
					int pantographState = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].TractionModel is ElectricEngine electricEngine && 
						    electricEngine.Components.TryGetTypedValue(EngineComponent.Pantograph, out Pantograph pantograph))
						{
							pantographState = (int)pantograph.State;
							break;
						}
					}
					lastResult = pantographState;
					break;
				case PanelSubject.Gears:
					int gearState = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].TractionModel is DieselEngine dieselEngine &&
						    dieselEngine.Components.TryGetTypedValue(EngineComponent.Gearbox, out Gearbox gearbox))
						{
							gearState = gearbox.CurrentGear;
							break;
						}
					}
					lastResult = gearState;
					break;
				case PanelSubject.Sanders:
					// sanders button is pressed
					int sandState = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].ReAdhesionDevice is Sanders sanders)
						{
							sandState = sanders.State >= SandersState.Active ? 1 :0;
							break;
						}
					}
					lastResult = sandState;
					break;
				case PanelSubject.Sanding:
					// sand is actually being dispensed / doing something
					sandState = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].ReAdhesionDevice is Sanders sanders && train.CurrentSpeed <= sanders.MaximumSpeed)
						{
							sandState = sanders.State == SandersState.Active ? 1 : 0;
							break;
						}
					}
					lastResult = sandState;
					break;
				case PanelSubject.Engine_Brake:
					if (!dynamicTrain.Handles.HasLocoBrake)
					{
						lastResult = 0;
						break;
					}
					for (int i = 0; i < FrameMapping.Length; i++)
					{
						if (FrameMapping[i].MappingValue >= (double)dynamicTrain.Handles.LocoBrake.Actual / dynamicTrain.Handles.LocoBrake.MaximumNotch)
						{
							lastResult = FrameMapping[i].FrameKey;
							break;
						}
					}
					break;
				case PanelSubject.Wheelslip:
					int wheelSlip = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].FrontAxle.CurrentWheelSlip || dynamicTrain.Cars[k].RearAxle.CurrentWheelSlip)
						{
							wheelSlip = 1;
							break;
						}
					}
					lastResult = wheelSlip;
					break;
				case PanelSubject.Clock:
					double hour = Math.Floor(CurrentHost.InGameTime / 3600.0);
					hour %= 24;
					if (UnitConversionFactor == 1)
					{
						if (hour > 12)
						{
							hour -= 12;
						}
					}
					double min = Math.Floor(CurrentHost.InGameTime / 60 % 60);
					double sec = CurrentHost.InGameTime % 60;
					switch (Digit)
					{
						case 7:
							// H
							lastResult = hour >= 10 ? (int)(hour / 10) : 0;
							break;
						case 6:
							// HH
							lastResult = (int)(hour % 10);
							break;
						case 5:
							// HH:
							lastResult = 11;
							break;
						case 4:
							// HH:M
							lastResult = min >= 10 ? (int)(min / 10) : 0;
							break;
						case 3:
							// HH:MM
							lastResult = (int)(min % 10);
							break;
						case 2:
							// HH:MM:
							lastResult = 11;
							break;
						case 1:
							// HH:MM:S
							lastResult = sec >= 10 ? (int)(sec / 10) : 0;
							break;
						case 0:
							// HH:MM:SS
							lastResult = (int)(sec % 10);
							break;
					}
					break;
				case PanelSubject.Alerter_Display:
					// can't use an extension on a dynamic directly, have to get to known type first
					Dictionary<SafetySystem, AbstractSafetySystem> safetySystems = dynamicTrain.Cars[dynamicTrain.DriverCar].SafetySystems;
					if (safetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out DriverSupervisionDevice dsd) && dsd.CurrentState == SafetySystemState.Alarm)
					{
						lastResult = 1;
					}
					if (safetySystems.TryGetTypedValue(SafetySystem.OverspeedDevice, out OverspeedDevice osd) && osd.CurrentState == SafetySystemState.Alarm)
					{
						lastResult = 1;
					}
					break;
				case PanelSubject.Penalty_App:
					safetySystems = dynamicTrain.Cars[dynamicTrain.DriverCar].SafetySystems;
					if (safetySystems.TryGetTypedValue(SafetySystem.DriverSupervisionDevice, out dsd) && dsd.CurrentState == SafetySystemState.Triggered)
					{
						lastResult = 1;
					}
					if (safetySystems.TryGetTypedValue(SafetySystem.OverspeedDevice, out osd) && osd.CurrentState == SafetySystemState.Triggered)
					{
						lastResult = 1;
					}
					break;
				case PanelSubject.Load_Meter:
				case PanelSubject.Ammeter:
				case PanelSubject.Ammeter_Abs:
					double amps = 0;
					if (dynamicTrain != null)
					{
						int totalMotors = 0;
						double ampsTotal = 0;
						for (int k = 0; k < dynamicTrain.Cars.Length; k++)
						{
							if (dynamicTrain.Cars[k].TractionModel is DieselEngine dieselEngine)
							{
								if (dieselEngine.Components.TryGetTypedValue(EngineComponent.TractionMotor, out TractionMotor t))
								{
									totalMotors++;
									ampsTotal += t.CurrentAmps;
								}
								else if (dieselEngine.Components.TryGetTypedValue(EngineComponent.RegenerativeTractionMotor, out RegenerativeTractionMotor rt))
								{
									totalMotors++;
									ampsTotal += rt.CurrentAmps;
								}
							}
						}

						if (totalMotors == 0)
						{
							amps = 0;
						}
						else
						{
							amps = ampsTotal / totalMotors;
							if (Subject == PanelSubject.Ammeter_Abs)
							{
								amps = Math.Abs(amps);
							}
						}
					}
					else
					{
						amps = 0;
					}
					MapDigitalResult(amps);
					break;
				case PanelSubject.Brake_Pipe:
					double bp = dynamicTrain.Cars[dynamicTrain.DriverCar].CarBrake.BrakePipe.CurrentPressure;
					bp *= UnitConversionFactor;
					MapDigitalResult(bp);
					break;
				case PanelSubject.Eq_Res:
					double er = dynamicTrain.Cars[dynamicTrain.DriverCar].CarBrake.EqualizingReservoir.CurrentPressure;
					er *= UnitConversionFactor;
					MapDigitalResult(er);
					break;
				case PanelSubject.Brake_Cyl:
					double bc = dynamicTrain.Cars[dynamicTrain.DriverCar].CarBrake.BrakeCylinder.CurrentPressure;
					bc *= UnitConversionFactor;
					MapDigitalResult(bc);
					break;
				case PanelSubject.Main_Res:
					double mr = dynamicTrain.Cars[dynamicTrain.DriverCar].CarBrake.MainReservoir.CurrentPressure;
					mr *= UnitConversionFactor;
					MapDigitalResult(mr);
					break;
				case PanelSubject.Cyl_Cocks:
					TractionModel tractionModel = dynamicTrain.Cars[carIndex].TractionModel;
					int cylinderCocksState = 0;
					if (tractionModel.Components.TryGetTypedValue(EngineComponent.CylinderCocks, out CylinderCocks cylinderCocks))
					{
						cylinderCocksState = cylinderCocks.Opened ? 1 : 0;
					}
					lastResult = cylinderCocksState;
					break;
				case PanelSubject.Blower:
					tractionModel = dynamicTrain.Cars[carIndex].TractionModel;
					int blowersState = 0;
					if (tractionModel.Components.TryGetTypedValue(EngineComponent.Blowers, out Blowers blowers))
					{
						blowersState = blowers.Active ? 1 : 0;
					}
					MapResult(blowersState);
					break;
			}
			return lastResult;
		}

		private void MapResult(double val)
		{
			for (int i = 0; i < FrameMapping.Length; i++)
			{
				if (FrameMapping[i].MappingValue >= val)
				{
					lastResult = FrameMapping[i].FrameKey;
					break;
				}
			}
		}

		private void MapDigitalResult(double val)
		{
			if (Digit == -1)
			{
				// color
				lastResult = FrameMapping[FrameMapping.Length - 1].FrameKey;
				for (int i = 0; i < FrameMapping.Length; i++)
				{
					if (FrameMapping[i].MappingValue <= val)
					{
						if (i == FrameMapping.Length - 1 || FrameMapping[i + 1].MappingValue > val)
						{
							lastResult = FrameMapping[i].FrameKey;
						}
						break;
					}
				}
			}
			else
			{
				// digit
				double absVal = Math.Abs(val);
				lastResult = (int)(absVal / (int)Math.Pow(10, Digit) % 10);
			}
		}

		public AnimationScript Clone()
		{
			return new CvfAnimation(CurrentHost, Subject, FrameMapping);
		}

		public double LastResult
		{
			get => lastResult;
			set { }
		}

		public double Maximum
		{
			get;
			set;
		}

		public double Minimum
		{
			get;
			set;
		}
	}
}
