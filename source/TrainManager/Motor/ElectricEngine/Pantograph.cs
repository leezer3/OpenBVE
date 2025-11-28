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

using System.Collections.Generic;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using SoundManager;

namespace TrainManager.Motor
{
	public class Pantograph : AbstractComponent
	{
		/// <summary>Whether the pantograph is currently raised</summary>
		public PantographState State;
		/// <summary>The sound played when the pantograph is lowered</summary>
		public CarSound LowerSound;
		/// <summary>The sound played when the pantograph is raised</summary>
		public CarSound RaiseSound;
		/// <summary>The sound played when the switch is toggled</summary>
		public CarSound SwitchToggle;
		/// <summary>The power supplies available to this pantograph</summary>
		public Dictionary<PowerSupplyTypes, PowerSupply> AvailablePowerSupplies => baseEngine.BaseCar.FrontAxle.Follower.AvailablePowerSupplies;

		public Pantograph(TractionModel engine) : base(engine)
		{
		}

		public void Raise()
		{
			if (State == PantographState.Lowered)
			{
				State = PantographState.Raised;
				RaiseSound?.Play(1.0, 1.0, baseEngine.BaseCar, false);
				SwitchToggle?.Play(1.0,1.0, baseEngine.BaseCar, false);
				if (baseEngine.BaseCar.baseTrain.Specs.PantographState != PantographState.Dewired)
				{
					baseEngine.BaseCar.baseTrain.Specs.PantographState = PantographState.Raised;
				}
			}
		}

		public void Lower()
		{
			if (State != PantographState.Lowered)
			{
				State = PantographState.Lowered;
				LowerSound?.Play(1.0,1.0, baseEngine.BaseCar, false);
				SwitchToggle?.Play(1.0, 1.0, baseEngine.BaseCar, false);
				if (baseEngine.BaseCar.baseTrain.Specs.PantographState != PantographState.Dewired)
				{
					baseEngine.BaseCar.baseTrain.Specs.PantographState = PantographState.Lowered;
				}
			}
		}
	}
}
