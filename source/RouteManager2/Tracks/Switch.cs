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

using System.Linq;
using OpenBveApi.Routes;

namespace RouteManager2.Tracks
{
	/// <summary>Holds the data for a single switch</summary>
	public class Switch
	{
		/// <summary>The type of switch</summary>
		public SwitchType Type;

		/// <summary>The currently set track</summary>
		public int CurrentlySetTrack => availableTracks[setTrack];

		/// <summary>The toe (root) rail of the switch</summary>
		public readonly int ToeRail;

		/// <summary>The name of the currently set track</summary>
		public string CurrentSetting
		{
			get
			{
				if (TrackNames == null || setTrack > TrackNames.Length)
				{
					return setTrack.ToString();
				}
				if (string.IsNullOrEmpty(TrackNames[setTrack]))
				{
					return "Rail " + CurrentlySetTrack;
				}
				return TrackNames[setTrack];
			}
		}

		/// <summary>The track names</summary>
		private readonly string[] TrackNames;

		/// <summary>The left-hand track index</summary>
		public readonly int LeftTrack;

		private int setTrack;

		/// <summary>The list of available track indicies</summary>
		private readonly int[] availableTracks;

		/// <summary>The track position</summary>
		public readonly double TrackPosition;

		/// <summary>The name of the switch to be displayed in-menu</summary>
		public readonly string Name;

		/// <summary>The nominal direction of the switch</summary>
		public readonly TrackDirection Direction;

		/// <summary>Whether the switch has a fixed route</summary>
		public readonly bool FixedRoute;

		/// <summary>Whether a train has run through the switch in the wrong direction</summary>
		/// <remarks>Toggle switch to reset</remarks>
		public bool RunThrough;

		public Switch(int[] tracks, string[] trackNames, int toeRail, int initialTrack, double trackPosition, SwitchType type, string name, bool fixedRoute, TrackDirection direction)
		{
			Type = type;
			if (direction == TrackDirection.Reverse)
			{
				trackNames = trackNames.Reverse().ToArray();
			}
			TrackNames = trackNames;
			ToeRail = toeRail;
			LeftTrack = type != SwitchType.LeftHanded ? tracks[0] : tracks[1];
			availableTracks = tracks;
			TrackPosition = trackPosition;
			for (int i = 0; i < availableTracks.Length; i++)
			{
				if (availableTracks[i] == initialTrack)
				{
					setTrack = i;
					break;
				}
			}

			Name = name;
			FixedRoute = fixedRoute;
			Direction = direction;
			RunThrough = false;
		}

		/// <summary>Toggles the switch to the next track</summary>
		public void Toggle()
		{
			if (FixedRoute)
			{
				return;
			}
			setTrack++;
			if (setTrack > availableTracks.Length - 1)
			{
				setTrack = 0;
			}
			RunThrough = false;
		}
	}
}
