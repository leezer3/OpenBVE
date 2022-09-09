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
				return BaseInjectionRate * Math.Abs(Engine.Car.baseTrain.Handles.Reverser.Actual) * Math.Abs(Engine.Regulator.Current);
			}
		}
		/// <summary>Whether the injector is active</summary>
		public bool Active;
		/// <summary>The sound played when the injector is activated</summary>
		public CarSound StartSound;
		/// <summary>Whether the start sound has been played</summary>
		private bool startSoundPlayed;
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
				Engine.Boiler.WaterLevel += InjectionRate * timeElapsed;
				Engine.Boiler.SteamPressure -= InjectionRate * timeElapsed;
				if (!startSoundPlayed)
				{
					if (StartSound != null)
					{
						StartSound.Play(Engine.Car, false);
					}
					
					startSoundPlayed = true;
				}
				else if(!StartSound.IsPlaying)
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
				if (startSoundPlayed && StopSound != null)
				{
					StopSound.Play(Engine.Car, false);
				}
				startSoundPlayed = false;
			}
		}
	}
}
