//Copyright (c) 2026, Marc Riera, The OpenBVE Project
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

using OpenBveApi.Interface;

namespace ZuikiInput
{
	/// <summary>Class representing a configuration profile for a controller.</summary>
	public class ControllerProfile
	{
		/// <summary>Whether to convert the handle notches to match the driver's train.</summary>
		public bool ConvertNotches;

		/// <summary>Whether to assign the minimum and maximum notches to the first and last notches, respectively.</summary>
		public bool KeepMinMax;

		/// <summary>Whether to map the hold brake to B1.</summary>
		public bool MapHoldBrake;

		/// <summary>An array with the input controls configured for each brake notch.</summary>
		internal readonly InputControl[] BrakeControls = new InputControl[ZuikiInput.BrakeControlsCount];

		/// <summary>An array with the input controls configured for each power notch.</summary>
		internal readonly InputControl[] PowerControls = new InputControl[ZuikiInput.PowerControlsCount];

		/// <summary>An array with the input controls configured for each reverser notch.</summary>
		internal readonly InputControl[] ReverserControls = new InputControl[ZuikiInput.ReverserControlsCount];

		/// <summary>An array with the input controls configured for each button.</summary>
		public InputControl[] ButtonControls = new InputControl[ZuikiInput.ButtonControlsCount];
	}
}
