using System;
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Collections.Generic;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static class WavefrontObjParser
	{
		private class MeshBuilder
		{
			internal List<VertexTemplate> Vertices;
			internal List<MeshFace> Faces;
			internal Material[] Materials;
			internal MeshBuilder()
			{
				this.Vertices = new List<VertexTemplate>();
				this.Faces = new List<MeshFace>();
				this.Materials = new Material[] { new Material() };
			}
		}

		/// <summary>Loads a Wavefront object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in.</param>
		/// <returns>The object loaded.</returns>
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding)
		{
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();

			MeshBuilder Builder = new MeshBuilder();

			/*
			 * Temporary arrays
			 */
			 List<Vector3> tempVertices = new List<Vector3>();
			List<Vector3> tempNormals = new List<Vector3>();
			List<Vector2> tempCoords = new List<Vector2>();
			Material[] TempMaterials = new Material[0];
			//Stores the current material
			int currentMaterial = -1;

			//Read the contents of the file
			string[] Lines = File.ReadAllLines(FileName, Encoding);

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
				for (int j = Arguments.Count -1; j >= 0; j--)
				{
					Arguments[j] = Arguments[j].Trim();
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
							Interface.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out vertex.Y))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out vertex.Z))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex at Line " + i);
						}
						tempVertices.Add(vertex);
						break;
					case "vt":
						//Vertex texture co-ords
						Vector2 coords = new Vector2();
						if (!double.TryParse(Arguments[1], out coords.X))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-ordinates at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out coords.Y))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-Ordinates at Line " + i);
						}
						tempCoords.Add(coords);
						break;
					case "vn":
						Vector3 normal = new Vector3();
						if (!double.TryParse(Arguments[1], out normal.X))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid X co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out normal.Y))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Y co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out normal.Z))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Z co-ordinate in Vertex Normal at Line " + i);
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
								Interface.AddMessage(MessageType.Warning, false, "Invalid Vertex index in Face " + f + " at Line " + i);
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
								Interface.AddMessage(MessageType.Warning, false, "Vertex index " + idx + " was greater than the available number of vertices in Face " + f + " at Line " + i);
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
										Interface.AddMessage(MessageType.Warning, false, "Invalid Texture Co-ordinate index in Face " + f + " at Line " + i);
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
										Interface.AddMessage(MessageType.Warning, false, "Texture Co-ordinate index " + currentCoord + " was greater than the available number of texture co-ordinates in Face " + f + " at Line " + i);
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
										Interface.AddMessage(MessageType.Warning, false, "Invalid Vertex Normal index in Face " + f + " at Line " + i);
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
										Interface.AddMessage(MessageType.Warning, false, "Vertex Normal index " + currentNormal + " was greater than the available number of normals in Face " + f + " at Line " + i);
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
						Builder.Faces.Add(currentMaterial == -1 ? new MeshFace(Vertices, 0) : new MeshFace(Vertices, (ushort)currentMaterial));
						break;
					case "g":
						//Starts a new face group and (normally) applies a new texture
						ApplyMeshBuilder(ref Object, Builder);
						Builder = new MeshBuilder();
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
						for (int m = 0; m < TempMaterials.Length; m++)
						{
							if (TempMaterials[m].Key.ToLowerInvariant() == Arguments[1].ToLowerInvariant())
							{
								bool mf = false;
								for (int k = 0; k < Builder.Materials.Length; k++)
								{
									if (Builder.Materials[k].Key.ToLowerInvariant() == Arguments[1].ToLowerInvariant())
									{
										mf = true;
										currentMaterial = k;
										break;
									}
								}
								if (!mf)
								{
									Array.Resize(ref Builder.Materials, Builder.Materials.Length + 1);
									Builder.Materials[Builder.Materials.Length - 1] = TempMaterials[m];
									currentMaterial = Builder.Materials.Length - 1;
								}
								break;
							}
							if (m == TempMaterials.Length)
							{
								Interface.AddMessage(MessageType.Error, true, "Material " + Arguments[1] + " was not found.");
								currentMaterial = -1;
							}
						}
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, "Unrecognised command " + Arguments[0]);
						break;
				}
			}
			ApplyMeshBuilder(ref Object, Builder);
			Object.Mesh.CreateNormals();
			return Object;
		}

		private static void LoadMaterials(string FileName, ref Material[] Materials)
		{
			string[] Lines = File.ReadAllLines(FileName);
			Material mm = new Material();
			bool fm = false;
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
					Arguments[j] = Arguments[j].Trim();
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
						if (fm == true)
						{
							Array.Resize(ref Materials, Materials.Length + 1);
							Materials[Materials.Length - 1] = mm;
						}
						mm = new Material();
						mm.Key = Arguments[1];
						fm = true;
						break;
					case "ka":
						//Ambient color not supported
						break;
					case "kd":
						//Equivilant to SetColor
						double r = 1, g = 1, b = 1;
						if (Arguments.Count >= 2 && !double.TryParse(Arguments[1], out r))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Ambient Color R in Material Definition for " + mm.Key);
						}
						if (Arguments.Count >= 3 && !double.TryParse(Arguments[2], out g))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Ambient Color G in Material Definition for " + mm.Key);
						}
						if (Arguments.Count >= 4 && !double.TryParse(Arguments[3], out b))
						{
							Interface.AddMessage(MessageType.Warning, false, "Invalid Ambient Color B in Material Definition for " + mm.Key);
						}
						r = 255 * r;
						g = 255 * g;
						b = 255 * b;
						mm.Color = new Color32((byte)r, (byte)g, (byte)b);
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
							Interface.AddMessage(MessageType.Warning, false, "Invalid Alpha in Material Definition for " + mm.Key);
						}
						mm.Color.A = (byte)((1 - a) * 255);
						break;
					case "map_kd":
					case "map_ka":
						string tday = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[Arguments.Count - 1]);
						if (File.Exists(tday))
						{
							mm.DaytimeTexture = tday;
						}
						else
						{
							Interface.AddMessage(MessageType.Error, true, "Material texture file " + Arguments[Arguments.Count -1] + " was not found.");
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
			Array.Resize(ref Materials, Materials.Length + 1);
			Materials[Materials.Length - 1] = mm;
		}

		private static void ApplyMeshBuilder(ref ObjectManager.StaticObject Object, MeshBuilder Builder)
		{
			if (Builder.Faces.Count != 0)
			{
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<MeshFace>(ref Object.Mesh.Faces, mf + Builder.Faces.Count);
				if (mm == 0)
				{
					if (Object.Mesh.Materials.Length == 0)
					{
						/*
						 * If the object has no materials defined at all, we need to add one
						 */
						Array.Resize(ref Object.Mesh.Materials, 1);
						Object.Mesh.Materials[0] = new MeshMaterial();
						Object.Mesh.Materials[0].Color = Color32.White;
						Object.Mesh.Materials[0].Flags = (byte)(0 | 0);
						Object.Mesh.Materials[0].DaytimeTexture = null;
						Object.Mesh.Materials[0].NighttimeTexture = null;
						mm++;
					}
				}
				if (Builder.Materials.Length > 0)
				{
					Array.Resize<MeshMaterial>(ref Object.Mesh.Materials, mm + Builder.Materials.Length);
				}
				else
				{
					/*
					 * If no materials have been defined for this face group, use the last material
					 */
					mm -= 1;
				}
				Array.Resize<VertexTemplate>(ref Object.Mesh.Vertices, mv + Builder.Vertices.Count);
				for (int i = 0; i < Builder.Vertices.Count; i++)
				{
					Object.Mesh.Vertices[mv + i] = new Vertex((Vertex)Builder.Vertices[i]);
				}
				for (int i = 0; i < Builder.Faces.Count; i++)
				{
					Object.Mesh.Faces[mf + i] = Builder.Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++)
					{
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Object.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Builder.Materials.Length; i++)
				{
					Object.Mesh.Materials[mm + i].Flags = (byte)((Builder.Materials[i].EmissiveColorUsed ? MeshMaterial.EmissiveColorMask : 0) | (Builder.Materials[i].TransparentColorUsed ? MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Builder.Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Builder.Materials[i].TransparentColor;
					if (Builder.Materials[i].DaytimeTexture != null)
					{
						Texture tday;
						if (Builder.Materials[i].TransparentColorUsed)
						{
							Textures.RegisterTexture(Builder.Materials[i].DaytimeTexture, new TextureParameters(null, new Color24(Builder.Materials[i].TransparentColor.R, Builder.Materials[i].TransparentColor.G, Builder.Materials[i].TransparentColor.B)), out tday);
						}
						else
						{
							Textures.RegisterTexture(Builder.Materials[i].DaytimeTexture, out tday);
						}
						Object.Mesh.Materials[mm + i].DaytimeTexture = tday;
					}
					else
					{
						Object.Mesh.Materials[mm + i].DaytimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].EmissiveColor = Builder.Materials[i].EmissiveColor;
					if (Builder.Materials[i].NighttimeTexture != null)
					{
						Texture tnight;
						if (Builder.Materials[i].TransparentColorUsed)
						{
							Textures.RegisterTexture(Builder.Materials[i].NighttimeTexture, new TextureParameters(null, new Color24(Builder.Materials[i].TransparentColor.R, Builder.Materials[i].TransparentColor.G, Builder.Materials[i].TransparentColor.B)), out tnight);
						}
						else
						{
							Textures.RegisterTexture(Builder.Materials[i].NighttimeTexture, out tnight);
						}
						Object.Mesh.Materials[mm + i].NighttimeTexture = tnight;
					}
					else
					{
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Builder.Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Builder.Materials[i].GlowAttenuationData;
					Object.Mesh.Materials[mm + i].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
				}
			}
		}
	}
}
