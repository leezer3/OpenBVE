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

using SoundManager;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Motor;
using TrainManager.Motor;

namespace TrainManager.SteamEngine
{
	public class BypassValve : AbstractComponent
	{
		/// <summary>The type of bypass valve</summary>
		public BypassValveType Type;
		/// <summary>The sound played when the bypass valve is active and working</summary>
		public CarSound PowerLoopSound;
		/// <summary>The sound played when the bypass valve is active and idle</summary>
		public CarSound IdleLoopSound;
		/// <summary>The amount of steam used</summary>
		private readonly double SteamUse;

		public BypassValve(TractionModel engine, BypassValveType type, double steamUse) : base(engine)
		{
			Type = type;
			SteamUse = steamUse;
		}

		public BypassValve(TractionModel engine, BypassValveType type, CarSound powerLoopSound, CarSound idleLoopSound, double steamUse) : base(engine)
		{
			Type = type;
			PowerLoopSound = powerLoopSound;
			IdleLoopSound = idleLoopSound;
			SteamUse = steamUse;
		}

		public override void Update(double timeElapsed)
		{
			if (Type == BypassValveType.None || baseEngine.Components.TryGetTypedValue(EngineComponent.Boiler, out Boiler boiler))
			{
				return;
			}
			
			if (Active == false)
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
						if (baseEngine.BaseCar.baseTrain.CurrentSpeed != 0 && baseEngine.BaseCar.baseTrain.Handles.Reverser.Actual == 0)
						{
							Active = true;
						}
						break;
					case BypassValveType.AutomaticRegulatorTriggered:
						if (baseEngine.BaseCar.baseTrain.CurrentSpeed != 0 && baseEngine.BaseCar.baseTrain.Handles.Reverser.Actual == 0)
						{
							if (baseEngine.BaseCar.baseTrain.Handles.Power.Actual != 0)
							{
								Active = true;
							}
						}
						break;
				}
			}
			else
			{
				boiler.CurrentSteamMass -= SteamUse * timeElapsed;
				if (!ActivationSound.IsPlaying)
				{
					if (baseEngine.BaseCar.baseTrain.Handles.Power.Actual > 0)
					{
						// Power loop sound
						if (PowerLoopSound != null && !PowerLoopSound.IsPlaying)
						{
							PowerLoopSound.Play(baseEngine.BaseCar, true);
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
							IdleLoopSound.Play(baseEngine.BaseCar, true);
						}
					}
				}

				if (Type > BypassValveType.Manual && baseEngine.BaseCar.baseTrain.Handles.Reverser.Actual != 0)
				{
					// Automatic valve type, so set back to normal
					Active = false;
				}
			}
		}

		public override void ControlDown(Translations.Command Control)
		{
			switch (Control)
			{
				case Translations.Command.BypassValve:
					if (Active)
					{
						LoopSound?.Stop();
						DeactivationSound?.Play(baseEngine.BaseCar, false);
						Active = false;
					}
					else
					{
						ActivationSound?.Play(baseEngine.BaseCar, false);
						Active = true;
					}
					break;
			}
		}
	}
}
