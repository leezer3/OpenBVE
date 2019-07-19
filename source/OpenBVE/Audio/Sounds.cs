using System.IO;
using System.Linq;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;
using OpenTK.Audio.OpenAL;
using SoundManager;

namespace OpenBve
{
	internal partial class Sounds : SoundsBase
	{
		/// <summary>Plays a car sound.</summary>
		/// <param name="sound">The car sound.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="car">The train car sound is attached to.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal void PlayCarSound(CarSound sound, double pitch, double volume, AbstractCar car, bool looped)
		{
			if (sound.Buffer == null)
			{
				return;
			}
			if (car == null)
			{
				throw new InvalidDataException("A valid car must be specified");
			}
			sound.Source = PlaySound(sound.Buffer, pitch, volume, sound.Position, car, looped);
		}

		/// <summary>Stops all sounds that are attached to the specified train.</summary>
		/// <param name="train">The train.</param>
		public override void StopAllSounds(object train)
		{
			if (train is TrainManager.Train)
			{
				var t = (TrainManager.Train) train;
				for (int i = 0; i < SourceCount; i++)
				{
					if (t.Cars.Contains(Sources[i].Parent))
					{
						if (Sources[i].State == SoundSourceState.Playing)
						{
							AL.DeleteSources(1, ref Sources[i].OpenAlSourceName);
							Sources[i].OpenAlSourceName = 0;
						}
						Sources[i].State = SoundSourceState.Stopped;
					}
				}
			}
		}
	}
}
