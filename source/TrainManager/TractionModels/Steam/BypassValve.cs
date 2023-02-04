//Simplified BSD License (BSD-2-Clause)
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
using TrainManager.Handles;
using TrainManager.TractionModels.Steam;

namespace TrainManager.TractionModels
{
	public class BypassValve
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The type of bypass valve</summary>
		public BypassValveType Type;
		/// <summary>Whether the bypass valve is active</summary>
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
		/// <summary>The sound played when bypass valve is activated</summary>
		public CarSound StartSound;
		/// <summary>The sound played when the bypass valve is active and working</summary>
		public CarSound PowerLoopSound;
		/// <summary>The sound played when the bypass valve is active and idle</summary>
		public CarSound IdleLoopSound;
		/// <summary>The sound played when the bypass valve is deactivated</summary>
		public CarSound StopSound;
		/// <summary>The amount of steam used</summary>
		private readonly double SteamUse;

		public BypassValve(SteamEngine engine, BypassValveType type, double steamUse)
		{
			Engine = engine;
			SteamUse = steamUse;
			Type = type;
		}

		public void Update(double timeElapsed)
		{
			if (Type == BypassValveType.None)
			{
				return;
			}
			Regulator regulator = Engine.Car.baseTrain.Handles.Power as Regulator;
			Cutoff cutoff = Engine.Car.baseTrain.Handles.Reverser as Cutoff;
			if (!Active)
			{
				if (IdleLoopSound != null && IdleLoopSound.IsPlaying)
				{
					IdleLoopSound.Stop();
				}

				if (PowerLoopSound != null && PowerLoopSound.IsPlaying)
				{
					PowerLoopSound.Stop();
				}
				switch (Type)
				{
					case BypassValveType.Manual:
						return;
					case BypassValveType.Automatic:
						if (Engine.Car.baseTrain.CurrentSpeed != 0 && cutoff.Actual == 0)
						{
							Active = true;
						}
						break;
					case BypassValveType.AutomaticRegulatorTriggered:
						if (Engine.Car.baseTrain.CurrentSpeed != 0 && cutoff.Actual == 0)
						{
							if (regulator.Actual != 0)
							{
								Active = true;
							}
						}
						break;
				}
			}
			else
			{
				Engine.Boiler.SteamPressure -= SteamUse * timeElapsed;
				if(!StartSound.IsPlaying)
				{
					if (Engine.Car.baseTrain.Handles.Power.Actual > 5)
					{
						// Power loop sound
						if (PowerLoopSound != null && !PowerLoopSound.IsPlaying)
						{
							PowerLoopSound.Play(Engine.Car, true);
						}
					}
					else
					{
						// Idle loop sound
						if (PowerLoopSound != null)
						{
							PowerLoopSound.Stop();
						}

						if (IdleLoopSound != null && !IdleLoopSound.IsPlaying)
						{
							IdleLoopSound.Play(Engine.Car, true);
						}
					}
				}

				if (Type > BypassValveType.Manual && cutoff.Actual != 0)
				{
					// Automatic valve type, so set back to normal
					Active = false;
				}
			}
		}

	}
}
