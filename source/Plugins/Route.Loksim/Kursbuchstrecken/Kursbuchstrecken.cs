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
using Formats.OpenBve;
using OpenBveApi.Interface;
using OpenBveApi.Math;

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
			AttributedXMLFile<LoksimNode, LoksimAttribute> kursbuchstreckenFile = new AttributedXMLFile<LoksimNode, LoksimAttribute>(fileName, "/KBS", Plugin.CurrentHost);
			Strecken = new Dictionary<string, Strecke>();
			while (kursbuchstreckenFile.RemainingSubBlocks > 0)
			{
				Block<LoksimNode, LoksimAttribute> subBlock = kursbuchstreckenFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case LoksimNode.Props:
						// Present, but appear to contain nothing useful to us at the moment
						break;
					case LoksimNode.Verbindung:
					case LoksimNode.Strecken:
						if (subBlock.RemainingSubBlocks == 0)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Empty list of " + subBlock.Key + " in Kursbuchstrecken " + fileName);
							break;
						}

						while (subBlock.RemainingSubBlocks > 0)
						{
							Block<LoksimNode, LoksimAttribute> nextBlock = subBlock.ReadNextBlock();
							if (nextBlock.Key == LoksimNode.Strecke)
							{
								ParseStrecken(nextBlock);
							}
							else
							{
								ParseVerbindung(nextBlock);
							}
						}
						break;
					case LoksimNode.StartPunkt:
						StartPoint = new StartPunkt(subBlock);
						break;
				}
			}
		}

		private void ParseStrecken(Block<LoksimNode, LoksimAttribute> streckenBlock)
		{
			if (streckenBlock.GetPath(LoksimAttribute.Name, Plugin.FileSystem.LoksimDataDirectory, out string streckeFile))
			{
				Strecke strecke = new Strecke(streckeFile);
				Strecken.Add(streckeFile.Replace(Plugin.FileSystem.LoksimDataDirectory, string.Empty), strecke);
			}
		}

		private void ParseVerbindung(Block<LoksimNode, LoksimAttribute> verbindungBlock)
		{
			Connections = new List<Connection>();
			Connections.Add(new Connection(verbindungBlock, this));
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
