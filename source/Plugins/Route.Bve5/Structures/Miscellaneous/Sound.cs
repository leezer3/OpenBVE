using System;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using SoundManager;

namespace Bve5RouteParser
{
	internal class Sound
	{
		/// <summary>The track position of the sound</summary>
		internal readonly double TrackPosition;
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
		/// <summary>The forwards tolerance for triggering the sound</summary>
		internal readonly double ForwardTolerance;
		/// <summary>The backwards tolerance for triggering the sound</summary>
		internal readonly double BackwardTolerance;

		internal Sound(double trackPosition, SoundHandle sound, double speed, Vector2 position = new Vector2(), double forwardTolerance = 0, double backwardTolerance = 0)
		{
			Radius = 15.0;
			SoundBuffer = sound;
			
			switch (speed)
			{
				case -1:
					Type = SoundType.Ambient;
					break;
				case 0:
					Type = SoundType.TrainStatic;
					break;
				default:
					Type = SoundType.TrainDynamic;
					break;
			}
			
			TrackPosition = trackPosition;
			Speed = speed;
			Position = position;
			ForwardTolerance = forwardTolerance;
			BackwardTolerance = backwardTolerance;
		}

		internal void Create(Vector3 pos, double StartingDistance, Vector2 Direction, double planar, double updown)
		{
			if (Type == SoundType.Ambient)
			{
				if (SoundBuffer != null)
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
					Plugin.CurrentHost.PlaySound(SoundBuffer, 1.0, 1.0, wpos, null, true);
				}
			}
		}
	}
}
