//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, The OpenBVE Project
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
using System.Linq;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace OpenBveApi.Routes
{
	/// <summary>Represents a background object</summary>
	public sealed class BackgroundObject : BackgroundHandle
	{
		/// <summary>The object used for this background</summary>
		public readonly UnifiedObject Object;
		/// <summary>The clipping distance required to fully render the object</summary>
		public readonly double ClipDistance = 0;
		/// <summary>The object state</summary>
		public readonly ObjectState ObjectState;

		/// <summary>Creates a new background object</summary>
		/// <param name="staticObject">The object to use for the background</param>
		/// <param name="backgroundImageDistance">The user-selected viewing distance</param>
		/// <param name="createCylinderCaps">Whether to auto-generate cylinder caps</param>
		/// <param name="fogDistance">The fog distance</param>
		public BackgroundObject(StaticObject staticObject, double backgroundImageDistance, bool createCylinderCaps = false, double fogDistance = 600)
		{
			FogDistance = fogDistance;
			BackgroundImageDistance = backgroundImageDistance;
			if (createCylinderCaps)
			{
				/*
				 * BVE5 uses object based backgrounds.
				 * Unfortunately, it's also restricted to the cab and in camera rotation.
				 * This means that objects tend to be a cylinder with no caps, which is *bad*
				 * as we have full camera rotation
				 */
				StaticObject newObject = (StaticObject)staticObject.Clone();
				List<int> vertexIndicies = new List<int>();

				bool wrapX = false, wrapY = false;
				Vector2 referenceCoords = Vector2.Null;

				for (int i = 0; i < newObject.Mesh.Vertices.Length; i++)
				{
					if (newObject.Mesh.Vertices[i].Coordinates.Y > 0)
					{
						vertexIndicies.Add(i);
						if (vertexIndicies.Count > 0)
						{
							if (newObject.Mesh.Vertices[i].TextureCoordinates.X != newObject.Mesh
									.Vertices[vertexIndicies[vertexIndicies.Count - 1]]
									.TextureCoordinates.X)
							{
								wrapX = true;
							}
							if (newObject.Mesh.Vertices[i].TextureCoordinates.Y != newObject.Mesh
									.Vertices[vertexIndicies[vertexIndicies.Count - 1]]
									.TextureCoordinates.Y)
							{
								wrapY = true;
							}
						}
					}
					else
					{
						referenceCoords = newObject.Mesh.Vertices[i].TextureCoordinates;
					}
				}

				// sort, as no guarantee the verticies are actually in clock-order
				vertexIndicies = vertexIndicies.OrderBy(x => System.Math.Atan2(newObject.Mesh.Vertices[x].Coordinates.X, newObject.Mesh.Vertices[x].Coordinates.Z)).ToList();

				int v = newObject.Mesh.Vertices.Length;

				List<int> finalVertexIndicies = new List<int>();
				for (int i = 0; i < vertexIndicies.Count; i++)
				{
					finalVertexIndicies.Add(vertexIndicies[i]);
					finalVertexIndicies.Add(v);
					finalVertexIndicies.Add(i == vertexIndicies.Count - 1 ? vertexIndicies[0] : vertexIndicies[i + 1]);
				}

				Array.Resize(ref newObject.Mesh.Vertices, v + 1);
				newObject.Mesh.Vertices[v] = new Vertex(0, newObject.Mesh.Vertices[vertexIndicies[0]].Coordinates.Y + 100, 0);

				// build initial texture coordinate- use the top 35% (as the bottom half of the skydome often has distant scenery)
				newObject.Mesh.Vertices[v].TextureCoordinates = newObject.Mesh.Vertices[vertexIndicies[0]].TextureCoordinates - ((newObject.Mesh.Vertices[vertexIndicies[0]].TextureCoordinates - referenceCoords) * 0.35);
				// no guarantee that the X or Y will be the vertical texture coordinate, so zero the other based on what we found earlier
				if (wrapX)
				{
					newObject.Mesh.Vertices[v].TextureCoordinates.X = 0;
				}
				if (wrapY)
				{
					newObject.Mesh.Vertices[v].TextureCoordinates.Y = 0;
				}


				int f = newObject.Mesh.Faces.Length;
				Array.Resize(ref newObject.Mesh.Faces, f + 1);
				newObject.Mesh.Faces[f] = new MeshFace(finalVertexIndicies.ToArray(), FaceFlags.Triangles);
				newObject.Mesh.Faces[f].Flags |= FaceFlags.Triangles;
				Object = newObject;
			}
			else
			{
				Object = staticObject;
			}

			// sort object faces to ensure correct draw order
			double[] distances = new double[staticObject.Mesh.Faces.Length];
			for (int i = 0; i < staticObject.Mesh.Faces.Length; i++)
			{
				if (staticObject.Mesh.Faces[i].Vertices.Length >= 3)
				{
					Vector4 v0 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[0]].Coordinates, 1.0);
					Vector4 v1 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[1]].Coordinates, 1.0);
					Vector4 v2 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[2]].Coordinates, 1.0);
					Vector4 w1 = v1 - v0;
					Vector4 w2 = v2 - v0;
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					v0.Z *= -1.0;
					w1.Z *= -1.0;
					w2.Z *= -1.0;
					Vector3 d = Vector3.Cross(w1.Xyz, w2.Xyz);
					double t = d.Norm();
					if (t != 0.0)
					{
						d /= t;
						t = Vector3.Dot(d, v0.Xyz);
						distances[i] = -t * t;
					}

				}
			}
			
			Array.Sort(distances, staticObject.Mesh.Faces);

			//As we are using an object based background, calculate the minimum clip distance
			for (int i = 0; i < staticObject.Mesh.Vertices.Length; i++)
			{
				double X = System.Math.Abs(staticObject.Mesh.Vertices[i].Coordinates.X);
				double Z = System.Math.Abs(staticObject.Mesh.Vertices[i].Coordinates.Z);

				if (X > ClipDistance)
				{
					ClipDistance = X;
				}

				if (Z > ClipDistance)
				{
					ClipDistance = Z;
				}
			}

			ObjectState = new ObjectState(Object as StaticObject);
		}

		/// <summary>Creates a new background object from an animated object collection</summary>
		/// <param name="animatedObject">The animated object collection to use for the background</param>
		/// <param name="backgroundImageDistance">The user-selected viewing distance</param>
		/// <param name="fogDistance">The fog distance</param>
		public BackgroundObject(AnimatedObjectCollection animatedObject, double backgroundImageDistance, double fogDistance = 600)
		{
			FogDistance = fogDistance;
			BackgroundImageDistance = backgroundImageDistance;
			AnimatedObjectCollection animatedObjectCollection = (AnimatedObjectCollection)animatedObject.Clone();
			//Register the internal dynamic object states so their VAOs are created
			foreach (AnimatedObject obj in animatedObjectCollection.Objects)
			{
				if (obj.States.Length == 0)
				{
					continue;
				}
				animatedObject.currentHost.CreateDynamicObject(ref obj.internalObject);
				obj.internalObject.Prototype = obj.States[0].Prototype;
				obj.CurrentState = 0;
				foreach (ObjectState state in obj.States)
				{
					if (state.Prototype == null)
					{
						continue;
					}
					foreach (VertexTemplate v in state.Prototype.Mesh.Vertices)
					{
						ClipDistance = System.Math.Max(ClipDistance, System.Math.Abs(v.Coordinates.X));
						ClipDistance = System.Math.Max(ClipDistance, System.Math.Abs(v.Coordinates.Z));
					}
				}
			}

			Object = animatedObjectCollection;
		}

		/// <inheritdoc/>
		public override void UpdateBackground(double secondsSinceMidnight, double timeElapsed, bool target)
		{
			if (Object is AnimatedObjectCollection animatedObjectCollection)
			{
				foreach (AnimatedObject obj in animatedObjectCollection.Objects)
				{
					if (obj.States.Length == 0)
					{
						continue;
					}
					obj.Update(null, 0, 0, Vector3.Zero, Vector3.Forward, Vector3.Up, Vector3.Right, true, true, timeElapsed, true);
				}
			}
			//Static objects require no updates
		}
	}
}
