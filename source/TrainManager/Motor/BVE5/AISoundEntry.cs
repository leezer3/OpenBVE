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

using SoundManager;

namespace TrainManager.Motor
{
	public class BVE5AISoundEntry
	{
		/// <summary>The sound for this sound entry</summary>
		internal readonly SoundBuffer Sound;
		/// <summary>The controller for this sound entry</summary>
		internal readonly BVE5AISoundControl Controller;
		/// <summary>
		/// The source for this sound
		/// </summary>
		internal SoundSource Source;

		public BVE5AISoundEntry(SoundBuffer sound, BVE5AISoundControl controller)
		{
			Sound = sound;
			Controller = controller;
		}
	}

	public enum BVE5AISoundControl
	{
		/// <summary>The sound controller is invalid</summary>
		Invalid,
		/// <summary>The sound is played whilst the train is stationary</summary>
		Stationary,
		/// <summary>The sound is played whilst the train is moving</summary>
		Rolling = 2,
		Run = 2,
		/// <summary>The sound is played whilst the train is accelerating</summary>
		Acceleration,
		/// <summary>The sound is played whilst the train is decelerating</summary>
		Deceleration,
		
	}
}
