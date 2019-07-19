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
	}
}
