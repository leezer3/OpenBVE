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
using System.Collections.Generic;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using AssimpNET.Obj;

namespace Plugin
{
	class AssimpObjParser
	{
		private static string currentFolder;
		private static string currentFile;

		internal static StaticObject ReadObject(string FileName)
		{
			currentFolder = System.IO.Path.GetDirectoryName(FileName);
			currentFile = FileName;

#if !DEBUG
			try
			{
#endif
				ObjFileParser parser = new ObjFileParser(System.IO.File.ReadAllLines(currentFile), null, System.IO.Path.GetFileNameWithoutExtension(currentFile), currentFile);
				Model model = parser.GetModel();

				StaticObject obj = new StaticObject(Plugin.currentHost);
				MeshBuilder builder = new MeshBuilder(Plugin.currentHost);

				List<Vertex> allVertices = new List<Vertex>();
				foreach (var vertex in model.Vertices)
				{
					allVertices.Add(new Vertex(vertex));
				}

				List<Vector2> allTexCoords = new List<Vector2>();
				foreach (var texCoord in model.TextureCoord)
				{
					allTexCoords.Add(new Vector2(texCoord.X, texCoord.Y));
				}

				List<Vector3> allNormals = new List<Vector3>();
				foreach (var normal in model.Normals)
				{
					allNormals.Add(new Vector3(normal.X, normal.Y, normal.Z));
				}

				foreach (AssimpNET.Obj.Mesh mesh in model.Meshes)
				{
					foreach (Face face in mesh.Faces)
					{
						int nVerts = face.Vertices.Count;
						if (nVerts == 0)
						{
							throw new Exception("nVertices must be greater than zero");
						}
						int v = builder.Vertices.Length;
						Array.Resize(ref builder.Vertices, v + nVerts);
						for (int i = 0; i < nVerts; i++)
						{
							builder.Vertices[v + i] = allVertices[(int)face.Vertices[i]];
						}

						int f = builder.Faces.Length;
						Array.Resize(ref builder.Faces, f + 1);
						builder.Faces[f] = new MeshFace();
						builder.Faces[f].Vertices = new MeshFaceVertex[nVerts];
						for (int i = 0; i < nVerts; i++)
						{
							builder.Faces[f].Vertices[i].Index = (ushort)i;
						}
						builder.Faces[f].Material = 1;

						int m = builder.Materials.Length;
						Array.Resize(ref builder.Materials, m + 1);
						builder.Materials[m] = new OpenBveApi.Objects.Material();
						uint materialIndex = mesh.MaterialIndex;
						if (materialIndex != AssimpNET.Obj.Mesh.NoMaterial)
						{
							AssimpNET.Obj.Material material = model.MaterialMap[model.MaterialLib[(int)materialIndex]];
							builder.Materials[m].Color = new Color32((byte)(255 * material.Diffuse.R), (byte)(255 * material.Diffuse.G), (byte)(255 * material.Diffuse.B), (byte)(255 * material.Diffuse.A));
#pragma warning disable 0219
							//Current openBVE renderer does not support specular color
							Color24 mSpecular = new Color24((byte)material.Specular.R, (byte)material.Specular.G, (byte)material.Specular.B);
#pragma warning restore 0219
							builder.Materials[m].EmissiveColor = new Color24((byte)(255 * material.Emissive.R), (byte)(255 * material.Emissive.G), (byte)(255 * material.Emissive.B));
							builder.Materials[m].Flags |= MaterialFlags.Emissive; //TODO: Check exact behaviour
							builder.Materials[m].TransparentColor = new Color24((byte)(255 * material.Transparent.R), (byte)(255 * material.Transparent.G), (byte)(255 * material.Transparent.B));
							builder.Materials[m].Flags |= MaterialFlags.TransparentColor;

							if (material.Texture != null)
							{
								builder.Materials[m].DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, material.Texture);
								if (!System.IO.File.Exists(builder.Materials[m].DaytimeTexture))
								{
									Plugin.currentHost.AddMessage(MessageType.Error, true, "Texure " + builder.Materials[m].DaytimeTexture + " was not found in file " + currentFile);
									builder.Materials[m].DaytimeTexture = null;
								}
							}
						}

						int nCoords = face.TexturCoords.Count;
						for (int i = 0; i < nCoords; i++)
						{
							builder.Vertices[i].TextureCoordinates = allTexCoords[(int)face.TexturCoords[i]];
						}

						int nNormals = face.Normals.Count;
						Vector3[] normals = new Vector3[nNormals];
						for (int i = 0; i < nNormals; i++)
						{
							normals[i] = allNormals[(int)face.Normals[i]];
							normals[i].Normalize();
						}
						for (int i = 0; i < nNormals; i++)
						{
							builder.Faces[0].Vertices[i].Normal = normals[i];
						}

						builder.Apply(ref obj);
						builder = new MeshBuilder(Plugin.currentHost);
					}
				}
				obj.Mesh.CreateNormals();
				return obj;
#if !DEBUG
			}
			catch (Exception e)
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, e.Message + " in " + FileName);
				return null;
			}
#endif
		}
	}
}
