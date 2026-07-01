//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using System.Configuration;
using System.Xml;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using RouteManager2.Events;
using RouteManager2.Stations;

namespace LokSimRouteParser
{
	internal class Track
	{
		internal readonly string Name;

		internal List<TrackElement> Elements;

		internal List<ObjectGroup> Objects;

		internal List<L3DRail> Rails;

		internal List<LandschaftsObjekt> Landschaftsobjekt;

		internal TrackType Type;

		internal string ParallelTrack;

		internal string WeicheTrack;

		internal double WeichePos;

		internal double ParallelOffset;

		internal double ParallelStart;

		internal double ParallelEnd;

		internal Vector3 StartPoint;

		internal double StartPos;

		internal Strecke Strecke;

		internal Guid Guid;

		internal Track(XmlNode node, Strecke strecke)
		{
			if (!node.HasChildNodes)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Loksim3D: Empty Gleis encountered in Strecke ");
				return;
			}
			
			Strecke = strecke;
			// track names can be re-used between different strecke so generate a GUID for unique identification
			Guid = Guid.NewGuid();

			Elements = new List<TrackElement>();
			Objects = new List<ObjectGroup>();
			Landschaftsobjekt = new List<LandschaftsObjekt>();
			StartPoint = new Vector3();

			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				switch (node.ChildNodes[i].Name)
				{
					case "Props":
						if (node.ChildNodes[i].Attributes == null)
						{
							break;
						}

						for (int j = 0; j < node.ChildNodes[i].Attributes.Count; j++)
						{
							if (!Enum.TryParse(node.ChildNodes[i].Attributes[j].Name, true, out PropsAttribute props))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised Props attribute " + node.ChildNodes[i].Attributes[j].Name + " in StartPunkt");
								continue;
							}
							switch (props)
							{
								case PropsAttribute.Name:
									Name = node.ChildNodes[i].Attributes[j].InnerText;
									break;
							}
						}
						break;
					case "TOPOLOGIE":
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Erstellung":
									// Creation parameters
									for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
									{
										switch (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name)
										{
											case "Props":
												if (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes == null)
												{
													break;
												}
												for (int l = 0; l < node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes.Count; l++)
												{
													switch (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].Name)
													{
														case "EndPunkt":
															// end point (M)
															break;
														case "ParallelGleis":
															// track to parallel
															ParallelTrack = node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText;
															if (!string.IsNullOrEmpty(ParallelTrack) && !Strecke.Tracks.ContainsKey(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText))
															{
																Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: The track to parallel with key " + ParallelTrack + " was not found.");
															}
															
															break;
														case "ParallelStart":
															NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, out ParallelStart);
															break;
														case "ParallelEnd":
															NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, out ParallelEnd);
															break;
														case "ParallelAbstand":
															// if TYP is set to parallel then distance from ParallelGleis
															NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, out ParallelOffset);
															break;
														case "StartPos":
															// starting position (M)
															NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, out StartPos);
															break;
														case "StartPunkt":
															// starting point on world tile
															Vector3.TryParse(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText.Split(';'), out StartPoint);
															break;
														case "Typ":
															if (!Enum.TryParse(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, true, out Type))
															{
																Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Loksim3D: Unknown track type " + node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText);
															}
															break;
														case "WeicheEnd":
															// switch track ON END (?)
															break;
														case "WeicheGleis":
															// switch to track key
															WeicheTrack = node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText;
															if (!string.IsNullOrEmpty(WeicheTrack) && !Strecke.Tracks.ContainsKey(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText))
															{
																Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: The switch track with key " + WeicheTrack + " was not found.");
															}
															break;
														case "WeicheGleisPos":
															// switch to track defined by WeicheGleis at position (M)
															NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes[l].InnerText, out WeichePos);
															break;
														case "WeicheGleisRichtung":
															// switch direction (? display on HUD, switch generation ?)
															break;
														case "WeicheName":
															// switch name
															break;
														case "WeicheStart":
															break;
														case "WeichenWinkel":
															break;
														case "Winkel":
															break;

													}
												}
												break;
										}
									}
									break;
								case "KURVE":
								case "STEIGUNG":
									for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
									{
										switch (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name)
										{
											case "Props":
												if (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes == null)
												{
													break;
												}

												if (node.ChildNodes[i].ChildNodes[j].Name == "STEIGUNG")
												{
													Elements.Add(new Pitch(node.ChildNodes[i].ChildNodes[j].ChildNodes[k]));
												}
												else
												{
													Elements.Add(new Curve(node.ChildNodes[i].ChildNodes[j].ChildNodes[k]));
												}
												break;
										}
									}
									break;
								case "Weiche":
									for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].ChildNodes.Count; k++)
									{
										switch (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Name)
										{
											case "Props":
												if (node.ChildNodes[i].ChildNodes[j].ChildNodes[k].Attributes == null)
												{
													break;
												}

												if (node.ChildNodes[i].ChildNodes[j].Name == "Weiche")
												{
													Elements.Add(new Weiche(node.ChildNodes[i].ChildNodes[j].ChildNodes[k]));
												}
												break;
										}
									}
									break;
								case "Ausweiche":
									// avoidance?
									break;
							}
						}
						break;
					case "Objecte":
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Object":
									// objects
									Objects.Add(new ObjectGroup(node.ChildNodes[i].ChildNodes[j]));
									break;
							}
						}
						break;
					case "Landschaft":
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Landschaftsobjekt":
									// ground objects
									Landschaftsobjekt.Add(new LandschaftsObjekt(node.ChildNodes[i].ChildNodes[j]));
									break;
							}
						}
						break;
				}
			}
		}

		internal void Create(Vector3 wpos, int newTrackKey, StaticObject defaultRail)
		{
			wpos += StartPoint; // ??
			if (ParallelOffset != 0)
			{
				wpos.X += ParallelOffset;
			}
			List<OpenBveApi.Routes.TrackElement> newElements = new List<OpenBveApi.Routes.TrackElement>();


			/*
			 * Track with Typ == Weiche
			 *
			 * Begins with a switch.
			 * Select Track defined by WeicheGleis
			 * WeicheGleisPos is the *relative* position on WeicheGleis of Element 0
			 */

			Vector2 direction = Vector2.Down;
			TrackFollower tf = new TrackFollower(Plugin.CurrentHost);
			Vector3 directionVector = Vector3.Left;
			switch (Type)
			{
				case TrackType.Parallel:
					tf.TrackIndex = Plugin.TrackKeys[Strecke.Tracks[ParallelTrack].Guid];
					tf.UpdateAbsolute(ParallelStart, true, false);
					directionVector.Rotate(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
					directionVector *= ParallelOffset;
					wpos = tf.WorldPosition + directionVector;
					direction = new Vector2(tf.WorldDirection.X, tf.WorldDirection.Y);
					break;
				case TrackType.Weiche:
					tf.TrackIndex = Plugin.TrackKeys[Strecke.Tracks[WeicheTrack].Guid];
					tf.UpdateAbsolute(WeichePos, true, false);
					directionVector.Rotate(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
					directionVector *= ParallelOffset;
					wpos = tf.WorldPosition + directionVector;
					direction = new Vector2(tf.WorldDirection.X, tf.WorldDirection.Y);
					break;
			}

			OpenBveApi.Routes.TrackElement te = new OpenBveApi.Routes.TrackElement(StartPos);
			te.WorldPosition = wpos;
			newElements.Add(te);
			

			for (int i = 0; i < Elements.Count; i++)
			{
				if (Type == TrackType.Parallel)
				{
					double sPos = StartPos;
					while (tf.TrackPosition < Elements[i].StartingDistance)
					{
						tf.UpdateRelative(1, true, false);
						Vector3 elementDirection = Vector3.Left;
						elementDirection.Rotate(tf.WorldDirection, tf.WorldUp, tf.WorldSide);
						elementDirection *= ParallelOffset;
						OpenBveApi.Routes.TrackElement ae = new OpenBveApi.Routes.TrackElement(sPos);
						ae.WorldPosition = tf.WorldPosition + elementDirection;
						newElements.Add(ae);
						StartPos++;
					}
				}
				Elements[i].Create(ref newElements, ref wpos, ref direction);
			}

			if (newTrackKey != 0)
			{
				Plugin.CurrentRoute.Tracks.Add(newTrackKey, new OpenBveApi.Routes.Track(Name));
			}
			
			Plugin.CurrentRoute.Tracks[newTrackKey].Elements = new OpenBveApi.Routes.TrackElement[newElements.Count];
			newElements.CopyTo(Plugin.CurrentRoute.Tracks[newTrackKey].Elements, 0);


			if (newTrackKey == 0)
			{
				/*
				 * TEMP HACKS TO ALLOW VIEWING
				 * REMOVE ONCE STATIONS ARE IMPLEMENTED
				 */
				int eL = Plugin.CurrentRoute.Tracks[0].Elements.Length - 1;
				Plugin.CurrentRoute.Stations = new[]
				{
					new RouteStation
					{
						Name = "Start",
						Stops = new[]
						{
							new StationStop
							{
								TrackPosition = Plugin.CurrentRoute.Tracks[newTrackKey].Elements[0].StartingTrackPosition
							}
						}
					},
					new RouteStation
					{
						Name = "End",
						Stops = new[]
						{
							new StationStop
							{
								TrackPosition = Plugin.CurrentRoute.Tracks[newTrackKey].Elements[eL].StartingTrackPosition
							}
						}
					}
				};
				Plugin.CurrentRoute.Tracks[newTrackKey].Elements[0].Events = new List<GeneralEvent>()
				{
					new StationStartEvent(Plugin.CurrentRoute, 0, 0)
				};
				Plugin.CurrentRoute.Tracks[newTrackKey].Elements[1].Events = new List<GeneralEvent>()
				{
					new StationEndEvent(Plugin.CurrentHost, Plugin.CurrentRoute, 0, 0)
				};
				Plugin.CurrentRoute.Tracks[newTrackKey].Elements[eL].Events = new List<GeneralEvent>()
				{
					new StationStartEvent(Plugin.CurrentRoute, 0, 1)
				};
			}

			TrackFollower t = new TrackFollower(Plugin.CurrentHost);
			t.TrackIndex = newTrackKey;
			for (int i = 0; i < Objects.Count; i++)
			{
				Objects[i].Create(t);
			}

			for (int i = 0; i < Landschaftsobjekt.Count; i++)
			{
				Landschaftsobjekt[i].Create(t);
			}

			double start = Plugin.CurrentHost.Tracks[0].Elements[0].StartingTrackPosition;
			double end = Plugin.CurrentHost.Tracks[0].Elements[Plugin.CurrentHost.Tracks[0].Elements.Length -1].StartingTrackPosition;

			t.UpdateAbsolute(start, true, false);
			
			string groundFile = Path.CombineFile(Plugin.FileSystem.GetDataFolder(new[] { "Compatibility" }),"Loksim\\Grass.csv");

			Plugin.CurrentHost.LoadObject(groundFile, System.Text.Encoding.UTF8, out UnifiedObject ground);
			
			while (t.TrackPosition < end)
			{
				t.UpdateRelative(10, true, false);
				Plugin.CurrentHost.CreateStaticObject((StaticObject)ground, t.WorldPosition, new ObjectCreationParameters(t.TrackPosition, t.TrackPosition + 25), Transformation.NullTransformation);
				if (defaultRail != null)
				{
					Plugin.CurrentHost.CreateStaticObject(defaultRail, t.WorldPosition, new ObjectCreationParameters(t.TrackPosition, t.TrackPosition + 25), new Transformation(t.WorldDirection, t.WorldUp, t.WorldSide));
				}
				
			}
		}
	}
}
