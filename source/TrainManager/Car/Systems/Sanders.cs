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

using SoundManager;

namespace TrainManager.Car.Systems
{
	public class Sanders : AbstractReAdhesionDevice
	{
		/// <summary>The sanding rate</summary>
		public double SandingRate;
		/// <summary>The level of sand in the sandbox</summary>
		public double SandLevel;
		/// <summary>The application time available before the sander cuts off</summary>
		public double ApplicationTime;
		/// <summary>The time required before activation if using automatic mode</summary>
		public double ActivationTime;
		/// <summary>If a number of shots is available, the number remaining</summary>
		public int NumberOfShots;
		/// <summary>The current state</summary>
		public SandersState State;
		/// <summary>The type of sanders</summary>
		public readonly SandersType Type;
		/// <summary>The sound played when the sanders are activated</summary>
		public CarSound ActivationSound;
		/// <summary>The sound played when the sanders are deactivated</summary>
		public CarSound DeActivationSound;
		/// <summary>The sound loop played when the sanders are active</summary>
		public CarSound LoopSound;
		/// <summary>The sound played when the sand is emptied</summary>
		public CarSound EmptySound;
		/// <summary>The sound played when activation is attempted whilst empty</summary>
		public CarSound EmptyActivationSound;
		/// <summary>The maximum speed at which the sanders are effective</summary>
		public readonly double MaximumSpeed;

		private bool emptied = false;

		private double timer;
		public Sanders(CarBase car, SandersType type, double maximumSpeed = double.MaxValue) : base(car)
		{
			Type = type;
			MaximumSpeed = maximumSpeed;
		}

		public override void Update(double timeElapsed)
		{
			if (Type == SandersType.NotFitted)
			{
				return;
			}

			if (State != SandersState.Inactive)
			{
				if ((ActivationSound != null && !ActivationSound.IsPlaying) || ActivationSound == null)
				{
					LoopSound?.Play(Car, true);
				}
			}
			else
			{
				LoopSound?.Stop();
			}

			if (SandLevel <= 0)
			{
				SandLevel = 0;
				if (SandingRate != 0)
				{
					State = SandersState.ActiveEmpty;
					if (!emptied)
					{
						EmptySound?.Play(Car, false);
						emptied = true;
					}
				}
			}
			else
			{
				if (SandingRate > 0 && SandingRate < double.MaxValue)
				{
					SandLevel -= SandingRate * timeElapsed;
				}
				
			}
			switch (Type)
			{
				case SandersType.Automatic:
					if (Car.FrontAxle.CurrentWheelSlip)
					{
						timer += timeElapsed;
						if (timer > ActivationTime && State == SandersState.Inactive && !emptied)
						{
							SetActive(true);
						}
					}
					else
					{
						timer = 0;
						if (State != SandersState.Inactive)
						{
							SetActive(false);
						}
					}
					break;
				case SandersType.NumberOfShots:
					if (State == SandersState.Inactive)
					{
						timer -= timeElapsed;
						if (timer <= 0)
						{
							Toggle();
						}
					}
					
					break;
			}
		}

		public void SetActive(bool willBeActive)
		{
			if (Type == SandersType.NotFitted)
			{
				return;
			}

			if (emptied)
			{
				if (Type != SandersType.Automatic && EmptyActivationSound != null)
				{
					// Assume that whatever is controlling the automatic activation has the brains not to do so
					// when the sand is empty
					EmptyActivationSound.Play(Car, false);
				}
				return;
			}

			// Deactivate
			if(State != SandersState.Inactive && !willBeActive) {
				if (Type == SandersType.NumberOfShots && timer > 0)
				{
					// Can't cancel shot based
					return;
				}

				DeActivationSound?.Play(Car, false);
			} else if(State == SandersState.Inactive && willBeActive) {
				if (Type == SandersType.NumberOfShots)
				{
					if (NumberOfShots <= 0)
					{
						return;
					}
					NumberOfShots--;
					timer = ApplicationTime;
				}

				ActivationSound?.Play(Car, false);
			}

			if (willBeActive)
			{
				State = SandersState.Active;
			}
		}

		public void Toggle()
		{
			SetActive(State != SandersState.Inactive);
		}
	}
}
