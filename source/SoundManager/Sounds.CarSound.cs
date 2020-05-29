using System;
using System.IO;
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
		/// <summary>A Vector3 describing the position of the sound source</summary>
		public readonly Vector3 Position;
		/// <summary>The car this sound is attached to</summary>
		private readonly AbstractCar Car;

		/// <summary>Creates a new car sound</summary>
		/// <param name="buffer">The sound buffer</param>
		/// <param name="position">The position that the sound is emitted from within the car</param>
		/// <param name="car">The parent car reference</param>
		/// <returns>The new car sound</returns>
		public CarSound(SoundBuffer buffer, Vector3 position, AbstractCar car)
		{
			this.Position = position;
			this.Source = null;
			this.Buffer = buffer;
			this.Car = car;
		}

		/// <summary>Creates a new empty car sound</summary>
		public CarSound()
		{
			this.Position = Vector3.Zero;
			this.Source = null;
			this.Buffer = null;
		}

		/// <summary>Plays the sound.</summary>
		/// <param name="pitch">The pitch change factor.</param>
		/// <param name="volume">The volume change factor.</param>
		/// <param name="looped">Whether to play the sound in a loop.</param>
		public void Play(double pitch, double volume, bool looped)
		{
			if (Buffer == null)
			{
				return;
			}
			if (Car == null)
			{
				throw new InvalidDataException("Sound does not have a valid car specified");
			}
			if (SoundsBase.Sources.Length == SoundsBase.SourceCount)
			{
				Array.Resize(ref SoundsBase.Sources, SoundsBase.Sources.Length << 1);
			}
			SoundsBase.Sources[SoundsBase.SourceCount] = new SoundSource(Buffer, Buffer.Radius, pitch, volume, Position, Car, looped);
			this.Source = SoundsBase.Sources[SoundsBase.SourceCount];
			SoundsBase.SourceCount++;
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

		/// <summary>Gets the duration of the sound</summary>
		public double Duration
		{
			get
			{
				Buffer.Load();
				return Buffer.Duration;
			}
		}
	}
}
