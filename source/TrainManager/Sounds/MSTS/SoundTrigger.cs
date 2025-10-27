//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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

using OpenBve.Formats.MsTs;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.MsTsSounds
{
	public abstract class SoundTrigger
	{
		/// <summary>Holds the list of possible sound buffers to be selected from</summary>
		internal readonly SoundBuffer[] Buffers;
		/// <summary>The selection method used to determine the buffer to play</summary>
		private readonly KujuTokenID bufferSelectionMethod;
		/// <summary>Whether the sound loops</summary>
		internal readonly bool SoundLoops;
		/// <summary>Timer used in derived updates</summary>
		internal double Timer;
		/// <summary>The previously selected buffer index</summary>
		internal int BufferIndex;
		/// <summary>Stores whether the trigger is currently active</summary>
		internal bool Triggered;
		/// <summary>Gets the actual sound buffer to be played</summary>
		internal SoundBuffer Buffer
		{
			get
			{
				switch (bufferSelectionMethod)
				{
					case KujuTokenID.SequentialSelection:
						BufferIndex++;
						if (BufferIndex > Buffers.Length - 1)
						{
							BufferIndex = 0;
						}
						break;
					case KujuTokenID.RandomSelection:
						BufferIndex = TrainManagerBase.RandomNumberGenerator.Next(0, Buffers.Length - 1);
						break;
				}
				return Buffers[BufferIndex];
			}
		}

		internal SoundTrigger(SoundBuffer buffer, bool soundLoops)
		{
			Buffers = new[] { buffer };
			SoundLoops = soundLoops;
		}

		internal SoundTrigger(SoundBuffer[] buffers, KujuTokenID selectionMethod, bool soundLoops)
		{
			Buffers = buffers;
			bufferSelectionMethod = selectionMethod;
			SoundLoops = soundLoops;
		}

		public virtual void Update(double timeElapsed, CarBase car, ref SoundBuffer soundBuffer, ref bool soundLoops)
		{

		}
	}
}
