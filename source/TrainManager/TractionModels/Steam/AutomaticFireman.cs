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

			// Now think about the fire
			if (Engine.Boiler.Firebox.FireMass < Engine.Boiler.Firebox.MaxArea * 200)
			{
				Engine.Boiler.Firebox.AddFuel();
				lastAction = AIResponse.Long;
				return;
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
