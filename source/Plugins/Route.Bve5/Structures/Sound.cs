//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Math;
using OpenBveApi.Sounds;
using RouteManager2.Events;
using System;

namespace Route.Bve5
{
	internal class Sound
	{
		/// <summary>The track position of the sound</summary>
		internal readonly double TrackPosition;
		/// <summary>The key of the soundfile in the sounds list</summary>
		internal readonly string Key;
		/// <summary>The type of sound</summary>
		internal readonly SoundType Type;
		/// <summary>The relative position of the sound</summary>
		internal readonly Vector2 Position;

		internal Sound(double trackPosition, string key, SoundType type, Vector2 position)
		{
			TrackPosition = trackPosition;
			Key = key;
			Type = type;
			Position = position;
		}

		internal void Create(RouteData Data, int currentElement, double startingDistance, Vector3 worldPosition, Vector2 worldDirection)
		{
			if (Type == SoundType.Ambient)
			{
				Data.Sound3Ds.TryGetValue(Key, out SoundHandle buffer);
				double d = TrackPosition - startingDistance;
				double dx = Position.X;
				double dy = Position.Y;
				double wa = Math.Atan2(worldDirection.Y, worldDirection.X);
				Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(0.0), Math.Sin(wa));
				w.Normalize();
				Vector3 s = new Vector3(worldDirection.Y, 0.0, -worldDirection.X);
				Vector3 u = Vector3.Cross(w, s);
				Vector3 wpos = worldPosition + new Vector3(s.X * dx + u.X * dy + w.X * d, s.Y * dx + u.Y * dy + w.Y * d, s.Z * dx + u.Z * dy + w.Z * d);
				if (buffer != null)
				{
					Plugin.CurrentHost.PlaySound(buffer, 1.0, 1.0, wpos, null, true);
				}
			}

			if (Type == SoundType.TrainStatic)
			{
				Data.Sounds.TryGetValue(Key, out SoundHandle buffer);

				if (buffer != null)
				{
					double d = TrackPosition - startingDistance;
					Plugin.CurrentRoute.Tracks[0].Elements[currentElement].Events.Add(new SoundEvent(Plugin.CurrentHost, d, buffer, true, true, false, false, Vector3.Zero, 0.0));
				}
			}
		}
	}
}
