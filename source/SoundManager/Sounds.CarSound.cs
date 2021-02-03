using System;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace SoundManager
{
	/// <summary>Defines a sound attached to a train car</summary>
	public class CarSound
	{
		/// <summary>The sound buffer to play</summary>
		public readonly SoundBuffer Buffer;
		/// <summary>The source of the sound within the car</summary>
		public SoundSource Source;
		/// <summary>A Vector3 describing the position of the sound source within the base car</summary>
		private readonly Vector3 Position;
		/// <summary>The target volume of the sound</summary>
		/// <remarks>Used when crossfading between multiple sounds of the same type</remarks>
		public double TargetVolume;
		
		/// <summary>Creates a new car sound</summary>
		/// <param name="buffer">The sound buffer</param>
		/// <param name="Position">The position that the sound is emitted from within the car</param>
		/// <returns>The new car sound</returns>
		public CarSound(SoundBuffer buffer, Vector3 Position)
		{
			this.Position = Position;
			this.Source = null;
			this.Buffer = buffer;
		}

		/// <summary>Creates a new empty car sound</summary>
		public CarSound()
		{
			this.Position = Vector3.Zero;
			this.Source = null;
			this.Buffer = null;
		}

		/// <summary>Plays the sound</summary>
		/// <param name="pitch">The pitch</param>
		/// <param name="volume">The volume</param>
		/// <param name="Car">The parent car</param>
		/// <param name="looped">Whether the sound is to be played looped</param>
		public void Play(double pitch, double volume, AbstractCar Car, bool looped)
		{
			if (Buffer != null)
			{
				if (SoundsBase.Sources.Length == SoundsBase.SourceCount)
				{
					Array.Resize(ref SoundsBase.Sources, SoundsBase.Sources.Length << 1);
				}
				SoundsBase.Sources[SoundsBase.SourceCount] = new SoundSource(Buffer, Buffer.Radius, pitch, volume, Position, Car, looped);
				this.Source = SoundsBase.Sources[SoundsBase.SourceCount];
				SoundsBase.SourceCount++;
			}
			
		}

		/// <summary>Unconditionally stops the playing sound</summary>
		public void Stop()
		{
			if (Source == null)
			{
				return;
			}
			Source.Stop();
		}
	}
}
