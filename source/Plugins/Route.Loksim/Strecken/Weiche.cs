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
using OpenBveApi.Math;

namespace LokSimRouteParser
{
	internal class Weiche : TrackElement
	{
		internal Weiche(XmlNode node)
		{
			if (node.Attributes == null)
			{
				return;
			}

			for (int i = 0; i < node.Attributes.Count; i++)
			{
				switch (node.Attributes[i].Name)
				{
					case "Position":
						// single position, minor hack
						StartingDistance = double.Parse(node.Attributes[i].InnerText);
						break;
				}
			}
		}

		internal override void Create(ref List<OpenBveApi.Routes.TrackElement> track, ref Vector3 Position, ref Vector2 Direction)
		{
			/*
			if (!track.ContainsKey(StartingDistance))
			{
				OpenBveApi.Routes.TrackElement tS = new OpenBveApi.Routes.TrackElement(StartingDistance)
				{
					WorldPosition = new Vector3(StartingDistance, 0, 0)
				};
				track.Add(StartingDistance, tS);
			}
			else
			{
				OpenBveApi.Routes.TrackElement tS = track[StartingDistance];
				track[StartingDistance] = tS;
			}
			*/
		}
	}
}
