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
using System.Xml;
using OpenBveApi.Interface;

namespace LokSimRouteParser
{
	internal class StartPunkt
	{
		/// <summary>The rail to start on</summary>
		internal string Track;
		/// <summary>The track position on that rail</summary>
		internal double Position;
		/// <summary>Whether to start in the reverse direction</summary>
		internal bool Reverse;
		/// <summary>The actual routefile to start in</summary>
		internal string RouteFile;

		internal StartPunkt(XmlNode node)
		{
			if (node.Attributes == null)
			{
				return;
			}

			for (int i = 0; i < node.Attributes.Count; i++)
			{
				if (!Enum.TryParse(node.Attributes[i].Name, true, out PropsAttribute props))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised Props attribute " +node.Attributes[i].Name + " in StartPunkt");
					continue;
				}
				switch (props)
				{
					case PropsAttribute.Gleis:
						Track = node.Attributes[i].InnerText;
						break;
					case PropsAttribute.Pos:
						Position = double.Parse(node.Attributes[i].InnerText);
						break;
					case PropsAttribute.Richtung:
						if (node.Attributes[i].InnerText == "TRUE")
						{
							Reverse = true;
						}
						break;
					case PropsAttribute.Strecke:
						RouteFile = node.Attributes[i].InnerText;
						break;
				}
			}
		}
	}
}
