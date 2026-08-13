//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using AssimpNET.Obj;
using OpenBveApi;
using Material = AssimpNET.Obj.Material;
using Mesh = AssimpNET.Obj.Mesh;

namespace Plugin
{
	internal class AssimpObjParser
	{
		private static string currentFolder;

		internal static StaticObject ReadObject(string fileName)
		{
			currentFolder = Path.GetDirectoryName(fileName);
			try
			{
				ObjFileParser parser = new ObjFileParser(System.IO.File.ReadAllLines(fileName), null, System.IO.Path.GetFileNameWithoutExtension(fileName), fileName);
				Model model = parser.GetModel();

				StaticObject obj = new StaticObject(Plugin.CurrentHost);
				MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
				Material lastMaterial = null;

				foreach (Mesh mesh in model.Meshes)
				{
					//mesh.Faces.GroupBy(x => x.Material).OrderByDescending(g => g.Count()).SelectMany(x => x).ToList();
					foreach (Face face in mesh.Faces)
					{
						if (face.Material != lastMaterial)
						{
							builder.Apply(ref obj);
							builder = new MeshBuilder(Plugin.CurrentHost);
							uint materialIndex = mesh.MaterialIndex;
							if (materialIndex != Mesh.NoMaterial)
							{
								Material material = model.MaterialMap[model.MaterialLib[(int)materialIndex]];
								builder.Materials[0].Color = new Color32(material.Diffuse);
#pragma warning disable 0219
								//Current openBVE renderer does not support specular color
								// ReSharper disable once UnusedVariable
								Color24 mSpecular = new Color24(material.Specular);
#pragma warning restore 0219
								// Wrap Color24 in Color32 for RGBA support; alpha defaults to 255 (opaque)
								builder.Materials[0].EmissiveColor = new Color32(new Color24(material.Emissive));
								builder.Materials[0].Flags |= MaterialFlags.Emissive; //TODO: Check exact behaviour
								if (material.TransparentUsed)
								{
									builder.Materials[0].TransparentColor = new Color24(material.Transparent);
									builder.Materials[0].Flags |= MaterialFlags.TransparentColor;
								}

								if (material.Texture != null)
								{
									builder.Materials[0].DaytimeTexture = Path.CombineFile(currentFolder, material.Texture);
									if (!System.IO.File.Exists(builder.Materials[0].DaytimeTexture))
									{
										Plugin.CurrentHost.AddMessage(MessageType.Error, true, "Texture " + builder.Materials[0].DaytimeTexture + " was not found in file " + fileName);
										builder.Materials[0].DaytimeTexture = null;
									}
								}
							}
						}
						
						if (face.Vertices.Count == 0)
						{
							throw new Exception("nVertices must be greater than zero");
						}
						int startingVertex = builder.Vertices.Count;
						for (int i = 0; i < face.Vertices.Count; i++)
						{
							VertexTemplate v = new Vertex(model.Vertices[(int)face.Vertices[i]] * model.ScaleFactor);
							
							if (model.TextureCoord.Count > 0 && i <= model.TextureCoord.Count && face.TexturCoords.Count > 0 && i <= face.TexturCoords.Count)
							{
								Vector2 textureCoordinate = new Vector2(model.TextureCoord[i].X, model.TextureCoord[i].Y);
								switch (model.Exporter)
								{
									case ModelExporter.SketchUp:
										textureCoordinate.X *= -1.0;
										textureCoordinate.Y *= -1.0;
										break;
									case ModelExporter.Blender:
									case ModelExporter.BlockBench:
										textureCoordinate.Y *= -1.0;
										break;
								}
								v.TextureCoordinates = textureCoordinate;
							}
							
							builder.Vertices.Add(v);
							
						}

						MeshFace f = new MeshFace(face.Vertices.Count);
						
						for (int i = 0; i < face.Vertices.Count; i++)
						{
							f.Vertices[i].Index = startingVertex + i;
							if (face.Normals.Count > i)
							{
								f.Vertices[i].Normal = model.Normals[(int)face.Normals[i]];
							}
						}
						
						f.Material = 0;
						f.Flags |= FaceFlags.Face2Mask;
						builder.Faces.Add(f);
						
						if (model.Exporter >= ModelExporter.UnknownLeftHanded)
						{
							Array.Reverse(builder.Faces[builder.Faces.Count -1].Vertices, 0, builder.Faces[builder.Faces.Count -1].Vertices.Length);
						}
						lastMaterial = face.Material;
					}
				}
				builder.Apply(ref obj);
				obj.Mesh.CreateNormals();
				return obj;
			}
			catch (Exception e)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, e.Message + " in " + fileName);
				return null;
			}
		}
	}
}
