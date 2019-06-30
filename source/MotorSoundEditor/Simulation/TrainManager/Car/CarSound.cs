using System.IO;
using MotorSoundEditor.Audio;
using OpenBveApi.Math;

namespace MotorSoundEditor.Simulation.TrainManager
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>Defines a sound attached to a train car</summary>
		internal struct CarSound
		{
			/// <summary>The sound buffer to play</summary>
			internal readonly Sounds.SoundBuffer Buffer;

			/// <summary>The source of the sound within the car</summary>
			internal Sounds.SoundSource Source;

			/// <summary>A Vector3 describing the position of the sound source</summary>
			internal Vector3 Position;

			private CarSound(Sounds.SoundBuffer buffer, Sounds.SoundSource source, Vector3 position)
			{
				Buffer = buffer;
				Source = source;
				Position = position;

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
				this = Empty;
				this.Position = Position;
				Source = null;

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
