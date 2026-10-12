// Portions of this file (CullVertices / GenerateVertexRemap / FilterTriangles /
// GeneratePositionRemap / OptimizeOverdraw) are a C# port of meshoptimizer
// (https://github.com/zeux/meshoptimizer), used under the MIT License:
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
using OpenBveApi.Math;

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
			FilterTriangles(mesh, ref f);
			MergeFaces(mesh, ref f);
			OptimizeOverdraw(mesh);
			// shrink the backing arrays down to the live entries
			if (m != mesh.Materials.Length)
			{
				Array.Resize(ref mesh.Materials, m);
			}
			if (f != mesh.Faces.Length)
			{
				Array.Resize(ref mesh.Faces, f);
			}
		}

		// Bucket count for an open-addressing table holding ~count entries (25% headroom).
		private static int HashBuckets(int count)
		{
			int buckets = 1;
			while (buckets < count + count / 4)
			{
				buckets *= 2;
			}
			return buckets;
		}

		// Fresh table with every slot marked empty. -1 is never a valid vertex, id, or key part.
		private static int[] NewTable(int size)
		{
			int[] table = new int[size];
			for (int i = 0; i < size; i++)
			{
				table[i] = -1;
			}
			return table;
		}

		private static bool IsTriangles(MeshFace face)
		{
			return (face.Flags & FaceFlags.FaceTypeMask) == FaceFlags.Triangles;
		}

		private static void EliminateInvalidFaces(Mesh mesh, ref int f)
		{
			// Drop faces too small to draw and trim leftovers that don't fill a primitive.
			// Survivors compact down in one pass, keeping their original order.
			int write = 0;
			for (int i = 0; i < f; i++)
			{
				int count = mesh.Faces[i].Vertices.Length;
				int trimmed, min;
				switch (mesh.Faces[i].Flags & FaceFlags.FaceTypeMask)
				{
					case FaceFlags.Triangles:
						min = 3;
						trimmed = count / 3 * 3;
						break;
					case FaceFlags.Quads:
						min = 4;
						trimmed = count & ~3;
						break;
					case FaceFlags.QuadStrip:
						min = 4;
						trimmed = count & ~1;
						break;
					default:
						min = 3;
						trimmed = count;
						break;
				}
				if (trimmed < min)
				{
					continue;
				}
				if (trimmed != count)
				{
					Array.Resize(ref mesh.Faces[i].Vertices, trimmed);
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
			// Drop materials no face uses, then squeeze the surviving face indices down.
			bool[] materialUsed = new bool[m];
			for (int i = 0; i < f; i++)
			{
				materialUsed[mesh.Faces[i].Material] = true;
			}
			int[] remap = new int[m];
			int newM = 0;
			for (int i = 0; i < m; i++)
			{
				remap[i] = materialUsed[i] ? newM++ : -1;
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
			// Fold equal materials into the first of their kind. Materials are compared
			// pairwise but faces are remapped in a single pass, so this costs O(m^2 + f).
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
			// first[] holds increasing indices, so copying forward never clobbers an unread entry
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
			// Merge duplicate vertices and drop ones no face uses. Skipped when the caller
			// wants vertices preserved. The dedup itself ports meshoptimizer's
			// generateVertexRemap / remapVertexBuffer / remapIndexBuffer (MIT, see header
			// above), comparing vertices via VertexTemplate.Equals instead of raw bytes.
			if (!preserveVertices && vertexCulling)
			{
				VertexTemplate[] vertices = mesh.Vertices;
				int[] remap = NewTable(vertices.Length);
				int uniqueCount = GenerateVertexRemap(vertices, mesh.Faces, faceCount, remap);
				VertexTemplate[] newVertices = new VertexTemplate[uniqueCount];
				for (int i = 0; i < vertices.Length; i++)
				{
					int newIndex = remap[i];
					if (newIndex != -1)
					{
						newVertices[newIndex] = vertices[i];
					}
				}
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
			int[] table = NewTable(HashBuckets(vertices.Length));
			int mask = table.Length - 1;
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
					uint h;
					unchecked
					{
						// MurmurHash3 fmix32 avalanche. VertexTemplate.GetHashCode has no bit
						// diffusion on its own (small integral floats hash with zero low bits),
						// which power-of-two masking needs. Keep the int->uint reinterpretation
						// in here: negative hash codes throw in checked (Debug) builds.
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
						bucket = (bucket + probe) & mask; // hash collision, triangular probing
					}
				}
			}
			return nextVertex;
		}

		private static void Triangulate(Mesh mesh, int f)
		{
			// Fan-triangulate polygons and quads, preserving wind order: 0-1-2, 0-2-3, 0-3-4, ...
			for (int i = 0; i < f; ++i)
			{
				FaceFlags type = mesh.Faces[i].Flags & FaceFlags.FaceTypeMask;
				if (type != FaceFlags.Quads && type != FaceFlags.Polygon)
				{
					continue;
				}
				MeshFaceVertex[] poly = mesh.Faces[i].Vertices;
				MeshFaceVertex[] tris = new MeshFaceVertex[(poly.Length - 2) * 3];
				for (int t = 0, v = 0; t < poly.Length - 2; t++)
				{
					tris[v++] = poly[0];
					tris[v++] = poly[t + 1];
					tris[v++] = poly[t + 2];
				}
				mesh.Faces[i].Vertices = tris;
				mesh.Faces[i].Flags &= ~FaceFlags.FaceTypeMask;
				mesh.Faces[i].Flags |= FaceFlags.Triangles;
			}
		}

		private static void Decompose(Mesh mesh, ref int f)
		{
			// Split faces holding several triangles/quads into one face each.
			// The extra faces are counted up front so the backing array grows at most once.
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
			// We already know exactly how big the result is, so allocate it once. Growing by
			// doubling instead costs a reallocation plus a copy per doubling, which for a dense
			// mesh means copying the whole face array several times over.
			if (needed != mesh.Faces.Length)
			{
				MeshFace[] grown = new MeshFace[needed];
				Array.Copy(mesh.Faces, grown, f);
				mesh.Faces = grown;
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

		private static void FilterTriangles(Mesh mesh, ref int f)
		{
			// Drop zero-area and repeated triangles (a port of meshoptimizer's filterIndexBuffer).
			// Triangles match by position only, ignoring rotation but not winding, so a mirrored
			// copy kept for double-sided rendering survives. Corner data (vertex indices and
			// normals) passes through untouched, as do non-triangle faces.
			int totalTris = 0;
			for (int i = 0; i < f; i++)
			{
				if (IsTriangles(mesh.Faces[i]))
				{
					totalTris += mesh.Faces[i].Vertices.Length / 3;
				}
			}
			if (totalTris == 0)
			{
				return;
			}
			int[] positionRemap = NewTable(mesh.Vertices.Length);
			GeneratePositionRemap(mesh.Vertices, mesh.Faces, f, positionRemap);

			int mask = HashBuckets(totalTris) - 1;
			int[] tableA = NewTable(mask + 1);
			int[] tableB = new int[mask + 1];
			int[] tableC = new int[mask + 1];
			// tableB/C need no init: tableA == -1 already means "empty, don't read the rest"

			int writeFace = 0;
			for (int i = 0; i < f; i++)
			{
				if (!IsTriangles(mesh.Faces[i]))
				{
					if (writeFace != i)
					{
						mesh.Faces[writeFace] = mesh.Faces[i];
					}
					writeFace++;
					continue;
				}
				MeshFaceVertex[] faceVertices = mesh.Faces[i].Vertices;
				int tris = faceVertices.Length / 3;
				// A trailing partial triple can't happen after EliminateInvalidFaces, but if it
				// ever does, carry it over verbatim instead of silently eating it.
				int remainder = faceVertices.Length - tris * 3;
				int writeTri = 0;
				for (int t = 0; t < tris; t++)
				{
					int a = positionRemap[faceVertices[t * 3].Index];
					int b = positionRemap[faceVertices[t * 3 + 1].Index];
					int c = positionRemap[faceVertices[t * 3 + 2].Index];
					if (a == b || a == c || b == c)
					{
						continue; // zero-area triangle
					}
					// Rotate the triple so the smallest corner comes first; mirrored
					// windings stay distinct and are kept.
					int ra = a, rb = b, rc = c;
					if (rb < ra && rb < rc)
					{
						ra = b;
						rb = c;
						rc = a;
					}
					else if (rc < ra && rc < rb)
					{
						ra = c;
						rb = a;
						rc = b;
					}
					uint h;
					unchecked
					{
						h = ((uint)ra * 73856093u) ^ ((uint)rb * 19349663u) ^ ((uint)rc * 83492791u);
						h &= (uint)mask;
					}
					bool duplicate = false;
					int bucket = (int)h;
					for (int probe = 0; ; probe++)
					{
						if (tableA[bucket] == -1)
						{
							tableA[bucket] = ra;
							tableB[bucket] = rb;
							tableC[bucket] = rc;
							break;
						}
						if (tableA[bucket] == ra && tableB[bucket] == rb && tableC[bucket] == rc)
						{
							duplicate = true;
							break;
						}
						bucket = (bucket + probe + 1) & mask; // hash collision, quadratic probing
					}
					if (duplicate)
					{
						continue;
					}
					if (writeTri != t)
					{
						faceVertices[writeTri * 3] = faceVertices[t * 3];
						faceVertices[writeTri * 3 + 1] = faceVertices[t * 3 + 1];
						faceVertices[writeTri * 3 + 2] = faceVertices[t * 3 + 2];
					}
					writeTri++;
				}
				if (writeTri == 0 && remainder == 0)
				{
					continue;
				}
				for (int k = 0; k < remainder; k++)
				{
					faceVertices[writeTri * 3 + k] = faceVertices[tris * 3 + k];
				}
				if (writeTri * 3 + remainder != faceVertices.Length)
				{
					Array.Resize(ref mesh.Faces[i].Vertices, writeTri * 3 + remainder);
				}
				if (writeFace != i)
				{
					mesh.Faces[writeFace] = mesh.Faces[i];
				}
				writeFace++;
			}
			f = writeFace;
		}

		/// <summary>Builds a dense first-referenced-wins id per distinct position (Coordinates only)</summary>
		/// <returns>The number of unique positions</returns>
		private static int GeneratePositionRemap(VertexTemplate[] vertices, MeshFace[] faces, int faceCount, int[] remap)
		{
			// Same open-addressing scheme as GenerateVertexRemap, but corners match on
			// Coordinates alone. Bit diffusion follows meshopt::VertexCustomHasher.
			int[] table = NewTable(HashBuckets(vertices.Length));
			int mask = table.Length - 1;
			int nextId = 0;
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
					Vector3 p = vertices[oldIndex].Coordinates;
					uint h;
					unchecked
					{
						// Keep the double->uint reinterpretation in here: negative hash codes
						// throw in checked (Debug) builds.
						uint x = (uint)p.X.GetHashCode();
						uint y = (uint)p.Y.GetHashCode();
						uint z = (uint)p.Z.GetHashCode();
						x ^= x >> 17;
						y ^= y >> 17;
						z ^= z >> 17;
						h = (x * 73856093u) ^ (y * 19349663u) ^ (z * 83492791u);
						h &= (uint)mask;
					}
					int bucket = (int)h;
					for (int probe = 1; ; probe++)
					{
						int entry = table[bucket];
						if (entry == -1)
						{
							table[bucket] = oldIndex;
							remap[oldIndex] = nextId++;
							break;
						}
						Vector3 q = vertices[entry].Coordinates;
						if (q.X == p.X && q.Y == p.Y && q.Z == p.Z)
						{
							remap[oldIndex] = remap[entry];
							break;
						}
						bucket = (bucket + probe) & mask; // hash collision, triangular probing
					}
				}
			}
			return nextId;
		}

		// Clusters may cost up to 5% more cache misses than the input order (upstream default).
		private const float OverdrawThreshold = 1.05f;

		private static void OptimizeOverdraw(Mesh mesh)
		{
			// Draw near triangles first to cut overdraw (ports meshopt_optimizeOverdraw).
			// Each Triangles face is its own draw call, so clusters never cross faces.
			// Only the order changes; the triples themselves are untouched.
			int vCount = mesh.Vertices.Length;
			if (vCount == 0)
			{
				return;
			}
			double cx = 0, cy = 0, cz = 0;
			for (int i = 0; i < vCount; i++)
			{
				Vector3 p = mesh.Vertices[i].Coordinates;
				cx += p.X;
				cy += p.Y;
				cz += p.Z;
			}
			Vector3 meshCentroid = new Vector3(cx / vCount, cy / vCount, cz / vCount);
			uint[] timestamps = new uint[vCount]; // shared FIFO-cache scratch, reset per use
			for (int i = 0; i < mesh.Faces.Length; i++)
			{
				if (IsTriangles(mesh.Faces[i]) && mesh.Faces[i].Vertices.Length >= 6)
				{
					OptimizeFaceOverdraw(mesh, i, meshCentroid, timestamps);
				}
			}
		}

		private static void OptimizeFaceOverdraw(Mesh mesh, int faceIndex, Vector3 meshCentroid, uint[] timestamps)
		{
			const uint cacheSize = 16; // FIFO depth the clustering models after upstream
			MeshFaceVertex[] input = (MeshFaceVertex[])mesh.Faces[faceIndex].Vertices.Clone();
			int triCount = input.Length / 3;

			int[] hard = new int[triCount];
			int hardCount = GenerateHardBoundaries(input, triCount, cacheSize, timestamps, hard);
			int[] soft = new int[triCount + 1];
			int softCount = GenerateSoftBoundaries(input, triCount, hard, hardCount, cacheSize, timestamps, soft);

			double[] sortData = new double[softCount];
			CalculateSortData(mesh, input, soft, softCount, meshCentroid, sortData);
			ushort[] sortKeys = new ushort[softCount];
			int[] sortOrder = new int[softCount];
			CalculateSortOrder(sortData, sortKeys, sortOrder, softCount);

			MeshFaceVertex[] output = mesh.Faces[faceIndex].Vertices;
			int offset = 0;
			for (int it = 0; it < softCount; it++)
			{
				int cluster = sortOrder[it];
				int begin = soft[cluster] * 3;
				int end = cluster + 1 < softCount ? soft[cluster + 1] * 3 : input.Length;
				Array.Copy(input, begin, output, offset, end - begin);
				offset += end - begin;
			}
		}

		private static int UpdateCache(int a, int b, int c, uint cacheSize, uint[] timestamps, ref uint timestamp)
		{
			// Feed a triangle through a FIFO vertex cache model, counting misses.
			int misses = 0;
			if (timestamp - timestamps[a] > cacheSize)
			{
				timestamps[a] = timestamp++;
				misses++;
			}
			if (timestamp - timestamps[b] > cacheSize)
			{
				timestamps[b] = timestamp++;
				misses++;
			}
			if (timestamp - timestamps[c] > cacheSize)
			{
				timestamps[c] = timestamp++;
				misses++;
			}
			return misses;
		}

		private static int GenerateHardBoundaries(MeshFaceVertex[] tris, int triCount, uint cacheSize, uint[] timestamps, int[] hard)
		{
			// Cut a cluster wherever a triangle shares no cached vertex with the recent past.
			Array.Clear(timestamps, 0, timestamps.Length);
			uint timestamp = cacheSize + 1;
			int count = 0;
			for (int i = 0; i < triCount; i++)
			{
				int misses = UpdateCache(tris[i * 3].Index, tris[i * 3 + 1].Index, tris[i * 3 + 2].Index,
					cacheSize, timestamps, ref timestamp);
				if (i == 0 || misses == 3)
				{
					hard[count++] = i;
				}
			}
			return count;
		}

		private static int GenerateSoftBoundaries(MeshFaceVertex[] tris, int triCount, int[] hard, int hardCount, uint cacheSize, uint[] timestamps, int[] soft)
		{
			// Split each hard cluster so every soft cluster shades at roughly the same
			// miss ratio, within OverdrawThreshold of its hard cluster.
			Array.Clear(timestamps, 0, timestamps.Length);
			uint timestamp = 0;
			int count = 0;
			for (int it = 0; it < hardCount; it++)
			{
				int start = hard[it];
				int end = it + 1 < hardCount ? hard[it + 1] : triCount;
				timestamp += cacheSize + 1; // reset cache
				int clusterMisses = 0;
				for (int i = start; i < end; i++)
				{
					clusterMisses += UpdateCache(tris[i * 3].Index, tris[i * 3 + 1].Index, tris[i * 3 + 2].Index,
						cacheSize, timestamps, ref timestamp);
				}
				double clusterThreshold = OverdrawThreshold * ((double)clusterMisses / (end - start));
				soft[count++] = start;
				timestamp += cacheSize + 1; // reset cache
				int runningMisses = 0;
				int runningFaces = 0;
				for (int i = start; i < end; i++)
				{
					runningMisses += UpdateCache(tris[i * 3].Index, tris[i * 3 + 1].Index, tris[i * 3 + 2].Index,
						cacheSize, timestamps, ref timestamp);
					runningFaces++;
					if ((double)runningMisses / runningFaces <= clusterThreshold)
					{
						soft[count++] = i + 1;
						timestamp += cacheSize + 1; // reset cache
						runningMisses = 0;
						runningFaces = 0;
					}
				}
				// The trailing cluster is unfinished by definition and shades badly on its
				// own, so fold it back into the previous one (also drops a redundant 'end').
				if (soft[count - 1] != start)
				{
					count--;
				}
			}
			return count;
		}

		private static void CalculateSortData(Mesh mesh, MeshFaceVertex[] tris, int[] clusters, int clusterCount, Vector3 meshCentroid, double[] sortData)
		{
			// Score each cluster by how much it faces away from the mesh center, so near
			// patches draw first. Centroid is area-weighted, normals are summed.
			for (int cluster = 0; cluster < clusterCount; cluster++)
			{
				int begin = clusters[cluster];
				int end = cluster + 1 < clusterCount ? clusters[cluster + 1] : tris.Length / 3;
				double area = 0;
				double cX = 0, cY = 0, cZ = 0;
				double nX = 0, nY = 0, nZ = 0;
				for (int i = begin; i < end; i++)
				{
					Vector3 p0 = mesh.Vertices[tris[i * 3].Index].Coordinates;
					Vector3 p1 = mesh.Vertices[tris[i * 3 + 1].Index].Coordinates;
					Vector3 p2 = mesh.Vertices[tris[i * 3 + 2].Index].Coordinates;
					double e1X = p1.X - p0.X, e1Y = p1.Y - p0.Y, e1Z = p1.Z - p0.Z;
					double e2X = p2.X - p0.X, e2Y = p2.Y - p0.Y, e2Z = p2.Z - p0.Z;
					double nx = e1Y * e2Z - e1Z * e2Y;
					double ny = e1Z * e2X - e1X * e2Z;
					double nz = e1X * e2Y - e1Y * e2X;
					double a = System.Math.Sqrt(nx * nx + ny * ny + nz * nz);
					cX += (p0.X + p1.X + p2.X) * (a / 3);
					cY += (p0.Y + p1.Y + p2.Y) * (a / 3);
					cZ += (p0.Z + p1.Z + p2.Z) * (a / 3);
					nX += nx;
					nY += ny;
					nZ += nz;
					area += a;
				}
				if (area != 0)
				{
					cX /= area;
					cY /= area;
					cZ /= area;
				}
				double nLen = System.Math.Sqrt(nX * nX + nY * nY + nZ * nZ);
				if (nLen != 0)
				{
					nX /= nLen;
					nY /= nLen;
					nZ /= nLen;
				}
				sortData[cluster] = (cX - meshCentroid.X) * nX + (cY - meshCentroid.Y) * nY + (cZ - meshCentroid.Z) * nZ;
			}
		}

		private static void CalculateSortOrder(double[] sortData, ushort[] sortKeys, int[] sortOrder, int clusterCount)
		{
			// Stable 11-bit counting sort, high scores first.
			double max = 1e-3;
			for (int i = 0; i < clusterCount; i++)
			{
				max = System.Math.Max(max, System.Math.Abs(sortData[i]));
			}
			for (int i = 0; i < clusterCount; i++)
			{
				double key = 0.5 - 0.5 * (sortData[i] / max);
				int q = (int)(key * 2047.0 + 0.5);
				sortKeys[i] = (ushort)(q < 0 ? 0 : q > 2047 ? 2047 : q);
			}
			int[] histogram = new int[2048];
			for (int i = 0; i < clusterCount; i++)
			{
				histogram[sortKeys[i]]++;
			}
			int sum = 0;
			for (int i = 0; i < 2048; i++)
			{
				int c = histogram[i];
				histogram[i] = sum;
				sum += c;
			}
			for (int i = 0; i < clusterCount; i++)
			{
				sortOrder[histogram[sortKeys[i]]++] = i;
			}
		}

		private static void MergeFaces(Mesh mesh, ref int f)
		{
			// Pack same-material triangles into shared faces to cut draw calls. Only Triangles
			// with matching material and sidedness merge; everything else keeps its own face.
			// Faces come out in first-seen order.
			if (f <= 1)
			{
				return;
			}
			// Keys are (material, Face2Mask), so there can never be more than twice the material
			// count distinct ones. A flat table sized off the material count replaces a
			// Dictionary that was being allocated with room for one entry per face.
			int materialCount = 0;
			for (int i = 0; i < f; i++)
			{
				if (mesh.Faces[i].Material + 1 > materialCount)
				{
					materialCount = mesh.Faces[i].Material + 1;
				}
			}
			int keySpace = 8;
			while (keySpace < materialCount * 4)
			{
				keySpace <<= 1;
			}
			int keyMask = keySpace - 1;
			int[] keyTable = NewTable(keySpace); // slot -> group id + 1, -1 is empty

			int[] faceGroup = new int[f];
			int[] groupFirst = new int[f];
			int[] groupVertices = new int[f];
			int[] groupFaces = new int[f];
			int groupCount = 0;
			for (int i = 0; i < f; i++)
			{
				if (!IsTriangles(mesh.Faces[i]))
				{
					faceGroup[i] = groupCount;
					groupFirst[groupCount] = i;
					groupVertices[groupCount] = mesh.Faces[i].Vertices.Length;
					groupFaces[groupCount] = 1;
					groupCount++;
					continue;
				}
				// Face2Mask is a single bit (0 or 8), so both fit in one int key
				int key = (mesh.Faces[i].Material << 4) | (int)(mesh.Faces[i].Flags & FaceFlags.Face2Mask);
				int slot = key & keyMask;
				int gid = -1;
				for (int entry = keyTable[slot]; entry != -1; entry = keyTable[slot])
				{
					MeshFace groupFirstFace = mesh.Faces[groupFirst[entry]];
					int groupKey = (groupFirstFace.Material << 4) | (int)(groupFirstFace.Flags & FaceFlags.Face2Mask);
					if (groupKey == key)
					{
						gid = entry;
						break;
					}
					slot = (slot + 1) & keyMask; // hash collision
				}
				if (gid < 0)
				{
					gid = groupCount++;
					keyTable[slot] = gid;
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
