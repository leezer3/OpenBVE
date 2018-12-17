using System;

namespace OpenBveShared
{
	public class ObjectList
	{
		public ObjectFace[] Faces;
		public int FaceCount;
		public BoundingBox[] BoundingBoxes;

		public ObjectList()
		{
			this.Faces = new ObjectFace[256];
			this.FaceCount = 0;
			this.BoundingBoxes = new BoundingBox[256];
		}

		/// <summary>Sorts the polgons contained within this ObjectList, near to far</summary>
		public void SortPolygons()
		{
			// calculate distance
			double cx = Camera.AbsoluteCameraPosition.X;
			double cy = Camera.AbsoluteCameraPosition.Y;
			double cz = Camera.AbsoluteCameraPosition.Z;
			for (int i = 0; i < FaceCount; i++)
			{
				int o = Faces[i].ObjectIndex;
				int f = Faces[i].FaceIndex;
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
					Faces[i].Distance = -t * t;
				}
			}

			// sort
			double[] distances = new double[FaceCount];
			for (int i = 0; i < FaceCount; i++)
			{
				distances[i] = Faces[i].Distance;
			}

			Array.Sort<double, ObjectFace>(distances, Faces, 0, FaceCount);
			// update objects
			for (int i = 0; i < FaceCount; i++)
			{
				Renderer.Objects[Faces[i].ObjectListIndex].FaceListReferences[Faces[i].FaceIndex].Index = i;
			}
		}
	}
}
