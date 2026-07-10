//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

using OpenBveApi.Math;
using OpenBveApi.Objects;
using System;
using System.Collections.Generic;

namespace Object.CsvB3d
{
	internal partial class NewParser
	{
		internal static void CreateCylinder(ref MeshBuilder Builder, int numFaces, double upperRadius, double lowerRadius, double height)
		{
			// parameters
			bool upperCap = upperRadius > 0.0;
			bool lowerCap = lowerRadius > 0.0;
			int m = (upperCap ? 1 : 0) + (lowerCap ? 1 : 0);
			upperRadius = Math.Abs(upperRadius);
			lowerRadius = Math.Abs(lowerRadius);
			double ns = height >= 0.0 ? 1.0 : -1.0;
			// initialization
			Vector3[] Normals = new Vector3[2 * numFaces];
			double d = 2.0 * Math.PI / numFaces;
			double g = 0.5 * height;
			double t = 0.0;
			double a = height != 0.0 ? Math.Atan((lowerRadius - upperRadius) / height) : 0.0;
			// vertices and normals
			int v = Builder.Vertices.Count;
			for (int i = 0; i < numFaces; i++)
			{
				double dx = Math.Cos(t);
				double dz = Math.Sin(t);
				double lx = dx * lowerRadius;
				double lz = dz * lowerRadius;
				double ux = dx * upperRadius;
				double uz = dz * upperRadius;
				Builder.Vertices.Add(new Vertex(ux, g, uz));
				Builder.Vertices.Add(new Vertex(lx, -g, lz));
				Vector3 normal = new Vector3(dx * ns, 0.0, dz * ns);
				Vector3 s = Vector3.Cross(normal, Vector3.Down);
				normal.Rotate(s, a);
				Normals[2 * i + 0] = new Vector3(normal);
				Normals[2 * i + 1] = new Vector3(normal);
				t += d;
			}
			// faces

			for (int i = 0; i < numFaces; i++)
			{
				int i0 = (2 * i + 2) % (2 * numFaces);
				int i1 = (2 * i + 3) % (2 * numFaces);
				int i2 = 2 * i + 1;
				int i3 = 2 * i;
				MeshFace f = new MeshFace(new[] { new MeshFaceVertex(v + i0, Normals[i0]), new MeshFaceVertex(v + i1, Normals[i1]), new MeshFaceVertex(v + i2, Normals[i2]), new MeshFaceVertex(v + i0, Normals[i0]), new MeshFaceVertex(v + i2, Normals[i2]), new MeshFaceVertex(v + i3, Normals[i3]) }, 0);
				f.Flags = FaceFlags.Triangles;
				Builder.Faces.Add(f);
			}

			for (int i = 0; i < m; i++)
			{
				List<MeshFaceVertex> vertices = new List<MeshFaceVertex>();
				for (int j = 0; j < numFaces; j++)
				{
					if (vertices.Count > 2)
					{
						vertices.Add(vertices[0]);
						vertices.Add(vertices[vertices.Count - 2]);
					}
					if (i == 0 & lowerCap)
					{
						// lower cap
						vertices.Add(new MeshFaceVertex(v + 2 * j + 1));
					}
					else
					{
						// upper cap
						vertices.Add(new MeshFaceVertex(v + 2 * (numFaces - j - 1)));
					}
				}
				vertices.Add(vertices[0]);
				vertices.Add(vertices[vertices.Count - 1]);
				vertices.Add(vertices[1]);
				MeshFace f = new MeshFace(vertices.ToArray(), 0);
				f.Flags = FaceFlags.Triangles;
				Builder.Faces.Add(f);
			}
		}
	}
}
