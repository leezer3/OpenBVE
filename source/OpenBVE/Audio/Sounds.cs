using System.IO;
using OpenBveApi.Trains;
using SoundManager;

namespace OpenBve
{
	internal partial class Sounds : SoundsBase
	{
		/// <summary>Plays a car sound.</summary>
		/// <param name="sound">The car sound.</param>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="train">The train the sound is attached to.</param>
		/// <param name="car">The car in the train the sound is attached to.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		/// <returns>The sound source.</returns>
		internal void PlayCarSound(TrainManager.CarSound sound, double pitch, double volume, AbstractTrain train, int car, bool looped)
		{
			if (sound.Buffer == null)
			{
				return;
			}
			if (train == null)
			{
				throw new InvalidDataException("A train and car must be specified");
			}
			sound.Source = PlaySound(sound.Buffer, pitch, volume, sound.Position, train, car, looped);
		}
	}
}
