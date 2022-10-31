using System;
using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class ExhaustSteamInjector
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The base injection rate</summary>
		private readonly double BaseInjectionRate;
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
		public CarSound InjectingLoopSound;
		/// <summary>The sound played when the injector is active and idle</summary>
		public CarSound IdleLoopSound;
		/// <summary>The sound played when the injector is stopped</summary>
		public CarSound StopSound;

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
				if(StartSound == null || !StartSound.IsPlaying)
				{
					if (BaseInjectionRate != 0)
					{
						if (InjectingLoopSound != null && !InjectingLoopSound.IsPlaying)
						{
							InjectingLoopSound.Play(Engine.Car, true);
						}
					}
					else
					{
						if (IdleLoopSound != null && !IdleLoopSound.IsPlaying)
						{
							InjectingLoopSound.Play(Engine.Car, true);
						}
					}
					
				}
			}
			else
			{
				if (InjectingLoopSound != null)
				{
					InjectingLoopSound.Stop();
				}
				if (IdleLoopSound != null)
				{
					IdleLoopSound.Stop();
				}
			}
		}
	}
}
