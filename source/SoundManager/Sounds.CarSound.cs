using OpenBveApi.Math;

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
		public Vector3 Position;
		
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
