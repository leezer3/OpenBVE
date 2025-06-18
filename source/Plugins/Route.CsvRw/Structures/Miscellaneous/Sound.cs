using System;
using OpenBveApi.Math;
using OpenBveApi.Sounds;

namespace CsvRwRouteParser
{
	internal class Sound : AbstractStructure
	{
		/// <summary>The sound buffer to play</summary>
		internal readonly SoundHandle SoundBuffer;
		/// <summary>The type of sound</summary>
		internal readonly SoundType Type;
		/// <summary>The relative sound position to the track position</summary>
		internal readonly Vector2 Position;
		/// <summary>The radius of the sound</summary>
		internal readonly double Radius;
		/// <summary>If a dynamic sound, the train speed at which the sound will be played at original speed</summary>
		internal readonly double Speed;
		/// <summary>Whether this is a MicSound</summary>
		private readonly bool IsMicSound;
		/// <summary>The forwards tolerance for triggering the sound</summary>
		internal readonly double ForwardTolerance;
		/// <summary>The backwards tolerance for triggering the sound</summary>
		internal readonly double BackwardTolerance;

		internal Sound(double trackPosition, string fileName, double speed, Vector2 position = new Vector2(), double forwardTolerance = 0, double backwardTolerance = 0, bool allCars = false, int railIndex = 0) : base(trackPosition, railIndex)
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
					Type = SoundType.Ambient;
					break;
				case 0:
					Type = allCars ? SoundType.TrainAllCarStatic : SoundType.TrainStatic;
					break;
				default:
					Type = allCars ? SoundType.TrainAllCarDynamic : SoundType.TrainDynamic;
					break;
			}
			
			Speed = speed;
			Position = position;
			ForwardTolerance = forwardTolerance;
			BackwardTolerance = backwardTolerance;
		}

		internal void Create(Vector3 pos, double StartingDistance, Vector2 Direction, double planar, double updown)
		{
			if (Type == SoundType.Ambient)
			{
				if (SoundBuffer != null || IsMicSound)
				{
					double d = TrackPosition - StartingDistance;
					double dx = Position.X;
					double dy = Position.Y;
					double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
					Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(updown), Math.Sin(wa));
					w.Normalize();
					Vector3 s = new Vector3(Direction.Y, 0.0, -Direction.X);
					Vector3 u = Vector3.Cross(w, s);
					Vector3 wpos = pos + new Vector3(s.X * dx + u.X * dy + w.X * d, s.Y * dx + u.Y * dy + w.Y * d, s.Z * dx + u.Z * dy + w.Z * d);
					if (IsMicSound)
					{
						Plugin.CurrentHost.PlayMicSound(wpos, BackwardTolerance, ForwardTolerance);
					}
					else
					{
						Plugin.CurrentHost.PlaySound(SoundBuffer, 1.0, 1.0, wpos, null, true);
					}
				}
			}
		}
	}
}
