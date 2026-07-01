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
using System.IO;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using Path = OpenBveApi.Path;

namespace LokSimRouteParser
{
	internal class Kursbuchstrecken
	{
		/// <summary>The list of connections between routefiles</summary>
		internal List<Connection> Connections;
		/// <summary>The list of routefiles</summary>
		internal Dictionary<string, Strecke> Strecken;
		/// <summary>The starting point</summary>
		internal StartPunkt StartPoint;

		internal Kursbuchstrecken(string fileName)
		{
			XmlDocument currentXML = new XmlDocument();
			try
			{
				currentXML.Load(fileName);
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Failed to load Kursbuchstrecken " + fileName);
				throw;
			}

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/KBS");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					throw new Exception("Loksim3D: No Kursbuchstrecken nodes in file " + fileName);
				}
				if (DocumentNodes.Count > 1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Only the first Kursbuchstrecken is supported in " + fileName);
				}

				for (int i = 0; i < DocumentNodes[0].ChildNodes.Count; i++)
				{
					if (!Enum.TryParse(DocumentNodes[0].ChildNodes[i].Name, true, out LoksimNode parsedNode))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised node " + DocumentNodes[0].ChildNodes[i].Name + " in Kursbuchstrecken " + fileName);
						continue;
					}
					switch (parsedNode)
					{
						case LoksimNode.Props:
							// These are present, but appear to contain nothing useful
							break;
						case LoksimNode.Strecken:
							// List of child nodes, each containing a single Props node
							if (DocumentNodes[0].ChildNodes[i].HasChildNodes)
							{
								ParseStrecken(DocumentNodes[0].ChildNodes[i]);
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Empty list of Strecken in Kursbuchstrecken " + fileName);
							}
							break;
						case LoksimNode.Verbindung:
							// List of child nodes, each containing a single Props node
							if (DocumentNodes[0].ChildNodes[i].HasChildNodes)
							{
								ParseVerbindung(DocumentNodes[0].ChildNodes[i]);
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Empty list of Strecken in Kursbuchstrecken " + fileName);
							}
							break;
						case LoksimNode.StartPunkt:
							for (int j = 0; j < DocumentNodes[0].ChildNodes[i].ChildNodes.Count; j++)
							{
								switch (DocumentNodes[0].ChildNodes[i].ChildNodes[j].Name)
								{
									case "Props":
										StartPoint = new StartPunkt(DocumentNodes[0].ChildNodes[i].ChildNodes[j]);
										break;
								}
							}
							break;
					}
				}
			}
		}

		private void ParseStrecken(XmlNode node)
		{
			Strecken = new Dictionary<string, Strecke>();
			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				if (!node.ChildNodes[i].HasChildNodes || node.ChildNodes[i].Name != "Strecke")
				{
					continue;
				}

				for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
				{
					if (!Enum.TryParse(node.ChildNodes[i].ChildNodes[j].Name, true, out LoksimNode parsedNode))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised node " + node.ChildNodes[i].ChildNodes[j].Name + " in Strecken");
						continue;
					}
					switch (parsedNode)
					{
						case LoksimNode.Props:
							if (node.ChildNodes[i].ChildNodes[j].Attributes != null)
							{
								for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].Attributes.Count; k++)
								{
									switch (node.ChildNodes[i].ChildNodes[j].Attributes[k].Name)
									{
										case "Name":
											string streckeFile = Path.CombineFile(Plugin.FileSystem.LoksimDataDirectory, node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText.TrimStart('\\'));
											if (!Strecken.ContainsKey(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText) && File.Exists(streckeFile))
											{
												Strecke strecke = new Strecke(streckeFile);
												Strecken.Add(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, strecke);
											}
											break;
									}
								}
							}
							break;
						default:
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected node " + node.ChildNodes[i].ChildNodes[j].Name + " encountered in LokSim3D Strecken list.");
							break;
					}
				}
			}
		}

		private void ParseVerbindung(XmlNode node)
		{
			Connections = new List<Connection>();
			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				if (!node.ChildNodes[i].HasChildNodes || node.ChildNodes[i].Name != "Name")
				{
					continue;
				}

				for (int j = 0; j < node.ChildNodes[j].ChildNodes.Count; j++)
				{
					switch (node.ChildNodes[i].ChildNodes[j].Name)
					{
						case "Props":
							Connections.Add(new Connection(node.ChildNodes[i].ChildNodes[j], this));
							break;
					}
				}
			}
		}

		internal void CreateWorld()
		{
			// For the minute, just create the first strecken and see what explodes horribly
			Strecke startingStrecke = Strecken[StartPoint.RouteFile];

			Vector3 startingWpos = Vector3.Zero;
			startingStrecke.Create(ref startingWpos); // vector starting point of world tile
		}
	}
}
