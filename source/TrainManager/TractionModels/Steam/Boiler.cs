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
		private readonly double BaseSteamGenerationRate;
		/// <summary>The live steam injector</summary>
		public LiveSteamInjector LiveSteamInjector;
		/// <summary>The exhaust steam injector</summary>
		public ExhaustSteamInjector ExhaustSteamInjector;
		/// <summary>The firebox</summary>
		public Firebox Firebox;
		/// <summary>The blowers</summary>
		public readonly Blowers Blowers;
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
		/// <summary>The rate water is converted to steam</summary>
		public double SteamGenerationRate => BaseSteamGenerationRate * Firebox.ConversionRate;

		private bool startSoundPlayed;

		internal Boiler(SteamEngine engine, double waterLevel, double maxWaterLevel, double steamPressure, double maxSteamPressure, double blowoffPressure, double minWorkingSteamPressure, double baseSteamGenerationRate)
		{
			Engine = engine;
			WaterLevel = waterLevel;
			MaxWaterLevel = maxWaterLevel;
			SteamPressure = steamPressure;
			MaxSteamPressure = maxSteamPressure;
			BlowoffPressure = blowoffPressure;
			MinWorkingSteamPressure = minWorkingSteamPressure;
			BaseSteamGenerationRate = baseSteamGenerationRate;
			/* More fudged averages for a large steam loco
			 * Base injection rate of 3L /s
			 * based on Davies and Metcalfe Monitor Type 11 (large tender locos)
			 */
			LiveSteamInjector = new LiveSteamInjector(engine, 3.0);
			ExhaustSteamInjector = new ExhaustSteamInjector(engine, 3.0);
			/*
			 * 10m square fire
			 * 1000c max temp
			 * 0.25kg of coal per sec ==> + 1c (burn rate of 1kg lasting 1 hour into 1000c)
			 * Fireman adds 3kg per shovelful
			 */
			Firebox = new Firebox(engine, 10, 1000, 0.25, 3);
			/*
			 * Double temp increase / fuel use
			 * Use approx 1psi / sec
			 */
			Blowers = new Blowers(engine, 2, 1);
		}

		internal void Update(double timeElapsed)
		{
			// injectors update first
			LiveSteamInjector.Update(timeElapsed);
			ExhaustSteamInjector.Update(timeElapsed);
			// now firebox
			Firebox.Update(timeElapsed);
			// convert water to steam pressure
			WaterLevel -= SteamGenerationRate;
			SteamPressure += SteamGenerationRate;
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
			Blowers.Update(timeElapsed);
		}
	}
}
