//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

using Formats.OpenBve;

namespace LokSimRouteParser
{
	internal class Station
	{
		/// <summary>The arrival time</summary>
		internal int Arrival;
		/// <summary>The departure time</summary>
		internal int Departure;
		/// <summary>Whether this is a stop point</summary>
		internal bool ShouldStop;
		/// <summary>Whether this is a normal stop</summary>
		internal bool NormalStop;
		/// <summary>Whether this is a request stop</summary>
		internal bool RequestStop;
		/// <summary>The minimum time stopped</summary>
		internal int StopTime;
		/// <summary>The station name</summary>
		internal string Name;
		/// <summary>Whether the station forces a red signal until departure</summary>
		internal bool ForcedRedSignal;
		/// <summary>The distance at which the pre-station stop alarm is sounded</summary>
		internal int StopAlarmDistance;
		/// <summary>The departure sound</summary>
		internal string DepartureSound;
		/// <summary>The arrival sound</summary>
		internal string ArrivalSound;
		/// <summary>Whether the doors are to open</summary>
		internal bool OpenDoors;

		internal Station(Block<LoksimNode, LoksimAttribute> stationBlock)
		{
			stationBlock.GetValue(LoksimAttribute.Abfahrt, out Arrival);
			stationBlock.GetValue(LoksimAttribute.Ankunft, out Departure);
			stationBlock.GetValue(LoksimAttribute.Haltepunkt, out ShouldStop);
			stationBlock.GetValue(LoksimAttribute.Bedarfshalt, out RequestStop);
			stationBlock.GetValue(LoksimAttribute.Betriebshalt, out NormalStop);
			stationBlock.GetValue(LoksimAttribute.SignalVorAusfahrt, out ForcedRedSignal);
			stationBlock.GetValue(LoksimAttribute.Haltdauer, out StopTime);
			stationBlock.GetValue(LoksimAttribute.Name, out Name);
			stationBlock.GetValue(LoksimAttribute.DistanzSoundVorBedarfsHalt, out StopAlarmDistance);
			stationBlock.GetPath(LoksimAttribute.SoundAnkunft, Plugin.FileSystem.LoksimDataDirectory, out ArrivalSound);
			stationBlock.GetPath(LoksimAttribute.SoundAbfahrt, Plugin.FileSystem.LoksimDataDirectory, out DepartureSound);
			/*
			 * SoundAnsage : announcement
			 * SoundHalt : stop
			 * SoundVorBedarfsHalt : stop alarm sound
			 * Zugfolgestelle:
			 * Train Following Point:
			 * ----------------------
			 *
			 * Section of track ahead may only be released if it is free of vehicles and not in use by train
			 * in opposite direction
			 *
			 * PZB related, probably interacts with the forced red signal.
			 */
		}
	}
}

