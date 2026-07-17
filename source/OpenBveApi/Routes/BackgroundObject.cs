using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Hosts;
using System;
using System.Collections.Generic;

namespace OpenBveApi.Routes
{
	/// <summary>Represents a background object</summary>
	public sealed class BackgroundObject : BackgroundHandle
	{
		/// <summary>The object used for this background (NOTE: Static objects only)</summary>
		public readonly StaticObject Object;
		/// <summary>The animated object collection used for this background (if an animated file is used)</summary>
		public readonly AnimatedObjectCollection AnimatedObject;
		/// <summary>The clipping distance required to fully render the object</summary>
		public readonly double ClipDistance = 0;
		/// <summary>The object state</summary>
		public readonly ObjectState ObjectState;

		/// <summary>Creates a new background object</summary>
		/// <param name="staticObject">The object to use for the background</param>
		/// <param name="backgroundImageDistance">The user-selected viewing distance</param>
		/// <param name="createCylinderCaps">Whether to auto-generate cylinder caps</param>
		public BackgroundObject(StaticObject staticObject, double backgroundImageDistance, bool createCylinderCaps = false)
		{
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
				for (int i = 0; i < newObject.Mesh.Vertices.Length; i++)
				{
					if (newObject.Mesh.Vertices[i].Coordinates.Y > 0)
					{
						vertexIndicies.Add(i);
					}
				}
				int v = newObject.Mesh.Vertices.Length;
				vertexIndicies.Add(v);
				Array.Resize(ref newObject.Mesh.Vertices, v + 1);
				newObject.Mesh.Vertices[v] = new Vertex(0, newObject.Mesh.Vertices[vertexIndicies[0]].Coordinates.Y + 100, 0);
				int f = newObject.Mesh.Faces.Length;
				Array.Resize(ref newObject.Mesh.Faces, f + 1);
				newObject.Mesh.Faces[f] = new MeshFace(vertexIndicies.ToArray(), FaceFlags.Face2Mask);
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
					Vector4 v0 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[0].Index].Coordinates, 1.0);
					Vector4 v1 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[1].Index].Coordinates, 1.0);
					Vector4 v2 = new Vector4(staticObject.Mesh.Vertices[staticObject.Mesh.Faces[i].Vertices[2].Index].Coordinates, 1.0);
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

			ObjectState = new ObjectState(Object);
		}

		/// <summary>Creates a new background object from an animated object collection</summary>
		/// <param name="animatedObject">The animated object collection to use for the background</param>
		/// <param name="backgroundImageDistance">The user-selected viewing distance</param>
		/// <param name="host">The host interface, used to register the dynamic object states</param>
		public BackgroundObject(AnimatedObjectCollection animatedObject, double backgroundImageDistance, HostInterface host)
		{
			BackgroundImageDistance = backgroundImageDistance;
			AnimatedObject = (AnimatedObjectCollection)animatedObject.Clone();
			//Register the internal dynamic object states so their VAOs are created
			foreach (AnimatedObject obj in AnimatedObject.Objects)
			{
				if (obj.States.Length == 0)
				{
					continue;
				}
				host.CreateDynamicObject(ref obj.internalObject);
				obj.internalObject.Prototype = obj.States[0].Prototype;
				obj.CurrentState = 0;
				foreach (ObjectState state in obj.States)
				{
					if (state.Prototype == null)
					{
						continue;
					}
					foreach (Vertex v in state.Prototype.Mesh.Vertices)
					{
						ClipDistance = System.Math.Max(ClipDistance, System.Math.Abs(v.Coordinates.X));
						ClipDistance = System.Math.Max(ClipDistance, System.Math.Abs(v.Coordinates.Z));
					}
				}
			}
		}

		/// <inheritdoc/>
		public override void UpdateBackground(double secondsSinceMidnight, double timeElapsed, bool target)
		{
			if (AnimatedObject != null)
			{
				foreach (AnimatedObject obj in AnimatedObject.Objects)
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
