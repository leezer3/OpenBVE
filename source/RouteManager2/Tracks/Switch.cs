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

namespace RouteManager2.Tracks
{
	/// <summary>Holds the data for a single switch</summary>
	public class Switch
	{
		/// <summary>The type of switch</summary>
		public SwitchType Type;

		/// <summary>The currently set track</summary>
		public int CurrentlySetTrack => availableTracks[setTrack];

		/// <summary>The name of the currently set track</summary>
		public string CurrentSetting
		{
			get
			{
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
		public readonly int[] availableTracks;

		/// <summary>The track position</summary>
		public readonly double TrackPosition;

		public readonly string Name;

		public Switch(int[] tracks, string[] trackNames, int initialTrack, double trackPosition, SwitchType type, string name)
		{
			Type = type;
			TrackNames = trackNames;
			LeftTrack = type != SwitchType.LeftHanded ? tracks[0] : tracks[1];
			availableTracks = tracks;
			TrackPosition = trackPosition;
			for (int i = 0; i < availableTracks.Length; i++)
			{
				if (availableTracks[i] == initialTrack)
				{
					setTrack = i;
				}
			}

			Name = name;
		}

		/// <summary>Toggles the switch to the next track</summary>
		public void Toggle()
		{
			setTrack++;
			if (setTrack > availableTracks.Length - 1)
			{
				setTrack = 0;
			}
		}
	}
}
