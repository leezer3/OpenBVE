using System;
using System.Collections.Generic;

namespace OpenBveApi.Objects
{
	/// <summary>Pure mesh optimization algorithms for <see cref="StaticObject"/></summary>
	/// <remarks>Host / platform policy (thresholds, viewer skips, vertex-count guards) lives in StaticObject.Optimization.cs</remarks>
	internal static class MeshOptimizer
	{
		internal static void Optimize(Mesh mesh, bool preserveVertices, bool vertexCulling)
		{
			int m = mesh.Materials.Length;
			int f = mesh.Faces.Length;
			EliminateInvalidFaces(mesh, ref f);
			EliminateUnusedMaterials(mesh, ref m, f);
			EliminateDuplicateMaterials(mesh, ref m, f);
			CullVertices(mesh, preserveVertices, vertexCulling);
			Triangulate(mesh, f);
			Decompose(mesh, ref f);
			MergeFaces(mesh, ref f);
			// finalize arrays
			if (m != mesh.Materials.Length)
			{
				Array.Resize(ref mesh.Materials, m);
			}
			if (f != mesh.Faces.Length)
			{
				Array.Resize(ref mesh.Faces, f);
			}
		}

		private static void EliminateInvalidFaces(Mesh mesh, ref int f)
		{
			// eliminate invalid faces and reduce incomplete faces
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				bool keep;
				switch (type)
				{
					case FaceFlags.Triangles:
						keep = mesh.Faces[i].Vertices.Length >= 3;
						if (keep)
						{
							int n = (mesh.Faces[i].Vertices.Length / 3) * 3;
							if (mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref mesh.Faces[i].Vertices, n);
							}
						}
						break;
					case FaceFlags.Quads:
						keep = mesh.Faces[i].Vertices.Length >= 4;
						if (keep)
						{
							int n = mesh.Faces[i].Vertices.Length & ~3;
							if (mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref mesh.Faces[i].Vertices, n);
							}
						}
						break;
					case FaceFlags.QuadStrip:
						keep = mesh.Faces[i].Vertices.Length >= 4;
						if (keep)
						{
							int n = mesh.Faces[i].Vertices.Length & ~1;
							if (mesh.Faces[i].Vertices.Length != n)
							{
								Array.Resize(ref mesh.Faces[i].Vertices, n);
							}
						}
						break;
					default:
						keep = mesh.Faces[i].Vertices.Length >= 3;
						break;
				}
				if (!keep)
				{
					for (int j = i; j < f - 1; j++)
					{
						mesh.Faces[j] = mesh.Faces[j + 1];
					}
					f--;
					i--;
				}
			}
		}

		private static void EliminateUnusedMaterials(Mesh mesh, ref int m, int f)
		{
			// eliminate unused materials
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++)
			{
				materialUsed[mesh.Faces[i].Material] = true;
			}
			for (int i = 0; i < m; i++)
			{
				if (!materialUsed[i])
				{
					for (int j = 0; j < f; j++)
					{
						if (mesh.Faces[j].Material > i)
						{
							mesh.Faces[j].Material--;
						}
					}
					for (int j = i; j < m - 1; j++)
					{
						mesh.Materials[j] = mesh.Materials[j + 1];
						materialUsed[j] = materialUsed[j + 1];
					}
					m--;
					i--;
				}
			}
		}

		private static void EliminateDuplicateMaterials(Mesh mesh, ref int m, int f)
		{
			// eliminate duplicate materials
			for (int i = 0; i < m - 1; i++)
			{
				for (int j = i + 1; j < m; j++)
				{
					if (mesh.Materials[i] == mesh.Materials[j])
					{
						for (int k = 0; k < f; k++)
						{
							if (mesh.Faces[k].Material == j)
							{
								mesh.Faces[k].Material = (ushort)i;
							}
							else if (mesh.Faces[k].Material > j)
							{
								mesh.Faces[k].Material--;
							}
						}
						for (int k = j; k < m - 1; k++)
						{
							mesh.Materials[k] = mesh.Materials[k + 1];
						}
						m--;
						j--;
					}
				}
			}
		}

		private static void CullVertices(Mesh mesh, bool preserveVertices, bool vertexCulling)
		{
			// Cull identical and unreferenced vertices based on the hidden vertexCulling option.
			// Replaced old very slow OrderedDictionary implementation with generic Dictionary.
			if (!preserveVertices && vertexCulling)
			{
				Dictionary<VertexTemplate, int> uniqueVertices = new Dictionary<VertexTemplate, int>();
				VertexTemplate[] newVertices = new VertexTemplate[mesh.Vertices.Length];
				int count = 0;
				// Iterate through all referenced vertices in the faces.
				// This automatically ignores and culls unreferenced 'garbage' vertices in the original Mesh.Vertices array.
				for (int i = 0; i < mesh.Faces.Length; i++)
				{
					for (int j = 0; j < mesh.Faces[i].Vertices.Length; j++)
					{
						int oldIndex = mesh.Faces[i].Vertices[j];
						VertexTemplate vertex = mesh.Vertices[oldIndex];
						// If the exact same vertex structure hasn't been cached yet, cache it and add it to our new array.
						if (!uniqueVertices.TryGetValue(vertex, out int newIndex))
						{
							newIndex = count;
							uniqueVertices.Add(vertex, newIndex);
							newVertices[count] = vertex;
							count++;
						}
						// Update the face to point to the new, deduplicated vertex index.
						mesh.Faces[i].Vertices[j].Index = newIndex;
					}
				}
				// Copy the unique vertices back into the mesh
				mesh.Vertices = new VertexTemplate[count];
				Array.Copy(newVertices, 0, mesh.Vertices, 0, count);
			}
		}

		private static void Triangulate(Mesh mesh, int f)
		{
			// structure optimization
			// Triangularize all polygons and quads into triangles
			for (int i = 0; i < f; ++i)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				// Only transform quads and polygons
				if (type == FaceFlags.Quads || type == FaceFlags.Polygon)
				{
					int startingVertexCount = mesh.Faces[i].Vertices.Length;
					// One triangle for the first three points, then one for each vertex
					// Wind order is maintained.
					// Ex: 0, 1, 2; 0, 2, 3; 0, 3, 4; 0, 4, 5;
					int triCount = startingVertexCount - 2;
					int vertexCount = triCount * 3;
					// Copy old array for use as we work
					MeshFaceVertex[] originalPoly = (MeshFaceVertex[])mesh.Faces[i].Vertices.Clone();
					// Resize new array
					Array.Resize(ref mesh.Faces[i].Vertices, vertexCount);
					// Reference to output vertices
					MeshFaceVertex[] outVerts = mesh.Faces[i].Vertices;
					// Triangularize
					for (int triIndex = 0, vertIndex = 0, oldVert = 2; triIndex < triCount; ++triIndex, ++oldVert)
					{
						// First vertex is always the 0th
						outVerts[vertIndex] = originalPoly[0];
						vertIndex += 1;
						// Second vertex is one behind the current working vertex
						outVerts[vertIndex] = originalPoly[oldVert - 1];
						vertIndex += 1;
						// Third vertex is current working vertex
						outVerts[vertIndex] = originalPoly[oldVert];
						vertIndex += 1;
					}
					// Mark as triangle
					mesh.Faces[i].Flags &= ~FaceFlags.FaceTypeMask;
					mesh.Faces[i].Flags |= FaceFlags.Triangles;
				}
			}
		}

		private static void Decompose(Mesh mesh, ref int f)
		{
			// decomposite TRIANGLES and QUADS
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				int faceCount = 0;
				FaceFlags faceBit = 0;
				if (type == FaceFlags.Triangles)
				{
					faceCount = 3;
					faceBit = FaceFlags.Triangles;
				}
				else if (type == FaceFlags.Quads)
				{
					faceCount = 4;
					faceBit = FaceFlags.Triangles;
				}
				if (faceCount == 3 || faceCount == 4)
				{
					if (mesh.Faces[i].Vertices.Length > faceCount)
					{
						int n = (mesh.Faces[i].Vertices.Length - faceCount) / faceCount;
						while (f + n > mesh.Faces.Length)
						{
							Array.Resize(ref mesh.Faces, mesh.Faces.Length << 1);
						}
						for (int j = 0; j < n; j++)
						{
							mesh.Faces[f + j].Vertices = new MeshFaceVertex[faceCount];
							for (int k = 0; k < faceCount; k++)
							{
								mesh.Faces[f + j].Vertices[k] = mesh.Faces[i].Vertices[faceCount + faceCount * j + k];
							}
							mesh.Faces[f + j].Material = mesh.Faces[i].Material;
							mesh.Faces[f + j].Flags = mesh.Faces[i].Flags;
							mesh.Faces[i].Flags &= ~FaceFlags.FaceTypeMask;
							mesh.Faces[i].Flags |= faceBit;
						}
						Array.Resize(ref mesh.Faces[i].Vertices, faceCount);
						f += n;
					}
				}
			}
		}

		private static void MergeFaces(Mesh mesh, ref int f)
		{
			// Squish faces that have the same material.
			bool[] canMerge = new bool[f];
			for (int i = 0; i < f - 1; ++i)
			{
				int mergeVertices = 0;
				// Type of current face
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				FaceFlags face = mesh.Faces[i].Flags & FaceFlags.Face2Mask;
				// Find faces that can be merged
				for (int j = i + 1; j < f; ++j)
				{
					FaceFlags type2 = mesh.Faces[j].Flags & FaceFlags.FaceTypeMask;
					FaceFlags face2 = mesh.Faces[j].Flags & FaceFlags.Face2Mask;
					// Conditions for face merger
					bool mergeable = type == FaceFlags.Triangles &&
					                 type == type2 &&
					                 face == face2 &&
					                 mesh.Faces[i].Material == mesh.Faces[j].Material;
					canMerge[j] = mergeable;
					mergeVertices += mergeable ? mesh.Faces[j].Vertices.Length : 0;
				}
				if (mergeVertices == 0)
				{
					continue;
				}
				// Current end of array index
				int lastVertexIt = mesh.Faces[i].Vertices.Length;
				// Resize current face's vertices to have enough room
				Array.Resize(ref mesh.Faces[i].Vertices, lastVertexIt + mergeVertices);
				// Merge faces
				for (int j = i + 1; j < f; ++j)
				{
					if (canMerge[j])
					{
						// Copy vertices
						mesh.Faces[j].Vertices.CopyTo(mesh.Faces[i].Vertices, lastVertexIt);
						// Adjust index
						lastVertexIt += mesh.Faces[j].Vertices.Length;
					}
				}
				// Remove now unused faces
				int jump = 0;
				for (int j = i + 1; j < f; ++j)
				{
					if (canMerge[j])
					{
						jump += 1;
					}
					else if (jump > 0)
					{
						mesh.Faces[j - jump] = mesh.Faces[j];
					}
				}
				// Remove faces removed from face count
				f -= jump;
			}
		}
	}
}
