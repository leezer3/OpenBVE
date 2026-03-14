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

using System;
using System.Collections.Generic;

namespace KatoInput
{
	/// <summary>Class representing the state of a controller.</summary>
	public class ControllerState
	{
		/// <summary>Enumeration representing brake notches.</summary>
		internal enum BrakeNotches
		{
			/// <summary>Brakes are released</summary>
			Released = 0,
			/// <summary>Brake notch B1</summary>
			B1 = 1,
			/// <summary>Brake notch B2</summary>
			B2 = 2,
			/// <summary>Brake notch B3</summary>
			B3 = 3,
			/// <summary>Brake notch B4</summary>
			B4 = 4,
			/// <summary>Brake notch B5</summary>
			B5 = 5,
			/// <summary>Brake notch B6</summary>
			B6 = 6,
			/// <summary>Brake notch B7</summary>
			B7 = 7,
			/// <summary>Brake notch B8</summary>
			B8 = 8,
			/// <summary>Emergency brake</summary>
			Emergency = 9,
		};

		/// <summary>Enumeration representing power notches.</summary>
		internal enum PowerNotches
		{
			/// <summary>Power is not applied</summary>
			N = 0,
			/// <summary>Power notch P1</summary>
			P1 = 1,
			/// <summary>Power notch P2</summary>
			P2 = 2,
			/// <summary>Power notch P3</summary>
			P3 = 3,
			/// <summary>Power notch P4</summary>
			P4 = 4,
			/// <summary>Power notch P5</summary>
			P5 = 5,
		};

		/// <summary>Enumeration representing reverser positions.</summary>
		internal enum ReverserPositions
		{
			/// <summary>Backward</summary>
			Backward = -1,
			/// <summary>Neutral</summary>
			Neutral = 0,
			/// <summary>Forward</summary>
			Forward = 1,
		};


		/// <summary>Whether the controller is connected or not.</summary>
		internal bool IsConnected;

		/// <summary>The brake notch reported by the controller.</summary>
		internal BrakeNotches BrakeNotch;

		/// <summary>The power notch reported by the controller.</summary>
		internal PowerNotches PowerNotch;

		/// <summary>The reverser position reported by the controller.</summary>
		internal ReverserPositions ReverserPosition;

	}
}
