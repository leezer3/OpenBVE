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
using System.IO;
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

		internal static StaticObject ReadObject(string fileName)
		{
			currentFolder = Path.GetDirectoryName(fileName);
			currentFile = fileName;
			rootMatrix = Matrix4D.NoTransformation;

#if !DEBUG
			try
			{
#endif
			byte[] buffer = File.ReadAllBytes(fileName);
				if (buffer.Length < 16 || buffer[0] != 120 | buffer[1] != 111 | buffer[2] != 102 | buffer[3] != 32)
				{
					// Object is actually a single line text file containing relative path to the 'real' X
					// Found in BRSigs\Night
					string relativePath = System.Text.Encoding.ASCII.GetString(buffer);
					if (!OpenBveApi.Path.ContainsInvalidChars(relativePath))
					{
						return ReadObject(OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), relativePath));
					}
				}
				XFileParser parser = new XFileParser(buffer);
				Scene scene = parser.GetImportedData();

				StaticObject obj = new StaticObject(Plugin.CurrentHost);
				MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);

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

					SetReferenceMaterials(scene, ref scene.RootNode);

					foreach (var mesh in scene.RootNode.Meshes)
					{
						MeshBuilder(ref obj, ref builder, mesh);
					}

					// Children Node
					for (int i = 0; i < scene.RootNode.Children.Count; i++)
					{
						Node node = scene.RootNode.Children[i];
						SetReferenceMaterials(scene, ref node);
						ChildrenNode(ref obj, ref builder, node);
					}
				}

				builder.Apply(ref obj, false, false);
				obj.Mesh.CreateNormals();
				if (rootMatrix != Matrix4D.NoTransformation)
				{
					for (int i = 0; i < obj.Mesh.Vertices.Length; i++)
					{
						obj.Mesh.Vertices[i].Coordinates.Transform(rootMatrix, false);
					}
				}
				return obj;
#if !DEBUG
			}
			catch (Exception e)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Error, false, e.Message + " in " + fileName);
				return null;
			}
#endif
		}

		private static void SetReferenceMaterials(Scene scene, ref Node node)
		{
			foreach (var mesh in node.Meshes)
			{
				for (int i = 0; i < mesh.Materials.Count; i++)
				{
					if (mesh.Materials[i].IsReference)
					{
						for (int j = 0; j < scene.GlobalMaterials.Count; j++)
						{
							if (scene.GlobalMaterials[j].Name == mesh.Materials[i].Name)
							{
								mesh.Materials[i] = scene.GlobalMaterials[j];
								break;
							}
						}
					}
				}
			}
			
		}

		private static void  MeshBuilder(ref StaticObject obj, ref MeshBuilder builder, AssimpNET.X.Mesh mesh)
		{
			if (builder.Vertices.Count != 0)
			{
				builder.Apply(ref obj, false, false);
				builder = new MeshBuilder(Plugin.CurrentHost);
			}

			int nVerts = mesh.Positions.Count;
			if (nVerts == 0)
			{
				//Some null objects contain an empty mesh
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "nVertices should be greater than zero in Mesh " + mesh.Name);
				return;
			}
			for (int i = 0; i < nVerts; i++)
			{
				builder.Vertices.Add(new Vertex(mesh.Positions[i]));
			}

			int nFaces = mesh.PosFaces.Count;
			if (nFaces == 0)
			{
				Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "nFaces should be greater than zero in Mesh " + mesh.Name);
				return;
			}
			for (int i = 0; i < nFaces; i++)
			{
				int fVerts = mesh.PosFaces[i].Indices.Count;
				MeshFace f = new MeshFace(fVerts);
				for (int j = 0; j < fVerts; j++)
				{
					f.Vertices[j].Index = (int)mesh.PosFaces[i].Indices[j];
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
				builder.Materials[m].Color = new Color32(mesh.Materials[i].Diffuse);
				double mPower = mesh.Materials[i].SpecularExponent; //TODO: Unsure what this does...
				Color24 mSpecular = new Color24(mesh.Materials[i].Specular);
				builder.Materials[m].EmissiveColor = new Color24(mesh.Materials[i].Emissive);
				builder.Materials[m].Flags |= MaterialFlags.Emissive; //TODO: Check exact behaviour
				if (Plugin.EnabledHacks.BlackTransparency)
				{
					builder.Materials[m].TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
					builder.Materials[m].Flags |= MaterialFlags.TransparentColor;
				}
				
				if (mesh.Materials[i].Textures.Count > 0)
				{
					string texturePath = mesh.Materials[i].Textures[0].Name;
					if (string.IsNullOrEmpty(texturePath))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"An empty texture was specified for material { mesh.Materials[i].Name }");
						builder.Materials[m].DaytimeTexture = null;
						break;
					}
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
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"Texture file path {texturePath} in file {currentFile} has the problem: {e.Message}");
						builder.Materials[m].DaytimeTexture = null;
					}

					if (builder.Materials[m].DaytimeTexture != null && !File.Exists(builder.Materials[m].DaytimeTexture))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, true, "Texture " + builder.Materials[m].DaytimeTexture + " was not found in file " + currentFile);
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
					if ((int)mesh.NormFaces[i].Indices[j] < normals.Length)
					{
						// Check normal index is valid
						builder.Faces[i].Vertices[j].Normal = normals[(int)mesh.NormFaces[i].Indices[j]];
					}
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
			foreach (var mesh in child.Meshes)
			{
				builder.TransformMatrix = child.TrafoMatrix;
				MeshBuilder(ref obj, ref builder, mesh);
				if (builder.Vertices.Count != 0)
				{
					builder.Apply(ref obj, false, false);
					builder = new MeshBuilder(Plugin.CurrentHost);
				}
			}
			foreach (var grandchild in child.Children)
			{
				ChildrenNode(ref obj, ref builder, grandchild);
			}

		}
	}
}
