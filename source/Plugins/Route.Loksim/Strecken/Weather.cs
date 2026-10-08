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

using System.Xml;
using OpenBveApi.Math;

namespace LokSimRouteParser
{
	/// <summary>Class describing a Loksim3D weather / skybox file</summary>
	internal class Weather
	{

		internal double DaytimeBrightness;

		internal double NightTimeBrightness;

		internal double DawnBegin;

		internal double DawnEnd;

		internal double DuskBegin;

		internal double DuskEnd;

		internal Weather(XmlNode node)
		{
			if (node.Attributes == null)
			{
				return;
			}

			for (int i = 0; i < node.ChildNodes.Count; i++)
			{
				switch (node.ChildNodes[i].Name)
				{
					case "AmbientIllumination":
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{
							switch (node.ChildNodes[i].ChildNodes[j].Name)
							{
								case "Props":
									for (int k = 0; k < node.ChildNodes[i].ChildNodes[j].Attributes.Count; k++)
									{
										switch (node.ChildNodes[i].ChildNodes[j].Attributes[k].Name)
										{
											case "BrightnessDay":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out DaytimeBrightness);
												break;
											case "BrightnessNight":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out NightTimeBrightness);
												break;
											case "DawnBegin":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out DawnBegin);
												break;
											case "DawnEnd":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out DawnEnd);
												break;
											case "DuskBegin":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out DuskBegin);
												break;
											case "DuskEnd":
												NumberFormats.TryParseDoubleVb6(node.ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, out DuskEnd);
												break;
										}
									}
									break;
							}
						}
						break;
					case "WeatherSet":
						for (int j = 0; j < node.ChildNodes[i].ChildNodes.Count; j++)
						{

						}
						break;
				}
			}
		}
	}
}
