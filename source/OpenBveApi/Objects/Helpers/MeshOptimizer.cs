// Portions of this file (CullVertices / GenerateVertexRemap) are a C# port of
// meshoptimizer (https://github.com/zeux/meshoptimizer), used under the MIT License:
//
// Copyright (c) 2016-2026 Arseny Kapoulkine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
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
			CullVertices(mesh, f, preserveVertices, vertexCulling);
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
			// single-pass compaction: write index only advances for kept faces (O(f))
			int write = 0;
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
					continue;
				}
				if (write != i)
				{
					mesh.Faces[write] = mesh.Faces[i];
				}
				write++;
			}
			f = write;
		}

		private static void EliminateUnusedMaterials(Mesh mesh, ref int m, int f)
		{
			// eliminate unused materials via a remap table: one pass over faces (O(m + f))
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++)
			{
				materialUsed[mesh.Faces[i].Material] = true;
			}
			int[] remap = new int[m];
			int newM = 0;
			for (int i = 0; i < m; i++)
			{
				if (materialUsed[i])
				{
					remap[i] = newM++;
				}
				else
				{
					remap[i] = -1;
				}
			}
			if (newM == m)
			{
				return;
			}
			for (int j = 0; j < f; j++)
			{
				mesh.Faces[j].Material = (ushort)remap[mesh.Faces[j].Material];
			}
			int write = 0;
			for (int i = 0; i < m; i++)
			{
				if (materialUsed[i])
				{
					if (write != i)
					{
						mesh.Materials[write] = mesh.Materials[i];
					}
					write++;
				}
			}
			m = newM;
		}

		private static void EliminateDuplicateMaterials(Mesh mesh, ref int m, int f)
		{
			// eliminate duplicate materials, keeping the first occurrence
			// pairwise == over distinct materials only: O(m^2 + f) instead of O(m^2 * f)
			if (m <= 1)
			{
				return;
			}
			int[] remap = new int[m];
			int[] first = new int[m];
			int unique = 0;
			for (int i = 0; i < m; i++)
			{
				int found = -1;
				for (int u = 0; u < unique; u++)
				{
					if (mesh.Materials[first[u]] == mesh.Materials[i])
					{
						found = u;
						break;
					}
				}
				if (found == -1)
				{
					remap[i] = unique;
					first[unique] = i;
					unique++;
				}
				else
				{
					remap[i] = found;
				}
			}
			if (unique == m)
			{
				return;
			}
			for (int k = 0; k < f; k++)
			{
				mesh.Faces[k].Material = (ushort)remap[mesh.Faces[k].Material];
			}
			// first[u] >= u, so the forward copy never overwrites a not-yet-read entry
			for (int u = 0; u < unique; u++)
			{
				if (first[u] != u)
				{
					mesh.Materials[u] = mesh.Materials[first[u]];
				}
			}
			m = unique;
		}

		private static void CullVertices(Mesh mesh, int faceCount, bool preserveVertices, bool vertexCulling)
		{
			// Cull identical and unreferenced vertices based on the hidden vertexCulling option.
			// Deduplication is a C# port of meshoptimizer's generateVertexRemap / remapVertexBuffer /
			// remapIndexBuffer (MIT, see header above), adapted to compare vertices via
			// VertexTemplate.GetHashCode / Equals instead of raw byte comparison.
			if (!preserveVertices && vertexCulling)
			{
				VertexTemplate[] vertices = mesh.Vertices;
				int[] remap = new int[vertices.Length];
				for (int i = 0; i < remap.Length; i++)
				{
					remap[i] = -1;
				}
				int uniqueCount = GenerateVertexRemap(vertices, mesh.Faces, faceCount, remap);
				// Compact the vertex buffer, dropping unreferenced 'garbage' vertices.
				VertexTemplate[] newVertices = new VertexTemplate[uniqueCount];
				for (int i = 0; i < vertices.Length; i++)
				{
					int newIndex = remap[i];
					if (newIndex != -1)
					{
						newVertices[newIndex] = vertices[i];
					}
				}
				// Rewrite the live faces to point at the deduplicated vertices.
				for (int i = 0; i < faceCount; i++)
				{
					MeshFaceVertex[] faceVertices = mesh.Faces[i].Vertices;
					for (int j = 0; j < faceVertices.Length; j++)
					{
						faceVertices[j].Index = remap[faceVertices[j].Index];
					}
				}
				mesh.Vertices = newVertices;
			}
		}

		/// <summary>Builds a first-referenced-wins remap table over the vertices used by the live faces</summary>
		/// <returns>The number of unique vertices</returns>
		private static int GenerateVertexRemap(VertexTemplate[] vertices, MeshFace[] faces, int faceCount, int[] remap)
		{
			// Open-addressing hash table with triangular probing, cf. meshopt::hashLookup.
			int buckets = 1;
			while (buckets < vertices.Length + vertices.Length / 4)
			{
				buckets *= 2;
			}
			int[] table = new int[buckets];
			for (int i = 0; i < buckets; i++)
			{
				table[i] = -1;
			}
			int mask = buckets - 1;
			int nextVertex = 0;
			for (int i = 0; i < faceCount; i++)
			{
				MeshFaceVertex[] faceVertices = faces[i].Vertices;
				for (int j = 0; j < faceVertices.Length; j++)
				{
					int oldIndex = faceVertices[j].Index;
					if (remap[oldIndex] != -1)
					{
						continue;
					}
					// MurmurHash3 fmix32 avalanche. VertexTemplate.GetHashCode has no bit diffusion
					// (small integral floats hash with zero low bits), which power-of-two masking requires.
					uint h;
					unchecked
					{
						// NB: the int->uint bit reinterpretation must stay in unchecked scope:
						// negative hash codes are common and throw in checked (Debug) builds.
						h = (uint)vertices[oldIndex].GetHashCode();
						h ^= h >> 16;
						h *= 0x85ebca6b;
						h ^= h >> 13;
						h *= 0xc2b2ae35;
						h ^= h >> 16;
						h &= (uint)mask;
					}
					int bucket = (int)h;
					for (int probe = 1; ; probe++)
					{
						int entry = table[bucket];
						if (entry == -1)
						{
							table[bucket] = oldIndex;
							remap[oldIndex] = nextVertex++;
							break;
						}
						if (vertices[entry].Equals(vertices[oldIndex]))
						{
							remap[oldIndex] = remap[entry];
							break;
						}
						// Hash collision, triangular probing.
						bucket = (bucket + probe) & mask;
					}
				}
			}
			return nextVertex;
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
			// pre-size once: the inner grow loop used to double repeatedly on large meshes
			int needed = f;
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				int faceCount = type == FaceFlags.Triangles ? 3 : type == FaceFlags.Quads ? 4 : 0;
				if (faceCount != 0 && mesh.Faces[i].Vertices.Length > faceCount)
				{
					needed += (mesh.Faces[i].Vertices.Length - faceCount) / faceCount;
				}
			}
			while (needed > mesh.Faces.Length)
			{
				Array.Resize(ref mesh.Faces, mesh.Faces.Length << 1);
			}
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
			// Group-by-key in first-appearance order: O(f + totalVertices) instead of O(f^2).
			// Merge rule is unchanged: Triangles with equal material and Face2Mask merge,
			// keeping the first face's flags; all other faces stay untouched singletons.
			if (f <= 1)
			{
				return;
			}
			Dictionary<int, int> groupByKey = new Dictionary<int, int>(f);
			int[] faceGroup = new int[f];
			int[] groupFirst = new int[f];
			int[] groupVertices = new int[f];
			int[] groupFaces = new int[f];
			int groupCount = 0;
			for (int i = 0; i < f; i++)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				if (type != FaceFlags.Triangles)
				{
					faceGroup[i] = groupCount;
					groupFirst[groupCount] = i;
					groupVertices[groupCount] = mesh.Faces[i].Vertices.Length;
					groupFaces[groupCount] = 1;
					groupCount++;
					continue;
				}
				int key = (mesh.Faces[i].Material << 4) | (int)(mesh.Faces[i].Flags & FaceFlags.Face2Mask);
				int gid;
				if (!groupByKey.TryGetValue(key, out gid))
				{
					gid = groupCount++;
					groupByKey[key] = gid;
					groupFirst[gid] = i;
					groupVertices[gid] = 0;
					groupFaces[gid] = 0;
				}
				faceGroup[i] = gid;
				groupVertices[gid] += mesh.Faces[i].Vertices.Length;
				groupFaces[gid]++;
			}
			if (groupCount == f)
			{
				return;
			}
			MeshFace[] merged = new MeshFace[groupCount];
			int[] groupOffset = new int[groupCount];
			for (int g = 0; g < groupCount; g++)
			{
				merged[g] = mesh.Faces[groupFirst[g]];
				if (groupFaces[g] > 1)
				{
					merged[g].Vertices = new MeshFaceVertex[groupVertices[g]];
					groupOffset[g] = 0;
				}
			}
			for (int i = 0; i < f; i++)
			{
				int g = faceGroup[i];
				if (groupFaces[g] > 1)
				{
					MeshFaceVertex[] src = mesh.Faces[i].Vertices;
					src.CopyTo(merged[g].Vertices, groupOffset[g]);
					groupOffset[g] += src.Length;
				}
			}
			for (int g = 0; g < groupCount; g++)
			{
				mesh.Faces[g] = merged[g];
			}
			f = groupCount;
		}
	}
}
