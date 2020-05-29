using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve.SafetySystems
{
	internal class PassAlarm
	{
		/// <summary>The type of pass alarm</summary>
		private readonly PassAlarmType Type;
		/// <summary>The sound played when this alarm is triggered</summary>
		internal CarSound Sound;
		/// <summary>Whether the pass alarm light is currently lit</summary>
		internal bool Lit;

		public PassAlarm(PassAlarmType type)
		{
			this.Type = type;
			this.Sound = new CarSound();
			this.Lit = false;
		}

		/// <summary>Triggers the pass alarm</summary>
		internal void Trigger()
		{
			Lit = true;
			if (Program.Sounds.IsPlaying(Sound.Source))
			{
				return;
			}
			switch (Type)
			{
				case PassAlarmType.Single:
					Sound.Play(1.0, 1.0,false);
					break;
				case PassAlarmType.Loop:
					Sound.Play(1.0, 1.0,true);
					break;
			}

		}

		/// <summary>Halts the pass alarm</summary>
		internal void Halt()
		{
			Lit = false;
			if (Sound != null)
			{
				Sound.Stop();
			}
		}
	}
	/// <summary>Defines the differing types of station pass alarm a train may be fitted with</summary>
	internal enum PassAlarmType
	{
		/// <summary>No pass alarm</summary>
		None = 0,
		/// <summary>The alarm sounds once</summary>
		Single = 1,
		/// <summary>The alarm loops until cancelled</summary>
		Loop = 2
	}
}
