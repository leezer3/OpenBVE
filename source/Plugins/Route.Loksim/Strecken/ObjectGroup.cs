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

using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LokSimRouteParser
{
	internal class ObjectGroup
	{
		/// <summary>The position of this on the rail</summary>
		internal double TrackPosition;
		/// <summary>The offset from the position</summary>
		internal Vector3 OffsetPosition;

		internal string Name;

		internal List<Object> Objects;
		internal ObjectGroup(XmlNode node)
		{
			// An ObjectGroup [Object in the XML] is somewhere roughly analogous to a world tile
			Objects = new List<Object>();
			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				switch (node.ChildNodes[i].Name)
				{
					case "Props":
						if (node.ChildNodes[i].Attributes != null)
						{
							for (int j = 0; j < node.ChildNodes[i].Attributes.Count; j++)
							{
								switch (node.ChildNodes[i].Attributes[j].Name)
								{
									case "Position":
										TrackPosition = double.Parse(node.ChildNodes[i].Attributes[j].InnerText);
										break;
									case "Offset":
										// We get an initial position to looking ahead on the transformed track
										// Then add an offset to get our final WPos
										Vector3.TryParse(node.ChildNodes[i].Attributes[j].InnerText, ';', out OffsetPosition);
										break;
									case "Qualitaet":
										/*
										 * Used by the renderer to determine visibility
										 * Our renderer can probably ignore this
										 * -----------------------------------------------
										 * 0 - Absolutely necessary (e.g. signals, tracks)
										 * 1 - Very important (e.g. railway embankments)
										 * 2 - Important (e.g. tunnels, bridges)
										 * 3 - Normal object (e.g houses)
										 * 4 - Unimportant object (e.g. trees)
										 * 5 - Completely unimportant (e.g. overpass- presumably road traffic)
										 */
										break;
									case "RollMaterial":
										// Set to true if this ObjectGroup is another train
										break;
									case "Name":
										Name = node.ChildNodes[i].Attributes[j].InnerText;
										break;
									case "DynamicVisibility":
										break;
									case "FixedDynamicVisibility":
										break;
									case "StreckenAbhaengigkeit":
										/*
										 * Route Dependency:
										 * ------------------
										 * 0 - Display independent of the train (default)
										 * 1 - Displayed only if no train is on this track
										 * 2 - Displayed only if train is on this track
										 */
										break;
								}
							}
						}
						break;
					case "Eintrag":
						Objects.Add(new Object(node.ChildNodes[i]));
						break;
				}
			}
		}

		internal void Create(TrackFollower t)
		{
			
			for (int i = 0; i < Objects.Count; i++)
			{
				// reset the track follower position, as we're using it for repetitions
				t.UpdateAbsolute(TrackPosition, true, false);
				Objects[i].Create(t, OffsetPosition);
			}
		}
	}

	internal class Object
	{
		internal double Distance;
		internal int NumberOfObjects;
		internal bool FollowsTrackHeight;
		internal bool FollowsLandHeight;
		internal UnifiedObject UnifiedObject;
		internal Vector3 Offset;
		internal double Position;
		internal Vector3 Rotation;
		internal bool FarVisible;
		internal LightStates Lighting;
		internal Dictionary<string, string> Properties;
		internal LoksimRandom Random;
		

		internal Object(XmlNode node)
		{
			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				if (!Enum.TryParse(node.ChildNodes[i].Name, out LoksimNode parsedNode))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised node " + node.ChildNodes[i].Name + " in LokSim3D Eintrag");
				}
				switch (parsedNode)
				{
					case LoksimNode.Props:
						if (node.ChildNodes[i].Attributes == null)
						{
							break;
						}

						for (int j = 0; j < node.ChildNodes[i].Attributes.Count; j++)
						{
							if (!Enum.TryParse(node.ChildNodes[i].Attributes[j].Name, out PropsAttribute props))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised Props attribute " + node.ChildNodes[i].Attributes[j].Name + " in LokSim3D Eintrag");
								continue;
							}
							switch (props)
							{
								case PropsAttribute.Abstand:
									// when repetition is used, distance to next object
									Distance = double.Parse(node.ChildNodes[i].Attributes[j].InnerText);
									break;
								case PropsAttribute.Anzahl:
									// number of objects to place
									NumberOfObjects = int.Parse(node.ChildNodes[i].Attributes[j].InnerText);
									break;
								case PropsAttribute.Datei:
									// path to the .l3dgrp or .l3dobj
									if (Plugin.previewOnly)
									{
										break;
									}
									// Path relative to the LokSim3D content folder
									if (string.IsNullOrEmpty(node.ChildNodes[i].Attributes[j].InnerText))
									{
										break;
									}
									string obj = Path.CombineFile(Plugin.FileSystem.LoksimDataDirectory, node.ChildNodes[i].Attributes[j].InnerText.Trim('\\'));
									if (Plugin.ObjectCache.TryGetValue(obj, out UnifiedObject))
									{
										break;
									}
									
									if (System.IO.File.Exists(obj))
									{
										Plugin.CurrentHost.LoadObject(obj, Encoding.UTF8, out UnifiedObject);
										Plugin.ObjectCache.Add(obj, UnifiedObject);
									}
									else
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "LokSim3D: Object " + node.ChildNodes[i].Attributes[j].InnerText + " was not found.");
									}
									break;
								case PropsAttribute.GleisHoeheFolgen:
									// 
									if (node.ChildNodes[i].Attributes[j].InnerText == "TRUE")
									{
										FollowsTrackHeight = true;
									}
									break;
								case PropsAttribute.HoeheLand:
									if (node.ChildNodes[i].Attributes[j].InnerText == "TRUE")
									{
										FollowsLandHeight = true;
									}
									break;
								case PropsAttribute.Position:
									// Z position of the object
									Position = double.Parse(node.ChildNodes[i].Attributes[j].InnerText);
									break;
								case PropsAttribute.Verschiebung:
									// X,Y,Z position from position
									Vector3.TryParse(node.ChildNodes[i].Attributes[j].InnerText, ';', out Offset);
									break;
								case PropsAttribute.WeitSichtbar:
									// used by the Loksim renderer
									if (node.ChildNodes[i].Attributes[j].InnerText == "TRUE")
									{
										FarVisible = true;
									}
									break;
								case PropsAttribute.Winkel:
									// X,Y,Z rotation
									Vector3.TryParse(node.ChildNodes[i].Attributes[j].InnerText, ';', out Rotation);
									break;
							}
						}
						break;
					case LoksimNode.Eigenschaften:
						/*
						 * Characteristics:
						 * This contains a single Props node
						 *
						 * The name of the each attribute within this Props node is passed into the .l3dgrp and subsequent .l3dobj
						 * files, and the value is passed as a variable to be returned using for use in functions etc.
						 * via the GroupProperty attribute.
						 *
						 */
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Props":
									Properties = new Dictionary<string, string>();

									for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].Attributes.Count; k++)
									{
										/*
										 * DEFAULT PROPERTIES
										 * ------------------
										 * STRECKENHEKTOMETER => Route 100m distance
										 * STRECKENMETER => Route distance
										 * WEICHENSTELLUNG => Setting of the associated weiche
										 */
										Properties.Add(node.ChildNodes[i].ChildNodes[j].Attributes[k].Name, node.ChildNodes[i].ChildNodes[j].Attributes[k].Value);
									}
									break;
							}
						}
						break;
					case LoksimNode.Lightning:
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Props":
									Lighting = new LightStates(node.ChildNodes[i].ChildNodes[j]);
									break;
							}
						}
						break;
					case LoksimNode.Random:
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Props":
									Random = new LoksimRandom(node.ChildNodes[i].ChildNodes[j]);
									break;
							}
						}
						break;
				}
			}
		}

		internal void Create(TrackFollower t, Vector3 offsetPosition)
		{
			if (UnifiedObject != null)
			{
				Vector3 o = new Vector3(Offset);
				o.Z += Position;
				o += offsetPosition;
				
				if (NumberOfObjects == 1)
				{
					if (Random != null)
					{
						Vector3 v = Random.GetPosition();
						o += v;
					}
					o.Rotate(t.WorldDirection, t.WorldUp, t.WorldSide);
					Vector3 objectWpos = t.WorldPosition + o;
					Transformation tr = new Transformation(t.WorldDirection, t.WorldUp, t.WorldSide);
					UnifiedObject.CreateObject(objectWpos, tr, new Transformation(Rotation.X.ToRadians(), Rotation.Y.ToRadians(), Rotation.Z.ToRadians()), new ObjectCreationParameters(t.TrackPosition, t.TrackPosition, t.TrackPosition + Distance));
				}
				else
				{
					for (int i = 0; i < NumberOfObjects; i++)
					{
						
						Vector3 objectWpos;
						Vector3 repetitionPosition = Vector3.Zero;
						if (i != 0)
						{
							t.UpdateRelative(Distance, true, false);
						}

						Vector3 oC = new Vector3(o);
						oC.Rotate(t.WorldDirection, t.WorldUp, t.WorldSide);
						objectWpos = t.WorldPosition + oC;
						
						//Vector3 objectWpos = t.WorldPosition + o;
						//Vector3 repetitionPosition = new Vector3(0, 0, i * Distance);
						if (Random != null)
						{
							repetitionPosition += Random.GetPosition();
						}
						repetitionPosition.Rotate(t.WorldDirection, t.WorldUp, t.WorldSide);
						Transformation tr = new Transformation(t.WorldDirection, t.WorldUp, t.WorldSide);
						UnifiedObject.CreateObject(objectWpos + repetitionPosition, tr, new Transformation(Rotation.X.ToRadians(), Rotation.Y.ToRadians(), Rotation.Z.ToRadians()), new ObjectCreationParameters(t.TrackPosition + i * Distance, t.TrackPosition  + i * Distance, t.TrackPosition + i * Distance));
					}
					
				}
					
			}
		}
	}

	internal class LoksimRandom
	{
		internal readonly int Seed;

		internal readonly double XValue;

		internal readonly double YValue;

		internal readonly double ZValue;

		internal readonly double XRotation;

		internal readonly double YRotation;

		internal readonly double ZRotation;


		private readonly Random random;

		internal LoksimRandom(XmlNode node)
		{
			if (node.Attributes != null)
			{
				for (int i = 0; i < node.Attributes.Count; i++)
				{
					switch (node.Attributes[i].Name)
					{
						case "SRAND":
							// random seed
							NumberFormats.TryParseIntVb6(node.Attributes[i].InnerText, out Seed);
							break;
						case "XValue":
							// plus / minus X axis
							NumberFormats.TryParseDoubleVb6(node.Attributes[i].InnerText, out XValue);
							break;
						case "YValue":
							// plus / minus Y axis
							NumberFormats.TryParseDoubleVb6(node.Attributes[i].InnerText, out YValue);
							break;
						case "ZValue":
							// plus / minus Z axis
							NumberFormats.TryParseDoubleVb6(node.Attributes[i].InnerText, out ZValue);
							break;
					}
				}
			}
			random = new Random(Seed);

		}

		internal Vector3 GetPosition()
		{
			double randomResult = random.NextDouble();
			// random result returns zero plus / minus the value
			Vector3 v = new Vector3(XValue - randomResult * XValue * 2, YValue - randomResult * YValue * 2, ZValue - randomResult * ZValue * 2);
			return v;
		}


	}

	internal class LightStates
	{
		internal double Day;
		internal double Night;
		internal bool TrainLit;
		internal LightStates(XmlNode node)
		{
			if (node.Attributes == null)
			{
				return;
			}

			for (int i = 0; i < node.Attributes.Count; i++)
			{
				switch (node.Attributes[i].Name)
				{
					case "DayLight":
						// Going to be roughly analogous to the BVE DNB values
						Day = double.Parse(node.Attributes[i].InnerText);
						break;
					case "NightLight":
						// Going to be roughly analogous to the BVE DNB values
						Night = double.Parse(node.Attributes[i].InnerText);
						break;
					case "LokLight":
						// Whether object is lit by light emitted by trains
						if (node.Attributes[i].InnerText == "TRUE")
						{
							TrainLit = true;
						}
						break;
				}
			}
		}
	}
}
