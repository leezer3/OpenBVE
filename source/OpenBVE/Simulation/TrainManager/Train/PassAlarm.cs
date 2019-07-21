using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal struct PassAlarm
		{
			private readonly TrainManager.Train baseTrain;

			internal PassAlarmType Type;

			internal CarSound Sound;

			internal SoundSource Source
			{
				get
				{
					return Sound.Source;
				}
			}

			public PassAlarm(Train baseTrain)
			{
				this.baseTrain = baseTrain;
				this.Type = PassAlarmType.None;
				this.Sound = new CarSound();
			}

			internal void Trigger()
			{
				SoundBuffer buffer = Sound.Buffer;
				if (buffer != null)
				{
					switch (Type)
					{
						case PassAlarmType.Single:
							Sound.Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, Sound.Position, baseTrain.Cars[baseTrain.DriverCar], false);
							break;
						case PassAlarmType.Loop:
							Sound.Source = Program.Sounds.PlaySound(buffer, 1.0, 1.0, Sound.Position, baseTrain.Cars[baseTrain.DriverCar], true);
							break;
					}
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
}
