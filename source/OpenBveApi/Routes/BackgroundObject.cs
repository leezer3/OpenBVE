using System;
using System.Collections.Generic;
using OpenBveApi.Objects;

namespace OpenBveApi.Routes
{
	/// <summary>Represents a background object</summary>
	public class BackgroundObject : BackgroundHandle
	{
		/// <summary>The object used for this background (NOTE: Static objects only)</summary>
		public readonly StaticObject Object;
		/// <summary>The clipping distance required to fully render the object</summary>
		public readonly double ClipDistance = 0;

		/// <summary>Creates a new background object</summary>
		/// <param name="Object">The object to use for the background</param>
		/// <param name="CreateCylinderCaps">Whether to auto-generate cylinder caps</param>
		public BackgroundObject(StaticObject Object, bool CreateCylinderCaps = false)
		{
			if (CreateCylinderCaps)
			{
				/*
				 * BVE5 uses object based backgrounds.
				 * Unfortunately, it's also restricted to the cab and in camera rotation.
				 * This means that objects tend to be a cylinder with no caps, which is *bad*
				 * as we have full camera rotation
				 */
				StaticObject newObject = (StaticObject)Object.Clone();
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
				MeshFaceVertex[] face = new MeshFaceVertex[vertexIndicies.Count];
				for (int i = 0; i < vertexIndicies.Count; i++)
				{
					face[i] = new MeshFaceVertex(vertexIndicies[i]);
				}

				newObject.Mesh.Faces[f] = new MeshFace(vertexIndicies.ToArray(), FaceFlags.Face2Mask);
				this.Object = newObject;
			}
			else
			{
				this.Object = Object;
			}
			

			//As we are using an object based background, calculate the minimum clip distance
			for (int i = 0; i < Object.Mesh.Vertices.Length; i++)
			{
				double X = System.Math.Abs(Object.Mesh.Vertices[i].Coordinates.X);
				double Z = System.Math.Abs(Object.Mesh.Vertices[i].Coordinates.Z);

				if (X > ClipDistance)
				{
					ClipDistance = X;
				}

				if (Z > ClipDistance)
				{
					ClipDistance = Z;
				}
			}
		}

		/// <inheritdoc/>
		public override void UpdateBackground(double SecondsSinceMidnight, double TimeElapsed, bool Target)
		{
			//No updates required
		}
	}
}
