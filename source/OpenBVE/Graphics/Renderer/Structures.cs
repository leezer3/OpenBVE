using System;
using OpenBveShared;

namespace OpenBve
{
	internal static partial class Renderer
	{
		// output mode
		internal enum OutputMode
		{
			/// <summary>Overlays are shown if active</summary>
			Default = 0,
			/// <summary>The debug overlay is shown (F10)</summary>
			Debug = 1,
			/// <summary>The ATS debug overlay is shown (F10)</summary>
			DebugATS = 2,
			/// <summary>No overlays are shown</summary>
			None = 3
		}

		// object list
		
		
		

		/// <summary>Sorts the polgons contained within this ObjectList, near to far</summary>
		private static void SortPolygons(ObjectList list)
		{
			// calculate distance
			double cx = Camera.AbsoluteCameraPosition.X;
			double cy = Camera.AbsoluteCameraPosition.Y;
			double cz = Camera.AbsoluteCameraPosition.Z;
			for (int i = 0; i < list.FaceCount; i++)
			{
				int o = list.Faces[i].ObjectIndex;
				int f = list.Faces[i].FaceIndex;
				if (GameObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3)
				{
					int v0 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
					int v1 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
					int v2 = GameObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
					double v0x = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
					double v0y = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
					double v0z = GameObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
					double v1x = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
					double v1y = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
					double v1z = GameObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
					double v2x = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
					double v2y = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
					double v2z = GameObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
					double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
					double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
					double dx = -w1z * w2y + w1y * w2z;
					double dy = w1z * w2x - w1x * w2z;
					double dz = -w1y * w2x + w1x * w2y;
					double t = dx * dx + dy * dy + dz * dz;
					if (t == 0.0) continue;
					t = 1.0 / Math.Sqrt(t);
					dx *= t;
					dy *= t;
					dz *= t;
					double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
					t = dx * w0x + dy * w0y + dz * w0z;
					list.Faces[i].Distance = -t * t;
				}
			}

			// sort
			double[] distances = new double[list.FaceCount];
			for (int i = 0; i < list.FaceCount; i++)
			{
				distances[i] = list.Faces[i].Distance;
			}

			Array.Sort<double, ObjectFace>(distances, list.Faces, 0, list.FaceCount);
			// update objects
			for (int i = 0; i < list.FaceCount; i++)
			{
				OpenBveShared.Renderer.Objects[list.Faces[i].ObjectListIndex].FaceListReferences[list.Faces[i].FaceIndex].Index = i;
			}
		}

		
	}
}
