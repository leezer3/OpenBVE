//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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

using OpenBveApi.Runtime;
using RouteManager2.Stations;

namespace Route.Bve5
{
	internal class Station
	{
		/// <summary>The displayed name of the station</summary>
		internal string Name;
		/// <summary>The station stop mode</summary>
		internal StationStopMode StopMode;
		/// <summary>The station type</summary>
		internal StationType StationType;
		/// <summary>The expected arrival time</summary>
		internal double ArrivalTime;
		/// <summary>The expected departure time</summary>
		internal double DepartureTime;
		/// <summary>The minimum time stopped</summary>
		internal double StopTime;
		/// <summary>The time if jumping to the station</summary>
		internal double DefaultTime = -1;
		/// <summary>Whether the signal is held to red until departure</summary>
		internal bool ForceStopSignal;
		/// <summary>Whether the departure signal is used</summary>
		internal bool DepartureSignalUsed;
		/// <summary>The minimum time for passengers to alight</summary>
		/// <remarks>Applies once the doors have opened</remarks>
		internal double AlightingTime;
		/// <summary>The passenger ratio after this station</summary>
		internal double PassengerRatio;
		/// <summary>The key for the arrival sound to be played</summary>
		internal string ArrivalSoundKey;
		/// <summary>The key for the departure sound to be played</summary>
		internal string DepartureSoundKey;
		/// <summary>The probability of the doors re-opening due to a failed closure attempt</summary>
		internal double ReopenDoor;
		/// <summary>The probability of an object interfering in the door operation</summary>
		internal double InterferenceInDoor;

		internal void Create(ref RouteStation newStation)
		{
			newStation.Name = Name;
			newStation.ArrivalTime = ArrivalTime;
			newStation.StopMode = StopMode;
			newStation.DepartureTime = DepartureTime;
			newStation.JumpTime = DefaultTime;
			newStation.Type = StationType;
			newStation.StopTime = StopTime;
			newStation.ForceStopSignal = ForceStopSignal;
			newStation.PassengerRatio = PassengerRatio;
			newStation.ReopenDoor = ReopenDoor;
			newStation.ReopenStationLimit = 5;
			newStation.InterferenceInDoor = InterferenceInDoor;
			newStation.MaxInterferingObjectRate = Plugin.RandomNumberGenerator.Next(1, 99);
		}
	}
}
