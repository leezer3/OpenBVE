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

using System;
using System.Collections.Generic;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private class RouteData
		{
			internal readonly List<string> TrackKeyList;
			internal readonly SortedList<double, Block> sortedBlocks;

			internal IList<Block> Blocks => sortedBlocks.Values;
			internal List<Station> StationList;
			internal ObjectDictionary Objects;
			internal List<Background> Backgrounds;
			internal List<SignalData> SignalObjects;
			internal SoundDictionary Sounds;
			internal SoundDictionary Sound3Ds;

			//Set units of speed initially to km/h
			//This represents 1km/h in m/s
			internal const double UnitOfSpeed = 0.277777777777778;

			internal double[] SignalSpeeds;

			internal RouteData()
			{
				sortedBlocks = new SortedList<double, Block>();
				TrackKeyList = new List<string>();
			}

			internal int FindOrAddBlock(double Distance)
			{
				if (sortedBlocks.ContainsKey(Distance))
				{
					return sortedBlocks.IndexOfKey(Distance);
				}
				Block NewBlock = new Block
				{
					Rails = new Rail[TrackKeyList.Count],
					StartingDistance = Distance,
					CurrentTrackState =
					{
						StartingTrackPosition = Distance
					},
					FreeObj = new List<FreeObj>[TrackKeyList.Count],
					Cracks = new List<Crack>(),
					Sections = new List<Section>(),
					Signals = new List<Signal>[TrackKeyList.Count],
					Transponders = new List<Transponder>(),
					Limits = new List<Limit>(),
					BrightnessChanges = new List<Brightness>(),
					SoundEvents = new List<Sound>(),
					RunSounds = new List<TrackSound>(),
					FlangeSounds = new List<TrackSound>()
				};
				sortedBlocks.Add(Distance, NewBlock);
				return sortedBlocks.IndexOfKey(Distance);
			}
		}
	}
}
