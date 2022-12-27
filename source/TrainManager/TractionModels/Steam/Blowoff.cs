using SoundManager;
using TrainManager.Trains;

namespace TrainManager.TractionModels.Steam
{
	public class Blowoff : AbstractDevice
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The blowoff pressure</summary>
		public double Pressure;
		/// <summary>The pressure at which the blowoff resets</summary>
		public double EndPressure;
		/// <summary>The rate of steam loss via the blowoff</summary>
		public double Rate;

		public Blowoff(SteamEngine engine, double rate, double pressure, double endPressure)
		{
			Engine = engine;
			Rate = rate;
			Pressure = pressure;
			EndPressure = endPressure;
		}

		public void Update(double timeElapsed)
		{
			if (Engine.Boiler.SteamPressure < Pressure)
			{
				Source.Stop();
				if (EndSound != null)
				{
					Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(EndSound, 1.0, 1.0, SoundPosition, Engine.Car, false);
				}
				return;
			}
			Engine.Boiler.SteamPressure -= Rate * timeElapsed;
			if (StartSound != null)
			{
				Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(StartSound, 1.0, 1.0, SoundPosition, Engine.Car, false);
			}

			if (Source.IsPlaying())
			{
				return;
			}
			if (LoopSound != null)
			{
				Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, Engine.Car, true);
			}
		}
	}
}
