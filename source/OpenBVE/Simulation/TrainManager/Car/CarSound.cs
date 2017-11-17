using OpenBveApi.Math;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Defines a sound attached to a train car</summary>
		internal struct CarSound
		{
			/// <summary>The sound buffer to play</summary>
			internal Sounds.SoundBuffer Buffer;
			/// <summary>The source of the sound within the car</summary>
			internal Sounds.SoundSource Source;
			/// <summary>A Vector3 describing the position of the sound source</summary>
			internal Vector3 Position;
			private CarSound(Sounds.SoundBuffer buffer, Sounds.SoundSource source, Vector3 position)
			{
				this.Buffer = buffer;
				this.Source = source;
				this.Position = position;
				
			}

			/// <summary>Defines a default empty sound</summary>
			internal static readonly CarSound Empty = new CarSound(null, null, new Vector3(0.0, 0.0, 0.0));

			/// <summary>Attempts to load a sound file into a car-sound</summary>
			/// <param name="FileName">The sound to load</param>
			/// <param name="Position">The position that the sound is emitted from within the car</param>
			/// <param name="Radius">The sound radius</param>
			/// <returns>The new car sound, or an empty car sound if load fails</returns>
			internal CarSound(string FileName, Vector3 Position, double Radius)
			{
				this = TrainManager.CarSound.Empty;
				this.Position = Position;
				this.Source = null;
				if (FileName != null)
				{
					if (System.IO.File.Exists(FileName))
					{
						this.Buffer = Sounds.RegisterBuffer(FileName, Radius);
					}
				}
			}
		}
	}
}
