using System;
using System.Collections.Generic;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using AssimpNET.Obj;

namespace OpenBve
{
	class AssimpObjParser
	{
		private static string currentFolder;
		private static string currentFile;

		internal static ObjectManager.StaticObject ReadObject(string FileName)
		{
			currentFolder = System.IO.Path.GetDirectoryName(FileName);
			currentFile = FileName;

#if !DEBUG
			try
			{
#endif
				ObjFileParser parser = new ObjFileParser(System.IO.File.ReadAllLines(currentFile), null, System.IO.Path.GetFileNameWithoutExtension(currentFile), currentFile);
				Model model = parser.GetModel();

				ObjectManager.StaticObject obj = new ObjectManager.StaticObject();
				MeshBuilder builder = new MeshBuilder();

				List<Vertex> allVertices = new List<Vertex>();
				foreach (var vertex in model.Vertices)
				{
					allVertices.Add(new Vertex(vertex.X, vertex.Y, vertex.Z));
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
						builder.Materials[m] = new Material();
						uint materialIndex = mesh.MaterialIndex;
						if (materialIndex != AssimpNET.Obj.Mesh.NoMaterial)
						{
							AssimpNET.Obj.Material material = model.MaterialMap[model.MaterialLib[(int)materialIndex]];
							builder.Materials[m].Color = new Color32((byte)(255 * material.Diffuse.R), (byte)(255 * material.Diffuse.G), (byte)(255 * material.Diffuse.B), (byte)(255 * material.Diffuse.A));
							Color24 mSpecular = new Color24((byte)material.Specular.R, (byte)material.Specular.G, (byte)material.Specular.B);
							builder.Materials[m].EmissiveColor = new Color24((byte)(255 * material.Emissive.R), (byte)(255 * material.Emissive.G), (byte)(255 * material.Emissive.B));
							builder.Materials[m].EmissiveColorUsed = true; //TODO: Check exact behaviour
							builder.Materials[m].TransparentColor = new Color24((byte)(255 * material.Transparent.R), (byte)(255 * material.Transparent.G), (byte)(255 * material.Transparent.B));
							builder.Materials[m].TransparentColorUsed = true;

							if (material.Texture != null)
							{
								builder.Materials[m].DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, material.Texture);
								if (!System.IO.File.Exists(builder.Materials[m].DaytimeTexture))
								{
									Interface.AddMessage(MessageType.Error, true, "Texure " + builder.Materials[m].DaytimeTexture + " was not found in file " + currentFile);
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
						builder = new MeshBuilder();
					}
				}
				obj.Mesh.CreateNormals();
				return obj;
#if !DEBUG
			}
			catch (Exception e)
			{
				Interface.AddMessage(MessageType.Error, false, e.Message + " in " + FileName);
				return null;
			}
#endif
		}
	}
}
