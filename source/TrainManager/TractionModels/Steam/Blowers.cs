﻿//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
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

namespace TrainManager.TractionModels.Steam
{
	public class Blowers
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Whether the blowers are active</summary>
		public bool Active
		{
			get => _Active;
			set
			{
				if (value)
				{
					if (!_Active)
					{
						if (StartSound != null)
						{
							StartSound.Play(Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Active)
					{
						if (StopSound != null)
						{
							StopSound.Play(Engine.Car, false);
						}
					}
				}
				_Active = value;
			}
		}
		/// <summary>Backing property for Active</summary>
		private bool _Active;
		/// <summary>The sound played when the blowers are activated</summary>
		public CarSound StartSound;
		/// <summary>The sound played when the blowers are active and working</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the blowers are deactivated</summary>
		public CarSound StopSound;
		/// <summary>The temperature increase ratio</summary>
		public readonly double Ratio;
		/// <summary>The amount of steam used</summary>
		private readonly double SteamUse;

		public Blowers(SteamEngine engine, double ratio, double steamUse)
		{
			Engine = engine;
			Ratio = ratio;
			SteamUse = steamUse;
		}

		public void Update(double timeElapsed)
		{
			if (Active)
			{
				Engine.Boiler.SteamPressure -= SteamUse * timeElapsed;
				if(StartSound == null || !StartSound.IsPlaying && LoopSound != null)
				{
					LoopSound.Play(Engine.Car, true);
				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();
				}
			}
		}
	}
}