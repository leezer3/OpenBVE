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

using OpenBveApi.Runtime;

namespace TrainManager.TractionModels.Steam
{
	/// <summary>A simple automatic fireman</summary>
	public class AutomaticFireman
	{
		/// <summary>Whether the Automatic Fireman is active</summary>
		public bool Active;
		/// <summary>The last response</summary>
		internal AIResponse lastAction;
		/// <summary>Holds a reference to the base steam engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Timer variable</summary>
		private double timer;
		/// <summary>Used to delay the next action</summary>
		private bool delayNextAction;

		internal AutomaticFireman(SteamEngine engine)
		{
			Engine = engine;
		}

		internal void Update(double timeElapsed)
		{
			timer += timeElapsed;
			switch (lastAction)
			{
				case AIResponse.Short:
					if (timer < 5)
					{
						return;
					}
					break;
				case AIResponse.Medium:
					if (timer < 15)
					{
						return;
					}
					break;
				case AIResponse.Long:
					if (timer < 30)
					{
						return;
					}
					break;
			}

			timer = 0;

			// Most worried about water level in the boiler
			if (Engine.Boiler.WaterLevel * Engine.Boiler.MaxWaterLevel < 0.6)
			{
				// Less than 60% water, so we need to add some
				if (Engine.Car.baseTrain.Handles.Power.Ratio > 0.3)
				{
					// Use the exhaust steam injector at more than 30% power
					if (!Engine.Boiler.ExhaustSteamInjector.Active)
					{
						Engine.Boiler.ExhaustSteamInjector.Active = true;
						lastAction = AIResponse.Medium;
						return;
					}
				}
				else
				{
					if (!Engine.Boiler.LiveSteamInjector.Active)
					{
						Engine.Boiler.LiveSteamInjector.Active = true;
						lastAction = AIResponse.Medium;
						return;
					}
				}
			}
			else if (Engine.Boiler.WaterLevel * Engine.Boiler.MaxWaterLevel < 0.2)
			{
				// Don't mess around with the exhaust steam injector at less than 20% water level!
				if (!Engine.Boiler.LiveSteamInjector.Active)
				{
					Engine.Boiler.LiveSteamInjector.Active = true;
					lastAction = AIResponse.Medium;
					return;
				}
			}

			// Now think about the fire- 80% mass is fine to do nothing
			if (Engine.Boiler.Firebox.FireMass < Engine.Boiler.Firebox.MaxArea * 16)
			{
				// With below 80% fire mass, fireman *thinks* about adding fuel, but we must either be hot enough for the temp drop not to matter or low on fuel
				if (Engine.Boiler.Firebox.FireMass < Engine.Boiler.Firebox.MaxArea * 8 || Engine.Boiler.Firebox.Temperature > Engine.Boiler.Firebox.MaxTemperature * 0.8)
				{
					Engine.Boiler.Firebox.AddFuel();
					lastAction = AIResponse.Long;
					return;
				}
				
			}

			// Apply cylinder cocks when appropriate
			if (Engine.Car.CurrentSpeed == 0)
			{
				// Check that we're not below working pressure- It's pointless to blast everything out if so
				if (!(Engine.Car.baseTrain.Handles.Power.Ratio > 0.5 && Engine.Boiler.SteamPressure < Engine.Boiler.MinWorkingSteamPressure))
				{
					if (!Engine.CylinderChest.CylinderCocks.Open)
					{
						if (!delayNextAction)
						{
							lastAction = AIResponse.Long;
							delayNextAction = true;
							return;
						}

						delayNextAction = false;
						Engine.CylinderChest.CylinderCocks.Open = true;
					}
				}
				
			}
			else
			{
				if (Engine.CylinderChest.CylinderCocks.Open)
				{
					if (!delayNextAction)
					{
						lastAction = AIResponse.Medium;
						delayNextAction = true;
						return;
					}

					delayNextAction = false;
					Engine.CylinderChest.CylinderCocks.Open = false;
				}
			}
		}
	}
}
