using OpenBveApi.Trains;
using SoundManager;

namespace TrainManager.SafetySystems
{
	public class PassAlarm
	{
		/// <summary>Holds the reference to the base train's driver car</summary>
		private readonly AbstractCar baseCar;
		/// <summary>The type of pass alarm</summary>
		private readonly PassAlarmType Type;
		/// <summary>The sound played when this alarm is triggered</summary>
		public CarSound Sound;
		/// <summary>Whether the pass alarm light is currently lit</summary>
		public bool Lit;

		/// <summary>Creates a new PassAlarm</summary>
		/// <param name="type">The type of PassAlarm</param>
		/// <param name="Car">A reference to the base car</param>
		public PassAlarm(PassAlarmType type, AbstractCar Car)
		{
			this.baseCar = Car;
			this.Type = type;
			this.Sound = new CarSound();
			this.Lit = false;
		}

		/// <summary>Triggers the pass alarm</summary>
		public void Trigger()
		{
			Lit = true;
			SoundBuffer buffer = Sound.Buffer;
			if (TrainManagerBase.currentHost.SoundIsPlaying(Sound.Source))
			{
				return;
			}
			if (buffer != null)
			{
				switch (Type)
				{
					case PassAlarmType.Single:
						Sound.Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(buffer, 1.0, 1.0, Sound.Position, baseCar, false);
						break;
					case PassAlarmType.Loop:
						Sound.Source = (SoundSource)TrainManagerBase.currentHost.PlaySound(buffer, 1.0, 1.0, Sound.Position, baseCar, true);
						break;
				}
			}
		}
		/// <summary>Halts the pass alarm</summary>
		public void Halt()
		{
			Lit = false;
			if (Sound != null)
			{
				Sound.Stop();
			}
		}
	}
	/// <summary>Defines the differing types of station pass alarm a train may be fitted with</summary>
	public enum PassAlarmType
	{
		/// <summary>No pass alarm</summary>
		None = 0,
		/// <summary>The alarm sounds once</summary>
		Single = 1,
		/// <summary>The alarm loops until cancelled</summary>
		Loop = 2
	}
}
