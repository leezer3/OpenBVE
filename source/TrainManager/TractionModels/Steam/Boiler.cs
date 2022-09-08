using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class Boiler
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The boiler water level</summary>
		public double WaterLevel;
		/// <summary>The maximum water level</summary>
		public readonly double MaxWaterLevel;
		/// <summary>The steam pressure level</summary>
		public double SteamPressure;
		/// <summary>The maximum steam pressure</summary>
		public readonly double MaxSteamPressure;
		/// <summary>The minimum working steam pressure</summary>
		public readonly double MinWorkingSteamPressure;
		/// <summary>The base water to steam conversion rate</summary>
		private readonly double BaseSteamConversionRate;
		/// <summary>The live steam injector</summary>
		public LiveSteamInjector LiveSteamInjector;
		/// <summary>The exhaust steam injector</summary>
		public ExhaustSteamInjector ExhaustSteamInjector;
		/// <summary>The firebox</summary>
		public Firebox Firebox;
		/// <summary>The blowoff pressure</summary>
		public double BlowoffPressure;
		/// <summary>The rate of steam loss via the blowoff</summary>
		public double BlowoffRate;
		/// <summary>The blowoff start sound</summary>
		public CarSound BlowoffStartSound;
		/// <summary>The blowoff loop sound</summary>
		public CarSound BlowoffLoopSound;
		/// <summary>The blowoff end sound</summary>
		public CarSound BlowoffEndSound;

		private bool startSoundPlayed;

		internal Boiler(SteamEngine engine, double waterLevel, double maxWaterLevel, double steamPressure, double maxSteamPressure, double baseSteamConversionRate, double minWorkingSteamPressure)
		{
			Engine = engine;
			WaterLevel = waterLevel;
			MaxWaterLevel = maxWaterLevel;
			SteamPressure = steamPressure;
			MaxSteamPressure = maxSteamPressure;
			BaseSteamConversionRate = baseSteamConversionRate;
			MinWorkingSteamPressure = minWorkingSteamPressure;
			LiveSteamInjector = new LiveSteamInjector(engine, 1.0);
			ExhaustSteamInjector = new ExhaustSteamInjector(engine, 1.0);
			Firebox = new Firebox(engine, 100, 250, 1);
		}

		internal void Update(double timeElapsed)
		{
			// injectors update first
			LiveSteamInjector.Update(timeElapsed);
			ExhaustSteamInjector.Update(timeElapsed);
			// now firebox
			Firebox.Update(timeElapsed);
			// convert water to steam pressure
			double waterToSteam = BaseSteamConversionRate * Firebox.ConversionRate;
			WaterLevel -= waterToSteam;
			SteamPressure += waterToSteam;
			// handle blowoff
			if (SteamPressure > BlowoffPressure)
			{
				SteamPressure -= BlowoffRate;
				if (!startSoundPlayed)
				{
					if (BlowoffStartSound != null)
					{
						BlowoffStartSound.Play(Engine.Car, false);
					}
					startSoundPlayed = true;
				}
				else if (!BlowoffStartSound.IsPlaying)
				{
					if (BlowoffLoopSound != null)
					{
						BlowoffLoopSound.Play(Engine.Car, true);
					}
				}
			}
			else
			{
				if (startSoundPlayed)
				{
					if (BlowoffLoopSound != null && BlowoffLoopSound.IsPlaying)
					{
						BlowoffLoopSound.Stop();
					}
					if (BlowoffEndSound != null)
					{
						BlowoffEndSound.Play(Engine.Car, false);
					}
					startSoundPlayed = false;
				}
			}
		}
	}
}
