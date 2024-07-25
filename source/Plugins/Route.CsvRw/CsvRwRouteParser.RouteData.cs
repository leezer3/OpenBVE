using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	internal partial class Parser
	{
		private class RouteData
		{
			internal double TrackPosition;
			internal double BlockInterval;
			/// <summary>OpenBVE runs internally in meters per second
			/// This value is used to convert between the speed set by Options.UnitsOfSpeed and m/s
			/// </summary>
			internal double UnitOfSpeed;
			internal bool SignedCant;
			internal bool FogTransitionMode;
			internal readonly StructureData Structure;
			internal SignalDictionary Signals;
			internal CompatibilitySignalObject[] CompatibilitySignals;
			internal Texture[] TimetableDaytime;
			internal Texture[] TimetableNighttime;
			internal BackgroundDictionary Backgrounds;
			internal double[] SignalSpeeds;
			internal List<Block> Blocks;
			internal Marker[] Markers;
			internal StopRequest[] RequestStops;
			internal int FirstUsedBlock;
			internal bool IgnorePitchRoll;
			internal bool LineEndingFix;
			internal bool ValueBasedSections = false;
			internal bool TurnUsed = false;
			internal bool SwitchUsed = false;

			internal List<string> ScriptedTrainFiles;
			/*
			 * HMMSIM
			 */
			internal readonly Dictionary<string, int> RailKeys = new Dictionary<string, int>();

			internal RouteData(bool previewOnly)
			{
				BlockInterval = 25.0;
				FirstUsedBlock = -1;
				Blocks = new List<Block>();
				Structure = new StructureData();
				ScriptedTrainFiles = new List<string>();
				Blocks.Add(new Block(previewOnly));
				Blocks[0].Rails.Add(0, new Rail(2.0, 1.0) { RailStarted = true });
				Blocks[0].RailType = new[] { 0 };
				Blocks[0].CurrentTrackState = new TrackElement(0.0);
				Blocks[0].RailCycles = new RailCycle[1];
				Blocks[0].RailCycles[0].RailCycleIndex = -1;
			}

			/// <summary>Creates any missing blocks</summary>
			/// <param name="ToIndex">The block index to process until</param>
			/// <param name="PreviewOnly">Whether this is a preview only</param>
			internal void CreateMissingBlocks(int ToIndex, bool PreviewOnly)
			{
				if (ToIndex >= Blocks.Count)
				{
					for (int i = Blocks.Count; i <= ToIndex; i++)
					{
						Blocks.Add(new Block(PreviewOnly));
						if (!PreviewOnly)
						{
							Blocks[i].Background = -1;
							Blocks[i].Fog = Blocks[i - 1].Fog;
							Blocks[i].FogDefined = false;
							Blocks[i].Cycle = Blocks[i - 1].Cycle;
							Blocks[i].Height = double.NaN;
							Blocks[i].SnowIntensity = Blocks[i - 1].SnowIntensity;
							Blocks[i].RainIntensity = Blocks[i - 1].RainIntensity;
							Blocks[i].WeatherObject = Blocks[i - 1].WeatherObject;
							Blocks[i].LightDefinition = Blocks[i - 1].LightDefinition;
							Blocks[i].DynamicLightDefinition = Blocks[i -1].DynamicLightDefinition;
							Blocks[i].Switches = new Switch[] { };
						}
						Blocks[i].RailCycles = Blocks[i - 1].RailCycles;
						Blocks[i].RailType = new int[Blocks[i - 1].RailType.Length];
						if (!PreviewOnly)
						{
							for (int j = 0; j < Blocks[i].RailType.Length; j++)
							{
								int rc = -1;
								if (Blocks[i].RailCycles.Length > j)
								{
									rc = Blocks[i].RailCycles[j].RailCycleIndex;
								}
								if (rc != -1 && Structure.RailCycles.Length > rc && Structure.RailCycles[rc].Length > 1)
								{
									int cc = Blocks[i].RailCycles[j].CurrentCycle;
									if (cc == Structure.RailCycles[rc].Length - 1)
									{
										Blocks[i].RailType[j] = Structure.RailCycles[rc][0];
										Blocks[i].RailCycles[j].CurrentCycle = 0;
									}
									else
									{
										cc++;
										Blocks[i].RailType[j] = Structure.RailCycles[rc][cc];
										Blocks[i].RailCycles[j].CurrentCycle++;
									}
								}
								else
								{
									Blocks[i].RailType[j] = Blocks[i - 1].RailType[j];
								}
							}
						}
						
						for (int j = 0; j < Blocks[i - 1].Rails.Count; j++)
						{
							int key = Blocks[i - 1].Rails.ElementAt(j).Key;
							Rail rail = new Rail(Blocks[i - 1].Rails[key].Accuracy,Blocks[i - 1].Rails[key].AdhesionMultiplier)
							{
								RailStarted = Blocks[i -1].Rails[key].RailStarted,
								RailStart = new Vector2(Blocks[i -1].Rails[key].RailStart),
								RailStartRefreshed = false,
								RailEnded = false,
								RailEnd = new Vector2(Blocks[i - 1].Rails[key].RailStart),
								IsDriveable = Blocks[i - 1].Rails[key].IsDriveable,
							};
							Blocks[i].Rails.Add(key, rail);
						}
						if (!PreviewOnly)
						{
							Blocks[i].RailWall = new Dictionary<int, WallDike>();
							for (int j = 0; j < Blocks[i - 1].RailWall.Count; j++)
							{
								int key = Blocks[i - 1].RailWall.ElementAt(j).Key;
								if (Blocks[i - 1].RailWall[key] == null || !Blocks[i - 1].RailWall[key].Exists)
								{
									continue;
								}
								Blocks[i].RailWall.Add(key, Blocks[i - 1].RailWall[key].Clone());
							}
							Blocks[i].RailDike = new Dictionary<int, WallDike>();
							for (int j = 0; j < Blocks[i - 1].RailDike.Count; j++)
							{
								int key = Blocks[i - 1].RailDike.ElementAt(j).Key;
								if (Blocks[i - 1].RailDike[key] == null || !Blocks[i - 1].RailDike[key].Exists)
								{
									continue;
								}
								Blocks[i].RailDike.Add(key, Blocks[i - 1].RailDike[key].Clone());
							}
							Blocks[i].RailPole = new Pole[Blocks[i - 1].RailPole.Length];
							for (int j = 0; j < Blocks[i].RailPole.Length; j++)
							{
								Blocks[i].RailPole[j] = Blocks[i - 1].RailPole[j];
							}

							for (int j = 0; j < Blocks[i - 1].PatternObjs.Count; j++)
							{
								int key = Blocks[i - 1].PatternObjs.ElementAt(j).Key;
								if (Blocks[i - 1].PatternObjs[key] == null || Blocks[i - 1].PatternObjs[key].Ends)
								{
									continue;
								}
								Blocks[i].PatternObjs.Add(key, Blocks[i - 1].PatternObjs[key].Clone());
							}
						}
						Blocks[i].Pitch = Blocks[i - 1].Pitch;
						Blocks[i].CurrentTrackState = Blocks[i - 1].CurrentTrackState;
						Blocks[i].Turn = 0.0;
					}
				}
			}
		}
		
	}
}
