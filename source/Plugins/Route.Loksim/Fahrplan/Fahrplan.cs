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
using System.Xml;
using OpenBveApi;
using OpenBveApi.Interface;

namespace LokSimRouteParser
{
	internal class Fahrplan
	{
		/// <summary>The author of this file</summary>
		internal string FileAuthor;
		/// <summary>The info text to display</summary>
		internal string FileInfo;
		/// <summary>The image file for this Fahrplan</summary>
		internal string FilePicture;
		/// <summary>The route data file for this Fahrplan</summary>
		internal string RouteFile;
		/// <summary>The stations list for this Fahrplan</summary>
		internal List<Station> Stations;
		/// <summary>The actual route data for this Fahrplan</summary>
		private Kursbuchstrecken Kursbuchstrecken;

		internal Fahrplan(string fileName)
		{
			XmlDocument currentXML = new XmlDocument();
			try
			{
				currentXML.Load(fileName);
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Failed to load Fahrplan " + fileName);
				throw;
			}

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/FPL");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					throw new Exception("No Fahrplan nodes in file " + fileName);
				}

				if (DocumentNodes.Count > 1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Only the first Fahrplan is supported in " + fileName);
				}
				for (int i = 0; i < DocumentNodes[0].ChildNodes.Count; i++)
				{
					if (!Enum.TryParse(DocumentNodes[0].ChildNodes[i].Name, out LoksimNode parsedNode))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised node " + DocumentNodes[0].ChildNodes[i].Name + " in LokSim3D Fahrplan " + fileName);
						continue;
					}
					switch (parsedNode)
					{
						case LoksimNode.Props:
							// Basic properties
							if (DocumentNodes[0].ChildNodes[i].Attributes != null)
							{
								for (int j = 0; j < DocumentNodes[0].ChildNodes[i].Attributes.Count; j++)
								{
									if (!Enum.TryParse(DocumentNodes[0].ChildNodes[i].Attributes[j].Name, true, out PropsAttribute props))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised Props attribute " + DocumentNodes[0].ChildNodes[i].Attributes[j].Name + " in LokSim3D Fahrplan " + fileName);
										continue;
									}
									switch (props)
									{
										case PropsAttribute.FileAuthor:
											FileAuthor = DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText;
											break;
										case PropsAttribute.FileInfo:
											FileInfo = DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText;
											break;
										case PropsAttribute.FilePicture:
											// Path relative to the current Fahrplan
											FilePicture = DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText;
											break;
										case PropsAttribute.KBS:
											// Path relative to the LokSim3D content folder
											RouteFile = Path.CombineFile(Plugin.FileSystem.LoksimDataDirectory, DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText.TrimStart('\\'));
											if (!System.IO.File.Exists(RouteFile))
											{
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Kurbuchstrecken file " + DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText + " was not found in Fahrplan " + fileName);
												RouteFile = string.Empty;
											}
											break;
										/*
										 * Brake positions are a letter, which relates somehow to the internal workings of the PZB system
										 *
										 * KNOWN POSITION VALUES:
										 * ----------------------
										 * R
										 * O
										 * G
										 * P
										 */
										case PropsAttribute.BremsArt:
											// brake type - PZB related (?)
											break;
										case PropsAttribute.BremsStellung:
											// brake position - Presumably at the start of the game (?)
											break;
										case PropsAttribute.Zeiten:
											// List of times
											break;
										case PropsAttribute.ZugArt:
											// train type
											break;
										case PropsAttribute.ZugGewicht:
											// total weight of train
											break;
										case PropsAttribute.ZugLaenge:
											// total length of train
											break;
										case PropsAttribute.ZugLimit:
											// speed limit for the train
											break;
									}
								}
							}
							break;
						case LoksimNode.PneuBremse:
						case LoksimNode.MgBremse:
							// Unsupported brake properties
							break;
						case LoksimNode.Haltestellen:
							// List of stations
							if (DocumentNodes[0].ChildNodes[i].HasChildNodes)
							{
								ParseStationList(DocumentNodes[0].ChildNodes[i]);
							}
							else
							{
								throw new Exception("Loksim3D: Empty station list in Fahrplan " + fileName);
							}
							break;
					}
				}
			}

			Kursbuchstrecken = new Kursbuchstrecken(RouteFile);
			Kursbuchstrecken.CreateWorld();
		}

		internal void ParseStationList(XmlNode node)
		{
			Stations = new List<Station>();
			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				switch (node.ChildNodes[i].Name)
				{
					case "Halt":
						if (node.ChildNodes[i].HasChildNodes)
						{
							for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
							{
								switch (node.ChildNodes[i].ChildNodes[j].Name)
								{
									case "Props":
										Stations.Add(new Station(node.ChildNodes[i].ChildNodes[j]));
										break;
								}
							}
						}
						break;
					default:
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unexpected node " + node.ChildNodes[i].Name + " in Fahrplan station list.");
						break;
				}
			}
		}
	}
}
