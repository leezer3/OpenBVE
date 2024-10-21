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

using OpenBveApi.Math;

namespace Route.Bve5
{
	internal class Repeater
	{
		/// <summary>The key used to refer to the repeater</summary>
		internal readonly string Key;
		/// <summary>They key for the controlling track</summary>
		internal string TrackKey;
		/// <summary>The current starting distance</summary>
		internal double StartingDistance;
		/// <summary>Whether the start has been refreshed in this block</summary>
		/// <remarks>e.g. command has been issued</remarks>
		internal bool StartRefreshed;
		/// <summary>The current ending distance for the repeater</summary>
		internal double EndingDistance;
		/// <summary>The repetition placement interval</summary>
		internal double Interval;
		/// <summary>The current set of object keys in use</summary>
		internal string[] ObjectKeys;
		/// <summary>The position of the repeater relative to the rail</summary>
		internal Vector3 Position;
		/// <summary>The yaw of the object (radians)</summary>
		internal double Yaw;
		/// <summary>The pitch of the object (radians)</summary>
		internal double Pitch;
		/// <summary>The roll of the object (radians)</summary>
		internal double Roll;
		/// <summary>The transform type used when placing the repeater</summary>
		internal ObjectTransformType Type;
		/// <summary>The span of the repeater</summary>
		internal double Span;

		internal Repeater(string key)
		{
			/*
			 * Note that only the key of the repeater may be set in the constructor.
			 * All other types may be by design changed at any point by issuing another repeater command
			 */
			Key = key;
		}
	}
}
