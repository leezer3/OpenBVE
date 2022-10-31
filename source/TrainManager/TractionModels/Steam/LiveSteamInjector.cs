using System;
using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class LiveSteamInjector
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The base injection rate</summary>
		private readonly double BaseInjectionRate;
		/// <summary>The injection rate whilst active</summary>
		public double InjectionRate
		{
			get
			{
				if (Engine.Boiler.SteamPressure < Engine.Boiler.MinWorkingSteamPressure)
				{
					return BaseInjectionRate * (Engine.Boiler.SteamPressure * Engine.Boiler.MinWorkingSteamPressure);
				}
				return BaseInjectionRate * Math.Abs(Engine.Car.baseTrain.Handles.Reverser.Actual) * Engine.Car.baseTrain.Handles.Power.Ratio;
			}
		}
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
							StartSound.Play(Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Active)
					{
						if (StopSound != null)
						{
							StopSound.Play(Engine.Car, false);
						}
					}
				}
				_Active = value;
			}
		}
		/// <summary>Backing property for Active</summary>
		private bool _Active;
		/// <summary>The sound played when the injector is activated</summary>
		public CarSound StartSound;
		/// <summary>The sound played when the injector is active and working</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the injector is stopped</summary>
		public CarSound StopSound;


		internal LiveSteamInjector(SteamEngine engine, double baseInjectionRate)
		{
			Engine = engine;
			BaseInjectionRate = baseInjectionRate;
		}

		public void Update(double timeElapsed)
		{
			if (Active)
			{
				// increase water level and decrease steam pressure
				double waterInjected = Math.Min(Engine.Tender.WaterLevel, InjectionRate * timeElapsed);
				Engine.Boiler.WaterLevel += waterInjected;
				Engine.Boiler.SteamPressure -= waterInjected;
				Engine.Tender.WaterLevel -= waterInjected;
				if(StartSound == null || !StartSound.IsPlaying)
				{
					if (LoopSound != null)
					{
						LoopSound.Play(Engine.Car, true);
					}
				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();
					
				}
			}
		}
	}
}
