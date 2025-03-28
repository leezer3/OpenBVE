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
using Object.Wavefront;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
	internal static class WavefrontObjParser
	{
		

		/// <summary>Loads a Wavefront object from a file.</summary>
		/// <param name="fileName">The text file to load the object from. Must be an absolute file name.</param>
		/// <param name="encoding">The encoding the file is saved in.</param>
		/// <returns>The object loaded.</returns>
		internal static StaticObject ReadObject(string fileName, System.Text.Encoding encoding)
		{
			ModelExporter modelExporter = ModelExporter.Unknown;
			StaticObject parsedObject = new StaticObject(Plugin.currentHost);

			MeshBuilder meshBuilder = new MeshBuilder(Plugin.currentHost);

			/*
			 * Temporary arrays
			 */
			 List<Vector3> tempVertices = new List<Vector3>();
			List<Vector3> tempNormals = new List<Vector3>();
			List<Vector2> tempCoords = new List<Vector2>();
			Dictionary<string, Material> tempMaterials = new Dictionary<string, Material>();
			//Stores the current material
			string currentMaterial = string.Empty;

			//Read the contents of the file
			string[] lines = File.ReadAllLines(fileName, encoding);

			double currentScale = 1.0;
			//Preprocess
			for (int i = 0; i < lines.Length; i++)
			{
				// Strip hash comments
				int c = lines[i].IndexOf("#", StringComparison.Ordinal);
				if (c >= 0)
				{
					int hash = lines[i].IndexOf('#');
					int eq = lines[i].IndexOf('=');
					if(lines[i].IndexOf("SketchUp", StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						modelExporter = ModelExporter.SketchUp;
					}

					if (lines[i].IndexOf("BlockBench", StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						modelExporter = ModelExporter.BlockBench;
					}

					if (lines[i].IndexOf("Blender", StringComparison.InvariantCultureIgnoreCase) != -1)
					{
						modelExporter = ModelExporter.Blender;
					}
					if(hash != -1 && eq != -1)
					{
						string afterHash = lines[i].Substring(hash + 1).Trim();
						if (afterHash.StartsWith("File units", StringComparison.InvariantCultureIgnoreCase))
						{
							string units = lines[i].Substring(eq + 1).Trim().ToLowerInvariant();
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
					lines[i] = lines[i].Substring(0, c);
				}
				// collect arguments
				List<string> arguments = new List<string>(lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.None));
				for (int j = arguments.Count -1; j >= 0; j--)
				{
					arguments[j] = arguments[j].Trim(new char[] { });
					if (arguments[j] == string.Empty)
					{
						arguments.RemoveAt(j);
					}
				}
				if (arguments.Count == 0)
				{
					continue;
				}

				WavefrontObjCommands cmd;
				Enum.TryParse(arguments[0], true, out cmd);


				switch (cmd)
				{
					case WavefrontObjCommands.V:
						Vector3 vertex = new Vector3();
						if (!double.TryParse(arguments[1], out vertex.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(arguments[2], out vertex.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(arguments[3], out vertex.Z))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex at Line " + i);
						}
						vertex *= currentScale;
						tempVertices.Add(vertex);
						break;
					case WavefrontObjCommands.VT:
						Vector2 coords = new Vector2();
						if (!double.TryParse(arguments[1], out coords.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-ordinates at Line " + i);
						}
						if (!double.TryParse(arguments[2], out coords.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-Ordinates at Line " + i);
						}
						tempCoords.Add(coords);
						break;
					case WavefrontObjCommands.VN:
						Vector3 normal = new Vector3();
						if (!double.TryParse(arguments[1], out normal.X))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(arguments[2], out normal.Y))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(arguments[3], out normal.Z))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex Normal at Line " + i);
						}
						tempNormals.Add(normal);
						break;
					case WavefrontObjCommands.VP:
						throw new NotSupportedException("Parameter space verticies are not supported by this parser");
					case WavefrontObjCommands.F:
						//Create the temp list to hook out the vertices 
						List<VertexTemplate> vertices = new List<VertexTemplate>();
						List<Vector3> normals = new List<Vector3>();
						for (int f = 1; f < arguments.Count; f++)
						{
							Vertex newVertex = new Vertex();
							string[] faceArguments = arguments[f].Split(new[] {'/'} , StringSplitOptions.None);
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
							if (modelExporter >= ModelExporter.UnknownLeftHanded)
							{
								newVertex.Coordinates.X *= -1.0;
							}
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

									switch (modelExporter)
									{
										case ModelExporter.SketchUp:
											newVertex.TextureCoordinates.X *= -1.0;
											newVertex.TextureCoordinates.Y *= -1.0;
											break;
										case ModelExporter.Blender:
										case ModelExporter.BlockBench:
											newVertex.TextureCoordinates.Y *= -1.0;
											break;
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
						MeshFaceVertex[] meshVerticies = new MeshFaceVertex[vertices.Count];
						for (int k = 0; k < vertices.Count; k++)
						{
							meshBuilder.Vertices.Add(vertices[k]);
							meshVerticies[k].Index = (ushort)(meshBuilder.Vertices.Count -1);
							meshVerticies[k].Normal = normals[k];
						}
						int materialIndex = -1;
						if (currentMaterial != string.Empty)
						{
							for (int m = 0; m < meshBuilder.Materials.Length; m++)
							{
								if (meshBuilder.Materials[m] == tempMaterials[currentMaterial])
								{
									materialIndex = m;
									break;
								}
							}
						}

						if (materialIndex == -1)
						{
							materialIndex = meshBuilder.Materials.Length;
							Array.Resize(ref meshBuilder.Materials, meshBuilder.Materials.Length + 1);
							if (tempMaterials.ContainsKey(currentMaterial))
							{
								meshBuilder.Materials[materialIndex] = tempMaterials[currentMaterial];
							}
							else
							{
								meshBuilder.Materials[materialIndex] = new Material();
							}
							
						}
						if (modelExporter >= ModelExporter.UnknownLeftHanded)
						{
							Array.Reverse(meshVerticies, 0, meshVerticies.Length);
						}
						meshBuilder.Faces.Add(currentMaterial == string.Empty ? new MeshFace(meshVerticies, 0) : new MeshFace(meshVerticies, (ushort)materialIndex));
						break;
					case WavefrontObjCommands.O:
					case WavefrontObjCommands.G:
						meshBuilder.Apply(ref parsedObject);
						meshBuilder = new MeshBuilder(Plugin.currentHost);
						break;
					case WavefrontObjCommands.S:
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
					case WavefrontObjCommands.MtlLib:
						//Loads the library of materials used by this file
						string materialsPath = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), arguments[1]);
						if (File.Exists(materialsPath))
						{
							LoadMaterials(materialsPath, ref tempMaterials);
						}
						break;
					case WavefrontObjCommands.UseMtl:
						currentMaterial = arguments[1].ToLowerInvariant();
						if (!tempMaterials.ContainsKey(currentMaterial))
						{
							currentMaterial = string.Empty;
							Plugin.currentHost.AddMessage(MessageType.Error, true, "Material " + arguments[1] + " was not found.");
						}
						break;
					default:
						Plugin.currentHost.AddMessage(MessageType.Warning, false, "Unrecognised command " + arguments[0]);
						break;
				}
			}
			meshBuilder.Apply(ref parsedObject);
			parsedObject.Mesh.CreateNormals();
			return parsedObject;
		}

		private static void LoadMaterials(string fileName, ref Dictionary<string, Material> materials)
		{
			string[] lines = File.ReadAllLines(fileName);
			string currentKey = string.Empty;
			//Preprocess
			for (int i = 0; i < lines.Length; i++)
			{
				// Strip hash comments
				int c = lines[i].IndexOf("#", StringComparison.Ordinal);
				if (c >= 0)
				{
					lines[i] = lines[i].Substring(0, c);
				}
				// collect arguments
				List<string> arguments = new List<string>(lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.None));
				for (int j = arguments.Count - 1; j >= 0; j--)
				{
					arguments[j] = arguments[j].Trim();
					if (arguments[j] == string.Empty)
					{
						arguments.RemoveAt(j);
					}
				}
				if (arguments.Count == 0)
				{
					continue;
				}

				WavefrontMtlCommands cmd;
				Enum.TryParse(arguments[0], true, out cmd);

				switch (cmd)
				{
					case WavefrontMtlCommands.NewMtl:
						currentKey = arguments[1].ToLowerInvariant(); //store as KVP, but case insensitive
						if (materials.ContainsKey(currentKey))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Material " + currentKey + " has been defined twice.");
						}
						else
						{
							materials.Add(currentKey, new Material());
						}
						break;
					case WavefrontMtlCommands.Ka:
						// Not yet supported
						break;
					case WavefrontMtlCommands.Kd:
						double r = 1, g = 1, b = 1;
						if (arguments.Count >= 2 && !double.TryParse(arguments[1], out r))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color R in Material Definition for " + currentKey);
						}
						if (arguments.Count >= 3 && !double.TryParse(arguments[2], out g))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color G in Material Definition for " + currentKey);
						}
						if (arguments.Count >= 4 && !double.TryParse(arguments[3], out b))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Ambient Color B in Material Definition for " + currentKey);
						}
						r = 255 * r;
						g = 255 * g;
						b = 255 * b;
						materials[currentKey].Color = new Color32((byte)r, (byte)g, (byte)b);
						break;
					case WavefrontMtlCommands.Ks:
					case WavefrontMtlCommands.Ke:
						// Not yet supported
						break;
					case WavefrontMtlCommands.D:
						double a = 1;
						if (arguments.Count >= 2 && !double.TryParse(arguments[1], out a))
						{
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Invalid Alpha in Material Definition for " + currentKey);
						}
						materials[currentKey].Color.A = (byte)(a * 255);
						break;
					case WavefrontMtlCommands.Map_Kd:
					case WavefrontMtlCommands.Map_Ka:
						if(Path.IsPathRooted(arguments[arguments.Count - 1]))
						{
							// Rooted path should not be used- Try looking beside the object instead
							Plugin.currentHost.AddMessage(MessageType.Warning, false, "Encountered rooted path for " + currentKey);
							arguments[arguments.Count - 1] = Path.GetFileName(arguments[arguments.Count - 1]);
						}
						string tday = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), arguments[arguments.Count - 1]);
						if (File.Exists(tday))
						{
							materials[currentKey].DaytimeTexture = tday;
						}
						else
						{
							Plugin.currentHost.AddMessage(MessageType.Error, true, "Material texture file " + arguments[arguments.Count -1] + " was not found.");
						}
						break;
					
					case WavefrontMtlCommands.Map_Ke:
					case WavefrontMtlCommands.Illum:
						// Not yet supported
						break;
				}
			}
		}
	}
}
