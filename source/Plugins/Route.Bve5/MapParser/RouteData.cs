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

using System.Collections.Generic;
using System.Linq;

namespace Route.Bve5
{
	internal class RouteData
	{
		internal readonly List<string> TrackKeyList;
		internal readonly SortedList<double, Bve5ScenarioParser.Block> sortedBlocks;
		internal IList<Bve5ScenarioParser.Block> Blocks => sortedBlocks.Values;
		internal Dictionary<string, Station> StationList;
		internal Bve5ScenarioParser.ObjectDictionary Objects;
		internal Bve5ScenarioParser.ObjectDictionary Backgrounds;
		internal List<Bve5ScenarioParser.SignalData> SignalObjects;
		internal Bve5ScenarioParser.SoundDictionary Sounds;
		internal Bve5ScenarioParser.SoundDictionary Sound3Ds;

		internal RouteData(HashSet<string> trackKeys)
		{
			sortedBlocks = new SortedList<double, Bve5ScenarioParser.Block>();
			TrackKeyList = trackKeys.ToList();
		}

		//Set units of speed initially to km/h
		//This represents 1km/h in m/s
		internal const double UnitOfSpeed = 0.277777777777778;

		internal double[] SignalSpeeds;

		internal void TryAddBlock(double Distance)
		{
			if (sortedBlocks.ContainsKey(Distance))
			{
				return;
			}
			Bve5ScenarioParser.Block NewBlock = new Bve5ScenarioParser.Block
			{
				Rails = TrackKeyList.ToDictionary(x => x, x => new Rail()),
				StartingDistance = Distance,
				CurrentTrackState =
				{
					StartingTrackPosition = Distance
				},
				FreeObjects = new Dictionary<string, List<FreeObj>>(),
				Cracks = new List<Crack>(),
				Sections = new List<Section>(),
				Signals = new List<Signal>[TrackKeyList.Count],
				Transponders = new List<Transponder>(),
				Limits = new List<Limit>(),
				BrightnessChanges = new List<Brightness>(),
				SoundEvents = new List<Sound>(),
				RunSounds = new List<RunSound>(),
				FlangeSounds = new List<FlangeSound>()
			};
			sortedBlocks.Add(Distance, NewBlock);
			int newIndex = sortedBlocks.IndexOfKey(Distance);
			if (newIndex > 0)
			{
				Blocks[newIndex].Fog = Blocks[newIndex - 1].Fog;
			}
		}

		internal int FindOrAddBlock(double Distance)
		{
			if (sortedBlocks.ContainsKey(Distance))
			{
				return sortedBlocks.IndexOfKey(Distance);
			}
			Bve5ScenarioParser.Block NewBlock = new Bve5ScenarioParser.Block
			{
				Rails = TrackKeyList.ToDictionary(x => x, x => new Rail()),
				StartingDistance = Distance,
				CurrentTrackState =
				{
					StartingTrackPosition = Distance
				},
				FreeObjects = new Dictionary<string, List<FreeObj>>(),
				Cracks = new List<Crack>(),
				Sections = new List<Section>(),
				Signals = new List<Signal>[TrackKeyList.Count],
				Transponders = new List<Transponder>(),
				Limits = new List<Limit>(),
				BrightnessChanges = new List<Brightness>(),
				SoundEvents = new List<Sound>(),
				RunSounds = new List<RunSound>(),
				FlangeSounds = new List<FlangeSound>()
			};
			sortedBlocks.Add(Distance, NewBlock);
			int newIndex = sortedBlocks.IndexOfKey(Distance);
			if (newIndex > 0)
			{
				Blocks[newIndex].Fog = Blocks[newIndex - 1].Fog;
			}
			return newIndex;
		}
	}
}
