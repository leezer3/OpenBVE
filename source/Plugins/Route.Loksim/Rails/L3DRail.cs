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

using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.Xml;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace LokSimRouteParser
{
	internal class L3DRail
	{
		internal double Gauge;

		internal double RailTopWidth;

		internal double RailTopHeight;

		internal double BallastWidth;

		internal bool UsesTransparentColor;

		internal List<Vector3> BallastPoints = new List<Vector3>();

		internal StaticObject Object;

		internal L3DRail(string fileName)
		{
			// https://eu07.pl/forum/showthread.php?tid=6244&page=2
			// rail is generated in 10m lengths
			// follows that the texture co-ordinates must be relative to said 10m
			// unsure as yet as to how switches are generated
			// must do some twiddling to sort for radius
			// It might be easier if we just generate a ~0.5m segment and use this?? 

			XmlDocument currentXML = new XmlDocument();
			try
			{
				currentXML.Load(fileName);
			}
			catch
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Failed to load L3DRail " + fileName);
				throw;
			}

			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/RAIL");
				if (DocumentNodes == null || DocumentNodes.Count == 0)
				{
					throw new Exception("Loksim3D: No L3DRail nodes in file " + fileName);
				}

				if (DocumentNodes.Count > 1)
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Only the first L3DRail is supported in " + fileName);
				}

				for (int i = 0; i < DocumentNodes[0].ChildNodes.Count; i++)
				{
					if (!Enum.TryParse(DocumentNodes[0].ChildNodes[i].Name, true, out LoksimNode parsedNode))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised node " + DocumentNodes[0].ChildNodes[i].Name + " in L3DRail " + fileName);
						continue;
					}

					switch (parsedNode)
					{
						case LoksimNode.Props:
							if (DocumentNodes[0].ChildNodes[i].Attributes != null)
							{
								for (int j = 0; j < DocumentNodes[0].ChildNodes[i].Attributes.Count; j++)
								{
									switch (DocumentNodes[0].ChildNodes[i].Attributes[j].Name)
									{
										case "Transparent":
											// appears that pure white is a fixed transparent color if this is set
											UsesTransparentColor = DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText == "TRUE";
											break;
										case "Spurbreite":
											NumberFormats.TryParseDoubleVb6(DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText, out Gauge);
											Gauge /= 2;
											break;
										case "Schienenbreite":
											NumberFormats.TryParseDoubleVb6(DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText, out RailTopWidth);
											break;
										case "Schienenhoehe":
											NumberFormats.TryParseDoubleVb6(DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText, out RailTopHeight);
											break;
										case "Bettungsbreite":
											NumberFormats.TryParseDoubleVb6(DocumentNodes[0].ChildNodes[i].Attributes[j].InnerText, out BallastWidth);
											break;
									}
								}
							}
							break;
						case LoksimNode.Hoehe:
							// ballast face points, each contains another props Hoehe with Vector3
							for (int j = 0; j < DocumentNodes[0].ChildNodes[i].ChildNodes.Count; j++)
							{
								for (int k = 0; k < DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes.Count; k++)
								{
									switch (DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes[k].Name)
									{
										case "Hoehe":
											Vector3.TryParse(DocumentNodes[0].ChildNodes[i].ChildNodes[j].Attributes[k].InnerText, ';', out Vector3 v);
											BallastPoints.Add(v);
											break;
									}
								}
								
							}
							break;
						case LoksimNode.TexSchieneOben:
							// texture co-ordinates for each segment of the rail top
							break;
						case LoksimNode.TexSchieneSeite:
							// texture co-ordinates for each segment of the rail side
							break;
						case LoksimNode.TexBettung:
							// texture co-ordinates for each segment of the ballast
							// (ballast must be a single face)
							break;
					}
				}
			}

			Object = new StaticObject(Plugin.CurrentHost);

			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			// rail L
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2, 10), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 10), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge, 0.2 + RailTopHeight, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge - RailTopWidth, 0.2 + RailTopHeight, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(-Gauge - RailTopWidth, 0.2 + RailTopHeight, 10), Vector2.Null));
			MeshFace face = new MeshFace(new[] { 0, 1, 2, 3 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			face = new MeshFace(new[] { 2, 3, 4, 5 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			builder.Apply(ref Object);
			builder = new MeshBuilder(Plugin.CurrentHost);
			// rail R
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2, 10), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 10), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge, 0.2 + RailTopHeight, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge + RailTopWidth, 0.2 + RailTopHeight, 0), Vector2.Null));
			builder.Vertices.Add(new Vertex(new Vector3(Gauge + RailTopWidth, 0.2 + RailTopHeight, 10), Vector2.Null));
			face = new MeshFace(new[] { 0, 1, 2, 3 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			face = new MeshFace(new[] { 2, 3, 4, 5 });
			face.Flags |= FaceFlags.Face2Mask;
			builder.Faces.Add(face);
			builder.Apply(ref Object);
		}
	}
}
