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
using OpenBveApi;
using TrainManager.Car.Systems;
using TrainManager.Motor;

namespace Train.MsTs
{
	/// <summary>Animation class handling CVF elements with keyframe mappings</summary>
	internal class CvfAnimation : AnimationScript
	{
		internal readonly PanelSubject Subject;

		internal readonly FrameMapping[] FrameMapping;

		internal readonly int Digit;

		internal readonly double UnitConversionFactor;

		private int lastResult;

		internal CvfAnimation(PanelSubject subject)
		{
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

		internal CvfAnimation(PanelSubject subject, FrameMapping[] frameMapping)
		{
			Subject = subject;
			FrameMapping = frameMapping;
			Minimum = 0;
			Maximum = FrameMapping.Length;
			Digit = -1;
		}

		internal CvfAnimation(PanelSubject subject, Units unit, int digit)
		{
			Subject= subject;
			switch (unit)
			{
				case Units.Miles_Per_Hour:
					UnitConversionFactor = 2.2369362920544;
					break;
				case Units.Kilometers_Per_Hour:
					UnitConversionFactor = 3.6;
					break;
			}
			Digit = digit;
		}

		internal CvfAnimation(PanelSubject subject, Units unit, FrameMapping[] frameMapping)
		{
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

		public double ExecuteScript(AbstractTrain train, int carIndex, Vector3 position, double trackPosition, int sectionIndex, bool isPartOfTrain, double timeElapsed, int currentState)
		{
			dynamic dynamicTrain = train;
			switch (Subject)
			{
				case PanelSubject.Throttle:
					for (int i = 0; i < FrameMapping.Length; i++)
					{
						if (FrameMapping[i].MappingValue >= (double)dynamicTrain.Handles.Power.Actual / dynamicTrain.Handles.Power.MaximumNotch)
						{
							lastResult = FrameMapping[i].FrameKey;
							break;
						}
					}
					break;
				case PanelSubject.Train_Brake:
					for (int i = 0; i < FrameMapping.Length; i++)
					{
						if (FrameMapping[i].MappingValue  >= (double)dynamicTrain.Handles.Brake.Actual / dynamicTrain.Handles.Brake.MaximumNotch)
						{
							lastResult = FrameMapping[i].FrameKey;
							break;
						}
					}
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
						if (speedLim == double.PositiveInfinity)
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
					if (Digit == -1)
					{
						// color
						for (int i = FrameMapping.Length - 1; i > 0; i--)
						{
							if (FrameMapping[i].MappingValue <= currentSpeed)
							{
								lastResult = FrameMapping[i].FrameKey;
								break;
							}
						}
					}
					else
					{
						// digit
						lastResult = (int)(currentSpeed / (int)Math.Pow(10, Digit) % 10);
					}
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
					int sandState = 0;
					for (int k = 0; k < dynamicTrain.Cars.Length; k++)
					{
						if (dynamicTrain.Cars[k].ReAdhesionDevice is Sanders sanders)
						{
							sandState = sanders.Active ? 1 :0;
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
			}
			return lastResult;
		}

		public AnimationScript Clone()
		{
			return new CvfAnimation(Subject, FrameMapping);
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
