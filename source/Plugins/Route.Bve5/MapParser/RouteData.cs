using System.Collections.Generic;
using OpenBveApi.Runtime;
using OpenBveApi.Sounds;
using OpenBveApi.Trains;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private class RouteData
		{
			internal List<string> TrackKeyList;
			internal List<Block> Blocks;
			internal List<Station> StationList;
			internal ObjectDictionary Objects;
			internal List<Background> Backgrounds;
			internal bool AccurateObjectDisposal = true;
			internal List<SignalData> SignalObjects;
			internal SoundDictionary Sounds;
			internal SoundDictionary Sound3Ds;

			//Set units of speed initially to km/h
			//This represents 1km/h in m/s
			internal double UnitOfSpeed = 0.277777777777778;

			internal double[] SignalSpeeds;

			internal int FindOrAddBlock(double Distance)
			{
				int Index = Blocks.FindIndex(x => x.StartingDistance == Distance);
				if (Index == -1)
				{
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
					Index = Blocks.FindLastIndex(x => x.StartingDistance < Distance) + 1;
					Blocks.Insert(Index, NewBlock);
				}
				return Index;
			}
		}
	}
}
