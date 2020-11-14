using System;
using System.Collections.Generic;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using System.Text.RegularExpressions;
using OpenBveApi;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using OpenBveApi.World;
using RouteManager2.Events;
using RouteManager2.Stations;

namespace MechanikRouteParser
{
	internal class Parser
	{
		private static RouteData currentRouteData;
		private static List<MechanikObject> AvailableObjects = new List<MechanikObject>();
		private static List<MechanikTexture> AvailableTextures = new List<MechanikTexture>();
		private static Dictionary<int, SoundHandle> AvailableSounds = new Dictionary<int, SoundHandle>();
		private static string RouteFolder;

		internal void ParseRoute(string routeFile, bool PreviewOnly)
		{
			if (!System.IO.File.Exists(routeFile))
			{
				return;
			}

			Plugin.CurrentRoute.AccurateObjectDisposal = ObjectDisposalMode.Mechanik;
			AvailableObjects = new List<MechanikObject>();
			AvailableTextures = new List<MechanikTexture>();
			AvailableSounds = new Dictionary<int, SoundHandle>();
			currentRouteData = new RouteData();
			//Load texture list
			RouteFolder = System.IO.Path.GetDirectoryName(routeFile);
			string tDat = OpenBveApi.Path.CombineFile(RouteFolder, "tekstury.dat");
			if (!System.IO.File.Exists(tDat))
			{
				/*
				 * TODO: Separate into a function
				 * Various Mechanik routes use custom hardcoded DAT paths
				 * Thus, we will need to keep a search list for both the trasa and tekstury files
				 */
				tDat = OpenBveApi.Path.CombineFile(RouteFolder, "s80_text.dat"); //S80 U-Bahn
				if (!System.IO.File.Exists(tDat))
				{
					return;
				}
			}
			LoadTextureList(tDat);
			string sDat = OpenBveApi.Path.CombineFile(RouteFolder, "dzwieki.dat");
			LoadSoundList(sDat);
			string[] routeLines = System.IO.File.ReadAllLines(routeFile);
			double yOffset = 0.0;
			for (int i = 0; i < routeLines.Length; i++)
			{
				int j = routeLines[i].IndexOf(@"//", StringComparison.Ordinal);
				if (j != -1)
				{
					//Split out comments
					routeLines[i] = routeLines[i].Substring(j, routeLines[i].Length - j);
				}
				if (String.IsNullOrWhiteSpace(routeLines[i]))
				{
					continue;
				}
				routeLines[i] = Regex.Replace(routeLines[i], @"\s+", " ");
				string[] Arguments = routeLines[i].Trim().Split(null);
				double trackPosition, scaleFactor;
				int Idx, blockIndex, textureIndex;
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
						if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out topLeft.X))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out topLeft.Y))
						{
							//Add message
							continue;
						}
						topLeft.Y = -topLeft.Y;
						if (Arguments.Length < 5 || !TryParseDistance(Arguments[4], out topLeft.Z))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 6 || !double.TryParse(Arguments[5], out scaleFactor))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 7 || !int.TryParse(Arguments[6], out textureIndex))
						{
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
						if (!TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (!int.TryParse(Arguments[2], out numPoints))
						{
							//Add message
							continue;
						}
						if (numPoints < 3 || numPoints > 5)
						{
							//Add message
							continue;
						}
						int v = 0;
						List<Vector3> points = new List<Vector3>();
						Vector3 currentPoint = new Vector3();
						double offset = 0;
						for (int p = 3; p < 24; p++)
						{
							switch (v)
							{
								case 0:
									if (!TryParseDistance(Arguments[p], out currentPoint.X))
									{
										//Add message
									}
									break;
								case 1:
									if (!TryParseDistance(Arguments[p], out currentPoint.Y))
									{
										//Add message
									}
									currentPoint.Y = -currentPoint.Y;
									currentPoint.Y += yOffset;
									break;
								case 2:
									if (!TryParseDistance(Arguments[p], out currentPoint.Z))
									{
										//Add message
									}
									if (points.Count > 0 && points.Count < numPoints)
									{
										if (points.Count == 1)
										{
											offset = Math.Min(currentPoint.Z, points[points.Count - 1].Z);
										}
										else
										{
											offset = Math.Min(currentPoint.Z, offset);
										}
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
							//Add message
							continue;
						}
						if (firstPoint != 0)
						{
							firstPoint -= 1;
						}
						double sx;
						double sy;
						if (!double.TryParse(Arguments[26], out sx))
						{
							//Add message
							continue;
						}
						if (!double.TryParse(Arguments[25], out sy))
						{
							//Add message
							continue;
						}
						if (!double.TryParse(Arguments[27], out scaleFactor))
						{
							//Add message
							continue;
						}
						if (!int.TryParse(Arguments[28], out textureIndex))
						{
							//Add message
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
								Idx = CreateHorizontalObject(tDat, sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, true, false);
								break;
							case "#t_p":
								//HORIZONTAL, no transparency
								Idx = CreateHorizontalObject(tDat, sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, false, true);
								break;
							case "#t":
								Idx = CreateHorizontalObject(tDat, sortedPoints, firstPoint, scaleFactor, sx, sy, textureIndex, false, false);
								break;
							default:
								//never hit
								Idx = -1;
								break;
						}
						
						blockIndex = currentRouteData.FindBlock(trackPosition);
						
						currentRouteData.Blocks[blockIndex].Objects.Add(new RouteObject(Idx, new Vector3(0, 0, 0)));
						break;
					case "'o":
						//Rotation marker for the player track, roughly equivilant to .turn
						double radians;
						Vector2 turnPoint = new Vector2();
						if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out turnPoint.X))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out turnPoint.Y))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 5 || !double.TryParse(Arguments[4], out radians))
						{
							//Add message
							continue;
						}
						double dist = trackPosition + Math.Sqrt(turnPoint.X*turnPoint.X+turnPoint.Y*turnPoint.Y);
						blockIndex = currentRouteData.FindBlock(dist);
						currentRouteData.Blocks[blockIndex].Turn = radians / 1000000.0;
						break;
					case "'k":
						//Rotates the world to zero after a curve
						//Going to give me a headache, but uncommon
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
						if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out soundPosition.X))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out soundPosition.Z))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 5 || !int.TryParse(Arguments[4], out soundNumber))
						{
							//Add message
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
							//Add message
							continue;
						}
						if (Arguments.Length < 8 || !TryParseBool(Arguments[7], out speedDependant))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 9 || !int.TryParse(Arguments[8], out volume))
						{
							//Add message
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
						Vector2 limitPos = new Vector2();
						if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out limitPos.X))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out limitPos.Y))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 5 || !double.TryParse(Arguments[4], out kph))
						{
							//Add message
							continue;
						}

						blockIndex = currentRouteData.FindBlock(trackPosition);
						currentRouteData.Blocks[blockIndex].SpeedLimit = kph;
						break;
					case "'z_z":
						//Station stop marker
						bool terminal;
						Vector2 stopPos = new Vector2();
						if (Arguments.Length < 2 || !TryParseDistance(Arguments[1], out trackPosition))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 3 || !TryParseDistance(Arguments[2], out stopPos.X))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 4 || !TryParseDistance(Arguments[3], out stopPos.Y))
						{
							//Add message
							continue;
						}
						if (Arguments.Length < 5 || !TryParseBool(Arguments[4], out terminal))
						{
							//Add message
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
				string f = OpenBveApi.Path.CombineFile(RouteFolder, "obloczki.bmp");
				if (System.IO.File.Exists(f))
				{
					Plugin.CurrentHost.RegisterTexture(f, new TextureParameters(null, null), out bt);
				}

				Plugin.CurrentRoute.CurrentBackground = new StaticBackground(bt, 2, false);
				Plugin.CurrentRoute.TargetBackground = Plugin.CurrentRoute.CurrentBackground;
			}

			Vector3 Position = new Vector3(0.0, 0.0, 0.0);
			Vector2 Direction = new Vector2(0.0, 1.0);
			Plugin.CurrentRoute.Tracks[0].Elements = new TrackElement[256];
			int CurrentTrackLength = 0;
			double StartingDistance = 0;
			for (int i = 0; i < currentRouteData.Blocks.Count; i++)
			{
				/*
				 * First loop to create the objects
				 */
				// normalize
				Normalize(ref Direction.X, ref Direction.Y);
				
				for (int j = 0; j < currentRouteData.Blocks[i].Objects.Count; j++)
				{
					AvailableObjects[currentRouteData.Blocks[i].Objects[j].objectIndex].Object.CreateObject(Position, StartingDistance, StartingDistance + 25, 100);
				}

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

				// finalize block
				Position.X += Direction.X * blockLength;
				Position.Z += Direction.Y * blockLength;

			}
			Position = new Vector3(0,0,0);
			Direction = new Vector2(0, 1);
			double currentSpeedLimit = Double.PositiveInfinity;
			for (int i = 0; i < currentRouteData.Blocks.Count; i++)
			{
				/*
				 * Second loop to create the track elements and transforms
				 */
				// normalize
				Normalize(ref Direction.X, ref Direction.Y);
				// track
				TrackElement WorldTrackElement = new TrackElement(currentRouteData.Blocks[i].StartingTrackPosition);

				int n = CurrentTrackLength;
				if (n >= Plugin.CurrentRoute.Tracks[0].Elements.Length)
				{
					Array.Resize<TrackElement>(ref Plugin.CurrentRoute.Tracks[0].Elements, Plugin.CurrentRoute.Tracks[0].Elements.Length << 1);
				}
				Plugin.CurrentRoute.Tracks[0].Elements[n] = WorldTrackElement;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldPosition = Position;
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection = Vector3.GetVector3(Direction, 0);
				Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X);
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

				// finalize block
				Position.X += Direction.X * blockLength;
				Position.Z += Direction.Y * blockLength;
				if (currentRouteData.Blocks[i].Turn != 0.0)
				{
					double ag = -Math.Atan(currentRouteData.Blocks[i].Turn);
					double cosag = Math.Cos(ag);
					double sinag = Math.Sin(ag);
					Direction.Rotate(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide.RotatePlane(cosag, sinag);
					Plugin.CurrentRoute.Tracks[0].Elements[n].WorldUp = Vector3.Cross(Plugin.CurrentRoute.Tracks[0].Elements[n].WorldDirection, Plugin.CurrentRoute.Tracks[0].Elements[n].WorldSide);
				}

				if (currentRouteData.Blocks[i].SpeedLimit != -1)
				{
					int e = Plugin.CurrentRoute.Tracks[0].Elements[n].Events.Length; 
					Array.Resize(ref Plugin.CurrentRoute.Tracks[0].Elements[n].Events, e + 1);
					Plugin.CurrentRoute.Tracks[0].Elements[n].Events[e] = new LimitChangeEvent(0, currentSpeedLimit, currentRouteData.Blocks[i].SpeedLimit);
					currentSpeedLimit = currentRouteData.Blocks[i].SpeedLimit;
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
			Array.Resize<TrackElement>(ref Plugin.CurrentRoute.Tracks[0].Elements, CurrentTrackLength);
			Plugin.CurrentRoute.Tracks[0].Elements[CurrentTrackLength -1].Events = new GeneralEvent[] { new TrackEndEvent(Plugin.CurrentHost, 25) };
		}

		private static int CreateHorizontalObject(string TdatPath, List<Vector3> Points, int firstPoint, double scaleFactor, double sx, double sy, int textureIndex, bool transparent, bool horizontal)
		{
			MechanikTexture t = new MechanikTexture();
			for (int i = 0; i < AvailableTextures.Count; i++)
			{
				if (AvailableTextures[i].Index == textureIndex)
				{
					t = AvailableTextures[i];
				}
			}
			MechanikObject o = new MechanikObject();
			o.TopLeft = new Vector3();
			o.TextureIndex = textureIndex;

			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);

			Vector3 min = new Vector3(Points[0].X, Points[0].Y, Points[0].Z);
			Vector3 max = new Vector3(Points[0].X, Points[0].Y, Points[0].Z);
			int v = Builder.Vertices.Length;
			Array.Resize(ref Builder.Vertices, v + Points.Count);
			
			for (int i = 0; i < Points.Count; i++)
			{
				Builder.Vertices[v + i] = new Vertex(Points[i]);
				if (Points[i].X > min.X)
				{
					min.X = Points[i].X;
					
				}
				if (Points[i].X < max.X)
				{
					max.X = Points[i].X;
				}
				if (Points[i].Y > min.Y)
				{
					min.Y = Points[i].Y;
				}
				if (Points[i].Y < max.Y)
				{
					max.Y = Points[i].Y;
				}
				if (Points[i].Z > min.Z)
				{
					min.Z = Points[i].Z;
				}
				if (Points[i].Z < max.Z)
				{
					max.Z = Points[i].Z;
				}
			}
			//size of face
			//BUG: Doesn't take into account diagonal faces or anything of that nature
			Vector3 faceSize = max - min;
			int fl = Builder.Faces.Length;
			Array.Resize(ref Builder.Faces, Builder.Faces.Length + 1);
			Builder.Faces[fl] = new MeshFace { Vertices = new MeshFaceVertex[Points.Count] , Flags = (byte)MeshFace.Face2Mask };
			for (int i = 0; i < Points.Count; i++)
			{
				Builder.Faces[fl].Vertices[i].Index = (ushort) i;
				Builder.Faces[fl].Vertices[i].Normal = new Vector3();
			}
			double tc1 = sx, tc2 = sy;
			if (horizontal)
			{
				tc1 = faceSize.X / (t.Width * sy * scaleFactor);
				tc2 = faceSize.Z / (t.Height * sx * scaleFactor);
			}
			else
			{
				if (faceSize.X == 0)
				{
					tc1 = faceSize.Z / (t.Width * sx * scaleFactor);
					tc2 = faceSize.Y / (t.Height * sy * scaleFactor);
				}
				else if (faceSize.Y == 0)
				{
					//BUG: Not sure why this needs negating at the minute.....
					tc1 = -(faceSize.X / (t.Width * sx * scaleFactor));
					tc2 = faceSize.Z / (t.Height * sy * scaleFactor);
				}
				else if (faceSize.Z == 0)
				{
					tc1 = faceSize.X / (t.Width * sx * scaleFactor);
					tc2 = faceSize.Y / (t.Height * sy * scaleFactor);
				}
			}

			/*
			 * BUG: Not sure this is right, but it makes a bunch more stuff work OK
			 */
			double tc3 = 0, tc4 = 0;
			if (Math.Abs(tc1) != tc1)
			{
				tc3 = tc1;
				tc1 = 0;
			}
			if (Math.Abs(tc2) != tc2)
			{
				tc4 = tc2;
				tc2 = 0;
				firstPoint = 0;
			}
			for (int i = 0; i < Points.Count; i++)
			{
				if (firstPoint >= Points.Count)
				{
					firstPoint = 0;
				}
				switch (i)
				{
					case 0:
						Builder.Vertices[firstPoint].TextureCoordinates = new Vector2(tc3, tc2);
						break;
					case 1:
						Builder.Vertices[firstPoint].TextureCoordinates = new Vector2(tc3, tc4);
						break;
					case 2:
						Builder.Vertices[firstPoint].TextureCoordinates = new Vector2(tc1, tc4);
						break;
					case 3:
						Builder.Vertices[firstPoint].TextureCoordinates = new Vector2(tc1, tc2);
						break;
				}
				firstPoint++;
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
			MechanikTexture t = new MechanikTexture();
			for (int i = 0; i < AvailableTextures.Count; i++)
			{
				if (AvailableTextures[i].Index == textureIndex)
				{
					t = AvailableTextures[i];
					break;
				}
			}
			MechanikObject o = new MechanikObject();
			o.TopLeft = new Vector3(0,0,0);
			o.TextureIndex = textureIndex;	
			//BUG: Not entirely sure why multiplying W & H by 5 makes this work....
			MeshBuilder Builder = new MeshBuilder(Plugin.CurrentHost);
			Builder.Vertices = new VertexTemplate[4];
			Builder.Vertices[0] = new Vertex(new Vector3(topLeft));
			//FIXME: WTF??
			//s.Add("Vertex " + topLeft + ", -1,1,0");
			Builder.Vertices[1] = new Vertex(new Vector3(topLeft.X + (t.Width * 5), topLeft.Y, topLeft.Z)); //upper right
			Builder.Vertices[2] = new Vertex(new Vector3((topLeft.X + (t.Width * 5)), (topLeft.Y - (t.Height * 5)), topLeft.Z)); //bottom right
			Builder.Vertices[3] = new Vertex(new Vector3(topLeft.X, (topLeft.Y - (t.Height * 5)), topLeft.Z)); //bottom left
			//Possibly change to Face, check this though (Remember that Mechanik was restricted to the cab, wheras we are not)
			Builder.Faces = new MeshFace[1];
			Builder.Faces[0] = new MeshFace { Vertices = new MeshFaceVertex[4], Flags = (byte)MeshFace.Face2Mask };
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
						string str = textureLines[i].Substring(0, l);
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
						MechanikTexture t = new MechanikTexture(path, s, k);
						AvailableTextures.Add(t);
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
						string str = soundLines[i].Substring(0, l);
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
	}
}
