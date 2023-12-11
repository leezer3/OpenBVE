//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
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

using OpenBveApi.Hosts;

namespace OpenBveApi.Interface
{
	/// <summary>The quick-reference strings displayed in-game</summary>
	public class InterfaceQuickReference
	{
		/// <summary>Reverser Forwards</summary>
		public string HandleForward;
		/// <summary>Reverser Neutral</summary>
		public string HandleNeutral;
		/// <summary>Reverser Reverse</summary>
		public string HandleBackward;
		/// <summary>Power P(n)</summary>
		public string HandlePower;
		/// <summary>Power Neutral</summary>
		public string HandlePowerNull;
		/// <summary>Brake B(n)</summary>
		public string HandleBrake;
		/// <summary>LocoBrake B(n)</summary>
		public string HandleLocoBrake;
		/// <summary>Brake / LocoBrake Neutral</summary>
		public string HandleBrakeNull;
		/// <summary>Air brake release</summary>
		public string HandleRelease;
		/// <summary>Air brake lap</summary>
		public string HandleLap;
		/// <summary>Air brake service</summary>
		public string HandleService;
		/// <summary>Brake emergency</summary>
		public string HandleEmergency;
		/// <summary>Hold brake applied</summary>
		public string HandleHoldBrake;
		/// <summary>Left Doors</summary>
		public string DoorsLeft;
		/// <summary>Right doors</summary>
		public string DoorsRight;
		/// <summary>Score (n)</summary>
		public string Score;

		internal InterfaceQuickReference(NewLanguage language)
		{
			HandleForward = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "forward" });
			HandleNeutral = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "neutral" });
			HandleBackward = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "backward" });
			HandlePower = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "power" });
			HandlePowerNull = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "powernull" });
			HandleBrake = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "brake" });
			HandleLocoBrake = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "locobrake" });
			HandleBrakeNull = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "brakenull" });
			HandleRelease = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "release" });
			HandleLap = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "lap" });
			HandleService = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "service" });
			HandleEmergency = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "emergency" });
			HandleHoldBrake = language.GetInterfaceString(HostApplication.OpenBve, new[] { "handles", "holdbrake" });
			DoorsLeft = language.GetInterfaceString(HostApplication.OpenBve, new[] { "doors", "left" });
			DoorsRight = language.GetInterfaceString(HostApplication.OpenBve, new[] { "doors", "right" });
			Score = language.GetInterfaceString(HostApplication.OpenBve, new[] { "misc", "score" });
		}
	}
}
