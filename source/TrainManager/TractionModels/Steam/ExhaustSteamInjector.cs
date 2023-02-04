//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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
using SoundManager;
using TrainManager.Trains;

namespace TrainManager.TractionModels.Steam
{
	public class ExhaustSteamInjector : AbstractDevice
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The base injection rate</summary>
		public double BaseInjectionRate;
		/// <summary>The injection rate whilst active</summary>
		public double InjectionRate => BaseInjectionRate * Math.Abs(Engine.Car.baseTrain.Handles.Reverser.Actual) * Engine.Car.baseTrain.Handles.Power.Ratio;
		/// <summary>Whether the injector is active</summary>
		public bool Active
		{
			get => _Active;
			set
			{
				if (value)
				{
					if (!_Active)
					{
						if (StartSound != null)
						{
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(StartSound, 1.0, 1.0, SoundPosition, Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Active)
					{
						if (EndSound != null)
						{
							Source.Stop();
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(EndSound, 1.0, 1.0, SoundPosition, Engine.Car, false);
						}
					}
				}
				_Active = value;
			}
		}
		/// <summary>Backing property for Active</summary>
		private bool _Active;

		internal ExhaustSteamInjector(SteamEngine engine, double baseInjectionRate)
		{
			Engine = engine;
			BaseInjectionRate = baseInjectionRate;
		}

		public void Update(double timeElapsed)
		{
			if (Active)
			{
				// just increase water level here
				double waterInjected = Math.Min(Engine.Tender.WaterLevel, InjectionRate * timeElapsed);
				Engine.Boiler.WaterLevel += waterInjected;
				Engine.Tender.WaterLevel -= waterInjected;
				if(StartSound == null || !Source.IsPlaying())
				{
					if (BaseInjectionRate != 0)
					{
						if (LoopSound != null)
						{
							Source.Stop();
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, Engine.Car, true);
						}
					}
					else
					{
						if (IdleLoopSound != null)
						{
							Source.Stop();
							Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(IdleLoopSound, 1.0, 1.0, SoundPosition, Engine.Car, true);
						}
					}
					
				}
			}
			else
			{
				Source.Stop();
			}
		}
	}
}
