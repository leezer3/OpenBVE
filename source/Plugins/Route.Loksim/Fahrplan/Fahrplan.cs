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

using System.Collections.Generic;
using System.Xml;
using Formats.OpenBve;
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

			AttributedXMLFile<LoksimNode, LoksimAttribute> fahrplanFile = new AttributedXMLFile<LoksimNode, LoksimAttribute>(fileName, "/FPL", Plugin.CurrentHost);
			fahrplanFile.GetValue(LoksimAttribute.FileAuthor, out FileAuthor);
			fahrplanFile.GetValue(LoksimAttribute.FileInfo, out FileInfo);
			fahrplanFile.GetPath(LoksimAttribute.FilePicture, Path.GetDirectoryName(fileName), out FilePicture);
			fahrplanFile.GetPath(LoksimAttribute.KBS, Plugin.FileSystem.LoksimDataDirectory, out RouteFile);
			// BremsArt : brake type, PZB related (?)
			// BremsStellung : brake position, presumably at start of game
			// Zeiten : list of times
			// ZugArt : train type
			// ZugGewicht : weight of train
			// ZugLaenge : length of train
			// ZugLimit : max speed for train

			// PneuBremse and MgBremse blocks : unsupported brake properties at the minute

			Stations = new List<Station>();
			fahrplanFile.ReadBlock(LoksimNode.Haltestellen, out Block<LoksimNode, LoksimAttribute> haltestellenBlock);
			while (haltestellenBlock.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> stationBlock = haltestellenBlock.ReadNextBlock();
				Stations.Add(new Station(stationBlock));
			}
			
			Kursbuchstrecken = new Kursbuchstrecken(RouteFile);
			Kursbuchstrecken.CreateWorld();
		}
	}
}
