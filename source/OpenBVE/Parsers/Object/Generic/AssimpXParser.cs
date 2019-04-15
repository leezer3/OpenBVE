using System;
using OpenBveApi.Colors;
using OpenBveApi.Objects;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using AssimpNET.X;

namespace OpenBve
{
	class AssimpXParser
	{
		private static string currentFolder;
		private static string currentFile;
		private static Matrix4D rootMatrix;

		internal static ObjectManager.StaticObject ReadObject(string FileName)
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

				ObjectManager.StaticObject obj = new ObjectManager.StaticObject();
				MeshBuilder builder = new MeshBuilder();

				// Global
				foreach (var mesh in scene.GlobalMeshes)
				{
					MeshBuilder(ref obj, ref builder, mesh);
				}

				if (scene.RootNode != null)
				{
					// Root Node
					if (scene.RootNode.TrafoMatrix != OpenTK.Matrix4.Zero)
					{
						rootMatrix = ConvertMatrix(scene.RootNode.TrafoMatrix);
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
				Interface.AddMessage(MessageType.Error, false, e.Message + " in " + FileName);
				return null;
			}
#endif
		}

		private static void  MeshBuilder(ref ObjectManager.StaticObject obj, ref MeshBuilder builder, AssimpNET.X.Mesh mesh)
		{
			if (builder.Vertices.Length != 0)
			{
				builder.Apply(ref obj);
				builder = new MeshBuilder();
			}

			int nVerts = mesh.Positions.Count;
			if (nVerts == 0)
			{
				//Some null objects contain an empty mesh
				Interface.AddMessage(MessageType.Warning, false, "nVertices should be greater than zero in Mesh " + mesh.Name);
			}
			int v = builder.Vertices.Length;
			Array.Resize(ref builder.Vertices, v + nVerts);
			for (int i = 0; i < nVerts; i++)
			{
				builder.Vertices[v + i] = new Vertex(mesh.Positions[i].X, mesh.Positions[i].Y, mesh.Positions[i].Z);
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
				builder.Materials[m] = new Material();
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
						Interface.AddMessage(MessageType.Error, true, "Texure " + builder.Materials[m].DaytimeTexture + " was not found in file " + currentFile);
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

		private static void ChildrenNode(ref ObjectManager.StaticObject obj, ref MeshBuilder builder, Node child)
		{
			builder.TransformMatrix = ConvertMatrix(child.TrafoMatrix);

			foreach (var mesh in child.Meshes)
			{
				MeshBuilder(ref obj, ref builder, mesh);
			}
			foreach (var grandchild in child.Children)
			{
				ChildrenNode(ref obj, ref builder, grandchild);
			}
		}

		private static Matrix4D ConvertMatrix(OpenTK.Matrix4 matrix)
		{
			Vector4 Row0 = new Vector4(matrix.Row0.X, matrix.Row0.Y, matrix.Row0.Z, matrix.Row0.W);
			Vector4 Row1 = new Vector4(matrix.Row1.X, matrix.Row1.Y, matrix.Row1.Z, matrix.Row1.W);
			Vector4 Row2 = new Vector4(matrix.Row2.X, matrix.Row2.Y, matrix.Row2.Z, matrix.Row2.W);
			Vector4 Row3 = new Vector4(matrix.Row3.X, matrix.Row3.Y, matrix.Row3.Z, matrix.Row3.W);
			return new Matrix4D(Row0, Row1, Row2, Row3);
		}
	}
}
