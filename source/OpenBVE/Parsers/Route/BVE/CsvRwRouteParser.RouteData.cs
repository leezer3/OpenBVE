using System;

namespace OpenBve
{
	internal partial class CsvRwRouteParser
	{
		private class RouteData
		{
			internal double TrackPosition;
			internal double BlockInterval;
			/// <summary>OpenBVE runs internally in meters per second
			/// This value is used to convert between the speed set by Options.UnitsOfSpeed and m/s
			/// </summary>
			internal double UnitOfSpeed;
			/// <summary>If this bool is set to FALSE, then objects will disappear when the block in which their command is placed is exited via by the camera
			/// Certain BVE2/4 era routes used this as an animation trick
			/// </summary>
			internal bool AccurateObjectDisposal;
			internal bool SignedCant;
			internal bool FogTransitionMode;
			internal StructureData Structure;
			internal SignalData[] Signals;
			internal CompatibilitySignalData[] CompatibilitySignals;
			internal Textures.Texture[] TimetableDaytime;
			internal Textures.Texture[] TimetableNighttime;
			internal BackgroundManager.BackgroundHandle[] Backgrounds;
			internal double[] SignalSpeeds;
			internal Block[] Blocks;
			internal Marker[] Markers;
			internal StopRequest[] RequestStops;
			internal int FirstUsedBlock;
			internal bool IgnorePitchRoll;
			internal bool LineEndingFix;

			/// <summary>Creates any missing blocks</summary>
			/// <param name="BlocksUsed">The total number of blocks currently used (Will be updated via ref)</param>
			/// <param name="ToIndex">The block index to process until</param>
			/// <param name="PreviewOnly">Whether this is a preview only</param>
			internal void CreateMissingBlocks(ref int BlocksUsed, int ToIndex, bool PreviewOnly)
			{
				if (ToIndex >= BlocksUsed)
				{
					while (Blocks.Length <= ToIndex)
					{
						Array.Resize<Block>(ref Blocks, Blocks.Length << 1);
					}
					for (int i = BlocksUsed; i <= ToIndex; i++)
					{
						Blocks[i] = new Block();
						if (!PreviewOnly)
						{
							Blocks[i].Background = -1;
							Blocks[i].Brightness = new Brightness[] { };
							Blocks[i].Fog = Blocks[i - 1].Fog;
							Blocks[i].FogDefined = false;
							Blocks[i].Cycle = Blocks[i - 1].Cycle;
							Blocks[i].RailCycle = Blocks[i - 1].RailCycle;
							Blocks[i].Height = double.NaN;
						}
						Blocks[i].RailType = new int[Blocks[i - 1].RailType.Length];
						if (!PreviewOnly)
						{
							for (int j = 0; j < Blocks[i].RailType.Length; j++)
							{
								int rc = -1;
								if (Blocks[i].RailCycle.Length > j)
								{
									rc = Blocks[i].RailCycle[j].RailCycleIndex;
								}
								if (rc != -1 && Structure.RailCycle.Length > rc && Structure.RailCycle[rc].Length > 1)
								{
									int cc = Blocks[i].RailCycle[j].CurrentCycle;
									if (cc == Structure.RailCycle[rc].Length - 1)
									{
										Blocks[i].RailType[j] = Structure.RailCycle[rc][0];
										Blocks[i].RailCycle[j].CurrentCycle = 0;
									}
									else
									{
										cc++;
										Blocks[i].RailType[j] = Structure.RailCycle[rc][cc];
										Blocks[i].RailCycle[j].CurrentCycle++;
									}
								}
								else
								{
									Blocks[i].RailType[j] = Blocks[i - 1].RailType[j];
								}
							}
						}
						Blocks[i].Rail = new Rail[Blocks[i - 1].Rail.Length];
						for (int j = 0; j < Blocks[i].Rail.Length; j++)
						{
							Blocks[i].Rail[j].RailStart = Blocks[i - 1].Rail[j].RailStart;
							Blocks[i].Rail[j].RailStartX = Blocks[i - 1].Rail[j].RailStartX;
							Blocks[i].Rail[j].RailStartY = Blocks[i - 1].Rail[j].RailStartY;
							Blocks[i].Rail[j].RailStartRefreshed = false;
							Blocks[i].Rail[j].RailEnd = false;
							Blocks[i].Rail[j].RailEndX = Blocks[i - 1].Rail[j].RailStartX;
							Blocks[i].Rail[j].RailEndY = Blocks[i - 1].Rail[j].RailStartY;
						}
						if (!PreviewOnly)
						{
							Blocks[i].RailWall = new WallDike[Blocks[i - 1].RailWall.Length];
							for (int j = 0; j < Blocks[i].RailWall.Length; j++)
							{
								Blocks[i].RailWall[j] = Blocks[i - 1].RailWall[j];
							}
							Blocks[i].RailDike = new WallDike[Blocks[i - 1].RailDike.Length];
							for (int j = 0; j < Blocks[i].RailDike.Length; j++)
							{
								Blocks[i].RailDike[j] = Blocks[i - 1].RailDike[j];
							}
							Blocks[i].RailPole = new Pole[Blocks[i - 1].RailPole.Length];
							for (int j = 0; j < Blocks[i].RailPole.Length; j++)
							{
								Blocks[i].RailPole[j] = Blocks[i - 1].RailPole[j];
							}
							Blocks[i].Form = new Form[] { };
							Blocks[i].Crack = new Crack[] { };
							Blocks[i].Signal = new Signal[] { };
							Blocks[i].Section = new Section[] { };
							Blocks[i].Sound = new Sound[] { };
							Blocks[i].Transponder = new Transponder[] { };
							Blocks[i].DestinationChanges = new DestinationEvent[] { };
							Blocks[i].RailFreeObj = new FreeObj[][] { };
							Blocks[i].GroundFreeObj = new FreeObj[] { };
							Blocks[i].PointsOfInterest = new PointOfInterest[] { };
						}
						Blocks[i].Pitch = Blocks[i - 1].Pitch;
						Blocks[i].Limit = new Limit[] { };
						Blocks[i].Stop = new Stop[] { };
						Blocks[i].Station = -1;
						Blocks[i].StationPassAlarm = false;
						Blocks[i].CurrentTrackState = Blocks[i - 1].CurrentTrackState;
						Blocks[i].Turn = 0.0;
						Blocks[i].Accuracy = Blocks[i - 1].Accuracy;
						Blocks[i].AdhesionMultiplier = Blocks[i - 1].AdhesionMultiplier;
					}
					BlocksUsed = ToIndex + 1;
				}
			}
		}
		
	}
}
