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
using System.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Objects;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using AssimpNET.X;

namespace Plugin
{
	class AssimpXParser
	{
		private static string currentFolder;
		private static string currentFile;
		private static Matrix4D rootMatrix;

		internal static StaticObject ReadObject(string FileName)
		{
			currentFolder = System.IO.Path.GetDirectoryName(FileName);
			currentFile = FileName;
			rootMatrix = Matrix4D.NoTransformation;

#if !DEBUG
			try
			{
#endif
				XFileParser parser = new XFileParser(System.IO.File.ReadAllBytes(FileName));
				Scene scene = parser.GetImportedData();

				StaticObject obj = new StaticObject(Plugin.currentHost);
				MeshBuilder builder = new MeshBuilder(Plugin.currentHost);

				if (scene.GlobalMaterials.Count != 0)
				{
					for (int i = 0; i < scene.GlobalMeshes.Count; i++)
					{
						for (int j = 0; j < scene.GlobalMeshes[i].Materials.Count; j++)
						{
							if (scene.GlobalMeshes[i].Materials[j].IsReference)
							{
								for (int k = 0; k < scene.GlobalMaterials.Count; k++)
								{
									if (scene.GlobalMaterials[k].Name == scene.GlobalMeshes[i].Materials[j].Name)
									{
										scene.GlobalMeshes[i].Materials[j] = scene.GlobalMaterials[k];
										break;
									}
								}
							}
						}
					}
				}
				

				// Global
				foreach (var mesh in scene.GlobalMeshes)
				{
					MeshBuilder(ref obj, ref builder, mesh);
				}

				if (scene.RootNode != null)
				{
					// Root Node
					if (scene.RootNode.TrafoMatrix != Matrix4D.Zero)
					{
						rootMatrix = new Matrix4D(scene.RootNode.TrafoMatrix);
					}

					foreach (var mesh in scene.RootNode.Meshes)
					{
						MeshBuilder(ref obj, ref builder, mesh);
					}

					// Children Node
					foreach (var node in scene.RootNode.Children)
					{
						ChildrenNode(ref obj, ref builder, node);
					}
				}

				builder.Apply(ref obj);
				obj.Mesh.CreateNormals();
				if (rootMatrix != Matrix4D.NoTransformation)
				{
					for (int i = 0; i < obj.Mesh.Vertices.Length; i++)
					{
						obj.Mesh.Vertices[i].Coordinates.Transform(rootMatrix);
					}
				}
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

		private static void  MeshBuilder(ref StaticObject obj, ref MeshBuilder builder, AssimpNET.X.Mesh mesh)
		{
			if (builder.Vertices.Count != 0)
			{
				builder.Apply(ref obj);
				builder = new MeshBuilder(Plugin.currentHost);
			}

			int nVerts = mesh.Positions.Count;
			if (nVerts == 0)
			{
				//Some null objects contain an empty mesh
				Plugin.currentHost.AddMessage(MessageType.Warning, false, "nVertices should be greater than zero in Mesh " + mesh.Name);
			}
			for (int i = 0; i < nVerts; i++)
			{
				builder.Vertices.Add(new Vertex(mesh.Positions[i]));
			}

			int nFaces = mesh.PosFaces.Count;
			for (int i = 0; i < nFaces; i++)
			{
				int fVerts = mesh.PosFaces[i].Indices.Count;
				if (nFaces == 0)
				{
					throw new Exception("fVerts must be greater than zero");
				}
				MeshFace f = new MeshFace();
				f.Vertices = new MeshFaceVertex[fVerts];
				for (int j = 0; j < fVerts; j++)
				{
					f.Vertices[j].Index = (ushort)mesh.PosFaces[i].Indices[j];
				}
				builder.Faces.Add(f);
			}

			int nMaterials = mesh.Materials.Count;
			int nFaceIndices = mesh.FaceMaterials.Count;
			for (int i = 0; i < nFaceIndices; i++)
			{
				int fMaterial = (int)mesh.FaceMaterials[i];
				MeshFace f = builder.Faces[i];
				f.Material = (ushort)(fMaterial + 1);
				builder.Faces[i] = f;
			}
			for (int i = 0; i < nMaterials; i++)
			{
				int m = builder.Materials.Length;
				Array.Resize(ref builder.Materials, m + 1);
				builder.Materials[m] = new OpenBveApi.Objects.Material();
				builder.Materials[m].Color = new Color32((byte)(255 * mesh.Materials[i].Diffuse.R), (byte)(255 * mesh.Materials[i].Diffuse.G), (byte)(255 * mesh.Materials[i].Diffuse.B), (byte)(255 * mesh.Materials[i].Diffuse.A));
				double mPower = mesh.Materials[i].SpecularExponent; //TODO: Unsure what this does...
				Color24 mSpecular = new Color24((byte)mesh.Materials[i].Specular.R, (byte)mesh.Materials[i].Specular.G, (byte)mesh.Materials[i].Specular.B);
				builder.Materials[m].EmissiveColor = new Color24((byte)(255 * mesh.Materials[i].Emissive.R), (byte)(255 * mesh.Materials[i].Emissive.G), (byte)(255 * mesh.Materials[i].Emissive.B));
				builder.Materials[m].Flags |= MaterialFlags.Emissive; //TODO: Check exact behaviour
				if (Plugin.EnabledHacks.BlackTransparency)
				{
					builder.Materials[m].TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
					builder.Materials[m].Flags |= MaterialFlags.TransparentColor;
				}
				
				if (mesh.Materials[i].Textures.Count > 0)
				{
					string texturePath = mesh.Materials[i].Textures[0].Name;

					// If the specified file name is an absolute path, make it the file name only.
					// Some object files specify absolute paths.
					// And BVE4/5 doesn't allow textures to be placed in a different directory than the object file.
					if (Plugin.EnabledHacks.BveTsHacks && OpenBveApi.Path.IsAbsolutePath(texturePath))
					{
						texturePath = texturePath.Split('/', '\\').Last();
					}

					try
					{
						builder.Materials[m].DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, texturePath);
					}
					catch (Exception e)
					{
						Plugin.currentHost.AddMessage(MessageType.Error, false, $"Texture file path {texturePath} in file {currentFile} has the problem: {e.Message}");
						builder.Materials[m].DaytimeTexture = null;
					}

					if (builder.Materials[m].DaytimeTexture != null && !System.IO.File.Exists(builder.Materials[m].DaytimeTexture))
					{
						Plugin.currentHost.AddMessage(MessageType.Error, true, "Texure " + builder.Materials[m].DaytimeTexture + " was not found in file " + currentFile);
						builder.Materials[m].DaytimeTexture = null;
					}
				}
			}

			if (mesh.TexCoords.Length > 0 && mesh.TexCoords[0] != null)
			{
				int nCoords = mesh.TexCoords[0].Count;
				for (int i = 0; i < nCoords; i++)
				{
					builder.Vertices[i].TextureCoordinates = new Vector2(mesh.TexCoords[0][i].X, mesh.TexCoords[0][i].Y);
				}
			}

			int nNormals = mesh.Normals.Count;
			Vector3[] normals = new Vector3[nNormals];
			for (int i = 0; i < nNormals; i++)
			{
				normals[i] = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
				normals[i].Normalize();
			}
			int nFaceNormals = mesh.NormFaces.Count;
			if (nFaceNormals > builder.Faces.Count)
			{
				throw new Exception("nFaceNormals must match the number of faces in the mesh");
			}
			for (int i = 0; i < nFaceNormals; i++)
			{
				int nVertexNormals = mesh.NormFaces[i].Indices.Count;
				if (nVertexNormals > builder.Faces[i].Vertices.Length)
				{
					throw new Exception("nVertexNormals must match the number of verticies in the face");
				}
				for (int j = 0; j < nVertexNormals; j++)
				{
					builder.Faces[i].Vertices[j].Normal = normals[(int)mesh.NormFaces[i].Indices[j]];
				}
			}

			int nVertexColors = (int)mesh.NumColorSets;
			for (int i = 0; i < nVertexColors; i++)
			{
				builder.Vertices[i] = new ColoredVertex((Vertex)builder.Vertices[i], new Color128(mesh.Colors[0][i].R, mesh.Colors[0][i].G, mesh.Colors[0][i].B, mesh.Colors[0][i].A));
			}
		}

		private static void ChildrenNode(ref StaticObject obj, ref MeshBuilder builder, Node child)
		{
			builder.TransformMatrix = new Matrix4D(child.TrafoMatrix);

			foreach (var mesh in child.Meshes)
			{
				MeshBuilder(ref obj, ref builder, mesh);
			}
			foreach (var grandchild in child.Children)
			{
				ChildrenNode(ref obj, ref builder, grandchild);
			}
		}
	}
}
