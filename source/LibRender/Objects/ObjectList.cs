using System;
using static LibRender.CameraProperties;

namespace LibRender
{
	/// <summary>A list of objects to be drawn by the renderer</summary>
	public class ObjectList
	{
		/// <summary>The faces within the object list</summary>
		public ObjectFace[] Faces;
		/// <summary>The total number of faces the object list contains</summary>
		public int FaceCount;
		/// <summary>The bounding boxes associated with the object list</summary>
		public BoundingBox[] BoundingBoxes;

		/// <summary>Creates a new ObjectList</summary>
		public ObjectList()
		{
			this.Faces = new ObjectFace[256];
			this.FaceCount = 0;
			this.BoundingBoxes = new BoundingBox[256];
		}

		/// <summary>Sorts the polgons contained within this ObjectList, near to far</summary>
		public void SortPolygons()
		{
			if (Faces == null || FaceCount <= 0)
			{
				return;
			}

			// calculate distance
			double cx = Camera.AbsolutePosition.X;
			double cy = Camera.AbsolutePosition.Y;
			double cz = Camera.AbsolutePosition.Z;
			for (int i = 0; i < FaceCount; i++)
			{
				int f = Faces[i].FaceIndex;
				if (Faces[i].ObjectReference.Mesh.Faces[f].Vertices.Length >= 3)
				{
					int v0 = Faces[i].ObjectReference.Mesh.Faces[f].Vertices[0].Index;
					int v1 = Faces[i].ObjectReference.Mesh.Faces[f].Vertices[1].Index;
					int v2 = Faces[i].ObjectReference.Mesh.Faces[f].Vertices[2].Index;
					double v0x = Faces[i].ObjectReference.Mesh.Vertices[v0].Coordinates.X;
					double v0y = Faces[i].ObjectReference.Mesh.Vertices[v0].Coordinates.Y;
					double v0z = Faces[i].ObjectReference.Mesh.Vertices[v0].Coordinates.Z;
					double v1x = Faces[i].ObjectReference.Mesh.Vertices[v1].Coordinates.X;
					double v1y = Faces[i].ObjectReference.Mesh.Vertices[v1].Coordinates.Y;
					double v1z = Faces[i].ObjectReference.Mesh.Vertices[v1].Coordinates.Z;
					double v2x = Faces[i].ObjectReference.Mesh.Vertices[v2].Coordinates.X;
					double v2y = Faces[i].ObjectReference.Mesh.Vertices[v2].Coordinates.Y;
					double v2z = Faces[i].ObjectReference.Mesh.Vertices[v2].Coordinates.Z;
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
