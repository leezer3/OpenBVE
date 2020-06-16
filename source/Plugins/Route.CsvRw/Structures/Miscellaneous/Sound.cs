using OpenBveApi.Math;
using OpenBveApi.Sounds;

namespace CsvRwRouteParser
{
	internal class Sound
	{
		/// <summary>The track position of the sound</summary>
		internal readonly double TrackPosition;
		/// <summary>The sound buffer to play</summary>
		internal readonly SoundHandle SoundBuffer;
		/// <summary>The type of sound</summary>
		internal readonly Parser.SoundType Type;
		/// <summary>The relative sound position to the track position</summary>
		internal readonly Vector2 Position;
		/// <summary>The radius of the sound</summary>
		internal readonly double Radius;
		/// <summary>If a dynamic sound, the train speed at which the sound will be played at original speed</summary>
		internal readonly double Speed;
		/// <summary>Whether this is a MicSound</summary>
		internal readonly bool IsMicSound;
		/// <summary>The forwards tolerance for triggering the sound</summary>
		internal readonly double ForwardTolerance;
		/// <summary>The backwards tolerance for triggering the sound</summary>
		internal readonly double BackwardTolerance;

		internal Sound(double trackPosition, string fileName, double speed, Vector2 position = new Vector2(), double forwardTolerance = 0, double backwardTolerance = 0)
		{
			//TODO:
			//This is always set to a constant 15.0 on loading a sound, and never touched again
			//I presume Michelle intended to have sounds with different radii available
			//This would require a custom or extended command which allowed the radius value to be set
			Radius = 15.0;
			if (string.IsNullOrEmpty(fileName))
			{
				IsMicSound = true;
			}
			else
			{
				Plugin.CurrentHost.RegisterSound(fileName, Radius, out SoundBuffer);
			}
			
			switch (speed)
			{
				case -1:
					Type = Parser.SoundType.World;
					break;
				case 0:
					Type = Parser.SoundType.TrainStatic;
					break;
				default:
					Type = Parser.SoundType.TrainDynamic;
					break;
			}
			
			TrackPosition = trackPosition;
			Speed = speed;
			Position = position;
			ForwardTolerance = forwardTolerance;
			BackwardTolerance = backwardTolerance;
		}
	}
}
