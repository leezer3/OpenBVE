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
	internal class Connection
	{
		internal bool End1;
		internal bool End2;
		internal string Platform1;
		internal string Platform2;
		internal string RouteFile1;
		internal string RouteFile2;
		internal Connection(XmlNode node, Kursbuchstrecken kursbuchstrecken)
		{
			if (node.Attributes != null)
			{
				for (int k = 0; k < node.Attributes.Count; k++)
				{
					if (!Enum.TryParse(node.Attributes[k].Name, out PropsAttribute props))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unknown attribute " + node.Attributes[k].Name + " encountered in Verbindung.");
					}
					switch (props)
					{
						case PropsAttribute.Ende1:
							End1 = node.Attributes[k].InnerText.ToLowerInvariant() == "true";
							break;
						case PropsAttribute.Ende2:
							End2 = node.Attributes[k].InnerText.ToLowerInvariant() == "true";
							break;
						case PropsAttribute.Gleis1:
							Platform1 = node.Attributes[k].InnerText;
							break;
						case PropsAttribute.Gleis2:
							Platform2 = node.Attributes[k].InnerText;
							break;
						case PropsAttribute.Strecke1:
							if (!kursbuchstrecken.Strecken.ContainsKey(node.Attributes[k].InnerText))
							{
								throw new Exception("Loksim3D: Strecke1 was missing from the Kursbuchstecken");
							}
							RouteFile1 = node.Attributes[k].InnerText;
							break;
						case PropsAttribute.Strecke2:
							if (!kursbuchstrecken.Strecken.ContainsKey(node.Attributes[k].InnerText))
							{
								throw new Exception("Loksim3D: Strecke2 was missing from the Kursbuchstecken");
							}
							RouteFile2 = node.Attributes[k].InnerText;
							break;
						default:
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unexpected attribute " + node.Attributes[k].Name + " encountered in Verbindung.");
							break;
					}
				}
			}
		}
	}
}
