//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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

namespace MechanikRouteParser
{
	/// <summary>A train triggered sound event</summary>
	internal class SoundEvent
	{
		/// <summary>The sound index from dzwieki.dat</summary>
		internal readonly int SoundIndex;
		/// <summary>Whether this plays looped until stopped</summary>
		internal readonly bool Looped;
		/// <summary>Whether this is speed dependant</summary>
		internal readonly bool SpeedDependant;
		/// <summary>The volume of the sound</summary>
		internal readonly int Volume;
		/// <summary>The sound's position vector</summary>
		internal readonly Vector3 Position;

		internal SoundEvent(int soundIndex, Vector3 position, bool looped, bool speedDependant, int volume)
		{
			Position = position;
			SoundIndex = soundIndex;
			Looped = looped;
			SpeedDependant = speedDependant;
			Volume = volume;
		}
	}
}
