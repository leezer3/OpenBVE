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
	public class CylinderCocks
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>Whether the cylinder cocks are open</summary>
		public bool Open
		{
			get => _Open;
			set
			{
				if (value)
				{
					if (!_Open)
					{
						if (StartSound != null)
						{
							StartSound.Play(Engine.Car, false);
						}
					}
				}
				else
				{
					if (_Open)
					{
						if (StopSound != null)
						{
							StopSound.Play(Engine.Car, false);
						}
					}
				}
				_Open = value;
			}
		}
		/// <summary>Backing property for Open</summary>
		private bool _Open;
		/// <summary>The sound played when the cylinder cocks are opened</summary>
		public CarSound StartSound;
		/// <summary>The sound played when the cylinder cocks are open with the regulator closed</summary>
		public CarSound IdleLoopSound;
		/// <summary>The sound played when the cylinder cocks are open with the regulator open</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the cylinder cocks are closed</summary>
		public CarSound StopSound;
		/// <summary>The amount of steam used when open at max regulator</summary>
		private readonly double SteamUse;

		public CylinderCocks(SteamEngine engine, double steamUse)
		{
			Engine = engine;
			SteamUse = steamUse;
		}

		public void Update(double timeElapsed)
		{
			if (Open)
			{
				Engine.Boiler.SteamPressure -= SteamUse * Engine.Car.baseTrain.Handles.Power.Actual * Engine.Car.baseTrain.Handles.Power.MaximumNotch * timeElapsed;
				if (StartSound == null || !StartSound.IsPlaying)
				{
					if (Engine.Car.baseTrain.Handles.Power.Actual != 0)
					{
						if (IdleLoopSound != null)
						{
							IdleLoopSound.Stop();
						}

						if (LoopSound != null)
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

						if (IdleLoopSound != null)
						{
							IdleLoopSound.Play(Engine.Car, true);
						}
					}

				}
			}
			else
			{
				if (LoopSound != null)
				{
					LoopSound.Stop();

				}

				if (IdleLoopSound != null)
				{
					IdleLoopSound.Stop();
				}
			}
		}
	}
}