using System.IO;
using OpenBveApi.Math;
using SoundManager;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Defines a sound attached to a train car</summary>
		internal struct CarSound
		{
			/// <summary>The sound buffer to play</summary>
			internal readonly SoundsBase.SoundBuffer Buffer;
			/// <summary>The source of the sound within the car</summary>
			internal SoundsBase.SoundSource Source;
			/// <summary>A Vector3 describing the position of the sound source</summary>
			internal Vector3 Position;
			private CarSound(SoundsBase.SoundBuffer buffer, SoundsBase.SoundSource source, Vector3 position)
			{
				this.Buffer = buffer;
				this.Source = source;
				this.Position = position;
				
			}

			/// <summary>Defines a default empty sound</summary>
			internal static readonly CarSound Empty = new CarSound(null, null, Vector3.Zero);

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
					if (File.Exists(FileName))
					{
						Buffer = Program.Sounds.RegisterBuffer(FileName, Radius);
					}
				}
			}
		}
	}
}
