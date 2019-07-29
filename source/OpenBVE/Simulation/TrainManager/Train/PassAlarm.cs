using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		internal class PassAlarm
		{
			/// <summary>Holds the reference to the base train</summary>
			private readonly TrainManager.Train baseTrain;
			/// <summary>The type of pass alarm</summary>
			internal PassAlarmType Type;
			/// <summary>The sound played when this alarm is triggered</summary>
			internal CarSound Sound;

			public PassAlarm(Train baseTrain)
			{
				this.baseTrain = baseTrain;
				this.Type = PassAlarmType.None;
				this.Sound = new CarSound();
			}
			/// <summary>Triggers the pass alarm</summary>
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
			/// <summary>Halts the pass alarm</summary>
			internal void Halt()
			{
				if (Sound != null)
				{
					Sound.Source.Stop();
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
