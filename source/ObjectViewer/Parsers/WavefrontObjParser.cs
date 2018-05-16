using System;
using System.IO;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Collections.Generic;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal static class WavefrontObjParser
	{

		// structures
		private class Material
		{
			internal Color32 Color;
			internal Color24 EmissiveColor;
			internal bool EmissiveColorUsed;
			internal Color24 TransparentColor;
			internal bool TransparentColorUsed;
			internal string DaytimeTexture;
			internal string NighttimeTexture;
			internal World.MeshMaterialBlendMode BlendMode;
			internal ushort GlowAttenuationData;
			internal string Key;
			internal Material()
			{
				this.Color = new Color32(255, 255, 255, 255);
				this.EmissiveColor = new Color24(0, 0, 0);
				this.EmissiveColorUsed = false;
				this.TransparentColor = new Color24(0, 0, 0);
				this.TransparentColorUsed = false;
				this.DaytimeTexture = null;
				this.NighttimeTexture = null;
				this.BlendMode = World.MeshMaterialBlendMode.Normal;
				this.GlowAttenuationData = 0;
				this.Key = string.Empty;
			}
		}
		private class MeshBuilder
		{
			internal List<VertexTemplate> Vertices;
			internal List<World.MeshFace> Faces;
			internal Material[] Materials;
			internal MeshBuilder()
			{
				this.Vertices = new List<VertexTemplate>();
				this.Faces = new List<World.MeshFace>();
				this.Materials = new Material[] { };
			}
		}

		/// <summary>Loads a Wavefront object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <param name="ForceTextureRepeatX">Whether to force TextureWrapMode.Repeat for referenced textures on the X-axis</param>
		/// <param name="ForceTextureRepeatY">Whether to force TextureWrapMode.Repeat for referenced textures on the Y-axis</param>
		/// <returns>The object loaded.</returns>
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
		{
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject
			{
				Mesh =
				{
					Faces = new World.MeshFace[] { },
					Materials = new World.MeshMaterial[] { },
					Vertices = new VertexTemplate[] { }
				}
			};

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
			string[] Lines = File.ReadAllLines(FileName);

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
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid X co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out vertex.Y))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Y co-ordinate in Vertex at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out vertex.Z))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Z co-ordinate in Vertex at Line " + i);
						}
						tempVertices.Add(vertex);
						break;
					case "vt":
						//Vertex texture co-ords
						Vector2 coords = new Vector2();
						if (!double.TryParse(Arguments[1], out coords.X))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-ordinates at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out coords.Y))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid X co-ordinate in Texture Co-Ordinates at Line " + i);
						}
						//Wavefront obj texture co-ords Y axis appear inverted v.s. BVE standard
						coords.Y = -coords.Y;
						tempCoords.Add(coords);
						break;
					case "vn":
						Vector3 normal = new Vector3();
						if (!double.TryParse(Arguments[1], out normal.X))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid X co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[2], out normal.Y))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Y co-ordinate in Vertex Normal at Line " + i);
						}
						if (!double.TryParse(Arguments[3], out normal.Z))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Z co-ordinate in Vertex Normal at Line " + i);
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
							if (!int.TryParse(faceArguments[0], out idx) || idx > tempVertices.Count)
							{
								Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Vertex index in Face " + f + " at Line " + i);
								continue;
							}
							newVertex.Coordinates = tempVertices[idx - 1];
							if (faceArguments.Length <= 1)
							{
								normals.Add(new Vector3());
							}
							else
							{
								if (!int.TryParse(faceArguments[1], out idx) || idx > tempCoords.Count)
								{
									if (!string.IsNullOrEmpty(faceArguments[1]))
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Texture Co-ordinate index in Face " + f + " at Line " + i);
									}
									newVertex.TextureCoordinates = new Vector2();
								}
								else
								{
									newVertex.TextureCoordinates = tempCoords[idx - 1];
								}
							}
							if (faceArguments.Length <= 2)
							{
								normals.Add(new Vector3());
							}
							else
							{
								if (!int.TryParse(faceArguments[2], out idx) || idx > tempNormals.Count)
								{
									if (!string.IsNullOrEmpty(faceArguments[2]))
									{
										Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Vertex Normal index in Face " + f + " at Line " + i);
									}
									normals.Add(new Vector3());
								}
								else
								{
									normals.Add(tempNormals[idx - 1]);
								}
							}
							vertices.Add(newVertex);
						}
						World.MeshFaceVertex[] Vertices = new World.MeshFaceVertex[vertices.Count];
						for (int k = 0; k < vertices.Count; k++)
						{
							int v = Builder.Vertices.FindIndex(a => a.Equals(vertices[k]));
							if (v != -1)
							{
								Vertices[k].Index = (ushort)v;
							}
							else
							{
								Builder.Vertices.Add(vertices[k]);
								Vertices[k].Index = (ushort)(Builder.Vertices.Count -1);
							}
							
							Vertices[k].Normal = normals[k];
						}
						Builder.Faces.Add(currentMaterial == -1 ? new World.MeshFace(Vertices, 0) : new World.MeshFace(Vertices, (ushort)currentMaterial));
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
								Interface.AddMessage(Interface.MessageType.Error, true, "Material " + Arguments[1] + " was not found.");
								currentMaterial = -1;
							}
						}
						break;
					default:
						Interface.AddMessage(Interface.MessageType.Warning, false, "Unrecognised command " + Arguments[0]);
						break;
				}
			}
			ApplyMeshBuilder(ref Object, Builder);
			World.CreateNormals(ref Object.Mesh);
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
						if (!double.TryParse(Arguments[1], out r))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Ambient Color R in Material Definition for " + mm.Key);
						}
						if (!double.TryParse(Arguments[2], out g))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Ambient Color G in Material Definition for " + mm.Key);
						}
						if (!double.TryParse(Arguments[3], out b))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Ambient Color B in Material Definition for " + mm.Key);
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
						if (!double.TryParse(Arguments[1], out a))
						{
							Interface.AddMessage(Interface.MessageType.Warning, false, "Invalid Alpha in Material Definition for " + mm.Key);
						}
						a *= 255;
						mm.Color.A = (byte)a;
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
							Interface.AddMessage(Interface.MessageType.Error, true, "Material texture file " + Arguments[Arguments.Count -1] + " was not found.");
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
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Builder.Faces.Count);
				if (mm == 0)
				{
					if (Object.Mesh.Materials.Length == 0)
					{
						/*
						 * If the object has no materials defined at all, we need to add one
						 */
						Array.Resize(ref Object.Mesh.Materials, 1);
						Object.Mesh.Materials[0] = new World.MeshMaterial();
						Object.Mesh.Materials[0].Color = new Color32(255,255,255);
						Object.Mesh.Materials[0].Flags = (byte)(0 | 0);
						Object.Mesh.Materials[0].DaytimeTexture = null;
						Object.Mesh.Materials[0].NighttimeTexture = null;
						mm++;
					}
				}
				if (Builder.Materials.Length > 0)
				{
					Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Builder.Materials.Length);
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
					Object.Mesh.Vertices[mv + i] = Builder.Vertices[i];
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
					Object.Mesh.Materials[mm + i].Flags = (byte)((Builder.Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | (Builder.Materials[i].TransparentColorUsed ? World.MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Builder.Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Builder.Materials[i].TransparentColor;
					if (Builder.Materials[i].DaytimeTexture != null)
					{
						Textures.Texture tday;
						if (Builder.Materials[i].TransparentColorUsed)
						{
							Textures.RegisterTexture(Builder.Materials[i].DaytimeTexture, new OpenBveApi.Textures.TextureParameters(null, new Color24(Builder.Materials[i].TransparentColor.R, Builder.Materials[i].TransparentColor.G, Builder.Materials[i].TransparentColor.B)), out tday);
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
						Textures.Texture tnight;
						if (Builder.Materials[i].TransparentColorUsed)
						{
							Textures.RegisterTexture(Builder.Materials[i].NighttimeTexture, new OpenBveApi.Textures.TextureParameters(null, new Color24(Builder.Materials[i].TransparentColor.R, Builder.Materials[i].TransparentColor.G, Builder.Materials[i].TransparentColor.B)), out tnight);
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
					Object.Mesh.Materials[mm + i].WrapMode = Textures.OpenGlTextureWrapMode.RepeatRepeat;
				}
			}
		}
	}
}
