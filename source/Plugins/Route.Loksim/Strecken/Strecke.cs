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
using Formats.OpenBve;
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
			string f = File.ReadAllText(fileName);
			f = f.Replace("Fahrt Gegenrichtung", "FahrtGegenrichtung");
			f = f.Replace("Fahrt Richtung", "FahrtRichtung");
			AttributedXMLFile<LoksimNode, LoksimAttribute> streckeFile = new AttributedXMLFile<LoksimNode, LoksimAttribute>(f, fileName, "/STRECKE", Plugin.CurrentHost);

			while (streckeFile.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> subBlock = streckeFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case LoksimNode.Props:
						// These are present, but the info in here isn't useful at present
						break;
					case LoksimNode.DefaulRail:
						if (subBlock.GetPath(LoksimAttribute.File, Plugin.FileSystem.LoksimDataDirectory, out string railFile))
						{
							L3DRail rail = new L3DRail(railFile);
							DefaultRail = rail.Object;
						}
						break;
					case LoksimNode.StdTexture:
						// Unknown - Empty in demo routes
						break;
					case LoksimNode.Himmelsrichtung:
						// Direction of travel (construction) for this routefile
						subBlock.GetValue(LoksimAttribute.Value, out Direction);
						break;
					case LoksimNode.GLEIS:
						if (subBlock.GetValue(LoksimAttribute.Name, out string trackName) && subBlock.ReadBlock(LoksimNode.TOPOLOGIE, out Block<LoksimNode, LoksimAttribute> topologyBlock))
						{
							// NOTE: Names are unique per Strecke, but a route can contain multiple Strecke with the same track keys
							Track newTrack = new Track(topologyBlock, trackName, this);
							

							if (subBlock.ReadBlock(LoksimNode.Objecte, out Block<LoksimNode, LoksimAttribute> objectsBlock))
							{
								while (objectsBlock.RemainingSubBlocks > 0)
								{
									Block<LoksimNode, LoksimAttribute> objectBlock = objectsBlock.ReadNextBlock();
									newTrack.Objects.Add(new ObjectGroup(objectBlock));
								}
							}

							if (subBlock.ReadBlock(LoksimNode.Landschaft, out Block<LoksimNode, LoksimAttribute> landschaftBlock))
							{
								while (landschaftBlock.RemainingSubBlocks > 0)
								{
									newTrack.Landschaftsobjekt.Add(new LandschaftsObjekt(landschaftBlock.ReadNextBlock()));
								}
								
							}
							Tracks.Add(trackName, newTrack);
						}
						break;
				}
			}
			XmlDocument currentXML = new XmlDocument();
			try
			{
				// The XML parser LokSim uses seems to allow space in XML attribute names,
				// despite the fact this is *not* technically valid
				
				currentXML.LoadXml(f);
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Failed to load Strecke " + fileName);
				throw;
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
