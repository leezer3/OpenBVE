using System;
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
			if (builder.Vertices.Length != 0)
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
			int v = builder.Vertices.Length;
			Array.Resize(ref builder.Vertices, v + nVerts);
			for (int i = 0; i < nVerts; i++)
			{
				builder.Vertices[v + i] = new Vertex(mesh.Positions[i]);
			}

			int nFaces = mesh.PosFaces.Count;
			int f = builder.Faces.Length;
			Array.Resize(ref builder.Faces, f + nFaces);
			for (int i = 0; i < nFaces; i++)
			{
				int fVerts = mesh.PosFaces[i].Indices.Count;
				if (nFaces == 0)
				{
					throw new Exception("fVerts must be greater than zero");
				}
				builder.Faces[f + i] = new MeshFace();
				builder.Faces[f + i].Vertices = new MeshFaceVertex[fVerts];
				for (int j = 0; j < fVerts; j++)
				{
					builder.Faces[f + i].Vertices[j].Index = (ushort)mesh.PosFaces[i].Indices[j];
				}
			}

			int nMaterials = mesh.Materials.Count;
			int nFaceIndices = mesh.FaceMaterials.Count;
			for (int i = 0; i < nFaceIndices; i++)
			{
				int fMaterial = (int)mesh.FaceMaterials[i];
				builder.Faces[i].Material = (ushort)(fMaterial + 1);
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
				builder.Materials[m].EmissiveColorUsed = true; //TODO: Check exact behaviour
				builder.Materials[m].TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
				builder.Materials[m].TransparentColorUsed = true;

				if (mesh.Materials[i].Textures.Count > 0)
				{
					builder.Materials[m].DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, mesh.Materials[i].Textures[0].Name);
					if (!System.IO.File.Exists(builder.Materials[m].DaytimeTexture))
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
			if (nFaceNormals > builder.Faces.Length)
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
