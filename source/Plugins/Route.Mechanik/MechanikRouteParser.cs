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

using System;
using System.Collections.Generic;
using System.IO;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using System.Text.RegularExpressions;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using OpenBveApi.World;
using RouteManager2.Events;
using RouteManager2.Stations;
using Path = OpenBveApi.Path;

namespace MechanikRouteParser
{
	internal class Parser
	{
		private static RouteData currentRouteData;
		private static List<MechanikObject> AvailableObjects = new List<MechanikObject>();
		private static Dictionary<int, MechanikTexture> AvailableTextures = new Dictionary<int, MechanikTexture>();
		private static Dictionary<int, SoundHandle> AvailableSounds = new Dictionary<int, SoundHandle>();
		private static string RouteFolder;

		internal void ParseRoute(string routeFile, bool PreviewOnly)
		{
			if (!File.Exists(routeFile))
			{
				return;
			}

			Plugin.CurrentRoute.AccurateObjectDisposal = ObjectDisposalMode.Mechanik;
			AvailableObjects = new List<MechanikObject>();
			AvailableTextures = new Dictionary<int, MechanikTexture>();
			AvailableSounds = new Dictionary<int, SoundHandle>();
			currentRouteData = new RouteData();
			if(PreviewOnly)
			{
				string routeImage = Path.CombineFile(RouteFolder, "laduj_01.jpg");
				if (File.Exists(routeImage))
				{
					Plugin.CurrentRoute.Image = routeImage;
				}

			}
			//Load texture list
			RouteFolder = System.IO.Path.GetDirectoryName(routeFile);
			string tDat = Path.CombineFile(RouteFolder, "tekstury.dat");
			if (!File.Exists(tDat))
			{
				/*
				 * TODO: Separate into a function
				 * Various Mechanik routes use custom hardcoded DAT paths
				 * Thus, we will need to keep a search list for both the trasa and tekstury files
				 */
				tDat = Path.CombineFile(RouteFolder, "s80_text.dat"); //S80 U-Bahn
				if (!File.Exists(tDat))
				{
					return;
				}
			}
			LoadTextureList(tDat);
			string sDat = Path.CombineFile(RouteFolder, "dzwieki.dat");
			if (!File.Exists(sDat))
			{
				sDat = Path.CombineFile(RouteFolder, "s80_snd.dat"); //S80 U-Bahn
				if (!File.Exists(tDat))
				{
					return;
				}
			}
			LoadSoundList(sDat);
			string[] routeLines = File.ReadAllLines(routeFile);
			double yOffset = 0.0;
			for (int i = 0; i < routeLines.Length; i++)
			{
				int j = routeLines[i].IndexOf(@"//", StringComparison.Ordinal);
				if (j != -1)
				{
					//Split out comments
					routeLines[i] = routeLines[i].Substring(0, j);
				}
				if (String.IsNullOrWhiteSpace(routeLines[i]))
				{
					continue;
				}
				routeLines[i] = Regex.Replace(routeLines[i], @"\s+", " ");
				string[] Arguments = routeLines[i].Trim().Split(null);
				double trackPosition, scaleFactor;
				int Idx = -1, blockIndex, textureIndex;
				if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
				{
					//Second argument is always track position in KM
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid track position encountered in " + Arguments[0] + " at line " + i);
					continue;
				}
				switch (Arguments[0].ToLowerInvariant())
				{
					case "'s":
						if (PreviewOnly)
						{
							continue;
						}
						/*
						 * PERPENDICULAR PLANE OBJECTS
						 * => Track Position
						 * => Top Left X
						 * =>          Y
						 * =>          Z
						 * => Scale factor (200px in image == 1m at factor 1)
						 *
						 */
						Vector3 topLeft = new Vector3();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out topLeft.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TopLeft X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out topLeft.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TopLeft Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						topLeft.Y = -topLeft.Y;
						if (Arguments.Length < 5 || !TryParseDistance(Arguments[4], out topLeft.Z))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TopLeft Z encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 6 || !double.TryParse(Arguments[5], out scaleFactor))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid ScaleFactor encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 7 || !int.TryParse(Arguments[6], out textureIndex))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TextureIndex encountered in " + Arguments[0] + " at line " + i);
							continue;
						}

						Idx = CreatePerpendicularPlane(topLeft, scaleFactor, textureIndex, true);
						blockIndex = currentRouteData.FindBlock(trackPosition);
						
						currentRouteData.Blocks[blockIndex].Objects.Add(new RouteObject(Idx, new Vector3(0,0,0)));
						break;
					case "#t":
					case "#t_p":
					case "#t_prz":
						if (PreviewOnly)
						{
							continue;
						}
						yOffset -= 0.001;
						/*
						 * HORIZONTAL PLANE OBJECTS
						 * => Track Position
						 * => Number of Points (3,4, or 5)
						 * => 5x Point declarations (X,Y,Z)
						 * => 6 unused
						 * => Point for beginning of texture (?Top left equivilant?)
						 * => Wrap W (?)
						 * => Wrap H (?)
						 * => Texture scale (Number of repetitions?)
						 * => Texture IDX
						 * => Furthest point: Determines when this vanishes (When the cab passes?)
						 */
						int numPoints, firstPoint;
						if (!int.TryParse(Arguments[2], out numPoints))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid NumberOfPoints encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (numPoints < 3 || numPoints > 5)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "NumberOfPoints must be between 3 and 5 in " + Arguments[0] + " at line " + i);
							continue;
						}
						int v = 0;
						List<Vector3> points = new List<Vector3>();
						Vector3 currentPoint = new Vector3();
						for (int p = 3; p < 24; p++)
						{
							switch (v)
							{
								case 0:
									if (!TryParseDistance(Arguments[p], out currentPoint.X))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid X encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
									}
									break;
								case 1:
									if (!TryParseDistance(Arguments[p], out currentPoint.Y))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Y encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
									}
									currentPoint.Y = -currentPoint.Y;
									currentPoint.Y += yOffset;
									break;
								case 2:
									if (!TryParseDistance(Arguments[p], out currentPoint.Z))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Z encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
									}
									break;
							}
							if (v < 2)
							{
								v++;
							}
							else
							{
								points.Add(currentPoint);
								v = 0;
							}
						}
						if (!int.TryParse(Arguments[24], out firstPoint))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FirstPoint encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (firstPoint != 0)
						{
							firstPoint -= 1;
						}
						double sx;
						double sy;
						if (!double.TryParse(Arguments[25], out sx))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Scale X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (!double.TryParse(Arguments[26], out sy))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Scale Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (!double.TryParse(Arguments[27], out scaleFactor))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid ScaleFactor encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (!int.TryParse(Arguments[28], out textureIndex))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TextureIndex encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						List<Vector3> sortedPoints = new List<Vector3>();
						/*
						 * Pull out the points making up our face
						 */
						for (int k = 0; k < numPoints; k++)
						{
							sortedPoints.Add(points[k]);
						}

						switch (Arguments[0].ToLowerInvariant())
						{
							case "#t_prz":
								//FREE, transparent
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, true, false);
								break;
							case "#t_p":
								//HORIZONTAL, no transparency
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, false, true);
								break;
							case "#t":
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, false, false);
								break;
						}
						
						blockIndex = currentRouteData.FindBlock(trackPosition);
						
						currentRouteData.Blocks[blockIndex].Objects.Add(new RouteObject(Idx, new Vector3(0, 0, 0)));
						break;
					case "'o":
						//Rotation marker for the player track, roughly equivilant to .turn
						double radians;
						Vector2 turnPoint = new Vector2();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out turnPoint.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TurnPoint X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out turnPoint.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TurnPoint Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !double.TryParse(Arguments[4], out radians))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid TurnRaidus encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						double dist = trackPosition + Math.Sqrt(turnPoint.X*turnPoint.X+turnPoint.Y*turnPoint.Y);
						blockIndex = currentRouteData.FindBlock(dist);
						currentRouteData.Blocks[blockIndex].Turn = radians / 1000000.0;
						break;
					case "'k":
						//Rotates the world to zero after a curve
						//Going to give me a headache, but uncommon
						Vector2 correctionPoint1 = new Vector2(), correctionPoint2 = new Vector2();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out correctionPoint1.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FirstCorrectionPoint X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out correctionPoint1.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FirstCorrectionPoint Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !TryParseDistance(Arguments[4], out correctionPoint2.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid SecondCorrectionPoint X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 6 || !TryParseDistance(Arguments[5], out correctionPoint2.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid SecondCorrectionPoint Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						double firstCorrectionDist = trackPosition + Math.Sqrt(correctionPoint1.X*correctionPoint1.X+correctionPoint1.Y*correctionPoint1.Y);
						double correctionDist = firstCorrectionDist + Math.Sqrt(correctionPoint2.X*correctionPoint2.X+correctionPoint2.Y*correctionPoint2.Y);
						blockIndex = currentRouteData.FindBlock(correctionDist);
						currentRouteData.Blocks[blockIndex].Correction = true;
						break;
					/*
					 * Both sounds and speed limits are invisible markers
					 * Currently unclear as to whether the X and Z must validate
					 * or whether they can be ignored; For the minute, let's
					 * assume that they must be validated
					 */
					case "'z_d":
						if (PreviewOnly)
						{
							continue;
						}
						//Sound marker
						int soundNumber;
						bool looped;
						bool speedDependant;
						int volume;
						Vector3 soundPosition = new Vector3();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out soundPosition.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out soundPosition.Z))
						{
							//CHECK: Should this actually be the Y position of the sound?
							//This seems far more logical to me.....
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Z encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !int.TryParse(Arguments[4], out soundNumber))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid SoundNumber encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						int valueCheck; //Documentation states this value must be 5, unclear as to purpose
						if (Arguments.Length < 6 || !int.TryParse(Arguments[5], out valueCheck) || valueCheck != 5)
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 7 || !TryParseBool(Arguments[6], out looped))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Looped encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 8 || !TryParseBool(Arguments[7], out speedDependant))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid SpeedDependant encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 9 || !int.TryParse(Arguments[8], out volume))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Volume encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (looped)
						{
							//As looped, use the position as-is
							blockIndex = currentRouteData.FindBlock(trackPosition);
							currentRouteData.Blocks[blockIndex].Sounds.Add(new SoundEvent(soundNumber, soundPosition, looped, speedDependant, volume));
						}
						else
						{
							//Otherwise, add the Z offset to the trackposition to give us the relative tpos for the event
							blockIndex = currentRouteData.FindBlock(trackPosition + soundPosition.Z);
							soundPosition.Z = 0;
							currentRouteData.Blocks[blockIndex].Sounds.Add(new SoundEvent(soundNumber, soundPosition, looped, speedDependant, volume));
						}
						
						break;
					case "'z_p":
						//Speed limit
						double kph;
						// ReSharper disable once NotAccessedVariable
						Vector2 limitPos = new Vector2();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out limitPos.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out limitPos.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !double.TryParse(Arguments[4], out kph))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Speed encountered in " + Arguments[0] + " at line " + i);
							continue;
						}

						blockIndex = currentRouteData.FindBlock(trackPosition);
						currentRouteData.Blocks[blockIndex].SpeedLimit = kph;
						break;
					case "'z_z":
						//Station stop marker
						bool terminal;
						Vector2 stopPos = new Vector2();
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out stopPos.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out stopPos.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !TryParseBool(Arguments[4], out terminal))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid IsTerminal encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						blockIndex = currentRouteData.FindBlock(trackPosition);
						if (currentRouteData.Blocks[blockIndex].stopMarker == null)
						{
							currentRouteData.Blocks[blockIndex].stopMarker = new StationStop();
						}
						if (terminal)
						{
							currentRouteData.Blocks[blockIndex].stopMarker.endPosition = stopPos.Y;
						}
						else
						{
							currentRouteData.Blocks[blockIndex].stopMarker.startPosition = stopPos.Y;
						}
						break;
					case "'sem":
						/*
						 * SIGNAL
						 * Aspects Listing:
						 * 0 - Red, 0km/h
						 * 2 - 40km/h
						 * 3 - 60km/h
						 * 4 - 100km/h
						 * 5 - Unlimited
						 * Type Listing:
						 * 1 - Equivilant to 2 aspect as specified in command
						 * 2 - Equivilant to 3 aspect, red + 2 aspects in command. This is used as a station stop signal, so the station needs to set HeldRedSignal (??)
						 *
						 * => Track Position
						 * => Position X,Y,Z
						 * => First aspect
						 * => Second aspect
						 * => Signal Type
						 */
						break;
					case "'z_s":
						/*
						 * WHISTLE SECTION MARKER
						 * Must sound the train horn between first and last markers
						 *
						 * => Track position
						 * => Location X, Z
						 * => Control- 1 for start, 0 for stop
						 */
						break;
					case "'z_shp":
						/*
						 * AUTO-BRAKE MARKER
						 * Unclear, but let's treat this as a brake application if signal is at aspect 0
						 *
						 * => Track position
						 * => Location X, Z
						 * => Control- Must be 1
						 */
						break;
					case "'z_jb":
						/*
						 * NEUTRAL SECTION
						 * Not currently implemented in BVE, but on the list of things to add
						 * Could possibly do it now??
						 *
						 * => Track position
						 * => Location X, Z
						 * => Control- 1 for start, 0 for stop
						 */
						break;
					default:
						//Mechanik ignores all unrecognised lines, and has no native error checking
						//This means we can't do anything further than checking if this is a recognised command start character
						if (Arguments[0].StartsWith("'") || Arguments[0].StartsWith("#"))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Unimplemented command " + Arguments[0] + " encountered at line " + i);
						}
						break;
				}

			}
			//Insert a stop in the first block, as Mechanik always starts at pos 0, wheras BVE starts at the first stop
			int blockZero = currentRouteData.FindBlock(0);
			currentRouteData.Blocks[blockZero].stopMarker = new StationStop
			{
				startPosition = 0,
				endPosition = 12.5
			};
			currentRouteData.Blocks.Sort((x, y) => x.StartingTrackPosition.CompareTo(y.StartingTrackPosition));
			currentRouteData.CreateMissingBlocks();
			ProcessRoute(PreviewOnly);
		}

		private static void ProcessRoute(bool PreviewOnly)
		{
			if (!PreviewOnly)
			{
				Texture bt = null;
				string f = Path.CombineFile(RouteFolder, "obloczki.bmp");
				if (System.IO.File.Exists(f))
				{
					Plugin.CurrentHost.RegisterTexture(f, new TextureParameters(null, null), out bt);
				}

				Plugin.CurrentRoute.CurrentBackground = new StaticBackground(bt, 2, false);
				Plugin.CurrentRoute.TargetBackground = Plugin.CurrentRoute.CurrentBackground;
			}

			Vector3 worldPosition = new Vector3(0.0, 0.0, 0.0);
			Vector2 worldDirection = new Vector2(0.0, 1.0);
			Vector3 trackPosition = new Vector3(0.0, 0.0, 0.0);
			Vector2 trackDirection = new Vector2(0.0, 1.0);
			Plugin.CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			int CurrentTrackLength = 0;
			double StartingDistance = 0;
			for (int i = 0; i < currentRouteData.Blocks.Count; i++)
			{
				// normalize
				Normalize(ref worldDirection.X, ref worldDirection.Y);
				Normalize(ref trackDirection.X, ref trackDirection.Y);


				// track
				TrackElement WorldTrackElement = new TrackElement(currentRouteData.Blocks[i].StartingTrackPosition);

				int n = CurrentTrackLength;
				if (n >= Plugin.CurrentRoute.Tracks[0].Elements.Length)
				{
					Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements, Plugin.CurrentRoute.Tracks[0].Elements.Length << 1);
				}
				Plugin.CurrentRoute.Tracks[0].Elements[n] = WorldTrackElement;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldPosition = trackPosition;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection = Vector3.GetVector3(trackDirection, 0);
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide = new Vector3(trackDirection.Y, 0.0, -trackDirection.X);
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				CurrentTrackLength++;

				double blockLength;
				if (i == currentRouteData.Blocks.Count - 1)
				{
					blockLength = 25;
				}
				else
				{
					blockLength = currentRouteData.Blocks[i + 1].StartingTrackPosition - currentRouteData.Blocks[i].StartingTrackPosition;
				}
				StartingDistance += blockLength;
				Transformation t = new Transformation();
				if (!PreviewOnly)
				{
					t = new Transformation(Math.Atan2(worldDirection.X, worldDirection.Y), 0, 0);
				}
				for (int j = 0; j < currentRouteData.Blocks[i].Objects.Count; j++)
				{
					AvailableObjects[currentRouteData.Blocks[i].Objects[j].objectIndex].Object.CreateObject(worldPosition, t, StartingDistance, StartingDistance + 25, 100);
				}
				// finalize block
				worldPosition.X += worldDirection.X * blockLength;
				worldPosition.Z += worldDirection.Y * blockLength;
				trackPosition.X += trackDirection.X * blockLength;
				trackPosition.Z += trackDirection.Y * blockLength;
				if (currentRouteData.Blocks[i].Turn != 0.0)
				{
					double ag = -Math.Atan(currentRouteData.Blocks[i].Turn);
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					trackDirection.Rotate(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				}
				if (i < currentRouteData.Blocks.Count - 1 && currentRouteData.Blocks[i + 1].Correction)
				{
					worldPosition = trackPosition;
					worldDirection = trackDirection;
				}

				for (int j = 0; j < currentRouteData.Blocks[i].Sounds.Count; j++)
				{
					if (!AvailableSounds.ContainsKey(currentRouteData.Blocks[i].Sounds[j].SoundIndex))
					{
						// -1 does somethig a little funny
						// Stops playing sounds (all??)
						// For the minute, let's ignore and see how much further we get
						continue;
					}
					int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
					Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
					Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new RouteManager2.Events.SoundEvent(0, AvailableSounds[currentRouteData.Blocks[i].Sounds[j].SoundIndex], true, false, currentRouteData.Blocks[i].Sounds[j].Looped, false, currentRouteData.Blocks[i].Sounds[j].Position, Plugin.CurrentHost);
				}

				if (currentRouteData.Blocks[i].stopMarker != null)
				{
					//Use the stop markers to generate station events
					//Mechanik doesn't support station names, so let's just call them Station N
					int s = Plugin.CurrentRoute.Stations.Length;
					Array.Resize(ref Plugin.CurrentRoute.Stations, s + 1);
					Plugin.CurrentRoute.Stations[s] = new RouteStation
					{
						Name = "Station " + (s + 1),
						OpenLeftDoors = true,
						OpenRightDoors = true,
						Stops = new [] 
						{ 
							new RouteManager2.Stations.StationStop
							{
								ForwardTolerance = currentRouteData.Blocks[i].stopMarker.startPosition, 
								BackwardTolerance = currentRouteData.Blocks[i].stopMarker.endPosition,
								Cars = 0,
								TrackPosition = currentRouteData.Blocks[i].StartingTrackPosition
							}
						}
					};
					
					int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
					Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
					Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new StationStartEvent(0, s);
				}
			}
			// insert station end events
			int lastStationBlock = 0;
			for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
			{
				int j = Plugin.CurrentRoute.Stations[i].Stops.Length - 1;
				if (j >= 0)
				{
					double p = Plugin.CurrentRoute.Stations[i].Stops[j].TrackPosition + Plugin.CurrentRoute.Stations[i].Stops[j].ForwardTolerance + 25.0;
					for (int k = lastStationBlock; k < Plugin.CurrentRoute.Tracks[0].Elements.Length; k++)
					{
						if (Plugin.CurrentRoute.Tracks[0].Elements[k].StartingTrackPosition > p)
						{
							int e = Plugin.CurrentRoute.Tracks[0].Elements[k].Events.Length; 
							Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[k].Events, e + 1);
							Plugin.CurrentRoute.Tracks[0].Elements[k].Events[e] = new StationEndEvent(0, i, Plugin.CurrentRoute, Plugin.CurrentHost);
						}
					}
				}
			}
			Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements, CurrentTrackLength);
			Plugin.CurrentRoute.Tracks[0].Elements[CurrentTrackLength -1].Events = new GeneralEvent[] { new TrackEndEvent(Plugin.CurrentHost, 25) };
		}

		private static int CreateHorizontalObject(List<Vector3> Points, int firstPoint, double scaleFactor, double sx, double sy, int textureIndex, bool transparent, bool horizontal)
		{
			MechanikTexture t = AvailableTextures[textureIndex];
			MechanikObject o = new MechanikObject();
			o.TopLeft = new Vector3();
			o.TextureIndex = textureIndex;

			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);
			int v = Builder.Vertices.Length;
			Array.Resize(ref Builder.Vertices, v + Points.Count);
			int fl = Builder.Faces.Length;
			Array.Resize(ref Builder.Faces, Builder.Faces.Length + 1);
			Builder.Faces[fl] = new MeshFace { Vertices = new MeshFaceVertex[Points.Count] , Flags = MeshFace.Face2Mask };
			for (int i = 0; i < Points.Count; i++)
			{
				Builder.Faces[fl].Vertices[i].Index = (ushort) i;
				Builder.Faces[fl].Vertices[i].Normal = new Vector3();
				Builder.Vertices[v + i] = new Vertex(Points[i]);
				Builder.Vertices[v + i].TextureCoordinates = FindTextureCoordinate(i, firstPoint, Points, scaleFactor, sx, sy, t);
			}

			
			Builder.Materials = new [] { new Material(t.Path) };
			if (transparent)
			{
				Builder.Materials[0].TransparentColor = Color24.Black;
				Builder.Materials[0].Flags = MaterialFlags.TransparentColor;
			}
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			Builder.Apply(ref obj);
			o.Object = obj;
			AvailableObjects.Add(o);
			return AvailableObjects.Count - 1;
		}

		private static int CreatePerpendicularPlane(Vector3 topLeft, double scaleFactor, int textureIndex, bool transparent)
		{
			for (int i = 0; i < AvailableObjects.Count; i++)
			{
				if (AvailableObjects[i].TopLeft == topLeft && AvailableObjects[i].ScaleFactor == scaleFactor && AvailableObjects[i].TextureIndex == textureIndex)
				{
					return i;
				}
			}

			MechanikTexture t = AvailableTextures[textureIndex];
			MechanikObject o = new MechanikObject();
			o.TopLeft = new Vector3(0,0,0);
			o.TextureIndex = textureIndex;	
			//BUG: Not entirely sure why multiplying W & H by 5 makes this work....
			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);
			Builder.Vertices = new VertexTemplate[4];
			Builder.Vertices[0] = new Vertex(new Vector3(topLeft));
			Builder.Vertices[1] = new Vertex(new Vector3(topLeft.X + (t.Width * 5), topLeft.Y, topLeft.Z)); //upper right
			Builder.Vertices[2] = new Vertex(new Vector3((topLeft.X + (t.Width * 5)), (topLeft.Y - (t.Height * 5)), topLeft.Z)); //bottom right
			Builder.Vertices[3] = new Vertex(new Vector3(topLeft.X, (topLeft.Y - (t.Height * 5)), topLeft.Z)); //bottom left
			//Possibly change to Face, check this though (Remember that Mechanik was restricted to the cab, wheras we are not)
			Builder.Faces = new MeshFace[1];
			Builder.Faces[0] = new MeshFace { Vertices = new MeshFaceVertex[4], Flags = MeshFace.Face2Mask };
			Builder.Faces[0].Vertices = new MeshFaceVertex[4];
			Builder.Faces[0].Vertices[0] = new MeshFaceVertex(0);
			Builder.Faces[0].Vertices[1] = new MeshFaceVertex(1);
			Builder.Faces[0].Vertices[2] = new MeshFaceVertex(2);
			Builder.Faces[0].Vertices[3] = new MeshFaceVertex(3);
			Builder.Vertices[1].TextureCoordinates = new Vector2(1,0);
			Builder.Vertices[2].TextureCoordinates = new Vector2(1,1);
			Builder.Vertices[3].TextureCoordinates = new Vector2(0,1);
			Builder.Vertices[0].TextureCoordinates = new Vector2(0,0);

			Builder.Materials = new [] { new Material(t.Path) };
			if (transparent)
			{
				Builder.Materials[0].TransparentColor = Color24.Black;
				Builder.Materials[0].Flags = MaterialFlags.TransparentColor;
			}
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			Builder.Apply(ref obj);
			o.Object = obj;
			AvailableObjects.Add(o);
			return AvailableObjects.Count - 1;
		}

		private static void LoadTextureList(string tDat)
		{
			string[] textureLines = System.IO.File.ReadAllLines(tDat);
			for (int i = 0; i < textureLines.Length; i++)
			{
				int j = textureLines[i].IndexOf(@"//", StringComparison.Ordinal);
				if (j != -1)
				{
					//Split out comments
					textureLines[i] = textureLines[i].Substring(j, textureLines[i].Length - 1);
				}
				if (String.IsNullOrWhiteSpace(textureLines[i]))
				{
					continue;
				}
				textureLines[i] = Regex.Replace(textureLines[i], @"\s+", " ");
				int k = 0;
				string s = null;
				for (int l = 0; l < textureLines[i].Length; l++)
				{
					if (!char.IsDigit(textureLines[i][l]))
					{
						k = int.Parse(textureLines[i].Substring(0, l));
						s = textureLines[i].Substring(l, textureLines[i].Length - l).Trim();
						break;
					}
				}
				if (!String.IsNullOrWhiteSpace(s))
				{
					string path = Path.CombineFile(System.IO.Path.GetDirectoryName(tDat), s);
					if (System.IO.File.Exists(path))
					{
						MechanikTexture t = new MechanikTexture(path, s);
						AvailableTextures.Add(k, t);
					}

				}

			}
		}

		private static void LoadSoundList(string sDat)
		{
			string[] soundLines = System.IO.File.ReadAllLines(sDat);
			for (int i = 0; i < soundLines.Length; i++)
			{
				int j = soundLines[i].IndexOf(@"//", StringComparison.Ordinal);
				if (j != -1)
				{
					//Split out comments
					soundLines[i] = soundLines[i].Substring(j, soundLines[i].Length - 1);
				}
				if (String.IsNullOrWhiteSpace(soundLines[i]))
				{
					continue;
				}
				soundLines[i] = Regex.Replace(soundLines[i], @"\s+", " ");
				int k = 0;
				string s = null;
				for (int l = 0; l < soundLines[i].Length; l++)
				{
					if (!char.IsDigit(soundLines[i][l]))
					{
						k = int.Parse(soundLines[i].Substring(0, l));
						s = soundLines[i].Substring(l, soundLines[i].Length - l).Trim();
						break;
					}
				}
				if (!String.IsNullOrWhiteSpace(s))
				{
					string path = Path.CombineFile(System.IO.Path.GetDirectoryName(sDat), s);
					if (System.IO.File.Exists(path))
					{
						SoundHandle handle;
						Plugin.CurrentHost.RegisterSound(path, out handle);
						AvailableSounds.Add(k, handle);
					}

				}

			}
		}

		private static void Normalize(ref double x, ref double y)
		{
			double t = x * x + y * y;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}

		private static bool TryParseDistance(string val, out double position)
		{
			if (double.TryParse(val, out position))
			{
				position /= 200; //200px per meter scale
				return true;
			}
			return false;
		}

		private static bool TryParseBool(string val, out bool boolean)
		{
			int value;
			int.TryParse(val, out value);
			switch (value)
			{
				case 0:
					boolean = false;
					return true;
				case 1:
					boolean = true;
					return true;
				default:
					boolean = false;
					return false;
			}
		}

		/// <summary>Finds the openGL texture co-ordinates for the specified point</summary>
		/// <param name="pointIndex">The point</param>
		/// <param name="firstPoint">The first point in the face winding</param>
		/// <param name="pointList">The list of points</param>
		/// <param name="scale">The texture scale factor</param>
		/// <param name="u"></param>
		/// <param name="v"></param>
		/// <param name="texture">The texture to use</param>
		/// <returns>The texture co-ordinate vector</returns>
		private static Vector2 FindTextureCoordinate(int pointIndex, int firstPoint, List<Vector3> pointList, double scale, double u, double v, MechanikTexture texture)
		{
			if (firstPoint > pointList.Count)
			{
				firstPoint = 0;
			}
			Vector3 V = new Vector3(pointList[1] - pointList[0]);
			Vector3 U_o = new Vector3(pointList[2] - pointList[1]);
			Vector3 U = U_o - Vector3.Project(U_o, V);
			Vector3 t1 = Vector3.Project(pointList[pointIndex] - pointList[firstPoint], U);

			double coordinatesX = 0.0;
			if (t1.Norm() != 0.0)
			{
				Vector3 newVect = Vector3.Normalize(t1) * Vector3.Normalize(U);
				double number = t1.Norm() * newVect.X + t1.Norm() * newVect.Y + t1.Norm() * newVect.Z;
				coordinatesX = number / scale / v / texture.Width;
			}

			Vector3 t2 = Vector3.Project(pointList[pointIndex] - pointList[firstPoint], V);
			
			double coordinatesY = 0.0;
			if (t2.Norm() != 0.0)
			{
				Vector3 newVect = Vector3.Normalize(t2) * Vector3.Normalize(V);
				double number = t2.Norm() * newVect.X + t2.Norm() * newVect.Y + t2.Norm() * newVect.Z;
				coordinatesY = number / scale / u / texture.Height;
			}
			//FIXME: Why does Y need negating??
			return new Vector2(coordinatesX, -coordinatesY);
		}

	}
}
