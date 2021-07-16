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
using OpenBveApi.Runtime;
using OpenBveApi.Textures;
using OpenBveApi.World;
using Route.Mechanik;
using RouteManager2.Events;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using Path = OpenBveApi.Path;
using SoundHandle = OpenBveApi.Sounds.SoundHandle;

namespace MechanikRouteParser
{
	internal partial class Parser
	{
		private static RouteData currentRouteData;
		private static List<MechanikObject> AvailableObjects = new List<MechanikObject>();
		private static Dictionary<int, MechanikTexture> AvailableTextures = new Dictionary<int, MechanikTexture>();
		private static Dictionary<int, SoundHandle> AvailableSounds = new Dictionary<int, SoundHandle>();
		private static string RouteFolder;
		private static string RouteFile;

		internal void ParseRoute(string routeFile, bool PreviewOnly)
		{
			if (!File.Exists(routeFile))
			{
				return;
			}

			RouteFile = routeFile;
			Plugin.CurrentRoute.AccurateObjectDisposal = ObjectDisposalMode.Mechanik;
			AvailableObjects = new List<MechanikObject>();
			AvailableTextures = new Dictionary<int, MechanikTexture>();
			AvailableSounds = new Dictionary<int, SoundHandle>();
			currentRouteData = new RouteData();
			RouteFolder = System.IO.Path.GetDirectoryName(routeFile);
			if(PreviewOnly)
			{
				string routeImage = Path.CombineFile(RouteFolder, "laduj_01.jpg");
				if (File.Exists(routeImage))
				{
					Plugin.CurrentRoute.Image = routeImage;
				}

			}
			//Load texture list
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
				double scaleFactor;
				int Idx = -1, blockIndex, textureIndex;
				if(Arguments.Length < 2)
				{
					continue;
				}
				if (!TryParseDistance(Arguments[1], out var trackPosition))
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
						topLeft.Y += yOffset;
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
						if (Idx != -1)
						{
							currentRouteData.Blocks[blockIndex].Objects.Add(new RouteObject(Idx, new Vector3(0,0,0)));
						}
						break;
					case "#t":
					case "#t_p":
					case "#t_prz":
						if (PreviewOnly)
						{
							continue;
						}
						yOffset -= 0.00035;
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
						List<Vector3> points = new List<Vector3>();
						Vector3 currentPoint = new Vector3();
						for (int p = 3; p < 24; p += 3)
						{
							if (p > Arguments.Length - 1)
							{
								break;
							}
							if (!TryParseDistance(Arguments[p], out currentPoint.X))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid X encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
							}
							if (!TryParseDistance(Arguments[p + 1], out currentPoint.Y))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Y encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
							}
							currentPoint.Y = -currentPoint.Y;
							currentPoint.Y += yOffset; //Mechanik stacks textures in order. Use this as a hack to stop Z-fighting
							if (Arguments.Length > p + 2 && !TryParseDistance(Arguments[p + 2], out currentPoint.Z))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Z encountered in Point " + p + " in " + Arguments[0] + " at line " + i);
							}
							points.Add(currentPoint);
						}
						if (Arguments.Length < 25 || !int.TryParse(Arguments[24], out firstPoint))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid FirstPoint encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (firstPoint != 0)
						{
							firstPoint -= 1;
						}
						Vector2 textureScale = Vector2.One;
						if (!double.TryParse(Arguments[25], out textureScale.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Scale X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (!double.TryParse(Arguments[26], out textureScale.Y))
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

						if(numPoints > points.Count)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, numPoints + " points were declared, but the actual number of loaded points was " + points.Count + " in " + Arguments[0] + " at line " + i);
							break;
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
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, textureScale, textureIndex, true);
								break;
							case "#t_p":
								//HORIZONTAL, no transparency
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, textureScale, textureIndex, false);
								break;
							case "#t":
								Idx = CreateHorizontalObject(sortedPoints, firstPoint, scaleFactor, textureScale, textureIndex, false);
								break;
						}
						
						blockIndex = currentRouteData.FindBlock(trackPosition);
						if (Idx != -1)
						{
							currentRouteData.Blocks[blockIndex].Objects.Add(new RouteObject(Idx, new Vector3(0, 0, 0)));
							currentRouteData.Blocks[blockIndex].YOffset = yOffset;
						}
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
						currentRouteData.Blocks[blockIndex].Correction = new Correction(correctionPoint1, correctionPoint2);
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
						/*
						 * STATION STOP MARKER
						 * => Track position
						 * => X position of marker
						 * => Z position of marker
						 * => TRUE if start of stop zone, FALSE for end of stop zone
						 */
						bool isStart;
						double stopPosX, stopPosZ;
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out stopPosX))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out stopPosZ))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Z encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						trackPosition += stopPosZ;
						if (Arguments.Length < 5 || !TryParseBool(Arguments[4], out isStart))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid IsStart encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						blockIndex = currentRouteData.FindBlock(trackPosition);
						if (isStart)
						{
							currentRouteData.Blocks[blockIndex].stopMarker.Add(new StationStop(new Vector2(stopPosX, 0), isStart));
						}
						else
						{
							currentRouteData.Blocks[blockIndex].stopMarker.Insert(0, new StationStop(new Vector2(stopPosX, 0), isStart));
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
						Vector3 signalPosition = new Vector3();
						int firstAspect, secondAspect;
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out signalPosition.X))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position X encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out signalPosition.Y))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Y encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 5 || !TryParseDistance(Arguments[4], out signalPosition.Z))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid Position Z encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 6 || !int.TryParse(Arguments[5], out firstAspect))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid CurrentAspect encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (Arguments.Length < 7 || !int.TryParse(Arguments[6], out secondAspect))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid NextAspect encountered in " + Arguments[0] + " at line " + i);
							continue;
						}

						int signalType;
						bool heldAtRed;
						if (Arguments.Length < 8 || !int.TryParse(Arguments[7], out signalType))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid HeldAtRed encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						else
						{
							switch (signalType)
							{
								case 1:
									heldAtRed = true;
									break;
								case 3:
									heldAtRed = false;
									break;
								default:
									heldAtRed = false;
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid HeldAtRed encountered in " + Arguments[0] + " at line " + i);
									break;
							}
						}
						blockIndex = currentRouteData.FindBlock(trackPosition + signalPosition.Z);
						signalPosition.Y = -signalPosition.Y;
						signalPosition.Y += yOffset;
						signalPosition.Z = 0; //Add signal position Z to tpos and zero so we get the correct section positioning
						Semaphore sem = new Semaphore((SignalAspect) firstAspect, (SignalAspect) secondAspect, heldAtRed, signalPosition);
						currentRouteData.Blocks[blockIndex].Signals.Add(sem);
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
						bool shouldBlow;
						if (Arguments.Length < 6 || !TryParseBool(Arguments[5], out shouldBlow))
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid HornControl variable encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						if (shouldBlow)
						{
							
							//Add 25m to the section start trigger for a reasonable single position AI horn event
							blockIndex = currentRouteData.FindBlock(trackPosition + 25);
							currentRouteData.Blocks[blockIndex].HornBlow = true;
						}
						break;
					case "'z_shp":
						/*
						 * AUTO-BRAKE MARKER
						 * Unclear, but let's treat this as a brake application if signal is at aspect 0
						 * For the moment, add a default ATS-S / ATS-P transponder
						 *
						 * => Track position
						 * => Location X, Z
						 * => Control- Must be 1
						 */
						int control;
						if (Arguments.Length < 6 || !int.TryParse(Arguments[5], out control) || control != 1)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Invalid AutoBrake Control variable encountered in " + Arguments[0] + " at line " + i);
							continue;
						}
						blockIndex = currentRouteData.FindBlock(trackPosition);
						currentRouteData.Blocks[blockIndex].SignalBeacon = true;
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
			currentRouteData.Blocks[blockZero].stopMarker.Add(new StationStop(new Vector2(-10, 0), true));
			currentRouteData.Blocks[blockZero].stopMarker.Add(new StationStop(new Vector2(-10, 0), false));
			int numTiles = (int)(currentRouteData.Blocks[currentRouteData.Blocks.Count - 1].StartingTrackPosition / 1000);
			for (int i = 1; i < numTiles; i++)
			{
				/*
				 * UNDOCUMENTED:
				 * Mechanik divides it's world into 'tiles' which can be fed into the route generator supplied
				 * Documentation for these tiles states that they must be a multiple of 500m in length
				 *
				 * Under certain circumstances, the sim appears to issue an undocumented internal correction.
				 * Presumably, this must have something to do with the 
				 *
				 * Known examples:
				 * IRT-NY 2km or divisors of work
				 * UK DMU Route 1km required. 2km and 500m **does not** work
				 * 
				 */
				int blockIndex = currentRouteData.FindBlock(i * 1000);
				if (currentRouteData.Blocks[blockIndex].Correction == null)
				{
					currentRouteData.Blocks[blockIndex].Correction = new Correction(Vector2.Null, Vector2.Null);
				}
			}
			currentRouteData.Blocks.Sort((x, y) => x.StartingTrackPosition.CompareTo(y.StartingTrackPosition));
			currentRouteData.CreateMissingBlocks();

			

			ProcessRoute(PreviewOnly);
		}

		private static readonly Vector3 eyePosition = new Vector3(0.7, 2.0, 0.0);

		private static void ProcessRoute(bool PreviewOnly)
		{
			if (!PreviewOnly)
			{
				Texture bt = null;
				string f = Path.CombineFile(RouteFolder, "obloczki.bmp");
				if (File.Exists(f))
				{
					Plugin.CurrentHost.RegisterTexture(f, new TextureParameters(null, null), out bt);
				}
				else
				{
					f = Path.CombineFile(Plugin.FileSystem.GetDataFolder("Compatibility"), "Mechanik\\greysky.png");
					if (File.Exists(f))
					{

						Plugin.CurrentHost.RegisterTexture(f, new TextureParameters(null, null), out bt);
					}
				}

				Plugin.CurrentRoute.CurrentBackground = new StaticBackground(bt, 2, false);
				Plugin.CurrentRoute.TargetBackground = Plugin.CurrentRoute.CurrentBackground;
			}

			Vector3 worldPosition = new Vector3();
			Vector2 worldDirection = new Vector2(0.0, 1.0);
			Vector3 trackPosition = new Vector3(0.0, 0.0, 0.0);
			Vector2 trackDirection = new Vector2(0.0, 1.0);
			Plugin.CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			
			int CurrentTrackLength = 0;
			double StartingDistance = 0;
			bool nextHeldAtRed = false;
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
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldPosition.Y = currentRouteData.Blocks[i].YOffset;
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
				foreach (StationStop stop in currentRouteData.Blocks[i].stopMarker)
				{
					if (stop.Start)
					{
						int s = Plugin.CurrentRoute.Stations.Length;
						Array.Resize(ref Plugin.CurrentRoute.Stations, s + 1);
						Plugin.CurrentRoute.Stations[s] = new RouteStation
						{
							Name = "Station " + (s + 1) + " - " + $"{currentRouteData.Blocks[i].StartingTrackPosition / 1000 : 0.00}" + "km",
							OpenLeftDoors = true,
							OpenRightDoors = true,
							ArrivalTime = -1,
							DepartureTime = -1,
							DefaultTrackPosition = currentRouteData.Blocks[i].StartingTrackPosition,
							StopTime = 30
						};
						int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
						Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new StationStartEvent(0, s);
					}
					else
					{
						int s = Plugin.CurrentRoute.Stations.Length - 1;
						int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
						Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new StationEndEvent(Plugin.CurrentHost, Plugin.CurrentRoute, 0, s);
						Plugin.CurrentRoute.Stations[s].Stops = new[]
						{
							new RouteManager2.Stations.StationStop
							{
								BackwardTolerance = 25,
								ForwardTolerance = 25,
								Cars = 0,
								TrackPosition = currentRouteData.Blocks[i].StartingTrackPosition
							}
						};
					}
					//TODO: Add the appropriate 3D face
				}
				if (!PreviewOnly)
				{
					Transformation t = new Transformation(Math.Atan2(worldDirection.X, worldDirection.Y), 0, 0);
					for (int j = 0; j < currentRouteData.Blocks[i].Objects.Count; j++)
					{
						AvailableObjects[currentRouteData.Blocks[i].Objects[j].objectIndex].Object.CreateObject(worldPosition + eyePosition, t, StartingDistance, StartingDistance + 25, 100);
					}
					foreach (Semaphore signal in currentRouteData.Blocks[i].Signals)
					{

						int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
						Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
						int s = Plugin.CurrentRoute.Sections.Length;
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new SectionChangeEvent(Plugin.CurrentRoute, 0, s, s + 1);
						Array.Resize(ref Plugin.CurrentRoute.Sections, s + 1);
						SectionAspect[] newAspects = {
							new SectionAspect(0, 0),
							new SectionAspect(1, signal.SpeedLimit)
						};
						if (s == 0)
						{
							Plugin.CurrentRoute.Sections[s] = new Section(currentRouteData.Blocks[i].StartingTrackPosition, newAspects, SectionType.IndexBased);
						}
						else
						{
							Plugin.CurrentRoute.Sections[s] = new Section(currentRouteData.Blocks[i].StartingTrackPosition, newAspects, SectionType.IndexBased, Plugin.CurrentRoute.Sections[s - 1]);
							Plugin.CurrentRoute.Sections[s - 1].NextSection = Plugin.CurrentRoute.Sections[s];
						}
						Plugin.CurrentRoute.Sections[s].StationIndex = -1;	
						if (nextHeldAtRed)
						{
							Plugin.CurrentRoute.Sections[s].StationIndex = 0;
							for (int k = Plugin.CurrentRoute.Stations.Length - 1; k > 0; k--)
							{
								if (Plugin.CurrentRoute.Stations[k].DefaultTrackPosition + 100 < currentRouteData.Blocks[i].StartingTrackPosition)
								{
									Plugin.CurrentRoute.Sections[s].StationIndex = k;
									break;
								}
							}

							nextHeldAtRed = false;
						}

						if (signal.HeldAtRed)
						{
							nextHeldAtRed = true;
						}
						
						signal.Object().CreateObject(worldPosition + eyePosition, t, Transformation.NullTransformation, s + 1, StartingDistance, 1.0);
					}

					if (currentRouteData.Blocks[i].HornBlow)
					{
						int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
						Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new HornBlowEvent(0, HornTypes.Primary, true);
					}
					if (currentRouteData.Blocks[i].SignalBeacon)
					{
						int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
						Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
						Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new TransponderEvent(Plugin.CurrentRoute, 0, 2, 0, Plugin.CurrentRoute.Sections.Length - 1, true);
					}
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
				if (i < currentRouteData.Blocks.Count - 1 && currentRouteData.Blocks[i + 1].Correction != null)
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
					Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new RouteManager2.Events.SoundEvent(Plugin.CurrentHost, 0, AvailableSounds[currentRouteData.Blocks[i].Sounds[j].SoundIndex], true, false, currentRouteData.Blocks[i].Sounds[j].Looped, false, currentRouteData.Blocks[i].Sounds[j].Position);
				}
			}
			Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements, CurrentTrackLength);
			for (int i = 0; i < Plugin.CurrentRoute.Stations.Length; i++)
			{
				//Add any missing station stop events
				if (Plugin.CurrentRoute.Stations[i].Stops == null || Plugin.CurrentRoute.Stations[i].Stops.Length == 0)
				{
					Plugin.CurrentRoute.Stations[i].Stops = new RouteManager2.Stations.StationStop[] { };
				}
			}
			Plugin.CurrentRoute.Tracks[0].Elements[CurrentTrackLength -1].Events = new GeneralEvent[] { new TrackEndEvent(Plugin.CurrentHost, 500) }; //Remember that Mechanik often has very long objects

			
			string routeHash = Path.GetChecksum(RouteFile);
			GetProperties(routeHash);
		}

		private static int CreateHorizontalObject(List<Vector3> Points, int firstPoint, double scaleFactor, Vector2 textureScale, int textureIndex, bool transparent)
		{
			if (!AvailableTextures.ContainsKey(textureIndex))
			{
				return -1;
			}
			MechanikTexture t = AvailableTextures[textureIndex];
			MechanikObject o = new MechanikObject(MechnikObjectType.Horizontal, Vector3.Zero, scaleFactor, textureIndex);

			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);
			Builder.Faces.Add(new MeshFace {Vertices = new MeshFaceVertex[Points.Count], Flags = FaceFlags.Face2Mask});
			for (int i = 0; i < Points.Count; i++)
			{
				Builder.Faces[Builder.Faces.Count -1].Vertices[i].Index = (ushort) i;
				Builder.Faces[Builder.Faces.Count -1].Vertices[i].Normal = Vector3.Zero;
				Builder.Vertices.Add(new Vertex(Points[i]));
				Builder.Vertices[Builder.Vertices.Count -1].TextureCoordinates = FindTextureCoordinate(i, firstPoint, Points, scaleFactor, textureScale, t);
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

			if (!AvailableTextures.ContainsKey(textureIndex))
			{
				return -1;
			}
			MechanikObject o = new MechanikObject(MechnikObjectType.Perpendicular, topLeft, scaleFactor, textureIndex);
			o.Object = CreateStaticObject(topLeft, scaleFactor, textureIndex, transparent);
			AvailableObjects.Add(o);
			return AvailableObjects.Count - 1;
		}


		internal static StaticObject CreateStaticObject(Vector3 topLeft, double scaleFactor, int textureIndex, bool transparent, bool invertY = false)
		{
			if (!AvailableTextures.ContainsKey(textureIndex))
			{
				return null;
			}
			MechanikTexture t = AvailableTextures[textureIndex];
			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);
			//Convert texture size to px/m and then multiply by scaleFactor to get the final vertex offset
			double scaledWidth = t.Width * 5 * scaleFactor;
			double scaledHeight = t.Height * 5 * scaleFactor;
			Builder.Vertices = new List<VertexTemplate>();
			Builder.Vertices.Add(new Vertex(new Vector3(topLeft)));
			Builder.Vertices.Add(new Vertex(new Vector3(topLeft.X + scaledWidth, topLeft.Y, topLeft.Z))); //upper right
			Builder.Vertices.Add(new Vertex(new Vector3(topLeft.X + scaledWidth, topLeft.Y - scaledHeight, topLeft.Z))); //bottom right
			Builder.Vertices.Add(new Vertex(new Vector3(topLeft.X, topLeft.Y - scaledHeight, topLeft.Z))); //bottom left
			//Possibly change to Face, check this though (Remember that Mechanik was restricted to the cab, wheras we are not)
			Builder.Faces = new List<MeshFace>();
			Builder.Faces.Add(new MeshFace { Vertices = new MeshFaceVertex[4], Flags = FaceFlags.Face2Mask });
			Builder.Faces[0].Vertices[0].Index = 0;
			Builder.Faces[0].Vertices[1].Index = 1;
			Builder.Faces[0].Vertices[2].Index = 2;
			Builder.Faces[0].Vertices[3].Index = 3;
			Builder.Vertices[0].TextureCoordinates = new Vector2(0,0);
			Builder.Vertices[1].TextureCoordinates = new Vector2(1,0);
			Builder.Vertices[2].TextureCoordinates = new Vector2(1,1);
			Builder.Vertices[3].TextureCoordinates = new Vector2(0,1);
			

			Builder.Materials = new [] { new Material(t.Path) };
			if (transparent)
			{
				Builder.Materials[0].TransparentColor = Color24.Black;
				Builder.Materials[0].Flags = MaterialFlags.TransparentColor;
			}
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			Builder.Apply(ref obj);
			return obj;
		}

		private static void LoadTextureList(string tDat)
		{
			string[] textureLines = File.ReadAllLines(tDat);
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
					if (File.Exists(path))
					{
						MechanikTexture t = new MechanikTexture(path, s);
						AvailableTextures.Add(k, t);
					}

				}

			}
		}

		private static void LoadSoundList(string sDat)
		{
			string[] soundLines = File.ReadAllLines(sDat);
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
					if (File.Exists(path))
					{
						Plugin.CurrentHost.RegisterSound(path, out var handle);
						AvailableSounds.Add(k, handle);
					}

				}

			}
		}

		/// <summary>Normalises 2 components of a 3D vector</summary>
		/// <param name="x">The first component</param>
		/// <param name="y">The second component</param>
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

		/// <summary>Parses a distance string formatted in Pixels per meter</summary>
		/// <param name="val">The distance string</param>
		/// <param name="position">The parsed distance</param>
		/// <returns>Whether parsing succeded</returns>
		private static bool TryParseDistance(string val, out double position)
		{
			if (double.TryParse(val, out position))
			{
				position /= 200; //200px per meter scale
				return true;
			}
			return false;
		}

		/// <summary>Parses an integer into a boolean</summary>
		/// <param name="val">The integer to parse</param>
		/// <param name="boolean">The parsed boolean</param>
		/// <returns>Whether parsing succeded</returns>
		private static bool TryParseBool(string val, out bool boolean)
		{
			int.TryParse(val, out var value);
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
		/// <param name="textureScale">A 2D vector representing the texture scaling to be applied</param>
		/// <param name="texture">The texture to use</param>
		/// <returns>The texture co-ordinate vector</returns>
		private static Vector2 FindTextureCoordinate(int pointIndex, int firstPoint, List<Vector3> pointList, double scale, Vector2 textureScale, MechanikTexture texture)
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
				coordinatesX = number / scale / textureScale.Y / texture.Width;
			}

			Vector3 t2 = Vector3.Project(pointList[pointIndex] - pointList[firstPoint], V);
			
			double coordinatesY = 0.0;
			if (t2.Norm() != 0.0)
			{
				Vector3 newVect = Vector3.Normalize(t2) * Vector3.Normalize(V);
				double number = t2.Norm() * newVect.X + t2.Norm() * newVect.Y + t2.Norm() * newVect.Z;
				coordinatesY = number / scale / textureScale.X / texture.Height;
			}
			//FIXME: Why does Y need negating??
			return new Vector2(coordinatesX, -coordinatesY);
		}

	}
}
