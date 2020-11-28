//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
	internal static class WavefrontObjParser
	{
		

		/// <summary>Loads a Wavefront object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in.</param>
		/// <returns>The object loaded.</returns>
		internal static StaticObject ReadObject(string FileName, System.Text.Encoding Encoding)
		{
			StaticObject Object = new StaticObject(Plugin.currentHost);

			MeshBuilder Builder = new MeshBuilder(Plugin.currentHost);

			/*
			 * Temporary arrays
			 */
			 List<Vector3> tempVertices = new List<Vector3>();
			List<Vector3> tempNormals = new List<Vector3>();
			List<Vector2> tempCoords = new List<Vector2>();
			Dictionary<string, Material> TempMaterials = new Dictionary<string, Material>();
			//Stores the current material
			string currentMaterial = string.Empty;

			//Read the contents of the file
			string[] Lines = File.ReadAllLines(FileName, Encoding);

			double currentScale = 1.0;

			//Preprocess
			for (int i = 0; i < Lines.Length; i++)
			{
				// Strip hash comments
				int c = Lines[i].IndexOf("#", StringComparison.Ordinal);
				if (c >= 0)
				{
					int eq = Lines[i].IndexOf('=');
					int hash = Lines[i].IndexOf('#');
					if(eq != -1 && hash != -1)
					{
						string afterHash = Lines[i].Substring(hash + 1).Trim();
						if (afterHash.StartsWith("File units", StringComparison.InvariantCultureIgnoreCase))
						{
							string units = Lines[i].Substring(eq + 1).Trim().ToLowerInvariant();
							switch (units)
							{
								/*
								 * Apply unit correction factor
								 * This is not a default obj feature, but seems to appear in Sketchup exported files
								 */
								case "millimeters":
									currentScale = 0.001;
									break;
								case "centimeters":
									currentScale = 0.01;
									break;
								case "meters":
									currentScale = 1.0;
									break;
								default:
									Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unrecognised units value " + units + " at line "+ i);
									break;
							}
						}
					}
					Lines[i] = Lines[i].Substring(0, c);
				}
				// collect arguments
				List<string> Arguments = new List<string>(Lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.None));
				for (int j = Arguments.Count -1; j >= 0; j--)
				{
					Arguments[j] = Arguments[j].Trim(new char[] { });
					if (Arguments[j] == string.Empty)
					{
						Arguments.RemoveAt(j);
					}
				}
				if (Arguments.Count == 0)
				{
					continue;
				}
				switch (Arguments[0].ToLowerInvariant())
				{
					case "v":
						//Vertex
						Vector3 vertex = new Vector3();
						if (!double.TryParse(Arguments[1], out vertex.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out vertex.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out vertex.Z))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex at Line " + i);
						}
						vertex *= currentScale;
						tempVertices.Add(vertex);
						break;
					case "vt":
						//Vertex texture co-ords
						Vector2 coords = new Vector2();
						if (!double.TryParse(Arguments[1], out coords.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-ordinates at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out coords.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-Ordinates at Line " + i);
						}
						tempCoords.Add(coords);
						break;
					case "vn":
						Vector3 normal = new Vector3();
						if (!double.TryParse(Arguments[1], out normal.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out normal.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out normal.Z))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex Normal at Line " + i);
						}
						tempNormals.Add(normal);
						//Vertex normals
						break;
					case "vp":
						//Parameter space verticies, not supported
						throw new NotSupportedException("Parameter space verticies are not supported by this parser");
					case "f":
						//Creates a new face

						//Create the temp list to hook out the vertices 
						List<VertexTemplate> vertices = new List<VertexTemplate>();
						List<Vector3> normals = new List<Vector3>();
						for (int f = 1; f < Arguments.Count; f++)
						{
							Vertex newVertex = new Vertex();
							string[] faceArguments = Arguments[f].Split(new char[] {'/'} , StringSplitOptions.None);
							int idx;
							if (!int.TryParse(faceArguments[0], out idx))
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Vertex index in Face " + f + " at Line " + i);
								continue;
							}

							int currentVertex = tempVertices.Count;
							if (idx != Math.Abs(idx))
							{
								//Offset, so we seem to need to add one....
								currentVertex++; 
								currentVertex += idx;
							}
							else
							{
								currentVertex = idx;
							}
							if (currentVertex > tempVertices.Count)
							{
								Plugin.currentHost.AddMessage(MessageType.Warning, false, "Vertex index " + idx + " was greater than the available number of vertices in Face " + f + " at Line " + i);
								continue;
							}
							newVertex.Coordinates = tempVertices[currentVertex - 1];
							if (faceArguments.Length <= 1)
							{
								normals.Add(new Vector3());
							}
							else
							{
								if (!int.TryParse(faceArguments[1], out idx))
								{
									if (!string.IsNullOrEmpty(faceArguments[1]))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Texture Co-ordinate index in Face " + f + " at Line " + i);
									}
									newVertex.TextureCoordinates = new Vector2();
								}
								else
								{
									int currentCoord = tempCoords.Count;
									if (idx != Math.Abs(idx))
									{
										//Offset, so we seem to need to add one....
										currentCoord++;
										currentCoord += idx;
									}
									else
									{
										currentCoord = idx;
									}
									if (currentCoord > tempCoords.Count)
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Texture Co-ordinate index " + currentCoord + " was greater than the available number of texture co-ordinates in Face " + f + " at Line " + i);
									}
									else
									{
										newVertex.TextureCoordinates = tempCoords[currentCoord - 1];
									}
									
								}
							}
							if (faceArguments.Length <= 2)
							{
								normals.Add(new Vector3());
							}
							else
							{
								if (!int.TryParse(faceArguments[2], out idx))
								{
									if (!string.IsNullOrEmpty(faceArguments[2]))
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Vertex Normal index in Face " + f + " at Line " + i);
									}
									normals.Add(new Vector3());
								}
								else
								{
									int currentNormal = tempNormals.Count;
									if (idx != Math.Abs(idx))
									{
										//Offset, so we seem to need to add one....
										currentNormal++;
										currentNormal += idx;
									}
									else
									{
										currentNormal = idx;
									}
									if (currentNormal > tempNormals.Count)
									{
										Plugin.currentHost.AddMessage(MessageType.Warning, false, "Vertex Normal index " + currentNormal + " was greater than the available number of normals in Face " + f + " at Line " + i);
										normals.Add(new Vector3());
									}
									else
									{
										normals.Add(tempNormals[currentNormal - 1]);
									}
								}
							}
							vertices.Add(newVertex);
						}
						MeshFaceVertex[] Vertices = new MeshFaceVertex[vertices.Count];
						for (int k = 0; k < vertices.Count; k++)
						{
							Builder.Vertices.Add(vertices[k]);
							Vertices[k].Index = (ushort)(Builder.Vertices.Count -1);
							Vertices[k].Normal = normals[k];
						}

						int materialIndex = -1;
						if (currentMaterial != string.Empty)
						{
							for (int m = 0; m < Builder.Materials.Length; m++)
							{
								if (Builder.Materials[m] == TempMaterials[currentMaterial])
								{
									materialIndex = m;
									break;
								}
							}
						}

						if (materialIndex == -1)
						{
							materialIndex = Builder.Materials.Length;
							Array.Resize(ref Builder.Materials, Builder.Materials.Length + 1);
							if (TempMaterials.ContainsKey(currentMaterial))
							{
								Builder.Materials[materialIndex] = TempMaterials[currentMaterial];
							}
							else
							{
								Builder.Materials[materialIndex] = new Material();
							}
							
						}
						Builder.Faces.Add(currentMaterial == string.Empty ? new MeshFace(Vertices, 0) : new MeshFace(Vertices, (ushort)materialIndex));
						break;
					case "g":
						//Starts a new face group and (normally) applies a new texture
						Builder.Apply(ref Object);
						Builder = new MeshBuilder(Plugin.currentHost);
						break;
					case "s":
						/* 
						 * Changes the smoothing group applied to these vertexes:
						 * 0- Disabled (Overriden by Vertex normals)
						 * Otherwise appears to be a bitmask (32 available groups)
						 * whereby faces within the same groups have their normals averaged
						 * to appear smooth joins
						 * 
						 * Not really supported at the minute, probably requires the engine 
						 * twiddling to deliberately support specifiying the shading type for a face
						 * 
						 */
						 break;
					case "mtllib":
						//Loads the library of materials used by this file
						string MaterialsPath = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[1]);
						if (File.Exists(MaterialsPath))
						{
							LoadMaterials(MaterialsPath, ref TempMaterials);
						}
						break;
					case "usemtl":
						currentMaterial = Arguments[1].ToLowerInvariant();
						if (!TempMaterials.ContainsKey(currentMaterial))
						{
							currentMaterial = string.Empty;
							Plugin.currentHost.AddMessage(MessageType.Error, true, "Material " + Arguments[1] + " was not found.");
						}
						break;
					default:
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unrecognised command " + Arguments[0]);
						break;
				}
			}
			Builder.Apply(ref Object);
			Object.Mesh.CreateNormals();
			return Object;
		}

		private static void LoadMaterials(string FileName, ref Dictionary<string, Material> Materials)
		{
			string[] Lines = File.ReadAllLines(FileName);
			string currentKey = string.Empty;
			//Preprocess
			for (int i = 0; i < Lines.Length; i++)
			{
				// Strip hash comments
				int c = Lines[i].IndexOf("#", StringComparison.Ordinal);
				if (c >= 0)
				{
					Lines[i] = Lines[i].Substring(0, c);
				}
				// collect arguments
				List<string> Arguments = new List<string>(Lines[i].Split(new char[] { ' ', '\t' }, StringSplitOptions.None));
				for (int j = Arguments.Count - 1; j >= 0; j--)
				{
					Arguments[j] = Arguments[j].Trim(new char[] { });
					if (Arguments[j] == string.Empty)
					{
						Arguments.RemoveAt(j);
					}
				}
				if (Arguments.Count == 0)
				{
					continue;
				}
				
				switch (Arguments[0].ToLowerInvariant())
				{
					case "newmtl":
						currentKey = Arguments[1].ToLowerInvariant(); //store as KVP, but case insensitive
						if (Materials.ContainsKey(currentKey))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Material " + currentKey + " has been defined twice.");
						}
						else
						{
							Materials.Add(currentKey, new Material());
						}
						break;
					case "ka":
						//Ambient color not supported
						break;
					case "kd":
						//Equivilant to SetColor
						double r = 1, g = 1, b = 1;
						if (Arguments.Count >= 2 && !double.TryParse(Arguments[1], out r))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color R in Material Definition for " + currentKey);
						}
						if (Arguments.Count >= 3 && !double.TryParse(Arguments[2], out g))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color G in Material Definition for " + currentKey);
						}
						if (Arguments.Count >= 4 && !double.TryParse(Arguments[3], out b))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color B in Material Definition for " + currentKey);
						}
						r = 255 * r;
						g = 255 * g;
						b = 255 * b;
						Materials[currentKey].Color = new Color32((byte)r, (byte)g, (byte)b);
						break;
					case "ks":
						//Specular color not supported
						break;
					case "ke":
						//Emissive color not supported
						break;
					case "d":
						//Sets the alpha value for the face
						double a = 1;
						if (Arguments.Count >= 2 && !double.TryParse(Arguments[1], out a))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Alpha in Material Definition for " + currentKey);
						}
						Materials[currentKey].Color.A = (byte)((1 - a) * 255);
						break;
					case "map_kd":
					case "map_ka":
						string tday = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[Arguments.Count - 1]);
						if (File.Exists(tday))
						{
							Materials[currentKey].DaytimeTexture = tday;
						}
						else
						{
							Plugin.currentHost.AddMessage(MessageType.Error, true, "Material texture file " + Arguments[Arguments.Count -1] + " was not found.");
						}
						break;
					
					case "map_ke":
						//Emissive color map not supported
						break;
					case "illum":
						//Illumination mode not supported
						break;
					
				}
			}
		}
	}
}
