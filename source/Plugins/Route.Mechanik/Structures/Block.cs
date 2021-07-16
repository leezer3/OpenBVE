//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using Route.Mechanik;

namespace MechanikRouteParser
{
	/// <summary>Represents a variable length route block</summary>
	internal class Block
	{
		/// <summary>The track position at which this block starts</summary>
		internal double StartingTrackPosition;
		/// <summary>The world objects placed in this block</summary>
		internal List<RouteObject> Objects = new List<RouteObject>();
		/// <summary>The turn radius in radians</summary>
		internal double Turn = 0.0;
		/// <summary>The speed limit for this block</summary>
		internal double SpeedLimit = -1;
		/// <summary>The sound events placed in this block</summary>
		internal List<SoundEvent> Sounds = new List<SoundEvent>();
		/// <summary>The station stop if applicable</summary>
		internal List<StationStop> stopMarker;
		/// <summary>The rotation / position correction to be issued after this block</summary>
		internal Correction Correction;
		/// <summary>The signals placed in this block</summary>
		internal List<Semaphore> Signals;
		/// <summary>The block Y offset</summary>
		internal double YOffset;
		/// <summary>Whether a HornBlow event should be issued in this block</summary>
		internal bool HornBlow;
		/// <summary>Whether a signal beacon is present</summary>
		internal bool SignalBeacon;

		internal Block(double TrackPosition, double Offset)
		{
			this.StartingTrackPosition = TrackPosition;
			Correction = null;
			this.stopMarker = new List<StationStop>();
			this.Signals = new List<Semaphore>();
			this.YOffset = Offset;
		}
	}
}
