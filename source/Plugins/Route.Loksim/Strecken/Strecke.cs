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
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace LokSimRouteParser
{
	internal class Strecke
	{
		/// <summary>The default rail object used</summary>
		internal StaticObject DefaultRail;
		
		internal string StdTexture;

		internal int Direction;

		internal Dictionary<string, Track> Tracks = new Dictionary<string, Track>();


		internal Strecke(string fileName)
		{
			XmlDocument currentXML = new XmlDocument();
			try
			{
				// The XML parser LokSim uses seems to allow space in XML attribute names,
				// despite the fact this is *not* technically valid
				string f = File.ReadAllText(fileName);
				f = f.Replace("Fahrt Gegenrichtung", "FahrtGegenrichtung");
				f = f.Replace("Fahrt Richtung", "FahrtRichtung");
				currentXML.LoadXml(f);
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Failed to load Strecke " + fileName);
				throw;
			}

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/STRECKE");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					throw new Exception("Loksim3D: No Strecke nodes in file " + fileName);
				}

				if (DocumentNodes.Count > 1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Only the first Strecke is supported in " + fileName);
				}

				for (int i = 0; i < DocumentNodes[0].ChildNodes.Count; i++)
				{
					switch (DocumentNodes[0].ChildNodes[i].Name)
					{
						case "Props":
							// These are present, but the info in here isn't useful at present
							break;
						case "DefaulRail":
							/*
							 * The default rail appearance
							 * From the files, LokSim seems to auto-generate the rail object somehow...
							 */
							for (int j = 0; j < DocumentNodes[0].ChildNodes[i].ChildNodes.Count; j++)
							{
								switch (DocumentNodes[0].ChildNodes[i].ChildNodes[j].Name)
								{
									case "Props":
										for (int k = 0; k < DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes.Count; k++)
										{
											switch (DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes[k].Name)
											{
												case "File":
													string railFile = Path.CombineFile(Plugin.FileSystem.LoksimDataDirectory, DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes[k].InnerText.TrimStart('\\'));
													L3DRail rail = new L3DRail(railFile);
													DefaultRail = rail.Object;
													break;
											}
										}
										break;
								}
							}
							break;
						case "StdTexture":
							// Unknown - Empty in demo routes
							break;
						case "Himmelsrichtung":
							// The direction of travel (construction) for this routefile
							if (!string.IsNullOrEmpty(DocumentNodes[0].ChildNodes[i].InnerText))
							{
								Direction = int.Parse(DocumentNodes[0].ChildNodes[i].InnerText);
							}
							break;
						case "GLEIS":
							// Creates a track
							// NOTE: Names are unique per Strecke, but a route can contain multiple Strecke with the same track keys
							Track newTrack = new Track(DocumentNodes[0].ChildNodes[i], this);
							Tracks.Add(newTrack.Name, newTrack);
							break;
					}
				}
			}
		}

		internal void Create(ref Vector3 wpos)
		{
			for (int i = 0; i < Tracks.Count; i++)
			{
				string trackKey = Tracks.ElementAt(i).Key;
				if (!Plugin.TrackKeys.ContainsKey(Tracks[trackKey].Guid))
				{
					Plugin.TrackKeys.Add(Tracks[trackKey].Guid, i);
				}
				Tracks[trackKey].Create(wpos, i, DefaultRail);
			}
		}
	}
}
